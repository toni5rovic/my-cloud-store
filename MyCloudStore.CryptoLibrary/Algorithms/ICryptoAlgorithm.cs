using System;
using System.Collections.Generic;
using System.Text;

namespace MyCloudStore.CryptoLibrary.Algorithms
{
	public interface ICryptoAlgorithm
	{
		byte[] Encrypt(byte[] key, byte[] counter);
		byte[] Decrypt(byte[] key, byte[] counter);
	}
}
