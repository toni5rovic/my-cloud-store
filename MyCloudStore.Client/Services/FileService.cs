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

		public async Task UploadFileAsync(Stream fs, string fileName)
		{
			using (var form = new MultipartFormDataContent())
			using (var streamContent = new StreamContent(fs))
			{
				form.Add(new StringContent(fileName, Encoding.UTF8, "text/plain"), "\"FileName\"");
				
				var byteArray = await streamContent.ReadAsByteArrayAsync();
				TigerHash tigerHasher = new TigerHash();
				byte[] hashed = tigerHasher.Hash(byteArray, byteArray.Length);
				string hashedBase64String = Convert.ToBase64String(hashed);
				form.Add(new StringContent(hashedBase64String, Encoding.UTF8, "text/plain"), "\"HashValue\"");
				form.Add(streamContent, "\"Content\"", fileName);
				
				var response = await this.httpClient.PostAsync("api/Files/", form);
			}
		}

		public async Task<List<FileResult>> GetFilesAsync()
		{
			string URL = "api/Files/";
			return await this.httpClient.GetJsonAsync<List<FileResult>>(URL);
		}

		public async Task<FileResult> GetFileAsync(Guid fileId)
		{
			string URL = "api/Files/" + fileId.ToString();
			return await this.httpClient.GetJsonAsync<FileResult>(URL);
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
	}
}
