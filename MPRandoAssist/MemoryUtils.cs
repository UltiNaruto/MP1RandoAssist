using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace MPRandoAssist
{
    class MemoryUtils
    {
        #region C imports
        const int PROCESS_WM_READ = 0x0010;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_OPERATION = 0x0008;
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess,IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, Int32 nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);
        #endregion

        internal static Byte[] Read(Process proc, long address, int size)
        {
            if (proc.HasExited)
                return null;
            if (size == 0)
                return new byte[0];
            byte[] datas = new byte[size];
            IntPtr readBytesCount = IntPtr.Zero;
            ReadProcessMemory(proc.Handle, new IntPtr(address), datas, size, out readBytesCount);
            return datas;
        }

        internal static Byte ReadUInt8(Process proc, long address)
        {
            return Read(proc, address, 1)[0];
        }

        internal static UInt16 ReadUInt16(Process proc, long address)
        {
            return BitConverter.ToUInt16(Read(proc, address, 2), 0);
        }

        internal static UInt32 ReadUInt32(Process proc, long address)
        {
            return BitConverter.ToUInt32(Read(proc, address, 4), 0);
        }

        internal static UInt64 ReadUInt64(Process proc, long address)
        {
            return BitConverter.ToUInt64(Read(proc, address, 8), 0);
        }

        internal static SByte ReadInt8(Process proc, long address)
        {
            return (SByte)Read(proc, address, 1)[0];
        }

        internal static Int16 ReadInt16(Process proc, long address)
        {
            return BitConverter.ToInt16(Read(proc, address, 2), 0);
        }

        internal static Int32 ReadInt32(Process proc, long address)
        {
            return BitConverter.ToInt32(Read(proc, address, 4), 0);
        }

        internal static Int64 ReadInt64(Process proc, long address)
        {
            return BitConverter.ToInt64(Read(proc, address, 8), 0);
        }

        internal static Single ReadFloat32(Process proc, long address)
        {
            return BitConverter.ToSingle(Read(proc, address, 4).Reverse().ToArray(), 0);
        }

        internal static Double ReadFloat64(Process proc, long address)
        {
            return BitConverter.ToDouble(Read(proc, address, 8).Reverse().ToArray(), 0);
        }

        internal static void Write(Process proc, long address, Byte[] datas)
        {
            if (proc.HasExited)
                return;
            if (datas == null)
                return;
            IntPtr writtenBytesCount = IntPtr.Zero;
            WriteProcessMemory(proc.Handle, new IntPtr(address), datas, datas.Length, out writtenBytesCount);
        }

        internal static void WriteUInt8(Process proc, long address, Byte value)
        {
            Write(proc, address, new Byte[] { value });
        }

        internal static void WriteUInt16(Process proc, long address, UInt16 value)
        {
            Write(proc, address, BitConverter.GetBytes(value));
        }

        internal static void WriteUInt32(Process proc, long address, UInt32 value)
        {
            Write(proc, address, BitConverter.GetBytes(value));
        }

        internal static void WriteUInt64(Process proc, long address, UInt64 value)
        {
            Write(proc, address, BitConverter.GetBytes(value));
        }

        internal static void WriteInt8(Process proc, long address, SByte value)
        {
            Write(proc, address, new Byte[] { (Byte)value });
        }

        internal static void WriteInt16(Process proc, long address, Int16 value)
        {
            Write(proc, address, BitConverter.GetBytes(value));
        }

        internal static void WriteInt32(Process proc, long address, Int32 value)
        {
            Write(proc, address, BitConverter.GetBytes(value));
        }

        internal static void WriteInt64(Process proc, long address, Int64 value)
        {
            Write(proc, address, BitConverter.GetBytes(value));
        }

        internal static void WriteFloat32(Process proc, long address, Single value)
        {
            Write(proc, address, BitConverter.GetBytes(value).Reverse().ToArray());
        }

        internal static void WriteFloat64(Process proc, long address, Double value)
        {
            Write(proc, address, BitConverter.GetBytes(value).Reverse().ToArray());
        }
    }
}
