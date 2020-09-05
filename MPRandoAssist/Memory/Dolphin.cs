using Prime.Memory.Constants;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
        private static IntPtr GameWindowHandle = IntPtr.Zero;
        private static _MP1 _MetroidPrime = null;
        private static Dictionary<String, Image> img = new Dictionary<String, Image>();

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
                return Encoding.ASCII.GetString(Utils.Read(dolphin, RAMBaseAddr, 6)).Trim('\0');
            }
        }

        public static int GameVersion
        {
            get
            {
                return (int)Utils.Read(dolphin, RAMBaseAddr + 7, 1)[0];
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
            Is32BitProcess = dolphin.MainModule.BaseAddress.ToInt64() < UInt32.MaxValue;
            return true;
        }

        internal static bool GameInit()
        {
            RAMBaseAddr = 0;
            var MaxAddress = Is32BitProcess ? Int32.MaxValue : Int64.MaxValue;
            long address = 0;
            Utils.MEMORY_BASIC_INFORMATION m;
            do
            {
                m = Utils.CS_VirtualQuery(dolphin, address);
				if (m.AllocationBase == IntPtr.Zero && m.Protect == Utils.AllocationProtectEnum.PAGE_NOACCESS)
                {
                    if (address == (long)m.BaseAddress + (long)m.RegionSize)
                        break;
                    address = (long)m.BaseAddress + (long)m.RegionSize;
                    continue;
                }
                if (m.Type != Utils.TypeEnum.MEM_MAPPED)
                {
                    if (address == (long)m.BaseAddress + (long)m.RegionSize)
                        break;
                    address = (long)m.BaseAddress + (long)m.RegionSize;
                    continue;
                }
                if (m.AllocationProtect != Utils.AllocationProtectEnum.PAGE_READWRITE)
                {
                    if (address == (long)m.BaseAddress + (long)m.RegionSize)
                        break;
                    address = (long)m.BaseAddress + (long)m.RegionSize;
                    continue;
                }
                if (m.State == Utils.StateEnum.MEM_FREE)
                {
                    if (address == (long)m.BaseAddress + (long)m.RegionSize)
                        break;
                    address = (long)m.BaseAddress + (long)m.RegionSize;
                    continue;
                }
                if (m.RegionSize.ToInt64() <= 0x20000)
                {
                    if (address == (long)m.BaseAddress + (long)m.RegionSize)
                        break;
                    address = (long)m.BaseAddress + (long)m.RegionSize;
                    continue;
                }
                RAMBaseAddr = Is32BitProcess ? m.AllocationBase.ToInt32() : m.AllocationBase.ToInt64();
                if (!IsValidGameCode(GameCode))
                {
                    RAMBaseAddr = 0;
                    if (address == (long)m.BaseAddress + (long)m.RegionSize)
                        break;
                    address = (long)m.BaseAddress + (long)m.RegionSize;
                    continue;
                }
                break;
            } while (address <= MaxAddress);
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
                        _MetroidPrime = new MP1_NTSC_1_00();
                    if (GameVersion == 2)
                        _MetroidPrime = new MP1_NTSC_1_02();
                }
                if (GameCode[3] == 'P')
                    _MetroidPrime = new MP1_PAL();
            }
            if (GameCode.Substring(0, 3) == "R3M")
            {
                if (GameCode[3] == 'E')
                    _MetroidPrime = new MPT_MP1_NTSC();
                if (GameCode[3] == 'P')
                    _MetroidPrime = new MPT_MP1_PAL();
            }
            return _MetroidPrime != null;
        }

        internal static Byte[] Read(long gc_address, int size, bool BigEndian=false)
        {
            try
            {
                long pc_address = RAMBaseAddr + (gc_address - Constants.GC.RAMBaseAddress);
                byte[] datas = Utils.Read(dolphin, pc_address, size);
                return BigEndian ? datas.Reverse().ToArray() : datas;
            } catch {
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

        internal static void Write(long gc_address, Byte[] datas, bool BigEndian=false)
        {
            try
            {
                long pc_address = RAMBaseAddr + (gc_address - Constants.GC.RAMBaseAddress);
                Utils.Write(dolphin, pc_address, BigEndian ? datas.Reverse().ToArray() : datas);
            } catch {}
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
