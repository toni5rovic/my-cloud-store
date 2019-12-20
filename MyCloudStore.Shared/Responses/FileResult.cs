using System;
using System.Collections.Generic;
using System.Text;

namespace MyCloudStore.Shared.Responses
{
	public class FileResult
	{
		public string FileName { get; set; }
		public byte[] Content { get; set; }
		public byte[] HashValue { get; set; }
	}
}
