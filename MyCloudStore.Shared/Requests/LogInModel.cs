using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MyCloudStore.Shared.Requests
{
	public class LogInModel
	{
		[Required]
		public string Email { get; set; }
		[Required]
		public string Password { get; set; }
		public bool RememberMe { get; set; }
	}
}
