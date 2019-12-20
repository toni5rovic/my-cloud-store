using System;
using System.Collections.Generic;
using System.Text;

namespace MyCloudStore.Shared.Responses
{
	public class LogInResult
	{
		public bool Successful { get; set; }
		public string Error { get; set; }
		public string Token { get; set; }
	}
}
