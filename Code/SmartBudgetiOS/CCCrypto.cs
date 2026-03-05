using System;
using System.Runtime.InteropServices;
//using MonoTouch;

namespace SmartBudgetiOS
{
	public class CCCrypto
	{
/*		public static byte[] aes_128_encrypt(byte[] data, byte[] key, byte[] IV)
		{
			IntPtr IV_in = Marshal.AllocCoTaskMem (IV.Length);
			Marshal.Copy (IV, 0, IV_in, IV.Length);
			IntPtr data_in = Marshal.AllocCoTaskMem (data.Length);
			Marshal.Copy (data, 0, data_in, data.Length);
			IntPtr key_in = Marshal.AllocCoTaskMem (key.Length);
			Marshal.Copy (key, 0, key_in, key.Length);

			IntPtr data_out = Marshal.AllocCoTaskMem (data.Length + 64);

			Int32 data_moved = 0;
			int res = CCCrypt (0, 0, 1, key_in, key.Length, IV_in, data_in, data.Length, data_out, data.Length + 64, ref data_moved);
			byte[] result = new byte[data_moved + IV.Length];
			Array.Copy (IV, 0, result, 0, IV.Length);
			Marshal.Copy (data_out, result, IV.Length, data_moved);

			Marshal.FreeCoTaskMem (data_out);
			Marshal.FreeCoTaskMem (key_in);
			Marshal.FreeCoTaskMem (data_in);
			Marshal.FreeCoTaskMem (IV_in);
			if (res != 0)
				throw new Exception ("CCCrypt failed to encrypt");
			return result;
		}
		public static byte[] aes_128_decrypt(byte[] data, byte[] key)
		{
			const int IV_len = 16;
			if(data.Length < IV_len )
				throw new Exception ("CCCrypt data length too small");
			IntPtr IV_in = Marshal.AllocCoTaskMem (IV_len);
			Marshal.Copy (data, 0, IV_in, IV_len);
			IntPtr data_in = Marshal.AllocCoTaskMem (data.Length - IV_len);
			Marshal.Copy (data, IV_len, data_in, data.Length - IV_len);
			IntPtr key_in = Marshal.AllocCoTaskMem (key.Length);
			Marshal.Copy (key, 0, key_in, key.Length);

			IntPtr data_out = Marshal.AllocCoTaskMem (data.Length + 64);

			Int32 data_moved = 0;
			int res = CCCrypt (1, 0, 1, key_in, key.Length, IV_in, data_in, data.Length - IV_len, data_out, data.Length + 64, ref data_moved);
			byte[] result = new byte[data_moved];
			Marshal.Copy (data_out, result, 0, data_moved);

			Marshal.FreeCoTaskMem (data_out);
			Marshal.FreeCoTaskMem (key_in);
			Marshal.FreeCoTaskMem (data_in);
			Marshal.FreeCoTaskMem (IV_in);
			if (res != 0)
				throw new Exception ("CCCrypt failed to decrypt");
			return result;
		}
		[DllImport (Constants.SecurityLibrary, EntryPoint="CCCrypt")]
		public extern static Int32 CCCrypt (
			Int32 op,
			Int32 alg,
			Int32 options,
			IntPtr key,
			Int32 keyLength,
			IntPtr iv, 
			IntPtr dataIn,
			Int32 dataInLength,
			IntPtr dataOut,
			Int32 dataOutAvailable,
			ref Int32 dataOutMoved);
			*/
	}
}

