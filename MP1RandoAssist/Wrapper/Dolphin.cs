using Imports;
using System;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Wrapper
{
    public static class Dolphin
    {
        static Process dolphin = null;
        static bool Is32BitProcess = false;
        internal static Metroid MetroidPrime = null;

        internal static bool IsRunning
        {
            get
            {
                return dolphin != null && !dolphin.HasExited;
            }
        }

        internal static String GameCode
        {
            get
            {
                return Encoding.ASCII.GetString(GCMem.Read(0x80000000, 6)).Trim('\0');
            }
        }

        internal static int GameVersion
        {
            get
            {
                return (int)GCMem.Read(0x80000007, 1)[0];
            }
        }

        internal static bool IsWiiGame
        {
            get
            {
                return GCMem.ReadUInt32(0x80000018) == 0x5D1C9EA3;
            }
        }

        internal static bool IsGCGame
        {
            get
            {
                return GCMem.ReadUInt32(0x8000001C) == 0xC2339F3D;
            }
        }

        static bool IsValidGameCode(String s, int i = 0)
        {
            if (s == "") return false;
            if (s.Length != 6) return false;
            if (i == 6) return true;
            if ((s[i] >= 'A' && s[i] <= 'Z') || (s[i] >= '0' && s[i] <= '9')) return IsValidGameCode(s, i + 1);
            else return false;
        }

        internal static bool Init()
        {
#if WINDOWS
            dolphin = Process.GetProcessesByName("dolphin").FirstOrDefault();
#elif LINUX
            dolphin = Process.GetProcessesByName("dolphin-emu").FirstOrDefault();
#endif
            if (dolphin == null)
                dolphin = Process.GetProcessesByName("MPR").FirstOrDefault();

            if (dolphin == null)
                return false;

            Is32BitProcess = dolphin.MainModule.BaseAddress.ToInt64() < UInt32.MaxValue;
            return true;
        }

        internal static bool GameInit()
        {
            long RAMBaseAddr = 0;
            var mmap_entries = ImportsMgr.EnumerateVirtualMemorySpaces(dolphin).Where(mmap_entry =>
                mmap_entry.Size == 0x2000000 &&
                !mmap_entry.IsPrivate &&
                mmap_entry.Permissions == (VirtualMemoryPermissions.READ | VirtualMemoryPermissions.WRITE)
            ).ToArray();
            foreach(var mmap_entry in mmap_entries)
            {
                RAMBaseAddr = mmap_entry.BaseAddress;
                GCMem.Init(dolphin, RAMBaseAddr);
                if (!IsValidGameCode(GameCode))
                {
                    RAMBaseAddr = 0;
                    GCMem.DeInit();
                    continue;
                }
                break;
            }
            return RAMBaseAddr != 0;
        }

        internal static bool InitMP()
        {
            MetroidPrime = null;
            try
            {
                if (IsGCGame)
                {
                    if (GameCode.Substring(0, 3) == "GM8")
                    {
                        if (GameCode[3] == 'E')
                        {
                            if (GameVersion == 0)
                                MetroidPrime = new Prime.MP1_NTSC_0_00();
                            if (GameVersion == 1)
                                MetroidPrime = new Prime.MP1_NTSC_0_01();
                            if (GameVersion == 2)
                                MetroidPrime = new Prime.MP1_NTSC_0_02();
                            if (GameVersion == 48)
                                MetroidPrime = new Prime.MP1_NTSC_K();
                        }
                        if (GameCode[3] == 'J')
                            MetroidPrime = new Prime.MP1_NTSC_J();
                        if (GameCode[3] == 'P')
                            MetroidPrime = new Prime.MP1_PAL();
                    }
                }
                if (IsWiiGame)
                {
                    UInt32 opcode = GCMem.ReadUInt32(0x8046d340);
                    if (GameCode.Substring(0, 2) == "R3")
                    {
                        // Trilogy
                        if (GameCode[2] == 'M')
                        {
                            if (GameCode[3] == 'E')
                            {
                                while ((opcode = GCMem.ReadUInt32(0x8046d340)) == 0x38000018)
                                {
                                    Thread.Sleep(10);
                                }
                                if (opcode == 0x4e800020)
                                {
                                    MetroidPrime = new Prime.MPT_MP1_NTSC_U();
                                }
                            }
                            if (GameCode[3] == 'P')
                            {
                                while ((opcode = GCMem.ReadUInt32(0x8046d340)) == 0x7c0000d0)
                                {
                                    Thread.Sleep(10);
                                }
                                if (opcode == 0x7c962378)
                                {
                                    MetroidPrime = new Prime.MPT_MP1_PAL();
                                }
                            }
                        }

                        if (GameCode[2] == 'I' && GameCode[3] == 'J')
                        {
                            while ((opcode = GCMem.ReadUInt32(0x8046d340)) == 0x806ddaec)
                            {
                                Thread.Sleep(10);
                            }
                            if (opcode == 0x53687566)
                            {
                                MetroidPrime = new Prime.MPT_MP1_NTSC_J();
                            }
                        }
                    }
                }
                return MetroidPrime != null;
            } catch {
                return false;
            }
        }
    }
}
