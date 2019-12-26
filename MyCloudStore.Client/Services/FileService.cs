using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Blazored.LocalStorage;
using System.Net.Http.Headers;
using System.IO;
using System.Text;
using MyCloudStore.Shared.Requests;
using MyCloudStore.Shared.Responses;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MyCloudStore.CryptoLibrary.Hash;
using Newtonsoft.Json;
using MyCloudStore.CryptoLibrary.Algorithms;
using System.Security.Cryptography;

namespace MyCloudStore.Client.Services
{
	public class FileService : IFileService
	{
		private readonly HttpClient httpClient;
		private readonly ILocalStorageService localStorage;

		public FileService(HttpClient httpClient,
			ILocalStorageService localStorage)
		{
			this.localStorage = localStorage;
			this.httpClient = httpClient;
		}

		public async Task UploadFileAsync(Stream fs, string fileName, string algorithm, string key)
		{
			string fileId = null;
			using (var form = new MultipartFormDataContent())
			using (var streamContent = new StreamContent(fs))
			{
				form.Add(new StringContent(fileName, Encoding.UTF8, "text/plain"), "\"FileName\"");
				
				var byteArray = await streamContent.ReadAsByteArrayAsync();
				TigerHash tigerHasher = new TigerHash();
				byte[] hashed = tigerHasher.Hash(byteArray, byteArray.Length);
				string hashedBase64String = Convert.ToBase64String(hashed);
				form.Add(new StringContent(hashedBase64String, Encoding.UTF8, "text/plain"), "\"HashValue\"");

				Console.WriteLine("Encryption left only");

				byte[] encrypted = await EncryptFile(byteArray, algorithm, key);
				Stream stream = new MemoryStream(encrypted);
				var encryptedStreamContent = new StreamContent(stream);
				form.Add(encryptedStreamContent, "\"Content\"", fileName);

				Console.WriteLine("Form ready to post");

				var response = await this.httpClient.PostAsync("api/Files/", form);

				encryptedStreamContent.Dispose();
				if (response.IsSuccessStatusCode)
				{
					var responseString = await response.Content.ReadAsStringAsync();
					var fileObject = JsonConvert.DeserializeObject<FileResult>(responseString);
					fileId = fileObject.Id.ToString();
					Console.WriteLine("Posted: " + fileId);
				}
			}

			if (String.IsNullOrEmpty(fileId))
				return;

			// add to localstorage
			ConfigFileEntry fileEntry = new ConfigFileEntry
			{
				Id = fileId,
				FileName = fileName,
				Algorithm = algorithm,
				Key = key
			};

			await AddToLocalStorage(fileEntry);
		}

		public async Task UploadConfigFileAsync(Stream stream, string fileName)
		{
			string json = "";
			using (StreamReader reader = new StreamReader(stream))
			{
				json = await reader.ReadToEndAsync();
			}

			var listOfFileEntries = JsonConvert.DeserializeObject<List<ConfigFileEntry>>(json);
			await localStorage.SetItemAsync("files", listOfFileEntries);
		}

		public async Task<string> DownloadConfigFileAsync()
		{
			List<ConfigFileEntry> result = await localStorage.GetItemAsync<List<ConfigFileEntry>>("files");
			if (result == null || result.Count == 0)
				return null;

			string json = JsonConvert.SerializeObject(result);
			return json;
		}

		public async Task<List<FileResult>> GetFilesAsync()
		{
			string URL = "api/Files/";
			return await this.httpClient.GetJsonAsync<List<FileResult>>(URL);
		}

		public async Task<FileResult> GetFileAsync(Guid fileId, string algorithm, string key)
		{
			string URL = "api/Files/" + fileId.ToString();
			var encryptedFile = await this.httpClient.GetJsonAsync<FileResult>(URL);

			encryptedFile.Content = await DecryptFile(encryptedFile.Content, algorithm, key);
			return encryptedFile;
		}

		public async Task<StorageSpace> GetStorageSpace()
		{
			string URL = "api/Files/storageSpace";
			return await this.httpClient.GetJsonAsync<StorageSpace>(URL);
		}

		private async Task<byte[]> EncryptFile(byte[] fileBytes, string algorithm, string key)
		{
			CryptoLibrary.CryptoCTRMode encryptor = new CryptoLibrary.CryptoCTRMode();
			if (algorithm == "A52")
			{
				A52 a52 = new A52();
				encryptor.SetCryptoAlgorithm(a52);
			}
			else if (algorithm == "RC4")
			{
				RC4 rc4 = new RC4();
				encryptor.SetCryptoAlgorithm(rc4);
			}

			byte[] keyBytes = Convert.FromBase64String(key);
			return encryptor.EncryptFile(fileBytes, keyBytes);
		}

		private async Task<byte[]> DecryptFile(byte[] fileBytes, string algorithm, string key)
		{
			CryptoLibrary.CryptoCTRMode encryptor = new CryptoLibrary.CryptoCTRMode();
			if (algorithm == "A52")
			{
				A52 a52 = new A52();
				encryptor.SetCryptoAlgorithm(a52);
			}
			else if (algorithm == "RC4")
			{
				RC4 rc4 = new RC4();
				encryptor.SetCryptoAlgorithm(rc4);
			}

			byte[] keyBytes = Convert.FromBase64String(key);
			return encryptor.DecryptFile(fileBytes, keyBytes);
		}

		public async Task<string> GetAlgorithm(Guid fileId)
		{
			var listOfFiles = await localStorage.GetItemAsync<List<ConfigFileEntry>>("files");
			if (listOfFiles == null)
				return null;

			var file = listOfFiles.Where(f => f.Id == fileId.ToString())
				.FirstOrDefault();
			
			if (file == null)
				return null;

			return file.Algorithm;
		}

		public async Task<string> GetKey(Guid fileId)
		{
			var listOfFiles = await localStorage.GetItemAsync<List<ConfigFileEntry>>("files");
			if (listOfFiles == null)
				return null;

			var file = listOfFiles.Where(f => f.Id == fileId.ToString())
				.FirstOrDefault();

			if (file == null)
				return null;

			return file.Key;
		}

		private async Task AddToLocalStorage(ConfigFileEntry fileEntry)
		{
			var listOfFiles = await localStorage.GetItemAsync<List<ConfigFileEntry>>("files");
			listOfFiles.Add(fileEntry);
			await localStorage.SetItemAsync("files", listOfFiles);
		}

		public async Task<byte[]> EncryptConfigFile(Stream fs, string fileName, string password)
		{
			byte[] fileContent = null;
			using (var memoryStream = new MemoryStream())
			{
				await fs.CopyToAsync(memoryStream);
				fileContent = memoryStream.ToArray();
			}

			string plaintext = Encoding.ASCII.GetString(fileContent);
			byte[] encrypted;
			using (var AES = new AesCryptoServiceProvider())
			{
				AES.IV = FileUtil.GetIV(fileName);
				AES.Key = FileUtil.GetKey(password, 32);

				ICryptoTransform encryptor = AES.CreateEncryptor(AES.Key, AES.IV);
				
				using (MemoryStream msEncrypt = new MemoryStream())
				{
					using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
					{
						using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
						{
							//Write all data to the stream.
							swEncrypt.Write(plaintext);
						}
						encrypted = msEncrypt.ToArray();
					}
				}
			}

			return encrypted;
		}

		public async Task<byte[]> DecryptConfigFile(Stream fs, string fileName, string password)
		{
			string plaintext = null;
			byte[] fileContent = null;
			using (var memoryStream = new MemoryStream())
			{
				//fs.CopyTo(memoryStream);
				await fs.CopyToAsync(memoryStream);
				fileContent = memoryStream.ToArray();
			}

			using (var AES = new AesCryptoServiceProvider())
			{
				AES.IV = FileUtil.GetIV(fileName);
				AES.Key = FileUtil.GetKey(password, 32);

				ICryptoTransform decryptor = AES.CreateDecryptor(AES.Key, AES.IV);

				// Create the streams used for decryption.
				using (MemoryStream msDecrypt = new MemoryStream(fileContent))
				{
					using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
					{
						using (StreamReader srDecrypt = new StreamReader(csDecrypt))
						{

							// Read the decrypted bytes from the decrypting stream
							// and place them in a string.
							plaintext = srDecrypt.ReadToEnd();
						}
					}
				}
			}

			return Encoding.ASCII.GetBytes(plaintext);
		}
	}

	public static class FileUtil
	{
		public async static Task SaveAs(IJSRuntime js, string filename, byte[] data)
		{
			await js.InvokeAsync<object>(
				"saveAsFile",
				filename,
				Convert.ToBase64String(data));
		}

		public static byte[] GetKey(string password, int length)
		{
			int keySize = length;
			if (password.Length == keySize)
			{
				Console.WriteLine("Password: " + password);
				return Encoding.ASCII.GetBytes(password);
			}

			if (password.Length > keySize)
			{
				password = password.Substring(0, keySize);
				Console.WriteLine("Password: " + password);
				return Encoding.ASCII.GetBytes(password);
			}

			// fileName < keySize
			string originalFileName = password;
			int i = 0;
			while (password.Length != keySize)
			{
				password = password + password[i];
				i++;
			}

			Console.WriteLine("Password: " + password);
			return Encoding.ASCII.GetBytes(password);
		}

		public static byte[] GetIV(string fileName)
		{
			if (fileName.Length == 16)
			{
				Console.WriteLine("IV: " + fileName);
				return Encoding.ASCII.GetBytes(fileName);
			}

			if (fileName.Length > 16)
			{
				fileName = fileName.Substring(0, 16);
				Console.WriteLine("IV: " + fileName);
				return Encoding.ASCII.GetBytes(fileName);
			}

			// fileName < 16
			string originalFileName = fileName;
			int i = 0;
			while (fileName.Length != 16)
			{
				fileName = fileName + fileName[i];
				i++;
			}

			Console.WriteLine("IV: " + fileName);
			return Encoding.ASCII.GetBytes(fileName);
		}
	}

	public class ConfigFileEntry
	{
		public string Id { get; set; }
		public string FileName { get; set; }
		public string Algorithm { get; set; }
		public string Key { get; set; }
	}
}
