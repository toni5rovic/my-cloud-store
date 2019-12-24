using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace MyCloudStore.Crypto.Hash
{
	public class TigerHash
	{
		private ulong A;
		private ulong B;
		private ulong C;

		private ulong AA;
		private ulong BB;
		private ulong CC;

		private const int blockSize = 64;
		private const int hashSize = 192;
		private const int messageBlockSizeBits = 512;
		private const int messageBlockSizeBytes = 64;
		private ulong dataToEncryptSize;

		private ulong[] X;

		private int blocksHashed;

		public int BlocksHashed { get { return blocksHashed; } }

		public TigerHash()
		{
			Initialize();
		}

		private void Initialize()
		{
			A = 0x0123456789ABCDEFUL;
			B = 0xFEDCBA9876543210UL;
			C = 0xF096A5B4C3B2E187UL;

			AA = BB = CC = 0;
			X = new ulong[8];

			blocksHashed= 0;
		}

		private void InitializeXs(byte[] messageBlock)
		{
			for (int i = 0; i < 8; i++)
				X[i] = messageBlock[i];
		}

		public byte[] Hash(byte[] array, int numOfBytes)
		{
			Initialize();

			int numOfMessageBlocks = numOfBytes / messageBlockSizeBytes;

			for (int i = 0; i < numOfMessageBlocks; i++)
			{
				byte[] messageBlock = new byte[messageBlockSizeBytes];
				Array.Copy(array, i * messageBlockSizeBytes, messageBlock, 0, messageBlockSizeBytes);
				HashMessageBlock(messageBlock);
			}

			int numOfFinalBytes = numOfBytes % 64; // 64B = 512b 

			if (numOfFinalBytes > 0)
			{
				byte[] finalBytes = new byte[numOfFinalBytes];
				Array.Copy(array, numOfMessageBlocks * messageBlockSizeBytes, finalBytes, 0, numOfFinalBytes);
				HashFinalMessageBlock(finalBytes, numOfFinalBytes, numOfBytes);
			}

			// TODO:
			// return A concat B  concat C;

			byte[] registerA = Helper.UlongToBytes(this.A);
			byte[] registerB = Helper.UlongToBytes(this.B);
			byte[] registerC = Helper.UlongToBytes(this.C);

			byte[] result = registerA;
			Helper.AppendBytes(ref result, registerB);
			Helper.AppendBytes(ref result, registerC);

			if (result.Length != 24)
				throw new Exception("Result of Tiger Hash must be 24 bytes long.");

			return result;
		}

		private void HashFinalMessageBlock(byte[] finalBytes, int numOfFinalBytes, int messageSizeWithoutPadding)
		{
			if (numOfFinalBytes < 0 || numOfFinalBytes > 64)
				throw new ArgumentException("numofFinalBytes must be in range [0, 64]");

			if (numOfFinalBytes < 56)
			{
				// Add padding, message size and then hash this block
				// Padding: 0x8000...000
				int paddingSize = 56 - numOfFinalBytes;
				byte[] padding = new byte[paddingSize];
				Array.Clear(padding, 0, paddingSize);
				padding[0] = 0x80;

				Helper.AppendBytes(ref finalBytes, padding);

				ulong msgSize = (ulong)messageSizeWithoutPadding;
				byte[] messageSizeBytes = Helper.UlongToBytes(msgSize);
				Helper.AppendBytes(ref finalBytes, messageSizeBytes);

				HashMessageBlock(finalBytes);
			}
			else
			{
				// 56 <= numOfFinalBytes < 64
				// 1. messageBlock = add padding to finalBytes, hash

				int paddingSize = 64 - numOfFinalBytes;
				byte[] paddingMsg1 = new byte[paddingSize];
				Array.Clear(paddingMsg1, 0, paddingSize);
				paddingMsg1[0] = 0x80;

				Helper.AppendBytes(ref finalBytes, paddingMsg1);

				HashMessageBlock(finalBytes);

				// 2. messageBlock = add 56B padding, then add message size, then hash

				paddingSize = 56;
				byte[] paddingMsg2 = new byte[paddingSize];
				Array.Clear(paddingMsg2, 0, paddingSize);

				ulong msgSize = (ulong)messageSizeWithoutPadding;
				byte[] messageSizeBytes = Helper.UlongToBytes(msgSize);
				Helper.AppendBytes(ref paddingMsg2, messageSizeBytes);

				HashMessageBlock(paddingMsg2);
			}
		}

		public void HashMessageBlock(byte[] messageBlock)
		{
			blocksHashed++;

			InitializeXs(messageBlock);

			AA = A;
			BB = B;
			CC = C;

			Pass(ref this.A, ref this.B, ref this.C, 5);
			KeySchedule();
			Pass(ref this.C, ref this.A, ref this.B, 7);
			KeySchedule();
			Pass(ref this.B, ref this.C, ref this.A, 9);

			A ^= AA;
			B -= BB;
			C += CC;
		}

		private void Pass(ref ulong a, ref ulong b, ref ulong c, int mul)
		{
			Round(ref a, ref b, ref c, 0, mul);
			Round(ref b, ref c, ref a, 1, mul);
			Round(ref c, ref a, ref b, 2, mul);
			Round(ref a, ref b, ref c, 3, mul);
			Round(ref b, ref c, ref a, 4, mul);
			Round(ref c, ref a, ref b, 5, mul);
			Round(ref a, ref b, ref c, 6, mul);
			Round(ref b, ref c, ref a, 7, mul);
		}

		private void Round(ref ulong a, ref ulong b, ref ulong c, int X_index, int mul)
		{
			c = c ^ X[X_index];
			byte[] CBytes = Helper.UlongToBytes(c);
			a -= (SBox.T1[CBytes[0]] ^ SBox.T2[CBytes[2]] ^ SBox.T3[CBytes[4]] ^ SBox.T4[CBytes[6]]);
			b += (SBox.T4[CBytes[1]] ^ SBox.T3[CBytes[3]] ^ SBox.T2[CBytes[5]] ^ SBox.T1[CBytes[7]]);
			b *= (ulong)mul;
		}

		private void KeySchedule()
		{
			X[0] -= X[7] ^ 0xA5A5A5A5A5A5A5A5UL;
			X[1] ^= X[0];
			X[2] += X[1];
			X[3] -= X[2] ^ ((~X[1]) << 19);
			X[4] ^= X[3];
			X[5] += X[4];
			X[6] -= X[5] ^ ((~X[4]) >> 23);
			X[7] ^= X[6];

			X[0] += X[7];
			X[1] -= X[0] ^ ((~X[7] << 19));
			X[2] ^= X[1];
			X[3] += X[2];
			X[4] -= X[3] ^ ((~X[2]) >> 23);
			X[5] ^= X[4];
			X[6] += X[5];
			X[7] -= X[6] ^ 0x0123456789ABCDEFUL;
		}
	}
}
