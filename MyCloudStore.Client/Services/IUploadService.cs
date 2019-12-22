using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace MyCloudStore.Client.Services
{
	public interface IUploadService
	{
		Task UploadFileAsync(Stream fs, string fileName);
	}
}
