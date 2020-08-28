using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HousingPos
{
    public static class Memory
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesWritten);
		public static byte[] Read(IntPtr address, int numBytesToRead)
		{
			bool flag = numBytesToRead <= 0;
			if (flag)
			{
				throw new ArgumentOutOfRangeException();
			}
			byte[] buffer = new byte[numBytesToRead];
			IntPtr intPtr;
			bool flag2 = !Memory.ReadProcessMemory(Process.GetCurrentProcess().Handle, address, buffer, numBytesToRead, out intPtr);
			if (flag2)
			{
				throw new Exception("ReadProcessMemory failed with win32 error " + Marshal.GetLastWin32Error().ToString());
			}
			return buffer;
		}
		public static byte[] Read(IntPtr address, byte[] buffer, int numBytesToRead)
		{
			bool flag = numBytesToRead <= 0 || numBytesToRead >= buffer.Length;
			if (flag)
			{
				throw new ArgumentOutOfRangeException();
			}
			IntPtr intPtr;
			bool flag2 = !Memory.ReadProcessMemory(Process.GetCurrentProcess().Handle, address, buffer, numBytesToRead, out intPtr);
			if (flag2)
			{
				throw new Exception("ReadProcessMemory failed with win32 error " + Marshal.GetLastWin32Error().ToString());
			}
			return buffer;
		}

		public static void Write(IntPtr address, byte[] bytes)
		{
			IntPtr intPtr;
			bool flag = !Memory.WriteProcessMemory(Process.GetCurrentProcess().Handle, address, bytes, bytes.Length, out intPtr);
			if (flag)
			{
				throw new Exception("WriteProcessMemory failed with win32 error " + Marshal.GetLastWin32Error().ToString());
			}
		}
	}
}
