using System;
using System.Collections.Generic;
using System.Text;

namespace MyCloudStore.Shared.Responses
{
	public class FileResult
	{
		public Guid Id { get; set; }
		public string FileName { get; set; }
		public byte[] Content { get; set; }
		public string HashValue { get; set; }
		public DateTime Created { get; set; }
	}
}
