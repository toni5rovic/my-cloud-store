﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using MyCloudStore.Shared.Responses;

namespace MyCloudStore.Client.Services
{
	public interface IFileService
	{
		Task UploadFileAsync(Stream fs, string fileName, string algorithm, string key);
		Task UploadConfigFileAsync(Stream fs, string fileName);
		Task<string> DownloadConfigFileAsync();
		Task<List<FileResult>> GetFilesAsync();
		Task<FileResult> GetFileAsync(Guid fileId, string algorithm, string key);
		Task<StorageSpace> GetStorageSpace();

		Task<string> GetAlgorithm(Guid fileId);
		Task<string> GetKey(Guid fileId);
	}
}
