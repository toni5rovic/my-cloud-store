using MyCloudStore.CryptoLibrary.Algorithms;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyCloudStore.CryptoLibrary
{
	public class CryptoCTRMode
	{
		private ICryptoAlgorithm cryptoAlgorithm = null;
		private int numOfFileChunks;
		private int numOfNonceAndCounterBytes;
		private int numOfNonceBytes;

		private byte[] nonceAndCounter;
		private byte[] nonce;

		private ushort counter;

		public CryptoCTRMode()
		{
		}

		public void SetCryptoAlgorithm(ICryptoAlgorithm algorithm)
		{
			cryptoAlgorithm = algorithm;
		}

		private void Initialize(int numOfFileBytes, byte[] key)
		{
			counter = 0;

			numOfFileChunks = numOfFileBytes / (cryptoAlgorithm.OutputLength / 8);

			numOfNonceAndCounterBytes = cryptoAlgorithm.CounterLength / 8;
			nonceAndCounter = new byte[numOfNonceAndCounterBytes];

			if (numOfNonceAndCounterBytes > 2)
			{
				numOfNonceBytes = numOfNonceAndCounterBytes - 2;
				nonce = new byte[numOfNonceBytes];

				int diff = numOfNonceBytes - key.Length;
				if (diff < 0)
					diff = 0;
				for (int i=0;i<diff;i++)
				{
					nonce[i] = 0;
				}

				for (int i = 0; i < key.Length; i++)
				{
					nonce[diff+i] = key[i];
				}
			}

		}

		private void MakeNonceAndCounter()
		{
			if (numOfNonceAndCounterBytes == 2)
			{
				nonceAndCounter[0] = (byte)((counter >> 8) & 0x00FF);
				nonceAndCounter[1] = (byte)(counter & 0x00FF);

				return;
			}

			for (int i = 0; i < numOfNonceBytes; i++)
			{
				nonceAndCounter[i] = nonce[i];
			}

			nonceAndCounter[numOfNonceAndCounterBytes - 2] = (byte)((counter >> 8) & 0x00FF);
			nonceAndCounter[numOfNonceAndCounterBytes - 1] = (byte)(counter & 0x00FF);
		}

		public byte[] EncryptFile(byte[] fileBytes, byte[] key)
		{
			Initialize(fileBytes.Length, key);

			byte[] cipherText = new byte[fileBytes.Length];

			for (int i = 0; i < numOfFileChunks; i++)
			{
				MakeNonceAndCounter();
				byte[] output = cryptoAlgorithm.Encrypt(key, nonceAndCounter);

				for (int j = 0; j < output.Length; j++)
				{
					cipherText[(i * (cryptoAlgorithm.OutputLength/8)) + j] = (byte)(fileBytes[(i * (cryptoAlgorithm.OutputLength/8)) + j] ^ output[j]);
				}

				counter++;
			}

			// treba i ostatak kriptovati
			int restOfBytes = fileBytes.Length % (cryptoAlgorithm.OutputLength / 8);
			MakeNonceAndCounter();
			byte[] lastOutput = cryptoAlgorithm.Encrypt(key, nonceAndCounter);
			for (int i = 0; i < restOfBytes; i++)
			{
				//int startOfLastChunk = (numOfFileChunks - 1) * (cryptoAlgorithm.OutputLength/8);
				int startOfLastChunk = numOfFileChunks * (cryptoAlgorithm.OutputLength / 8);
				cipherText[startOfLastChunk + i] = (byte)(fileBytes[startOfLastChunk + i] ^ lastOutput[i]);
			}

			return cipherText;
		}

		public byte[] DecryptFile(byte[] cipherBytes, byte[] key)
		{
			return EncryptFile(cipherBytes, key);
		}
	}
}
