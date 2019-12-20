using MyCloudStore.Shared.Requests;
using MyCloudStore.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCloudStore.Client.Services
{
	public interface IAuthService
	{
		Task<LogInResult> LogIn(LogInModel loginModel);
		Task LogOut();
		Task<RegisterResult> Register(RegisterModel registerModel);
	}
}
