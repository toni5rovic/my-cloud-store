using Blazor.FileReader;
using Blazored.LocalStorage;
using Blazored.Modal;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using MyCloudStore.Client.Services;
using System.Net.Http;

namespace MyCloudStore.Client
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBlazoredLocalStorage();
            services.AddBlazoredModal();
            services.AddAuthorizationCore();
            var client = new HttpClient();
            client.BaseAddress = new System.Uri("https://localhost:44387/");
            services.AddSingleton<HttpClient>(client);

            services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IFileService, FileService>();
            services.AddFileReaderService(options =>
            {
                options.UseWasmSharedBuffer = true;
                options.InitializeOnFirstCall = true;
            });
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
