﻿using Prime.Memory.Constants;
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

        public static _MP1 MetroidPrime {
            get
            {
                return _MetroidPrime;
            }
        }

        public static bool IsRunning {
            get
            {
                return dolphin == null ? true : !dolphin.HasExited;
            }
        }

        public static String GameCode {
            get
            {
                return Encoding.ASCII.GetString(Utils.Read(dolphin, RAMBaseAddr, 6)).Trim('\0');
            }
        }

        public static int GameVersion
        {
            get
            {
                return (int)Utils.ReadUInt8(dolphin, RAMBaseAddr + 7);
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
            var windowHandles = WinAPI.Imports.GetAllChildWindowHandlesByWindowTitles(dolphin);
            GameWindowHandle = IntPtr.Zero;
            foreach (var wH in windowHandles)
            {
                if (wH.Value.Count(new Func<char, bool>((c) => c == '|')) == 5)
                    GameWindowHandle = wH.Key;
                if (GameWindowHandle != IntPtr.Zero)
                    break;
                Thread.Sleep(1);
            }
            if (GameWindowHandle == IntPtr.Zero)
                return false;
            RAMBaseAddr = 0;
            var MaxAddress = Is32BitProcess ? Int32.MaxValue : Int64.MaxValue;
            long address = 0;
            Utils.MEMORY_BASIC_INFORMATION m;
            do
            {
                Thread.Sleep(1);
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
            if (GameCode[3] == 'E')
            {
                if (GameVersion == 0)
                    _MetroidPrime = new MP1_NTSC_1_00();
                if (GameVersion == 1)
                    return false;
                if (GameVersion == 2)
                    _MetroidPrime = new MP1_NTSC_1_02();
            }
            else if (GameCode[3] == 'P')
                _MetroidPrime = new MP1_PAL();
            else
                return false;
            return true;
        }

        internal static Byte[] Read(long gc_address, int size, bool BigEndian=false)
        {
            if (!IsRunning) return null;
            long pc_address = RAMBaseAddr + (gc_address - Constants.GC.RAMBaseAddress);
            byte[] datas = Utils.Read(dolphin, pc_address, size);
            return BigEndian ? datas.Reverse().ToArray() : datas;
        }

        internal static Byte ReadUInt8(long gc_address)
        {
            if (!IsRunning) return 0;
            return Read(gc_address, 1)[0];
        }

        internal static UInt16 ReadUInt16(long gc_address)
        {
            if (!IsRunning) return 0;
            return BitConverter.ToUInt16(Read(gc_address, 2, true), 0);
        }

        internal static UInt32 ReadUInt32(long gc_address)
        {
            if (!IsRunning) return 0;
            return BitConverter.ToUInt32(Read(gc_address, 4, true), 0);
        }

        internal static UInt64 ReadUInt64(long gc_address)
        {
            if (!IsRunning) return 0;
            return BitConverter.ToUInt64(Read(gc_address, 8, true), 0);
        }

        internal static SByte ReadInt8(long gc_address)
        {
            if (!IsRunning) return 0;
            return (SByte)Read(gc_address, 1)[0];
        }

        internal static Int16 ReadInt16(long gc_address)
        {
            if (!IsRunning) return 0;
            return BitConverter.ToInt16(Read(gc_address, 2, true), 0);
        }

        internal static Int32 ReadInt32(long gc_address)
        {
            if (!IsRunning) return 0;
            return BitConverter.ToInt32(Read(gc_address, 4, true), 0);
        }

        internal static Int64 ReadInt64(long gc_address)
        {
            if (!IsRunning) return 0;
            return BitConverter.ToInt64(Read(gc_address, 8, true), 0);
        }

        internal static Single ReadFloat32(long gc_address)
        {
            if (!IsRunning) return Single.NaN;
            return BitConverter.ToSingle(Read(gc_address, 4, true), 0);
        }

        internal static Double ReadFloat64(long gc_address)
        {
            if (!IsRunning) return Double.NaN;
            return BitConverter.ToDouble(Read(gc_address, 8, true), 0);
        }

        internal static void Write(long gc_address, Byte[] datas, bool BigEndian=false)
        {
            if (!IsRunning) return;
            long pc_address = RAMBaseAddr + (gc_address - Constants.GC.RAMBaseAddress);
            Utils.Write(dolphin, pc_address, BigEndian ? datas.Reverse().ToArray() : datas);
        }

        internal static void WriteUInt8(long gc_address, Byte value)
        {
            if (!IsRunning) return;
            Write(gc_address, new Byte[] { value });
        }

        internal static void WriteUInt16(long gc_address, UInt16 value)
        {
            if (!IsRunning) return;
            Write(gc_address, BitConverter.GetBytes(value), true);
        }

        internal static void WriteUInt32(long gc_address, UInt32 value)
        {
            if (!IsRunning) return;
            Write(gc_address, BitConverter.GetBytes(value), true);
        }

        internal static void WriteUInt64(long gc_address, UInt64 value)
        {
            if (!IsRunning) return;
            Write(gc_address, BitConverter.GetBytes(value), true);
        }

        internal static void WriteInt8(long gc_address, SByte value)
        {
            if (!IsRunning) return;
            Write(gc_address, new Byte[] { (Byte)value });
        }

        internal static void WriteInt16(long gc_address, Int16 value)
        {
            if (!IsRunning) return;
            Write(gc_address, BitConverter.GetBytes(value), true);
        }

        internal static void WriteInt32(long gc_address, Int32 value)
        {
            if (!IsRunning) return;
            Write(gc_address, BitConverter.GetBytes(value), true);
        }

        internal static void WriteInt64(long gc_address, Int64 value)
        {
            if (!IsRunning) return;
            Write(gc_address, BitConverter.GetBytes(value), true);
        }

        internal static void WriteFloat32(long gc_address, Single value)
        {
            if (!IsRunning) return;
            Write(gc_address, BitConverter.GetBytes(value), true);
        }

        internal static void WriteFloat64(long gc_address, Double value)
        {
            if (!IsRunning) return;
            Write(gc_address, BitConverter.GetBytes(value), true);
        }
    }
}