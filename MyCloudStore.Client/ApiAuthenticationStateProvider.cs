using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyCloudStore.Client
{
	public class ApiAuthenticationStateProvider : AuthenticationStateProvider
	{
		private readonly HttpClient httpClient;
		private readonly ILocalStorageService localStorage;

		public ApiAuthenticationStateProvider(HttpClient httpClient, ILocalStorageService localStorage)
		{
			this.httpClient = httpClient;
			this.localStorage = localStorage;
		}

        /// <summary>
        /// This method is called by the CascadingAuthenticationState component
        /// to determine if the current user is authenticated or not.
        /// We check to see if there is an auth token in local storage. 
        /// If there is no token in local storage, then we return a new AuthenticationState with a blank
        /// claims principal, which means user is not authenticated.
        /// 
        /// If there is a token, we retrieve it and set the default auth. header for the HttpClient.
        /// We then return a new AuthenticationState with a new claims principal with claims from the token.
        /// Claims are extracted from the token by ParseClaimsFromJwt method.
        /// </summary>
        /// <returns></returns>
		public override async Task<AuthenticationState> GetAuthenticationStateAsync()
		{
			var savedToken = await localStorage.GetItemAsync<string>("authToken");

			if (string.IsNullOrEmpty(savedToken))
			{
				return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
			}

			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", savedToken);
			return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(savedToken))));
		}

		public void MarkUserAsAuthenticated(string email)
		{
			var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, email) }, "apiauth"));
			var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
			NotifyAuthenticationStateChanged(authState);
		}

		public void MarkUserAsLoggedOut()
		{
			var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
			var authState = Task.FromResult(new AuthenticationState(anonymousUser));
			NotifyAuthenticationStateChanged(authState);
		}

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            keyValuePairs.TryGetValue(ClaimTypes.Role, out object roles);

            if (roles != null)
            {
                if (roles.ToString().Trim().StartsWith("["))
                {
                    var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString());

                    foreach (var parsedRole in parsedRoles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, parsedRole));
                    }
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, roles.ToString()));
                }

                keyValuePairs.Remove(ClaimTypes.Role);
            }

            claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString())));

            return claims;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
