using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyCloudStore.Shared.Requests
{
	public class FileModel
	{
		public string FileName { get; set; }
		public IFormFile Content { get; set; }
		public string HashValue { get; set; }
	}
}
