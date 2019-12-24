using System;
using System.Collections;
using System.Text;

namespace MyCloudStore.CryptoLibrary.Algorithms
{
	public enum Register
	{
		R1,
		R2,
		R3,
		R4
	}

	public class A52 : ICryptoAlgorithm
	{
		private BitArray R1;
		private BitArray R2;
		private BitArray R3;
		private BitArray R4;

		private BitArray Kc;
		private BitArray IV;

		private int[] MaskR1 = { 13, 16, 17, 18 };
		private int[] MaskR2 = { 20, 21 };
		private int[] MaskR3 = { 7, 20, 21, 22 };
		private int[] MaskR4 = { 11, 16 };

		public A52()
		{
			R1 = new BitArray(19, false);
			R2 = new BitArray(22, false);
			R3 = new BitArray(23, false);
			R4 = new BitArray(17, false);
		}

		private void KeySetup()
		{
			R1.SetAll(false);
			R2.SetAll(false);
			R3.SetAll(false);
			R4.SetAll(false);

			for (int i = 0; i < 64; i++)
			{
				ClockAll();
				R1[0] = R1[0] ^ Kc[i];
				R2[0] = R2[0] ^ Kc[i];
				R3[0] = R3[0] ^ Kc[i];
				R4[0] = R4[0] ^ Kc[i];
			}

			for (int i = 0; i < 22; i++)
			{
				ClockAll();
				R1[0] = R1[0] ^ IV[i];
				R2[0] = R2[0] ^ IV[i];
				R3[0] = R3[0] ^ IV[i];
				R4[0] = R4[0] ^ IV[i];
			}

			R1[15] = true;
			R2[16] = true;
			R3[18] = true;
			R4[10] = true;
		}

		private void ClockAll()
		{
			ClockOne(Register.R1);
			ClockOne(Register.R2);
			ClockOne(Register.R3);
			ClockOne(Register.R4);
		}

		private bool MajorityVote(bool bit1, bool bit2, bool bit3)
		{
			return (bit1 & bit2) ^ (bit1 & bit3) ^ (bit2 & bit3);
		}

		private bool Feedback(Register registerToCalculateFeedbackFor)
		{
			bool result = false;
			switch (registerToCalculateFeedbackFor)
			{
				case Register.R1:
					result = R1[13] ^ R1[16] ^ R1[17] ^ R1[18];
					break;
				case Register.R2:
					result = R2[20] ^ R2[21];
					break;
				case Register.R3:
					result = R3[7] ^ R3[20] ^ R3[21] ^ R3[22];
					break;
				case Register.R4:
					result = R4[11] ^ R4[16];
					break;
			}

			return result;
		}

		private void ClockOne(Register registerToClock)
		{
			bool feedback = Feedback(registerToClock);
			switch (registerToClock)
			{
				case Register.R1:
					R1 = Helper.LeftShift(R1);
					R1.Set(0, feedback);
					break;
				case Register.R2:
					R2 = Helper.LeftShift(R2);
					R2.Set(0, feedback);
					break;
				case Register.R3:
					R3 = Helper.LeftShift(R3);
					R3.Set(0, feedback);
					break;
				case Register.R4:
					R4 = Helper.LeftShift(R4);
					R4.Set(0, feedback);
					break;
			}
		}

		private bool GetOutputBit()
		{
			bool majorityVoteBit = MajorityVote(R4[3], R4[7], R4[10]);

			if (R4[10] == majorityVoteBit)
				ClockOne(Register.R1);
			if (R4[3] == majorityVoteBit)
				ClockOne(Register.R2);
			if (R4[7] == majorityVoteBit)
				ClockOne(Register.R3);

			ClockOne(Register.R4);
			bool majorityBit_R1 = MajorityVote(R1[15], R1[14] ^ true, R1[12]);
			bool majorityBit_R2 = MajorityVote(R2[16] ^ true, R2[13], R2[9]);
			bool majorityBit_R3 = MajorityVote(R3[18], R3[16], R3[13] ^ true);
			bool MSB_R1 = R1[18];
			bool MSB_R2 = R2[21];
			bool MSB_R3 = R3[22];

			return majorityBit_R1 ^ majorityBit_R2 ^ majorityBit_R3 ^ MSB_R1 ^ MSB_R2 ^ MSB_R3;
		}

		public byte[] Decrypt(byte[] key, byte[] counter)
		{
			return Encrypt(key, counter);
		}

		public byte[] Encrypt(byte[] key, byte[] counter)
		{
			this.Kc = new BitArray(key);
			this.IV = new BitArray(counter);
			bool[] nonce = new bool[] { true, true, false, true, false, true };
			this.IV = Prepend(this.IV, new BitArray(nonce));

			KeySetup();

			// 99 cycles are not counting
			for (int i = 0; i < 100; i++)
				GetOutputBit();

			BitArray output = new BitArray(1);
			for (int i = 0; i < 128; i++)
			{
				bool outputBit = GetOutputBit();
				BitArray ba = new BitArray(1, outputBit);

				if (i == 0)
					output[0] = outputBit;
				else
					output = Append(output, ba);
			}

			byte[] toReturn = new byte[(output.Length - 1) / 8 + 1];
			output.CopyTo(toReturn, 0);
			return toReturn;
		}

		private static BitArray Prepend(BitArray current, BitArray before)
		{
			var bools = new bool[current.Count + before.Count];
			before.CopyTo(bools, 0);
			current.CopyTo(bools, before.Count);
			return new BitArray(bools);
		}

		public static BitArray Append(BitArray current, BitArray after)
		{
			var bools = new bool[current.Count + after.Count];
			current.CopyTo(bools, 0);
			after.CopyTo(bools, current.Count);
			return new BitArray(bools);
		}
	}
}
