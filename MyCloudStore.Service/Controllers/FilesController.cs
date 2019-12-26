using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MyCloudStore.Service.DataLayer;
using MyCloudStore.Service.DataLayer.Models;
using MyCloudStore.Shared.Requests;
using MyCloudStore.Shared.Responses;
using File = MyCloudStore.Service.DataLayer.Models.File;
using FileResult = MyCloudStore.Shared.Responses.FileResult;

namespace MyCloudStore.Service.Controllers
{
	[Route("api/[controller]")]
	[Authorize]
	[ApiController]
	public class FilesController : ControllerBase
	{
		private readonly AppDbContext context;
		private readonly IConfiguration configuration;

		public FilesController(AppDbContext context, IConfiguration configuration)
		{
			this.context = context;
			this.configuration = configuration;
		}

		// TODO: return only files from current user
		// GET: api/Files
		[HttpGet]
		public async Task<ActionResult<IList<FileResult>>> GetFiles()
		{
			var claims = HttpContext.User.Claims;
			var email = claims.Where(c => c.Type == ClaimTypes.Name).FirstOrDefault().Value;
			var user = await context.AppUsers.FirstOrDefaultAsync(u => u.Email == email);

			if (user == null)
			{
				return BadRequest();
			}

			// Get all files from the DB 
			var files = await context.Files
				.Where(f => f.UserId == user.Id)
				.ToListAsync();

			List<FileResult> toReturn = new List<FileResult>();
			foreach (var dbFile in files)
			{
				FileResult fileResult = new FileResult
				{
					Id = dbFile.Id,
					FileName = dbFile.FileName,
					HashValue = Convert.ToBase64String(dbFile.HashValue),
					Created = dbFile.Created
				};

				toReturn.Add(fileResult);
			}

			return toReturn;
		}

		// GET: api/Files/5
		[HttpGet("{id}")]
		public async Task<ActionResult<FileResult>> GetFile(Guid id)
		{
			var file = await context.Files.FindAsync(id);

			if (file == null)
			{
				return NotFound();
			}

			var claims = HttpContext.User.Claims;
			var email = claims.Where(c => c.Type == ClaimTypes.Name).FirstOrDefault().Value;
			var user = await context.AppUsers.FirstOrDefaultAsync(u => u.Email == email);

			if (file.UserId != user.Id)
			{
				return Unauthorized();
			}

			//////////////////////
			// TODO: cuva se FilePath u bazi, koristi to!
			string clientFilesFolder = configuration.GetValue<string>("ClientFilesFolderPath");

			string userFolderPath = Path.Combine(clientFilesFolder, user.Id.ToString());
			if (!Directory.Exists(userFolderPath))
			{
				return NotFound();
			}			

			string filePath = Path.Combine(userFolderPath, file.Id.ToString());
			//////////////////////

			if (!System.IO.File.Exists(filePath))
			{
				return NotFound();
			}

			byte[] fileBuffer;
			fileBuffer = await System.IO.File.ReadAllBytesAsync(filePath);

			// TODO: check if buffer is null

			FileResult fileResult = new FileResult
			{
				FileName = file.FileName,
				HashValue = Convert.ToBase64String(file.HashValue),
				Content = fileBuffer
			};

			return fileResult;
		}

		// GET: api/Files/storageSpace
		[HttpGet]
		[Route("storageSpace")]
		public async Task<ActionResult<StorageSpace>> StorageSpace()
		{
			var claims = HttpContext.User.Claims;
			var email = claims.Where(c => c.Type == ClaimTypes.Name).FirstOrDefault().Value;
			var user = (await context.AppUsers.Include(u => u.Files)
				.FirstOrDefaultAsync(u => u.Email == email));

			//////////////////////
			// TODO: cuva se FilePath u bazi, koristi to!
			string clientFilesFolder = configuration.GetValue<string>("ClientFilesFolderPath");

			string userFolderPath = Path.Combine(clientFilesFolder, user.Id.ToString());
			if (!Directory.Exists(userFolderPath))
			{
				return new StorageSpace()
				{
					MaxKBs = user.MaxKBs,
					CurrentKBs = 0
				};
			}

			var files = user.Files.ToList();

			long sumBytes = 0;
			foreach(var file in files)
			{
				string filePath = Path.Combine(userFolderPath, file.Id.ToString());
				long length = new System.IO.FileInfo(filePath).Length;
				sumBytes += length;
			}

			double sumKBs = sumBytes / 1024.0;

			StorageSpace storageSpace = new StorageSpace
			{
				MaxKBs = user.MaxKBs,
				CurrentKBs = sumKBs
			};

			return storageSpace;
		}

		// PUT: api/Files/5
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for
		// more details see https://aka.ms/RazorPagesCRUD.
		[HttpPut("{id}")]
		public async Task<IActionResult> PutFile(Guid id, File file)
		{
			if (id != file.Id)
			{
				return BadRequest();
			}

			context.Entry(file).State = EntityState.Modified;

			try
			{
				await context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!FileExists(id))
				{
					return NotFound();
				}
				else
				{
					throw;
				}
			}

			return NoContent();
		}

		// POST: api/Files
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for
		// more details see https://aka.ms/RazorPagesCRUD.
		[HttpPost]
		public async Task<ActionResult<File>> PostFile([FromForm] FileModel fileModel)
		{
			if (fileModel == null)
			{
				return BadRequest();
			}

			var claims = HttpContext.User.Claims;
			var email = claims.Where(c => c.Type == ClaimTypes.Name).FirstOrDefault().Value;
			var user = await context.AppUsers.FirstOrDefaultAsync(u => u.Email == email);

			string clientFilesFolder = configuration.GetValue<string>("ClientFilesFolderPath");

			string userFolder = Path.Combine(clientFilesFolder, user.Id.ToString());
			if (!Directory.Exists(userFolder))
			{
				Directory.CreateDirectory(userFolder);
			}

			// to be sure file with the same Guid does not exist already
			Guid fileGuid;
			string filePath;
			var ime = fileModel.Content.FileName;
			do
			{
				fileGuid = Guid.NewGuid();
				filePath = Path.Combine(userFolder, fileGuid.ToString());
			} while (System.IO.File.Exists(filePath));


			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await fileModel.Content.CopyToAsync(stream);
			}
			//await System.IO.File.WriteAllBytesAsync(filePath, fileModel.Content.);

			byte[] hashValueBytes = Convert.FromBase64String(fileModel.HashValue);
			File fileToAdd = new File
			{
				Id = fileGuid,
				FileName = fileModel.FileName,
				Path = filePath,
				HashValue = hashValueBytes,
				Created = DateTime.UtcNow,
				UserId = user.Id
			};

			await context.Files.AddAsync(fileToAdd);
			await context.SaveChangesAsync();

			return CreatedAtAction("GetFile", new { id = fileToAdd.Id }, fileToAdd);
		}

		// DELETE: api/Files/5
		[HttpDelete("{id}")]
		public async Task<ActionResult<File>> DeleteFile(Guid id)
		{
			var file = await context.Files.FindAsync(id);
			if (file == null)
			{
				return NotFound();
			}

			context.Files.Remove(file);
			await context.SaveChangesAsync();

			return file;
		}

		private bool FileExists(Guid id)
		{
			return context.Files.Any(e => e.Id == id);
		}
	}
}
