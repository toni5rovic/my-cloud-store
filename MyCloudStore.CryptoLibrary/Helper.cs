using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MyCloudStore.CryptoLibrary
{
	public static class Helper
	{
		public static byte GetByteFromULONG(ulong number, int byteIndex)
		{
			int shift = byteIndex * 8;
			ulong result = number >> shift;
			result = result & 0x00000000000000FF;
			return (byte)result;
		}

		public static byte[] UlongToBytes(ulong number)
		{
			byte[] result = new byte[8];
			for (int i = 0; i < 8; i++)
			{
				result[i] = (byte)(number & 0x00000000000000FF);
				number = number >> 8;
			}

			return result;
		}

		public static void AppendBytes(ref byte[] dst, byte[] src)
		{
			int oldLength = dst.Length;
			Array.Resize(ref dst, dst.Length + src.Length);
			for (int j = 0; j < src.Length; j++)
			{
				dst[oldLength + j] = src[j];
			}
		}

		private static readonly uint[] _lookup32 = CreateLookup32();

		private static uint[] CreateLookup32()
		{
			var result = new uint[256];
			for (int i = 0; i < 256; i++)
			{
				string s = i.ToString("X2");
				result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
			}
			return result;
		}

		public static string ByteArrayToHexViaLookup32(byte[] bytes)
		{
			var lookup32 = _lookup32;
			var result = new char[bytes.Length * 2];
			for (int i = 0; i < bytes.Length; i++)
			{
				var val = lookup32[bytes[i]];
				result[2 * i] = (char)val;
				result[2 * i + 1] = (char)(val >> 16);
			}
			return new string(result);
		}

		public static string BytesToHexCode(byte[] array)
		{
			StringBuilder hex = new StringBuilder(array.Length * 2);
			foreach (byte b in array)
				hex.AppendFormat("{0:x2}", b);
			return hex.ToString();
		}

		public static BitArray LeftShift(BitArray bitArray)
		{
			//bool[] new_arr = new bool[(bitArray.Count)];
			//for (int i = 0; i < bitArray.Count - 1; i++)
			//	new_arr[i] = bitArray[i + 1];

			//return new BitArray(new_arr);

			for (int i=bitArray.Length-1;i>0;i--)
			{
				bitArray[i] = bitArray[i - 1];
			}

			bitArray[0] = false;
			return bitArray;
		}
	}
}
