using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MPRandoAssist
{
    public partial class Form1 : Form
    {
        #region Dolphin Instances and Checks
        Process dolphin;
        long RAMBaseAddr;
        long MPOffset;
        bool Is32BitProcess;
        bool Exiting = false;
        #endregion

        #region Auto Refill Timestamps
        internal long AutoRefill_Missiles_LastTime = 0;
        internal long AutoRefill_PowerBombs_LastTime = 0;
        internal long Regenerate_Health_LastTime = 0;
        #endregion

        #region Constants
        internal const String OBTAINED = "O";
        internal const String UNOBTAINED = "X";
        internal const long AUTOREFILL_DELAY_IN_SEC = 2;
        internal const long AUTOREFILL_DELAY = AUTOREFILL_DELAY_IN_SEC * 1000;
        internal const long REGEN_HEALTH_COOLDOWN_IN_MIN = 2;
        internal const long REGEN_HEALTH_COOLDOWN_IN_SEC = REGEN_HEALTH_COOLDOWN_IN_MIN * 60;
        internal const long REGEN_HEALTH_COOLDOWN = REGEN_HEALTH_COOLDOWN_IN_SEC * 1000;

        internal const long OFF_MORPHBALLBOMBS_COUNT = 0x457D1B;
        internal const long OFF_GAME_STATUS = 0x457F4D;
        internal const long OFF_HEALTH = -4;
        internal const long OFF_CRITICAL_HEALTH = OFF_HEALTH+4;
        internal const long OFF_ICEBEAM_OBTAINED = 0x27; // 0xDF0BF7
        internal const long OFF_WAVEBEAM_OBTAINED = 0x2F; // 0xDF0BFF
        internal const long OFF_PLASMABEAM_OBTAINED = 0x37; // 0xDF0C07
        internal const long OFF_MISSILES = 0x3B; // 0xDF0C0B
        internal const long OFF_MAX_MISSILES = OFF_MISSILES + 4;
        internal const long OFF_MORPHBALLBOMBS_OBTAINED = 0x4F; // 0xDF0C1F
        internal const long OFF_POWERBOMBS = 0x53; // 0xDF0C23
        internal const long OFF_MAX_POWERBOMBS = OFF_POWERBOMBS + 4;
        internal const long OFF_FLAMETHROWER_OBTAINED = 0x5F; // 0xDF0C2F
        internal const long OFF_THERMALVISOR_OBTAINED = 0x67; // 0xDF0C37
        internal const long OFF_CHARGEBEAM_OBTAINED = 0x6F; // 0xDF0C3F
        internal const long OFF_SUPERMISSILE_OBTAINED = 0x77; // 0xDF0C47
        internal const long OFF_GRAPPLEBEAM_OBTAINED = 0x7F; // 0xDF0C4F
        internal const long OFF_XRAYVISOR_OBTAINED = 0x87; // 0xDF0C57
        internal const long OFF_ICESPREADER_OBTAINED = 0x8F; // 0xDF0C5F
        internal const long OFF_SPACEBOOTS_OBTAINED = 0x97; // 0xDF0C67
        internal const long OFF_MORPHBALL_OBTAINED = 0x9F; // 0xDF0C6F
        internal const long OFF_BOOSTBALL_OBTAINED = 0xAF; // 0xDF0C7F
        internal const long OFF_SPIDERBALL_OBTAINED = 0xB7; // 0xDF0C87
        internal const long OFF_GRAVITYSUIT_OBTAINED = 0xC7; // 0xDF0C97
        internal const long OFF_VARIASUIT_OBTAINED = 0xCF; // 0xDF0C9F
        internal const long OFF_PHAZONSUIT_OBTAINED = 0xD7; // 0xDF0CA7
        internal const long OFF_ENERGYTANKS_OBTAINED = 0xDF; // 0xDF0CAF
        internal const long OFF_WAVEBUSTER_OBTAINED = 0xFF; // 0xDF0CCF
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

        public String Game_Code
        {
            get
            {
                return Encoding.ASCII.GetString(MemoryUtils.Read(this.dolphin, this.RAMBaseAddr, 6)).Trim('\0');
            }
        }

        #region Metroid Prime
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
            get {
                return (ushort)MemoryUtils.ReadFloat32(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_HEALTH);
            }
            set {
                MemoryUtils.WriteFloat32(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_HEALTH, (float)value);
            }
        }

        internal ushort MaxHealth
        {
            get
            {
                return (ushort)(MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_ENERGYTANKS_OBTAINED) * 100 + 99);
            }
        }

        internal bool HaveIceBeam
        {
            get
            {
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_ICEBEAM_OBTAINED) > 0;
            }
            set
            {
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_ICEBEAM_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveWaveBeam
        {
            get
            {
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_WAVEBEAM_OBTAINED) > 0;
            }
            set
            {
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_WAVEBEAM_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HavePlasmaBeam
        {
            get
            {
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_PLASMABEAM_OBTAINED) > 0;
            }
            set
            {
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_PLASMABEAM_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal ushort Missiles
        {
            get
            {
                return MemoryUtils.ReadUInt16(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_MISSILES);
            }
            set
            {
                MemoryUtils.WriteUInt16(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_MISSILES, value);
            }
        }

        internal ushort MaxMissiles
        {
            get
            {
                return MemoryUtils.ReadUInt16(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_MAX_MISSILES);
            }
            set
            {
                MemoryUtils.WriteUInt16(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_MAX_MISSILES, value);
            }
        }

        internal bool HaveMorphBallBombs
        {
            get
            {
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_MORPHBALLBOMBS_OBTAINED) > 0;
            }
            set
            {
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_MORPHBALLBOMBS_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal byte PowerBombs
        {
            get
            {
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_POWERBOMBS);
            }
            set
            {
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_POWERBOMBS, value);
            }
        }

        internal byte MaxPowerBombs
        {
            get
            {
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_MAX_POWERBOMBS);
            }
            set
            {
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_MAX_POWERBOMBS, value);
            }
        }

        internal bool HaveFlamethrower
        {
            get
            {
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_FLAMETHROWER_OBTAINED) > 0;
            }
            set
            {
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_FLAMETHROWER_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveThermalVisor
        {
            get
            {
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_THERMALVISOR_OBTAINED) > 0;
            }
            set
            {
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_THERMALVISOR_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveChargeBeam
        {
            get
            {
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_CHARGEBEAM_OBTAINED) > 0;
            }
            set
            {
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_CHARGEBEAM_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveSuperMissile
        {
            get
            {
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_SUPERMISSILE_OBTAINED) > 0;
            }
            set
            {
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_SUPERMISSILE_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveGrappleBeam
        {
            get
            {
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_GRAPPLEBEAM_OBTAINED) > 0;
            }
            set
            {
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_GRAPPLEBEAM_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveXRayVisor
        {
            get
            {
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_XRAYVISOR_OBTAINED) > 0;
            }
            set
            {
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_XRAYVISOR_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveIceSpreader
        {
            get
            {
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_ICESPREADER_OBTAINED) > 0;
            }
            set
            {
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_ICESPREADER_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveSpaceBoots
        {
            get
            {
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_SPACEBOOTS_OBTAINED) > 0;
            }
            set
            {
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_SPACEBOOTS_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveMorphBall
        {
            get {
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_MORPHBALL_OBTAINED) > 0;
            }
            set {
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_MORPHBALL_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveBoostBall
        {
            get
            {
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_BOOSTBALL_OBTAINED) > 0;
            }
            set
            {
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_BOOSTBALL_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveSpiderBall
        {
            get
            {
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_SPIDERBALL_OBTAINED) > 0;
            }
            set
            {
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_SPIDERBALL_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveGravitySuit
        {
            get
            {
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_GRAVITYSUIT_OBTAINED) > 0;
            }
            set
            {
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_GRAVITYSUIT_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveVariaSuit
        {
            get
            {
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_VARIASUIT_OBTAINED) > 0;
            }
            set
            {
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_VARIASUIT_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HavePhazonSuit
        {
            get
            {
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_PHAZONSUIT_OBTAINED) > 0;
            }
            set
            {
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_PHAZONSUIT_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveWavebuster
        {
            get
            {
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_WAVEBUSTER_OBTAINED) > 0;
            }
            set
            {
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + this.MPOffset + OFF_WAVEBUSTER_OBTAINED, (byte)(value ? 1 : 0));
            }
        }
        #endregion

        public Form1()
        {
            InitializeComponent();
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
            long address = 0x8000000;
            do
            {
                MEMORY_BASIC_INFORMATION m;
                if(VirtualQueryEx(this.dolphin.Handle, (IntPtr)address, out m, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION)))==0)
                    break;
                String game_code = Encoding.ASCII.GetString(MemoryUtils.Read(this.dolphin, this.Is32BitProcess ? m.AllocationBase.ToInt32() : m.AllocationBase.ToInt64(), 8)).Trim('\0');
                bool game_code_check = ((game_code.Length - 6)*(game_code.Length-3)) <= 0;
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
            try
            {
                this.dolphin = Process.GetProcessesByName("dolphin").Length == 0 ? null : Process.GetProcessesByName("dolphin").First();
                if (dolphin == null)
                {
                    MessageBox.Show("Dolphin is not running!\r\nExiting...");
                    this.Close();
                }
                this.Is32BitProcess = this.dolphin.MainModule.BaseAddress.ToInt64() < UInt32.MaxValue;
                this.RAMBaseAddr = GetRAMBaseAddr();
                if (this.RAMBaseAddr == 0)
                {
                    MessageBox.Show("Metroid Prime is not running!\r\nExiting...");
                    this.Close();
                }
                if (!Game_Code.StartsWith("GM8"))
                {
                    MessageBox.Show("Metroid Prime is not running!\r\nExiting...");
                    this.Close();
                }
                this.MPOffset = GetMPOffset();
                this.comboBox1.SelectedIndex = 0;
                this.comboBox1.Update();
                this.comboBox2.SelectedIndex = 0;
                this.comboBox2.Update();
                this.comboBox3.SelectedIndex = 0;
                this.comboBox3.Update();
                this.timer1.Enabled = true;
            } catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.Close();
            }
        }

        private long GetMPOffset()
        {
            long result = BytePattern.Find(MemoryUtils.Read(this.dolphin, this.RAMBaseAddr, 0x1210000), "42480000000000??000000??3E4CCCCD");
            if (result == -2)
                throw new FormatException("Hex string must be of length of power 2");
            if(result == -1)
                throw new Exception("Couldn't find pattern");
            return result;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.Exiting)
                return;
            try
            {
                if (!Game_Code.StartsWith("GM8"))
				{
                    new Thread(() => MessageBox.Show("Either Dolphin or the game is not running!\r\nExiting...")).Start();
                    this.Close();
					this.Exiting = true;
                    return;
				}
                if (Game_Status != 1)
                    return;
                this.label1.Text = "Missiles : " + Missiles + " / " + MaxMissiles;
                if (comboBox1.SelectedIndex == 1)
                    AutoRefillMissiles();
                else if (comboBox1.SelectedIndex == 2)
                    Missiles = MaxMissiles;
                this.label2.Text = "Morph Ball Bombs : " + MorphBallBombs + " / " + MaxMorphBallBombs;
                if (comboBox2.SelectedIndex == 1)
                    MorphBallBombs = MaxMorphBallBombs;
                this.label3.Text = "Power Bombs : " + PowerBombs + " / " + MaxPowerBombs;
                if (comboBox3.SelectedIndex == 1)
                    AutoRefillPowerBombs();
                else if (comboBox3.SelectedIndex == 2)
                    PowerBombs = MaxPowerBombs;
                this.label4.Text = "HP : " + Health + " / " + MaxHealth;
                if (Health < 30)
                    this.label4.Text += " /!\\";
                this.label5.Text = "Morph Ball : " + (HaveMorphBall ? OBTAINED : UNOBTAINED);
                this.label6.Text = "Thermal Visor : " + (HaveThermalVisor ? OBTAINED : UNOBTAINED);
                this.label7.Text = "XRay Visor : " + (HaveXRayVisor ? OBTAINED : UNOBTAINED);
                this.label8.Text = "Morph Ball Bombs : " + (HaveMorphBallBombs ? OBTAINED : UNOBTAINED);
                this.label9.Text = "Missile Launcher : " + (MaxMissiles > 0 ? OBTAINED : UNOBTAINED);
                this.label10.Text = "Super Missile : " + (HaveSuperMissile ? OBTAINED : UNOBTAINED);
                this.label11.Text = "Plasma Beam : " + (HavePlasmaBeam ? OBTAINED : UNOBTAINED);
                this.label12.Text = "Ice Beam : " + (HaveIceBeam ? OBTAINED : UNOBTAINED);
                this.label13.Text = "Wave Beam : " + (HaveWaveBeam ? OBTAINED : UNOBTAINED);
                this.label14.Text = "Charge Beam : " + (HaveChargeBeam ? OBTAINED : UNOBTAINED);
                this.label15.Text = "Grapple Beam : " + (HaveGrappleBeam ? OBTAINED : UNOBTAINED);
                this.label16.Text = "Spider Ball : " + (HaveSpiderBall ? OBTAINED : UNOBTAINED);
                this.label17.Text = "Boost Ball : " + (HaveBoostBall ? OBTAINED : UNOBTAINED);
                this.label18.Text = "Power Bombs : " + (MaxPowerBombs > 0 ? OBTAINED : UNOBTAINED);
                this.label19.Text = "Varia Suit : " + (HaveVariaSuit ? OBTAINED : UNOBTAINED);
                this.label20.Text = "Gravity Suit : " + (HaveGravitySuit ? OBTAINED : UNOBTAINED);
                this.label21.Text = "Phazon Suit : " + (HavePhazonSuit ? OBTAINED : UNOBTAINED);
                this.label22.Text = "Wavebuster : " + (HaveWavebuster ? OBTAINED : UNOBTAINED);
                this.label23.Text = "Ice Spreader : " + (HaveIceSpreader ? OBTAINED : UNOBTAINED);
                this.label24.Text = "Flamethrower : " + (HaveFlamethrower ? OBTAINED : UNOBTAINED);
                this.label25.Text = "Space Boots : " + (HaveSpaceBoots ? OBTAINED : UNOBTAINED);
            } catch
            {
                if (!this.Exiting)
                {
                    new Thread(() => MessageBox.Show("Either Dolphin or the game is not running!\r\nExiting...")).Start();
                    this.Close();
					this.Exiting = true;
                }
            }
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
                DateTime remainingTime = new DateTime((REGEN_HEALTH_COOLDOWN - (curTime - Regenerate_Health_LastTime))*TimeSpan.TicksPerMillisecond);
                MessageBox.Show("You can regenerate in "+(remainingTime.Minute == 0 ? "" : remainingTime.Minute+" minute"+(remainingTime.Minute > 1 ? "s ":" "))+ (remainingTime.Second == 0 ? "" : remainingTime.Second + " second" + (remainingTime.Second > 1 ? "s" : "")));
                return;
            }
            Health = MaxHealth;
            Regenerate_Health_LastTime = curTime;
        }
    }
}
