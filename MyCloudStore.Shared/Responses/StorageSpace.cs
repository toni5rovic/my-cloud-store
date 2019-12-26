using System;
using System.Collections.Generic;
using System.Text;

namespace MyCloudStore.Shared.Responses
{
	public class StorageSpace
	{
		public long MaxKBs { get; set; }
		public double CurrentKBs { get; set; }
	}
}
