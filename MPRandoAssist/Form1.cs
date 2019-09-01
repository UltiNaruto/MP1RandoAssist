using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MPRandoAssist
{
    public partial class Form1 : Form
    {
        bool IsLoadingSettings = false;

        #region Dolphin Instances and Checks
        Process dolphin;
        long RAMBaseAddr;
        bool Is32BitProcess;
        bool Exiting = false;
        bool WasInSaveStation = false;
        bool EditingInventory = false;
        #endregion

        #region Auto Refill Timestamps
        internal long AutoRefill_Missiles_LastTime = 0;
        internal long AutoRefill_PowerBombs_LastTime = 0;
        internal long Regenerate_Health_LastTime = 0;
        #endregion

        #region Constants
        internal const long GCBaseRamAddr = 0x80000000;
        internal const long OFF_CGAMEGLOBALOBJECTS = 0x457798;
        internal const long OFF_CGAMESTATE = OFF_CGAMEGLOBALOBJECTS + 0x134;
        internal const long OFF_PLAYTIME = 0xA0;
        internal const long OFF_CSTATEMANAGER = 0x45A1A8;
        internal const long OFF_CWORLD = 0x850;
        internal const long OFF_ROOM_ID = 0x68;
        internal const long OFF_WORLD_ID = 0x6C;
        internal const long OFF_CPLAYERSTATE = 0x8B8;
        internal const String OBTAINED = "O";
        internal const String UNOBTAINED = "X";
        internal const long AUTOREFILL_DELAY_IN_SEC = 2;
        internal const long AUTOREFILL_DELAY = AUTOREFILL_DELAY_IN_SEC * 1000;
        internal const long REGEN_HEALTH_COOLDOWN_IN_MIN = 2;
        internal const long REGEN_HEALTH_COOLDOWN_IN_SEC = REGEN_HEALTH_COOLDOWN_IN_MIN * 60;
        internal const long REGEN_HEALTH_COOLDOWN = REGEN_HEALTH_COOLDOWN_IN_SEC * 1000;

        internal const long OFF_MORPHBALLBOMBS_COUNT = 0x457D1B;
        internal const long OFF_GAME_STATUS = 0x457F4D;
        internal const long OFF_HEALTH = 0x0C;
        internal const long OFF_CRITICAL_HEALTH = OFF_HEALTH + 4;
        internal const long OFF_POWERBEAM_OBTAINED = 0x2F;
        internal const long OFF_ICEBEAM_OBTAINED = 0x37;
        internal const long OFF_WAVEBEAM_OBTAINED = 0x3F;
        internal const long OFF_PLASMABEAM_OBTAINED = 0x47;
        internal const long OFF_MISSILES = 0x4B;
        internal const long OFF_MAX_MISSILES = OFF_MISSILES + 4;
        internal const long OFF_SCANVISOR_OBTAINED = 0x57;
        internal const long OFF_MORPHBALLBOMBS_OBTAINED = 0x5F;
        internal const long OFF_POWERBOMBS = 0x63;
        internal const long OFF_MAX_POWERBOMBS = OFF_POWERBOMBS + 4;
        internal const long OFF_FLAMETHROWER_OBTAINED = 0x6F;
        internal const long OFF_THERMALVISOR_OBTAINED = 0x77;
        internal const long OFF_CHARGEBEAM_OBTAINED = 0x7F;
        internal const long OFF_SUPERMISSILE_OBTAINED = 0x87;
        internal const long OFF_GRAPPLEBEAM_OBTAINED = 0x8F;
        internal const long OFF_XRAYVISOR_OBTAINED = 0x97;
        internal const long OFF_ICESPREADER_OBTAINED = 0x9F;
        internal const long OFF_SPACEJUMPBOOTS_OBTAINED = 0xA7;
        internal const long OFF_MORPHBALL_OBTAINED = 0xAF;
        internal const long OFF_COMBATVISOR_OBTAINED = 0xB7;
        internal const long OFF_BOOSTBALL_OBTAINED = 0xBF;
        internal const long OFF_SPIDERBALL_OBTAINED = 0xC7;
        internal const long OFF_POWERSUIT_OBTAINED = 0xCF;
        internal const long OFF_GRAVITYSUIT_OBTAINED = 0xD7;
        internal const long OFF_VARIASUIT_OBTAINED = 0xDF;
        internal const long OFF_PHAZONSUIT_OBTAINED = 0xE7;
        internal const long OFF_ENERGYTANKS_OBTAINED = 0xEF;
        internal const long OFF_ENERGYREFILL_OBTAINED = 0xFF;
        internal const long OFF_WAVEBUSTER_OBTAINED = 0x10F;
        internal const long OFF_ARTIFACT_OF_TRUTH_OBTAINED = 0x117;
        internal const long OFF_ARTIFACT_OF_STRENGTH_OBTAINED = 0x11F;
        internal const long OFF_ARTIFACT_OF_ELDER_OBTAINED = 0x127;
        internal const long OFF_ARTIFACT_OF_WILD_OBTAINED = 0x12F;
        internal const long OFF_ARTIFACT_OF_LIFEGIVER_OBTAINED = 0x137;
        internal const long OFF_ARTIFACT_OF_WARRIOR_OBTAINED = 0x13F;
        internal const long OFF_ARTIFACT_OF_CHOZO_OBTAINED = 0x147;
        internal const long OFF_ARTIFACT_OF_NATURE_OBTAINED = 0x14F;
        internal const long OFF_ARTIFACT_OF_SUN_OBTAINED = 0x157;
        internal const long OFF_ARTIFACT_OF_WORLD_OBTAINED = 0x15F;
        internal const long OFF_ARTIFACT_OF_SPIRIT_OBTAINED = 0x167;
        internal const long OFF_ARTIFACT_OF_NEWBORN_OBTAINED = 0x16F;
        #endregion

        #region C Imports
        public enum AllocationProtectEnum : uint
        {
            PAGE_EXECUTE = 0x00000010,
            PAGE_EXECUTE_READ = 0x00000020,
            PAGE_EXECUTE_READWRITE = 0x00000040,
            PAGE_EXECUTE_WRITECOPY = 0x00000080,
            PAGE_NOACCESS = 0x00000001,
            PAGE_READONLY = 0x00000002,
            PAGE_READWRITE = 0x00000004,
            PAGE_WRITECOPY = 0x00000008,
            PAGE_GUARD = 0x00000100,
            PAGE_NOCACHE = 0x00000200,
            PAGE_WRITECOMBINE = 0x00000400
        }

        public enum StateEnum : uint
        {
            MEM_COMMIT = 0x1000,
            MEM_FREE = 0x10000,
            MEM_RESERVE = 0x2000
        }

        public enum TypeEnum : uint
        {
            MEM_IMAGE = 0x1000000,
            MEM_MAPPED = 0x40000,
            MEM_PRIVATE = 0x20000
        }

        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public AllocationProtectEnum AllocationProtect;
            public IntPtr RegionSize;
            public StateEnum State;
            public AllocationProtectEnum Protect;
            public TypeEnum Type;
        }

        [DllImport("kernel32.dll")]
        static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);
        #endregion

        #region Dolphin
        public String Game_Code
        {
            get
            {
                return Encoding.ASCII.GetString(MemoryUtils.Read(this.dolphin, this.RAMBaseAddr, 6)).Trim('\0');
            }
        }
        #endregion

        #region Metroid Prime
        internal uint CurrentWorld
        {
            get
            {
                long WorldOffset = this.GetWorldOffset();
                if (WorldOffset == -1)
                    return UInt32.MaxValue;
                return MemoryUtils.ReadUInt32BE(this.dolphin, this.RAMBaseAddr + WorldOffset + OFF_WORLD_ID);
            }
        }

        internal uint CurrentRoom
        {
            get
            {
                long WorldOffset = this.GetWorldOffset();
                if (WorldOffset == -1)
                    return UInt32.MaxValue;
                return MemoryUtils.ReadUInt32BE(this.dolphin, this.RAMBaseAddr + WorldOffset + OFF_ROOM_ID);
            }
        }

        internal bool IsInSaveStationRoom
        {
            get
            {
                if (CurrentWorld == 0x0A) // Impact Crater
                {
                    return CurrentRoom == 0x00;   // Entrance
                }
                else if (CurrentWorld == 0x11) // Magmoor Caverns
                {
                    return CurrentRoom == 0x03 || // Save Station Magmoor A
                           CurrentRoom == 0x1C;   // Save Station Magmoor B
                }
                else if (CurrentWorld == 0x13) // Phazon Mines
                {
                    return CurrentRoom == 0x04 || // Save Station Mines A
                           CurrentRoom == 0x1E || // Save Station Mines B
                           CurrentRoom == 0x22;   // Save Station Mines C
                }
                else if (CurrentWorld == 0x18) // Chozo Ruins
                {
                    return CurrentRoom == 0x16 || // Save Station 1
                           CurrentRoom == 0x27 || // Save Station 2
                           CurrentRoom == 0x3B;   // Save Station 3
                }
                else if (CurrentWorld == 0x19) // Tallon Overworld
                {
                    return CurrentRoom == 0x00 || // Landing Site
                           CurrentRoom == 0x1C;   // Save Station in Crashed Frigate
                }
                else if (CurrentWorld == 0x1B) // Phendrana Drifts
                {
                    return CurrentRoom == 0x04 || // Save Station B
                           CurrentRoom == 0x11 || // Save Station A
                           CurrentRoom == 0x21 || // Save Station D
                           CurrentRoom == 0x2D;   // Save Station C
                }

                return false;
            }
        }
        internal byte MorphBallBombs
        {
            get
            {
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + OFF_MORPHBALLBOMBS_COUNT);
            }
            set
            {
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + OFF_MORPHBALLBOMBS_COUNT, value);
            }
        }

        internal const byte MaxMorphBallBombs = 3;

        internal sbyte Game_Status
        {
            get
            {
                return MemoryUtils.ReadInt8(this.dolphin, this.RAMBaseAddr + OFF_GAME_STATUS);
            }
        }

        internal ushort Health
        {
            get
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return 0;
                return (ushort)MemoryUtils.ReadFloat32(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_HEALTH);
            }
            set
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return;
                MemoryUtils.WriteFloat32(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_HEALTH, (float)value);
            }
        }

        internal ushort MaxHealth
        {
            get
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return 0;
                return (ushort)(MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_ENERGYTANKS_OBTAINED) * 100 + 99);
            }
        }

        internal bool HaveIceBeam
        {
            get
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_ICEBEAM_OBTAINED) > 0;
            }
            set
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_ICEBEAM_OBTAINED - 4, (byte)(value ? 1 : 0));
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_ICEBEAM_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveWaveBeam
        {
            get
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_WAVEBEAM_OBTAINED) > 0;
            }
            set
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_WAVEBEAM_OBTAINED - 4, (byte)(value ? 1 : 0));
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_WAVEBEAM_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HavePlasmaBeam
        {
            get
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_PLASMABEAM_OBTAINED) > 0;
            }
            set
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_PLASMABEAM_OBTAINED - 4, (byte)(value ? 1 : 0));
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_PLASMABEAM_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal ushort Missiles
        {
            get
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return 0;
                return MemoryUtils.ReadUInt16(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_MISSILES);
            }
            set
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return;
                MemoryUtils.WriteUInt16(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_MISSILES, value);
            }
        }

        internal ushort MaxMissiles
        {
            get
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return 0;
                return MemoryUtils.ReadUInt16(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_MAX_MISSILES);
            }
            set
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return;
                MemoryUtils.WriteUInt16(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_MAX_MISSILES, value);
            }
        }

        internal bool HaveScanVisor
        {
            get
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_SCANVISOR_OBTAINED) > 0;
            }
            set
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_SCANVISOR_OBTAINED - 4, (byte)(value ? 1 : 0));
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_SCANVISOR_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveMorphBallBombs
        {
            get
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_MORPHBALLBOMBS_OBTAINED) > 0;
            }
            set
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_MORPHBALLBOMBS_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal byte PowerBombs
        {
            get
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return 0;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_POWERBOMBS);
            }
            set
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_POWERBOMBS, value);
            }
        }

        internal byte MaxPowerBombs
        {
            get
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return 0;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_MAX_POWERBOMBS);
            }
            set
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_MAX_POWERBOMBS, value);
            }
        }

        internal bool HaveFlamethrower
        {
            get
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_FLAMETHROWER_OBTAINED) > 0;
            }
            set
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_FLAMETHROWER_OBTAINED - 4, (byte)(value ? 1 : 0));
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_FLAMETHROWER_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveThermalVisor
        {
            get
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_THERMALVISOR_OBTAINED) > 0;
            }
            set
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_THERMALVISOR_OBTAINED - 4, (byte)(value ? 1 : 0));
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_THERMALVISOR_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveChargeBeam
        {
            get
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_CHARGEBEAM_OBTAINED) > 0;
            }
            set
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_CHARGEBEAM_OBTAINED - 4, (byte)(value ? 1 : 0));
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_CHARGEBEAM_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveSuperMissile
        {
            get
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_SUPERMISSILE_OBTAINED) > 0;
            }
            set
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_SUPERMISSILE_OBTAINED - 4, (byte)(value ? 1 : 0));
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_SUPERMISSILE_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveGrappleBeam
        {
            get
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_GRAPPLEBEAM_OBTAINED) > 0;
            }
            set
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_GRAPPLEBEAM_OBTAINED - 4, (byte)(value ? 1 : 0));
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_GRAPPLEBEAM_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveXRayVisor
        {
            get
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_XRAYVISOR_OBTAINED) > 0;
            }
            set
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_XRAYVISOR_OBTAINED - 4, (byte)(value ? 1 : 0));
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_XRAYVISOR_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveIceSpreader
        {
            get
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_ICESPREADER_OBTAINED) > 0;
            }
            set
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_ICESPREADER_OBTAINED - 4, (byte)(value ? 1 : 0));
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_ICESPREADER_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveSpaceJumpBoots
        {
            get
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_SPACEJUMPBOOTS_OBTAINED) > 0;
            }
            set
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_SPACEJUMPBOOTS_OBTAINED - 4, (byte)(value ? 1 : 0));
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_SPACEJUMPBOOTS_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveMorphBall
        {
            get
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_MORPHBALL_OBTAINED) > 0;
            }
            set
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_MORPHBALL_OBTAINED - 4, (byte)(value ? 1 : 0));
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_MORPHBALL_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveCombatVisor
        {
            get
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_COMBATVISOR_OBTAINED) > 0;
            }
            set
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_COMBATVISOR_OBTAINED - 4, (byte)(value ? 1 : 0));
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_COMBATVISOR_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveBoostBall
        {
            get
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_BOOSTBALL_OBTAINED) > 0;
            }
            set
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_BOOSTBALL_OBTAINED - 4, (byte)(value ? 1 : 0));
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_BOOSTBALL_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveSpiderBall
        {
            get
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_SPIDERBALL_OBTAINED) > 0;
            }
            set
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_SPIDERBALL_OBTAINED - 4, (byte)(value ? 1 : 0));
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_SPIDERBALL_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HavePowerSuit
        {
            get
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_POWERSUIT_OBTAINED) > 0;
            }
            set
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_POWERSUIT_OBTAINED - 4, (byte)(value ? 1 : 0));
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_POWERSUIT_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveGravitySuit
        {
            get
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_GRAVITYSUIT_OBTAINED) > 0;
            }
            set
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_GRAVITYSUIT_OBTAINED - 4, (byte)(value ? 1 : 0));
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_GRAVITYSUIT_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveVariaSuit
        {
            get
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_VARIASUIT_OBTAINED) > 0;
            }
            set
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_VARIASUIT_OBTAINED - 4, (byte)(value ? 1 : 0));
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_VARIASUIT_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HavePhazonSuit
        {
            get
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_PHAZONSUIT_OBTAINED) > 0;
            }
            set
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_PHAZONSUIT_OBTAINED - 4, (byte)(value ? 1 : 0));
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_PHAZONSUIT_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveWavebuster
        {
            get
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_WAVEBUSTER_OBTAINED) > 0;
            }
            set
            {
                long PlayerState = GetPlayerStateOffset();
                if (PlayerState == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_WAVEBUSTER_OBTAINED - 4, (byte)(value ? 1 : 0));
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_WAVEBUSTER_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool Artifacts(int index)
        {
            long PlayerState = GetPlayerStateOffset();
            if (PlayerState == -1)
                return false;
            if (index < 0)
                throw new Exception("Index can't be negative");
            switch (index)
            {
                case 0:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_ARTIFACT_OF_TRUTH_OBTAINED) > 0;
                case 1:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_ARTIFACT_OF_STRENGTH_OBTAINED) > 0;
                case 2:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_ARTIFACT_OF_ELDER_OBTAINED) > 0;
                case 3:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_ARTIFACT_OF_WILD_OBTAINED) > 0;
                case 4:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_ARTIFACT_OF_LIFEGIVER_OBTAINED) > 0;
                case 5:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_ARTIFACT_OF_WARRIOR_OBTAINED) > 0;
                case 6:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_ARTIFACT_OF_CHOZO_OBTAINED) > 0;
                case 7:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_ARTIFACT_OF_NATURE_OBTAINED) > 0;
                case 8:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_ARTIFACT_OF_SUN_OBTAINED) > 0;
                case 9:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_ARTIFACT_OF_WORLD_OBTAINED) > 0;
                case 10:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_ARTIFACT_OF_SPIRIT_OBTAINED) > 0;
                case 11:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + PlayerState + OFF_ARTIFACT_OF_NEWBORN_OBTAINED) > 0;
                default:
                    throw new Exception("There are no artifacts past the 12th artifact");
            }
        }
        #endregion

        void SafeClose()
        {
            if (InvokeRequired)
                Invoke(new Action(() => SafeClose()));
            else
                Close();
        }

        public Form1()
        {
            InitializeComponent();
        }

        void LoadSettings()
        {
            if (!File.Exists("MPRandoAssist.ini"))
                SaveSettings();
            using (var file = new StreamReader(File.OpenRead("MPRandoAssist.ini")))
            {
                IsLoadingSettings = true;
                while (!file.EndOfStream)
                {
                    String line = file.ReadLine();
                    if (!line.Contains('='))
                        continue;
                    String[] setting = line.Split('=');
                    if (setting[0] == "DarkMode")
                        this.checkBox1.Checked = setting[1] == "ON";
                }
                IsLoadingSettings = false;
            }
        }

        void SaveSettings()
        {
            using (var file = new StreamWriter(File.OpenWrite("MPRandoAssist.ini")))
            {
                file.WriteLine("DarkMode=" + (this.checkBox1.Checked ? "ON" : "OFF"));
            }
        }

        long GetCurTimeInMilliseconds()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        bool IsValidChar(char c)
        {
            if (c >= 'A' && c <= 'Z')
                return true;
            if (c >= '0' && c <= '9')
                return true;
            return false;
        }

        long GetRAMBaseAddr()
        {
            long MaxAddress = Int64.MaxValue;
            long address = GCBaseRamAddr;
            do
            {
                MEMORY_BASIC_INFORMATION m;
                if (VirtualQueryEx(this.dolphin.Handle, (IntPtr)address, out m, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION))) == 0)
                    break;
                String game_code = Encoding.ASCII.GetString(MemoryUtils.Read(this.dolphin, this.Is32BitProcess ? m.AllocationBase.ToInt32() : m.AllocationBase.ToInt64(), 8)).Trim('\0');
                bool game_code_check = ((game_code.Length - 6) * (game_code.Length - 3)) <= 0;
                for (int i = 0; i < game_code.Length; i++) if (!IsValidChar(game_code[i])) game_code_check = false;
                if (m.Type == TypeEnum.MEM_MAPPED && m.AllocationProtect == AllocationProtectEnum.PAGE_READWRITE && m.State != StateEnum.MEM_FREE && m.RegionSize.ToInt64() > 0x20000 && game_code_check)
                {
                    if (this.Is32BitProcess)
                        return m.AllocationBase.ToInt32();
                    else
                        return m.AllocationBase.ToInt64();
                }
                if (address == (long)m.BaseAddress + (long)m.RegionSize)
                    break;
                address = (long)m.BaseAddress + (long)m.RegionSize;
            } while (address <= MaxAddress);
            return 0;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadSettings();
            try
            {
                this.dolphin = Process.GetProcessesByName("dolphin").Length == 0 ? null : Process.GetProcessesByName("dolphin").First();
                if (dolphin == null)
                {
                    MessageBox.Show("Dolphin is not running!\r\nExiting...");
                    SafeClose();
                    return;
                }
                this.Is32BitProcess = this.dolphin.MainModule.BaseAddress.ToInt64() < UInt32.MaxValue;
                this.RAMBaseAddr = GetRAMBaseAddr();
                if (this.RAMBaseAddr == 0)
                {
                    MessageBox.Show("Metroid Prime is not running!\r\nExiting...");
                    SafeClose();
                    return;
                }
                if (!Game_Code.StartsWith("GM8"))
                {
                    MessageBox.Show("Metroid Prime is not running!\r\nExiting...");
                    SafeClose();
                    return;
                }
                this.comboBox1.SelectedIndex = 0;
                this.comboBox1.Update();
                this.comboBox2.SelectedIndex = 0;
                this.comboBox2.Update();
                this.comboBox3.SelectedIndex = 0;
                this.comboBox3.Update();
                GCSettings.LatencyMode = GCLatencyMode.LowLatency;
                var Worker = new BackgroundWorker();
                Worker.DoWork += (s, ev) =>
                {
                    while (!this.Exiting)
                    {
                        try
                        {
                            if (!Game_Code.StartsWith("GM8"))
                            {
                                new Thread(() => MessageBox.Show("Either Dolphin or the game is not running!\r\nExiting...")).Start();
                                SafeClose();
                                this.Exiting = true;
                                break;
                            }
                            if (Game_Status != 1)
                                continue;
                            this.label26.SetTextSafely(GetIGTAsString());
                            if (WasInSaveStation != IsInSaveStationRoom && IsInSaveStationRoom)
                                Health = MaxHealth;
                            WasInSaveStation = IsInSaveStationRoom;
                            this.label1.SetTextSafely("Missiles : " + Missiles + " / " + MaxMissiles);
                            if (comboBox1.GetSelectedIndex() == 1)
                                AutoRefillMissiles();
                            else if (comboBox1.GetSelectedIndex() == 2)
                                Missiles = MaxMissiles;
                            this.label2.SetTextSafely("Morph Ball Bombs : " + MorphBallBombs + " / " + MaxMorphBallBombs);
                            if (comboBox2.GetSelectedIndex() == 1)
                                MorphBallBombs = MaxMorphBallBombs;
                            this.label3.SetTextSafely("Power Bombs : " + PowerBombs + " / " + MaxPowerBombs);
                            if (comboBox3.GetSelectedIndex() == 1)
                                AutoRefillPowerBombs();
                            else if (comboBox3.GetSelectedIndex() == 2)
                                PowerBombs = MaxPowerBombs;
                            this.label4.SetTextSafely("HP : " + Health + " / " + MaxHealth+(Health < 30 ? " /!\\" : ""));
                            if (!EditingInventory)
                            {
                                this.morphBallChkBox.SetChecked(HaveMorphBall);
                                this.thermalVisorChkBox.SetChecked(HaveThermalVisor);
                                this.xrayVisorChkBox.SetChecked(HaveXRayVisor);
                                this.morphBallBombsChkBox.SetChecked(HaveMorphBallBombs);
                                this.missileLauncherChkBox.SetChecked(MaxMissiles > 0);
                                this.superMissileChkBox.SetChecked(HaveSuperMissile);
                                this.plasmaBeamChkBox.SetChecked(HavePlasmaBeam);
                                this.iceBeamChkBox.SetChecked(HaveIceBeam);
                                this.waveBeamChkBox.SetChecked(HaveWaveBeam);
                                this.chargeBeamChkBox.SetChecked(HaveChargeBeam);
                                this.grappleBeamChkBox.SetChecked(HaveGrappleBeam);
                                this.spiderBallChkBox.SetChecked(HaveSpiderBall);
                                this.boostBallChkBox.SetChecked(HaveBoostBall);
                                this.powerBombsChkBox.SetChecked(MaxPowerBombs > 0);
                                this.variaSuitChkBox.SetChecked(HaveVariaSuit);
                                this.gravitySuitChkBox.SetChecked(HaveGravitySuit);
                                this.phazonSuitChkBox.SetChecked(HavePhazonSuit);
                                this.wavebusterChkBox.SetChecked(HaveWavebuster);
                                this.iceSpreaderChkBox.SetChecked(HaveIceSpreader);
                                this.flamethrowerChkBox.SetChecked(HaveFlamethrower);
                                this.spaceJumpBootsChkBox.SetChecked(HaveSpaceJumpBoots);
                            }
                            for (int i = 0; i < 12; i++)
                            {
                                ((Label)this.groupBox3.Controls["lblArtifact_" + (i + 1)]).SetTextSafely(this.groupBox3.Controls["lblArtifact_" + (i + 1)].Text.Split(':')[0] + ": " + (Artifacts(i) ? OBTAINED : UNOBTAINED));
                            }
                        }
                        catch
                        {
                            if (!this.Exiting)
                            {
                                new Thread(() => MessageBox.Show("Either Dolphin or the game is not running!\r\nExiting...")).Start();
                                SafeClose();
                                this.Exiting = true;
                            }
                        }
                        Thread.Sleep(100);
                    }
                };
                Worker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.Close();
            }
        }

        private long GetIGT()
        {
            long GC_CGameState = MemoryUtils.ReadUInt32BE(this.dolphin, this.RAMBaseAddr + OFF_CGAMESTATE);
            if (GC_CGameState < GCBaseRamAddr)
                return -1;
            GC_CGameState -= GCBaseRamAddr;
            double time = MemoryUtils.ReadFloat64(this.dolphin, this.RAMBaseAddr + GC_CGameState + OFF_PLAYTIME) * 1000;
            return (long)time;
        }

        private String GetIGTAsString()
        {
            long IGT = GetIGT();
            if (IGT == -1)
                return "00:00:00.000";
            return String.Format("{0:00}:{1:00}:{2:00}.{3:000}", IGT / (60 * 60 * 1000), (IGT / (60 * 1000)) % 60, (IGT / 1000) % 60, IGT % 1000);
        }

        private long GetWorldOffset()
        {
            long GC_CWorld = MemoryUtils.ReadUInt32BE(this.dolphin, this.RAMBaseAddr + OFF_CSTATEMANAGER + OFF_CWORLD);
            if (GC_CWorld < GCBaseRamAddr)
                return -1;
            return GC_CWorld - GCBaseRamAddr;
        }

        private long GetPlayerStateOffset()
        {
            long GC_CPlayerState = MemoryUtils.ReadUInt32BE(this.dolphin, this.RAMBaseAddr + OFF_CSTATEMANAGER + OFF_CPLAYERSTATE);
            if (GC_CPlayerState < GCBaseRamAddr)
                return -1;
            GC_CPlayerState = MemoryUtils.ReadUInt32BE(this.dolphin, this.RAMBaseAddr + (GC_CPlayerState - GCBaseRamAddr));
            if (GC_CPlayerState < GCBaseRamAddr)
                return -1;
            return GC_CPlayerState - GCBaseRamAddr;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
        }

        private void AutoRefillMissiles()
        {
            long curTime = GetCurTimeInMilliseconds();
            if (Missiles == MaxMissiles)
                AutoRefill_Missiles_LastTime = curTime + AUTOREFILL_DELAY;
            if (MaxMissiles == 0)
                return;
            if (Missiles + 1 > MaxMissiles)
                return;
            if (curTime - AutoRefill_Missiles_LastTime <= AUTOREFILL_DELAY)
                return;
            Missiles++;
            AutoRefill_Missiles_LastTime = curTime;
        }

        private void AutoRefillPowerBombs()
        {
            long curTime = GetCurTimeInMilliseconds();
            if (PowerBombs == MaxPowerBombs)
                AutoRefill_PowerBombs_LastTime = curTime + AUTOREFILL_DELAY;
            if (MaxPowerBombs == 0)
                return;
            if (PowerBombs + 1 > MaxPowerBombs)
                return;
            if (curTime - AutoRefill_PowerBombs_LastTime <= AUTOREFILL_DELAY)
                return;
            PowerBombs++;
            AutoRefill_PowerBombs_LastTime = curTime;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            long curTime = GetCurTimeInMilliseconds();
            if (Health == MaxHealth)
            {
                Regenerate_Health_LastTime = curTime + REGEN_HEALTH_COOLDOWN;
                MessageBox.Show("You have all your HP!");
                return;
            }
            if (MaxHealth == 0)
                return;
            if (curTime - Regenerate_Health_LastTime <= REGEN_HEALTH_COOLDOWN)
            {
                DateTime remainingTime = new DateTime((REGEN_HEALTH_COOLDOWN - (curTime - Regenerate_Health_LastTime)) * TimeSpan.TicksPerMillisecond);
                MessageBox.Show("You can regenerate in " + (remainingTime.Minute == 0 ? "" : remainingTime.Minute + " minute" + (remainingTime.Minute > 1 ? "s " : " ")) + (remainingTime.Second == 0 ? "" : remainingTime.Second + " second" + (remainingTime.Second > 1 ? "s" : "")));
                return;
            }
            Health = MaxHealth;
            Regenerate_Health_LastTime = curTime;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                this.BackColor = Color.Black;
                this.ForeColor = Color.Gray;
                this.groupBox1.ForeColor = Color.Gray;
                this.groupBox2.ForeColor = Color.Gray;
                this.groupBox3.ForeColor = Color.Gray;
                this.comboBox1.BackColor = Color.Black;
                this.comboBox1.ForeColor = Color.Gray;
                this.comboBox2.BackColor = Color.Black;
                this.comboBox2.ForeColor = Color.Gray;
                this.comboBox3.BackColor = Color.Black;
                this.comboBox3.ForeColor = Color.Gray;
                this.button1.BackColor = Color.Black;
            }
            else
            {
                this.BackColor = Color.LightGoldenrodYellow;
                this.ForeColor = Color.Black;
                this.groupBox1.ForeColor = Color.Black;
                this.groupBox2.ForeColor = Color.Black;
                this.groupBox3.ForeColor = Color.Black;
                this.comboBox1.BackColor = Color.LightGoldenrodYellow;
                this.comboBox1.ForeColor = Color.Black;
                this.comboBox2.BackColor = Color.LightGoldenrodYellow;
                this.comboBox2.ForeColor = Color.Black;
                this.comboBox3.BackColor = Color.LightGoldenrodYellow;
                this.comboBox3.ForeColor = Color.Black;
                this.button1.BackColor = Color.LightGoldenrodYellow;
            }
            if (!IsLoadingSettings)
                SaveSettings();
        }

        private void morphBallChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            HaveMorphBall = morphBallChkBox.Checked;
            EditingInventory = false;
        }

        private void morphBallBombsChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            HaveMorphBallBombs = morphBallBombsChkBox.Checked;
            EditingInventory = false;
        }

        private void spiderBallChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            HaveSpiderBall = spiderBallChkBox.Checked;
            EditingInventory = false;
        }

        private void boostBallChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            HaveBoostBall = boostBallChkBox.Checked;
            EditingInventory = false;
        }

        private void powerBombsChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            if (missileLauncherChkBox.Checked)
            {
                using (var dialog = new PowerBombsCountDialog(this.checkBox1.Checked))
                    if (dialog.ShowDialog() == DialogResult.OK)
                        MaxPowerBombs = PowerBombs = dialog.PowerBombsCount;
            }
            else
            {
                MaxPowerBombs = 0;
                PowerBombs = MaxPowerBombs;
            }
            EditingInventory = false;
        }

        private void missileLauncherChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            if (missileLauncherChkBox.Checked)
            {
                using (var dialog = new MissileCountDialog(this.checkBox1.Checked))
                    if (dialog.ShowDialog() == DialogResult.OK)
                        MaxMissiles = Missiles = dialog.MissileCount;
            }
            else
            {
                MaxMissiles = 0;
                Missiles = MaxMissiles;
            }
            EditingInventory = false;
        }

        private void superMissileChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            HaveSuperMissile = superMissileChkBox.Checked;
            EditingInventory = false;
        }

        private void variaSuitChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            HaveVariaSuit = variaSuitChkBox.Checked;
            EditingInventory = false;
        }

        private void gravitySuitChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            HaveGravitySuit = gravitySuitChkBox.Checked;
            EditingInventory = false;
        }

        private void phazonSuitChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            HavePhazonSuit = phazonSuitChkBox.Checked;
            EditingInventory = false;
        }

        private void spaceJumpBootsChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            HaveSpaceJumpBoots = spaceJumpBootsChkBox.Checked;
            EditingInventory = false;
        }

        private void thermalVisorChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            HaveThermalVisor = thermalVisorChkBox.Checked;
            EditingInventory = false;
        }

        private void xrayVisorChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            HaveXRayVisor = xrayVisorChkBox.Checked;
            EditingInventory = false;
        }

        private void waveBeamChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            HaveWaveBeam = waveBeamChkBox.Checked;
            EditingInventory = false;
        }

        private void iceBeamChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            HaveIceBeam = iceBeamChkBox.Checked;
            EditingInventory = false;
        }

        private void plasmaBeamChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            HavePlasmaBeam = plasmaBeamChkBox.Checked;
            EditingInventory = false;
        }

        private void chargeBeamChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            HaveChargeBeam = chargeBeamChkBox.Checked;
            EditingInventory = false;
        }

        private void grappleBeamChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            HaveGrappleBeam = grappleBeamChkBox.Checked;
            EditingInventory = false;
        }

        private void wavebusterChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            HaveWavebuster = wavebusterChkBox.Checked;
            EditingInventory = false;
        }

        private void iceSpreaderChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            HaveIceSpreader = iceSpreaderChkBox.Checked;
            EditingInventory = false;
        }

        private void flamethrowerChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            HaveFlamethrower = flamethrowerChkBox.Checked;
            EditingInventory = false;
        }
    }
}
