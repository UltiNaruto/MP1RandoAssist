using Prime.Memory.Constants;
using System;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;

namespace Prime.Memory
{
    class Dolphin
    {
        private static Process dolphin = null;
        private static bool Is32BitProcess = false;
        private static long RAMBaseAddr = 0;
        private static _MP1 _MetroidPrime = null;

        public static _MP1 MetroidPrime
        {
            get
            {
                return _MetroidPrime;
            }
        }

        public static bool IsRunning
        {
            get
            {
                return dolphin == null ? true : !dolphin.HasExited;
            }
        }

        public static String GameCode
        {
            get
            {
                return Encoding.ASCII.GetString(Read(0x80000000, 6)).Trim('\0');
            }
        }

        public static int GameVersion
        {
            get
            {
                return (int)ReadInt8(0x80000007);
            }
        }

        private static bool IsValidGameCode(String s, int i=0)
        {
            if (s == "") return false;
            if (s.Length != 6) return false;
            if (i == 6) return true;
            if ((s[i] >= 'A' && s[i] <= 'Z') || (s[i] >= '0' && s[i] <= '9')) return IsValidGameCode(s, i + 1);
            else return false;
        }

        internal static bool Init()
        {
            dolphin = Process.GetProcessesByName("dolphin").Length == 0 ? null : Process.GetProcessesByName("dolphin").First();
            if (dolphin == null)
                return false;
            return true;
        }

        internal static bool GameInit()
        {
            RAMBaseAddr = 0;
            var MaxAddress = Is32BitProcess ? Int32.MaxValue : Int64.MaxValue;
            long address = Is32BitProcess ? 0 : 0x7ffe0000;
            Utils.MEMORY_BASIC_INFORMATION m;
            do
            {
                try {
                    MemoryMappedFile.OpenExisting("dolphin-emu." + dolphin.Id).Dispose();
                } catch {
                    return false;
                }
                m = Utils.CS_VirtualQuery(dolphin, address);
                if (m.Type == Utils.TypeEnum.MEM_MAPPED &&
                    m.RegionSize.ToInt64() == 0x2000000)
                {
                    RAMBaseAddr = Is32BitProcess ? m.AllocationBase.ToInt32() : m.AllocationBase.ToInt64();
                    if (!IsValidGameCode(GameCode))
                        RAMBaseAddr = 0;
                }
                if (address == (long)m.BaseAddress + (long)m.RegionSize)
                    break;
                address = (long)m.BaseAddress + (long)m.RegionSize;
                Thread.Sleep(1);
            } while (address <= MaxAddress && RAMBaseAddr == 0);
            return RAMBaseAddr != 0;
        }

        internal static bool InitMP()
        {
            _MetroidPrime = null;
            if (GameCode.Substring(0, 3) == "GM8")
            {
                if (GameCode[3] == 'E')
                {
                    if (GameVersion == 0)
                        _MetroidPrime = new MP1_NTSC_U_1_00();
                    if (GameVersion == 2)
                        _MetroidPrime = new MP1_NTSC_U_1_02();
                    if (GameVersion == 48)
                        _MetroidPrime = new MP1_NTSC_K();
                }
                if (GameCode[3] == 'P')
                    _MetroidPrime = new MP1_PAL();
                if (GameCode[3] == 'J')
                    _MetroidPrime = new MP1_NTSC_J();
            }
            if (GameCode.Substring(0, 3) == "R3M")
            {
                if (GameCode[3] == 'E')
                    _MetroidPrime = new MPT_MP1_NTSC_U();
                if (GameCode[3] == 'P')
                    _MetroidPrime = new MPT_MP1_PAL();
            }
            return _MetroidPrime != null;
        }

        internal static Byte[] Read(long gc_address, int size, bool BigEndian = false)
        {
            try
            {
                long pc_address = RAMBaseAddr + (gc_address - Constants.GC.RAMBaseAddress);
                byte[] datas = Utils.Read(dolphin, pc_address, size);
                return BigEndian ? datas.Reverse().ToArray() : datas;
            }
            catch
            {
                return null;
            }
        }

        internal static Byte ReadUInt8(long gc_address)
        {
            byte[] datas = Read(gc_address, 1);
            if (datas == null)
                return 0;
            return datas[0];
        }

        internal static UInt16 ReadUInt16(long gc_address)
        {
            byte[] datas = Read(gc_address, 2, true);
            if (datas == null)
                return 0;
            return BitConverter.ToUInt16(datas, 0);
        }

        internal static UInt32 ReadUInt32(long gc_address)
        {
            byte[] datas = Read(gc_address, 4, true);
            if (datas == null)
                return 0;
            return BitConverter.ToUInt32(datas, 0);
        }

        internal static UInt64 ReadUInt64(long gc_address)
        {
            byte[] datas = Read(gc_address, 8, true);
            if (datas == null)
                return 0;
            return BitConverter.ToUInt64(datas, 0);
        }

        internal static SByte ReadInt8(long gc_address)
        {
            byte[] datas = Read(gc_address, 1);
            if (datas == null)
                return 0;
            return (SByte)datas[0];
        }

        internal static Int16 ReadInt16(long gc_address)
        {
            byte[] datas = Read(gc_address, 2, true);
            if (datas == null)
                return 0;
            return BitConverter.ToInt16(datas, 0);
        }

        internal static Int32 ReadInt32(long gc_address)
        {
            byte[] datas = Read(gc_address, 4, true);
            if (datas == null)
                return 0;
            return BitConverter.ToInt32(datas, 0);
        }

        internal static Int64 ReadInt64(long gc_address)
        {
            byte[] datas = Read(gc_address, 8, true);
            if (datas == null)
                return 0;
            return BitConverter.ToInt64(datas, 0);
        }

        internal static Single ReadFloat32(long gc_address)
        {
            byte[] datas = Read(gc_address, 4, true);
            if (datas == null)
                return Single.NaN;
            return BitConverter.ToSingle(datas, 0);
        }

        internal static Double ReadFloat64(long gc_address)
        {
            byte[] datas = Read(gc_address, 8, true);
            if (datas == null)
                return Double.NaN;
            return BitConverter.ToDouble(datas, 0);
        }

        internal static void Write(long gc_address, Byte[] datas, bool BigEndian = false)
        {
            try
            {
                long pc_address = RAMBaseAddr + (gc_address - Constants.GC.RAMBaseAddress);
                Utils.Write(dolphin, pc_address, BigEndian ? datas.Reverse().ToArray() : datas);
            }
            catch { }
        }

        internal static void WriteUInt8(long gc_address, Byte value)
        {
            Write(gc_address, new Byte[] { value });
        }

        internal static void WriteUInt16(long gc_address, UInt16 value)
        {
            Write(gc_address, BitConverter.GetBytes(value), true);
        }

        internal static void WriteUInt32(long gc_address, UInt32 value)
        {
            Write(gc_address, BitConverter.GetBytes(value), true);
        }

        internal static void WriteUInt64(long gc_address, UInt64 value)
        {
            Write(gc_address, BitConverter.GetBytes(value), true);
        }

        internal static void WriteInt8(long gc_address, SByte value)
        {
            Write(gc_address, new Byte[] { (Byte)value });
        }

        internal static void WriteInt16(long gc_address, Int16 value)
        {
            Write(gc_address, BitConverter.GetBytes(value), true);
        }

        internal static void WriteInt32(long gc_address, Int32 value)
        {
            Write(gc_address, BitConverter.GetBytes(value), true);
        }

        internal static void WriteInt64(long gc_address, Int64 value)
        {
            Write(gc_address, BitConverter.GetBytes(value), true);
        }

        internal static void WriteFloat32(long gc_address, Single value)
        {
            Write(gc_address, BitConverter.GetBytes(value), true);
        }

        internal static void WriteFloat64(long gc_address, Double value)
        {
            Write(gc_address, BitConverter.GetBytes(value), true);
        }
    }
}
