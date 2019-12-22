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

namespace MyCloudStore.Client.Services
{
	public class UploadService : IUploadService
	{
		private readonly HttpClient httpClient;
		private readonly ILocalStorageService localStorage;

		public UploadService(HttpClient httpClient,
			ILocalStorageService localStorage)
		{
			this.localStorage = localStorage;
			this.httpClient = httpClient;
		}

		public async Task UploadFileAsync(Stream fs, string fileName)
		{
			//var bearerToken = await this.localStorage.GetItemAsync<string>("bearer");
			//this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", bearerToken);

			using (var form = new MultipartFormDataContent())
			using (var streamContent = new StreamContent(fs))
			{
				form.Add(new StringContent(fileName, Encoding.UTF8, "text/plain"), "\"FileName\"");
				
				var byteArray = await streamContent.ReadAsByteArrayAsync();
				byteArray = byteArray.Take(100).ToArray();
				string base64ByteArray = Convert.ToBase64String(byteArray);
				form.Add(new StringContent(base64ByteArray, Encoding.UTF8, "text/plain"), "\"HashValue\"");


				//form.Add(new ByteArrayContent(byteArray), "\"HashValue\"");
				form.Add(streamContent, "\"Content\"", fileName);
				//form.Add(new StringContent("{ \"fileName\" : \"opsa.txt\" }"), "Info");
				//form.Add(new StringContent("{ \"hashValue\" : \"\" }"), "Info");
				var response = await this.httpClient.PostAsync("api/Files/", form);
			}

			//using (var form = new MultipartFormDataContent())
			//using (var streamContent = new StreamContent(fs))
			//{
			//	FileModel fileModel = new FileModel
			//	{
			//		Content = await streamContent.ReadAsByteArrayAsync(),
			//		FileName = fileName,
			//		HashValue = 
			//	};
				


			//	form.Add(new StringContent(fileName, Encoding.UTF8, "text/plain"), "\"FileName\"");
			//	var byteArray = await streamContent.ReadAsByteArrayAsync();
			//	byteArray = byteArray.Take(100).ToArray();
			//	form.Add(new ByteArrayContent(byteArray), "\"HashValue\"");
			//	//form.Add(new StringContent("12345", Encoding.UTF8, "text/plain"), "\"HashValue\"");
			//	form.Add(streamContent, "\"Content\"", fileName);
			//	//form.Add(new StringContent("{ \"fileName\" : \"opsa.txt\" }"), "Info");
			//	//form.Add(new StringContent("{ \"hashValue\" : \"\" }"), "Info");
			//	var response = await this.httpClient.PostAsync("api/Files/", form);
			//}
		}
	}
}
