using System;
using System.Collections.Generic;
using System.Text;

namespace MyCloudStore.CryptoLibrary.Algorithms
{
	public class RC4 : ICryptoAlgorithm
	{
		private int[] S_vector;
		private byte[] key;
		private byte[] plaintext;

		private int keyLength = 128;
		private int counterLength = 256;
		private int outputLength = 256;

		public int KeyLength { get { return keyLength; } }
		public int CounterLength { get { return counterLength; } }
		public int OutputLength { get { return outputLength; } }

		public RC4()
		{
			S_vector = new int[256];
		}

		private void KeySchedulingAlgorithm()
		{
			for (int i = 0; i < 256; i++)
				S_vector[i] = i;

			int j = 0;
			for (int i = 0; i < 256; i++)
			{
				j = (j + S_vector[i] + key[i % key.Length]) % 256;

				int temp = S_vector[i];
				S_vector[i] = S_vector[j];
				S_vector[j] = temp;
			}
		}

		private byte[] PseudoRandomGenerationAlgorithm()
		{
			int i = 0;
			int j = 0;
			List<byte> output = new List<byte>();
			for (int k = 0; k < plaintext.Length; k++)
			{
				i = (i + 1) % 256;
				j = (j + S_vector[i]) % 256;

				int temp = S_vector[i];
				S_vector[i] = S_vector[j];
				S_vector[j] = temp;

				int K = S_vector[(S_vector[i] + S_vector[j]) % 256];
				byte[] K_bytes = K_bytes = BitConverter.GetBytes(K);
				if (BitConverter.IsLittleEndian)
					Array.Reverse(K_bytes);

				output.AddRange(K_bytes);
			}

			return output.ToArray();
		}

		public byte[] Decrypt(byte[] key, byte[] plaintext)
		{
			return Encrypt(key, plaintext);
		}

		public byte[] Encrypt(byte[] key, byte[] plaintext)
		{
			this.key = key;
			this.plaintext = plaintext;

			KeySchedulingAlgorithm();
			byte[] output = PseudoRandomGenerationAlgorithm();

			byte[] result = new byte[plaintext.Length];
			for (int i = 0; i < plaintext.Length; i++)
			{
				result[i] = (byte)(plaintext[i] ^ output[i]);
			}

			return result;
		}
	}
}
