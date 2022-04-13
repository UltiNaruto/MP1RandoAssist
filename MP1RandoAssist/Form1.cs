using Wrapper;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;

namespace MP1RandoAssist
{
    public partial class Form1 : Form
    {
        bool IsLoadingSettings = false;

        #region Dolphin Instances and Checks
        bool Exiting = false;
        bool detectVersion = false;
        bool emuInit = false;
        bool gameInit = false;
        bool WasInSaveStation = false;
        bool EditingInventory = false;
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
            Version version = new AssemblyName(Assembly.GetExecutingAssembly().FullName).Version;
            InitializeComponent();
            this.Text += $" build {version.Build}";
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

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadSettings();
            try
            {
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
                    CheckBox chkBox = null;
                    int health = 0;
                    int max_health = 0;
                    int missiles = 0;
                    int max_missiles = 0;
                    int morph_ball_bombs = 0;
                    int power_bombs = 0;
                    int max_power_bombs = 0;

                    emuInit = Dolphin.Init();
                    if (emuInit)
                    {
                        gameInit = Dolphin.GameInit();
                        if (gameInit)
                        {
                            detectVersion = Dolphin.InitMP();
                        }
                    }

                    while (!this.Exiting)
                    {
                        try
                        {
                            if (emuInit && gameInit && detectVersion)
                            {
                                if (Dolphin.GameCode == "")
                                    throw new Exception();

                                if (!Dolphin.MetroidPrime.IsIngame())
                                {
                                    Thread.Sleep(1);
                                    continue;
                                }

                                health = Dolphin.MetroidPrime.GetHealth();
                                max_health = Dolphin.MetroidPrime.GetMaxHealth();
                                missiles = Dolphin.MetroidPrime.GetAmmo("Missiles");
                                max_missiles = Dolphin.MetroidPrime.GetPickupCount("Missiles");
                                morph_ball_bombs = Dolphin.MetroidPrime.GetAmmo("Morph Ball Bombs");
                                power_bombs = Dolphin.MetroidPrime.GetAmmo("Power Bombs");
                                max_power_bombs = Dolphin.MetroidPrime.GetPickupCount("Power Bombs");

                                this.label5.SetTextSafely(Dolphin.MetroidPrime.IGTAsStr());
                                if (WasInSaveStation != Dolphin.MetroidPrime.IsInSaveStationRoom && Dolphin.MetroidPrime.IsInSaveStationRoom)
                                    Dolphin.MetroidPrime.SetHealth(Dolphin.MetroidPrime.GetMaxHealth());
                                WasInSaveStation = Dolphin.MetroidPrime.IsInSaveStationRoom;
                                this.label1.SetTextSafely("Missiles : " + missiles + " / " + max_missiles);
                                this.label2.SetTextSafely("Morph Ball Bombs : " + morph_ball_bombs + " / 3");
                                this.label3.SetTextSafely("Power Bombs : " + power_bombs + " / " + max_power_bombs);
                                this.label4.SetTextSafely("HP : " + health + " / " + max_health + (health < 30 ? " /!\\" : ""));
                                if (!EditingInventory)
                                {
                                    this.morphBallChkBox.SetChecked(Dolphin.MetroidPrime.HasPickup("Morph Ball"));
                                    this.scanVisorChkBox.SetChecked(Dolphin.MetroidPrime.HasPickup("Scan Visor"));
                                    this.thermalVisorChkBox.SetChecked(Dolphin.MetroidPrime.HasPickup("Thermal Visor"));
                                    this.xrayVisorChkBox.SetChecked(Dolphin.MetroidPrime.HasPickup("XRay Visor"));
                                    this.morphBallBombsChkBox.SetChecked(Dolphin.MetroidPrime.HasPickup("Morph Ball Bombs"));
                                    this.missileLauncherChkBox.SetChecked(Dolphin.MetroidPrime.HasPickup("Missiles"));
                                    this.superMissileChkBox.SetChecked(Dolphin.MetroidPrime.HasPickup("Super Missile"));
                                    this.plasmaBeamChkBox.SetChecked(Dolphin.MetroidPrime.HasPickup("Plasma Beam"));
                                    this.iceBeamChkBox.SetChecked(Dolphin.MetroidPrime.HasPickup("Ice Beam"));
                                    this.waveBeamChkBox.SetChecked(Dolphin.MetroidPrime.HasPickup("Wave Beam"));
                                    this.chargeBeamChkBox.SetChecked(Dolphin.MetroidPrime.HasPickup("Charge Beam"));
                                    this.grappleBeamChkBox.SetChecked(Dolphin.MetroidPrime.HasPickup("Grapple Beam"));
                                    this.spiderBallChkBox.SetChecked(Dolphin.MetroidPrime.HasPickup("Spider Ball"));
                                    this.boostBallChkBox.SetChecked(Dolphin.MetroidPrime.HasPickup("Boost Ball"));
                                    this.powerBombsChkBox.SetChecked(Dolphin.MetroidPrime.HasPickup("Power Bombs"));
                                    this.variaSuitChkBox.SetChecked(Dolphin.MetroidPrime.HasPickup("Varia Suit"));
                                    this.gravitySuitChkBox.SetChecked(Dolphin.MetroidPrime.HasPickup("Gravity Suit"));
                                    this.phazonSuitChkBox.SetChecked(Dolphin.MetroidPrime.HasPickup("Phazon Suit"));
                                    this.wavebusterChkBox.SetChecked(Dolphin.MetroidPrime.HasPickup("Wavebuster"));
                                    this.iceSpreaderChkBox.SetChecked(Dolphin.MetroidPrime.HasPickup("Ice Spreader"));
                                    this.flamethrowerChkBox.SetChecked(Dolphin.MetroidPrime.HasPickup("Flamethrower"));
                                    this.spaceJumpBootsChkBox.SetChecked(Dolphin.MetroidPrime.HasPickup("Space Jump Boots"));
                                }
                                for (int i = 0; i < 12; i++)
                                {
                                    chkBox = (CheckBox)this.tableLayoutPanel4.Controls["artifact" + i + "ChkBox"];
                                    chkBox.SetChecked(Dolphin.MetroidPrime.HasPickup(chkBox.Text));
                                }
                                HandleRefillMissiles(comboBox1.GetSelectedIndex());
                                if (comboBox2.GetText() == "Instant Refill")
                                    Dolphin.MetroidPrime.SetAmmo("Morph Ball Bombs", 3);
                                HandleRefillPowerBombs(comboBox3.GetSelectedIndex());
                            }
                            else
                            {
                                if (!this.Exiting)
                                {
                                    new Thread(() => MessageBox.Show("Either Dolphin or the game is not running!\r\nExiting...")).Start();
                                    SafeClose();
                                    this.Exiting = true;
                                }
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

        private void HandleRefillMissiles(int type)
        {
            if (Dolphin.MetroidPrime == null)
                return;

            int missiles = Dolphin.MetroidPrime.GetAmmo("Missiles");
            int max_missiles = Dolphin.MetroidPrime.GetPickupCount("Missiles");
            long curTime = GetCurTimeInMilliseconds();
            if (max_missiles == 0)
                return;

            if (missiles == max_missiles)
            {
                AutoRefill_Missiles_LastTime = curTime + AUTOREFILL_DELAY;
                return;
            }

            if (type == 1)
            {
                if (curTime - AutoRefill_Missiles_LastTime <= AUTOREFILL_DELAY)
                    return;
                Dolphin.MetroidPrime.SetAmmo("Missiles", missiles + 1);
                AutoRefill_Missiles_LastTime = curTime;
            }
            if(type == 2)
            {
                Dolphin.MetroidPrime.SetAmmo("Missiles", max_missiles);
            }
        }

        private void HandleRefillPowerBombs(int type)
        {
            if (Dolphin.MetroidPrime == null)
                return;

            int power_bombs = Dolphin.MetroidPrime.GetAmmo("Power Bombs");
            int max_power_bombs = Dolphin.MetroidPrime.GetPickupCount("Power Bombs");
            long curTime = GetCurTimeInMilliseconds();
            if (max_power_bombs == 0)
                return;

            if (power_bombs == max_power_bombs)
            {
                AutoRefill_PowerBombs_LastTime = curTime + AUTOREFILL_DELAY;
                return;
            }

            if (type == 1)
            {
                if (curTime - AutoRefill_PowerBombs_LastTime <= AUTOREFILL_DELAY)
                    return;
                Dolphin.MetroidPrime.SetAmmo("Power Bombs", power_bombs + 1);
                AutoRefill_PowerBombs_LastTime = curTime;
            }

            if(type == 2)
            {
                Dolphin.MetroidPrime.SetAmmo("Power Bombs", max_power_bombs);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Dolphin.MetroidPrime == null)
                return;

            int health = Dolphin.MetroidPrime.GetHealth();
            int max_health = Dolphin.MetroidPrime.GetMaxHealth();
            long curTime = GetCurTimeInMilliseconds();

            if (health == max_health)
            {
                Regenerate_Health_LastTime = curTime + REGEN_HEALTH_COOLDOWN;
                MessageBox.Show("You have all your HP!");
                return;
            }

            if (curTime - Regenerate_Health_LastTime <= REGEN_HEALTH_COOLDOWN)
            {
                DateTime remainingTime = new DateTime((REGEN_HEALTH_COOLDOWN - (curTime - Regenerate_Health_LastTime)) * TimeSpan.TicksPerMillisecond);
                MessageBox.Show("You can regenerate in " + (remainingTime.Minute == 0 ? "" : remainingTime.Minute + " minute" + (remainingTime.Minute > 1 ? "s " : " ")) + (remainingTime.Second == 0 ? "" : remainingTime.Second + " second" + (remainingTime.Second > 1 ? "s" : "")));
                return;
            }
            Dolphin.MetroidPrime.SetHealth(max_health);
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
            if (Dolphin.MetroidPrime == null)
                return;

            EditingInventory = true;
            Dolphin.MetroidPrime.SetPickupCount("Morph Ball", morphBallChkBox.Checked ? 1 : 0);
            EditingInventory = false;
        }

        private void morphBallBombsChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Dolphin.MetroidPrime == null)
                return;

            EditingInventory = true;
            Dolphin.MetroidPrime.SetPickupCount("Morph Ball Bombs", morphBallBombsChkBox.Checked ? 1 : 0);
            EditingInventory = false;
        }

        private void spiderBallChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Dolphin.MetroidPrime == null)
                return;

            EditingInventory = true;
            Dolphin.MetroidPrime.SetPickupCount("Spider Ball", spiderBallChkBox.Checked ? 1 : 0);
            EditingInventory = false;
        }

        private void boostBallChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Dolphin.MetroidPrime == null)
                return;

            EditingInventory = true;
            Dolphin.MetroidPrime.SetPickupCount("Boost Ball", boostBallChkBox.Checked ? 1 : 0);
            EditingInventory = false;
        }

        private void powerBombsChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Dolphin.MetroidPrime == null)
                return;

            EditingInventory = true;
            if (missileLauncherChkBox.Checked)
            {
                using (var dialog = new PowerBombsCountDialog(this.checkBox1.Checked))
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        Dolphin.MetroidPrime.SetAmmo("Power Bombs", dialog.PowerBombsCount);
                        Dolphin.MetroidPrime.SetPickupCount("Power Bombs", dialog.PowerBombsCount);
                    }
            }
            else
            {
                Dolphin.MetroidPrime.SetAmmo("Power Bombs", 0);
                Dolphin.MetroidPrime.SetPickupCount("Power Bombs", 0);
            }
            EditingInventory = false;
        }

        private void missileLauncherChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Dolphin.MetroidPrime == null)
                return;

            EditingInventory = true;
            if (missileLauncherChkBox.Checked)
            {
                using (var dialog = new MissileCountDialog(this.checkBox1.Checked))
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        Dolphin.MetroidPrime.SetAmmo("Missiles", dialog.MissileCount);
                        Dolphin.MetroidPrime.SetPickupCount("Missiles", dialog.MissileCount);
                    }
            }
            else
            {
                Dolphin.MetroidPrime.SetAmmo("Missiles", 0);
                Dolphin.MetroidPrime.SetPickupCount("Missiles", 0);
            }
            EditingInventory = false;
        }

        private void superMissileChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Dolphin.MetroidPrime == null)
                return;

            EditingInventory = true;
            Dolphin.MetroidPrime.SetPickupCount("Super Missile", superMissileChkBox.Checked ? 1 : 0);
            EditingInventory = false;
        }

        private void variaSuitChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Dolphin.MetroidPrime == null)
                return;

            EditingInventory = true;
            Dolphin.MetroidPrime.SetPickupCount("Varia Suit", variaSuitChkBox.Checked ? 1 : 0);
            EditingInventory = false;
        }

        private void gravitySuitChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Dolphin.MetroidPrime == null)
                return;

            EditingInventory = true;
            Dolphin.MetroidPrime.SetPickupCount("Gravity Suit", gravitySuitChkBox.Checked ? 1 : 0);
            EditingInventory = false;
        }

        private void phazonSuitChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Dolphin.MetroidPrime == null)
                return;

            EditingInventory = true;
            Dolphin.MetroidPrime.SetPickupCount("Phazon Suit", phazonSuitChkBox.Checked ? 1 : 0);
            EditingInventory = false;
        }

        private void spaceJumpBootsChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Dolphin.MetroidPrime == null)
                return;

            EditingInventory = true;
            Dolphin.MetroidPrime.SetPickupCount("Space Jump Boots", spaceJumpBootsChkBox.Checked ? 1 : 0);
            EditingInventory = false;
        }

        private void scanVisorChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Dolphin.MetroidPrime == null)
                return;

            EditingInventory = true;
            Dolphin.MetroidPrime.SetPickupCount("Scan Visor", scanVisorChkBox.Checked ? 1 : 0);
            EditingInventory = false;
        }

        private void thermalVisorChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Dolphin.MetroidPrime == null)
                return;

            EditingInventory = true;
            Dolphin.MetroidPrime.SetPickupCount("Thermal Visor", thermalVisorChkBox.Checked ? 1 : 0);
            EditingInventory = false;
        }

        private void xrayVisorChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Dolphin.MetroidPrime == null)
                return;

            EditingInventory = true;
            Dolphin.MetroidPrime.SetPickupCount("XRay Visor", xrayVisorChkBox.Checked ? 1 : 0);
            EditingInventory = false;
        }

        private void waveBeamChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Dolphin.MetroidPrime == null)
                return;

            EditingInventory = true;
            Dolphin.MetroidPrime.SetPickupCount("Wave Beam", waveBeamChkBox.Checked ? 1 : 0);
            EditingInventory = false;
        }

        private void iceBeamChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Dolphin.MetroidPrime == null)
                return;

            EditingInventory = true;
            Dolphin.MetroidPrime.SetPickupCount("Ice Beam", iceBeamChkBox.Checked ? 1 : 0);
            EditingInventory = false;
        }

        private void plasmaBeamChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Dolphin.MetroidPrime == null)
                return;

            EditingInventory = true;
            Dolphin.MetroidPrime.SetPickupCount("Plasma Beam", plasmaBeamChkBox.Checked ? 1 : 0);
            EditingInventory = false;
        }

        private void chargeBeamChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Dolphin.MetroidPrime == null)
                return;

            EditingInventory = true;
            Dolphin.MetroidPrime.SetPickupCount("Charge Beam", chargeBeamChkBox.Checked ? 1 : 0);
            EditingInventory = false;
        }

        private void grappleBeamChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Dolphin.MetroidPrime == null)
                return;

            EditingInventory = true;
            Dolphin.MetroidPrime.SetPickupCount("Grapple Beam", grappleBeamChkBox.Checked ? 1 : 0);
            EditingInventory = false;
        }

        private void wavebusterChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Dolphin.MetroidPrime == null)
                return;

            EditingInventory = true;
            Dolphin.MetroidPrime.SetPickupCount("Wavebuster", wavebusterChkBox.Checked ? 1 : 0);
            EditingInventory = false;
        }

        private void iceSpreaderChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Dolphin.MetroidPrime == null)
                return;

            EditingInventory = true;
            Dolphin.MetroidPrime.SetPickupCount("Ice Spreader", iceSpreaderChkBox.Checked ? 1 : 0);
            EditingInventory = false;
        }

        private void flamethrowerChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Dolphin.MetroidPrime == null)
                return;

            EditingInventory = true;
            Dolphin.MetroidPrime.SetPickupCount("Flamethrower", flamethrowerChkBox.Checked ? 1 : 0);
            EditingInventory = false;
        }

        private void fusionSuitChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Dolphin.MetroidPrime == null)
                return;

            if (fusionSuitChkBox.Checked)
                Dolphin.MetroidPrime.CurrentSuitVisual |= 4;
            else
                Dolphin.MetroidPrime.CurrentSuitVisual &= ~(uint)4;
        }

        private void artifactsChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Dolphin.MetroidPrime == null)
                return;

            CheckBox chkBox = (CheckBox)sender;
            EditingInventory = true;
            Dolphin.MetroidPrime.SetPickupCount(chkBox.Text, chkBox.Checked ? 1 : 0);
            EditingInventory = false;
        }
    }
}
