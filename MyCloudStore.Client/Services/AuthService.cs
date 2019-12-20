using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MyCloudStore.Shared.Requests;
using MyCloudStore.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyCloudStore.Client.Services
{
	public class AuthService : IAuthService
	{
		private readonly HttpClient httpClient;
		private readonly AuthenticationStateProvider authenticationStateProvider;
		private readonly ILocalStorageService localStorage;

		public AuthService(HttpClient httpClient,
							AuthenticationStateProvider authenticationStateProvider,
							ILocalStorageService localStorage)
		{
			this.httpClient = httpClient;
			this.httpClient.BaseAddress = new Uri("https://localhost:44387/");
			this.authenticationStateProvider = authenticationStateProvider;
			this.localStorage = localStorage;
		}

		public async Task<LogInResult> LogIn(LogInModel loginModel)
		{
			var loginAsJson = JsonSerializer.Serialize(loginModel);
			var response = await this.httpClient.PostAsync("api/users/login", new StringContent(loginAsJson, Encoding.UTF8, "application/json"));
			var loginResult = JsonSerializer.Deserialize<LogInResult>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

			if (!response.IsSuccessStatusCode)
			{
				return loginResult;
			}

			await this.localStorage.SetItemAsync("authToken", loginResult.Token);
			((ApiAuthenticationStateProvider)authenticationStateProvider).MarkUserAsAuthenticated(loginModel.Email);
			this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", loginResult.Token);

			return loginResult;
		}

		public async Task LogOut()
		{
			await this.localStorage.RemoveItemAsync("authToken");
			((ApiAuthenticationStateProvider)authenticationStateProvider).MarkUserAsLoggedOut();
			this.httpClient.DefaultRequestHeaders.Authorization = null;
		}

		public async Task<RegisterResult> Register(RegisterModel registerModel)
		{
			var result = await this.httpClient.PostJsonAsync<RegisterResult>("api/users/register", registerModel);

			return result;
		}
	}
}
