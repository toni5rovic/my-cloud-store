using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using MyCloudStore.Shared.Responses;

namespace MyCloudStore.Client.Services
{
	public interface IFileService
	{
		Task UploadFileAsync(Stream fs, string fileName);
		Task<List<FileResult>> GetFilesAsync();
		Task<FileResult> GetFileAsync(Guid fileId);
	}
}
