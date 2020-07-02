using Prime.Memory;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Threading;
using System.Windows.Forms;

namespace MPRandoAssist
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
                    while (!this.Exiting)
                    {
                        try
                        {
                            if (!emuInit)
                            {
                                emuInit = Dolphin.Init();
                                if (!emuInit)
                                {
                                    Thread.Sleep(1);
                                    continue;
                                }
                            }
                            if (emuInit && !gameInit)
                            {
                                gameInit = Dolphin.GameInit();
                                if (!gameInit)
                                {
                                    Thread.Sleep(1);
                                    continue;
                                }
                            }
                            if (emuInit && gameInit && !detectVersion)
                            {
                                detectVersion = Dolphin.InitMP();
                                if (!detectVersion)
                                {
                                    Thread.Sleep(1);
                                    continue;
                                }
                            }
                            if (emuInit && gameInit && detectVersion)
                            {
                                if (Dolphin.MetroidPrime.CGameState == -1)
                                {
                                    Thread.Sleep(1);
                                    continue;
                                }
                                this.label5.SetTextSafely(Dolphin.MetroidPrime.IGTAsStr);
                                if (WasInSaveStation != Dolphin.MetroidPrime.IsInSaveStationRoom && Dolphin.MetroidPrime.IsInSaveStationRoom)
                                    Dolphin.MetroidPrime.Health = Dolphin.MetroidPrime.MaxHealth;
                                WasInSaveStation = Dolphin.MetroidPrime.IsInSaveStationRoom;
                                this.label1.SetTextSafely("Missiles : " + Dolphin.MetroidPrime.Missiles + " / " + Dolphin.MetroidPrime.MaxMissiles);
                                this.label2.SetTextSafely("Morph Ball Bombs : " + Dolphin.MetroidPrime.MorphBallBombs + " / " + Dolphin.MetroidPrime.MaxMorphBallBombs);
                                this.label3.SetTextSafely("Power Bombs : " + Dolphin.MetroidPrime.PowerBombs + " / " + Dolphin.MetroidPrime.MaxPowerBombs);
                                this.label4.SetTextSafely("HP : " + Dolphin.MetroidPrime.Health + " / " + Dolphin.MetroidPrime.MaxHealth + (Dolphin.MetroidPrime.Health < 30 ? " /!\\" : ""));
                                if (!EditingInventory)
                                {
                                    this.morphBallChkBox.SetChecked(Dolphin.MetroidPrime.HaveMorphBall);
                                    this.thermalVisorChkBox.SetChecked(Dolphin.MetroidPrime.HaveThermalVisor);
                                    this.xrayVisorChkBox.SetChecked(Dolphin.MetroidPrime.HaveXRayVisor);
                                    this.morphBallBombsChkBox.SetChecked(Dolphin.MetroidPrime.HaveMorphBallBombs);
                                    this.missileLauncherChkBox.SetChecked(Dolphin.MetroidPrime.MaxMissiles > 0);
                                    this.superMissileChkBox.SetChecked(Dolphin.MetroidPrime.HaveSuperMissile);
                                    this.plasmaBeamChkBox.SetChecked(Dolphin.MetroidPrime.HavePlasmaBeam);
                                    this.iceBeamChkBox.SetChecked(Dolphin.MetroidPrime.HaveIceBeam);
                                    this.waveBeamChkBox.SetChecked(Dolphin.MetroidPrime.HaveWaveBeam);
                                    this.chargeBeamChkBox.SetChecked(Dolphin.MetroidPrime.HaveChargeBeam);
                                    this.grappleBeamChkBox.SetChecked(Dolphin.MetroidPrime.HaveGrappleBeam);
                                    this.spiderBallChkBox.SetChecked(Dolphin.MetroidPrime.HaveSpiderBall);
                                    this.boostBallChkBox.SetChecked(Dolphin.MetroidPrime.HaveBoostBall);
                                    this.powerBombsChkBox.SetChecked(Dolphin.MetroidPrime.MaxPowerBombs > 0);
                                    this.variaSuitChkBox.SetChecked(Dolphin.MetroidPrime.HaveVariaSuit);
                                    this.gravitySuitChkBox.SetChecked(Dolphin.MetroidPrime.HaveGravitySuit);
                                    this.phazonSuitChkBox.SetChecked(Dolphin.MetroidPrime.HavePhazonSuit);
                                    this.wavebusterChkBox.SetChecked(Dolphin.MetroidPrime.HaveWavebuster);
                                    this.iceSpreaderChkBox.SetChecked(Dolphin.MetroidPrime.HaveIceSpreader);
                                    this.flamethrowerChkBox.SetChecked(Dolphin.MetroidPrime.HaveFlamethrower);
                                    this.spaceJumpBootsChkBox.SetChecked(Dolphin.MetroidPrime.HaveSpaceJumpBoots);
                                }
                                for (int i = 0; i < 12; i++)
                                {
                                    ((Label)this.groupBox3.Controls["lblArtifact_" + (i + 1)]).SetTextSafely(this.groupBox3.Controls["lblArtifact_" + (i + 1)].Text.Split(':')[0] + ": " + (Dolphin.MetroidPrime.Artifacts(i) ? OBTAINED : UNOBTAINED));
                                }
                                HandleRefillMissiles(comboBox1.GetSelectedIndex());
                                if (comboBox2.GetSelectedIndex() == 1)
                                    Dolphin.MetroidPrime.MorphBallBombs = Dolphin.MetroidPrime.MaxMorphBallBombs;
                                HandleRefillPowerBombs(comboBox3.GetSelectedIndex());
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
            if (Dolphin.MetroidPrime.MaxMissiles == 0)
                return;

            if (Dolphin.MetroidPrime.Missiles + 1 > Dolphin.MetroidPrime.MaxMissiles)
                return;

            if (type == 1)
            {
                long curTime = GetCurTimeInMilliseconds();
                if (Dolphin.MetroidPrime.Missiles == Dolphin.MetroidPrime.MaxMissiles)
                    AutoRefill_Missiles_LastTime = curTime + AUTOREFILL_DELAY;

                if (curTime - AutoRefill_Missiles_LastTime <= AUTOREFILL_DELAY)
                    return;
                Dolphin.MetroidPrime.Missiles++;
                AutoRefill_Missiles_LastTime = curTime;
            }
            if(type == 2)
            {
                Dolphin.MetroidPrime.Missiles = Dolphin.MetroidPrime.MaxMissiles;
            }
        }

        private void HandleRefillPowerBombs(int type)
        {
            if (Dolphin.MetroidPrime.MaxPowerBombs == 0)
                return;

            if (Dolphin.MetroidPrime.PowerBombs + 1 > Dolphin.MetroidPrime.MaxPowerBombs)
                return;

            if (type == 1)
            {
                long curTime = GetCurTimeInMilliseconds();
                if (Dolphin.MetroidPrime.PowerBombs == Dolphin.MetroidPrime.MaxPowerBombs)
                    AutoRefill_PowerBombs_LastTime = curTime + AUTOREFILL_DELAY;

                if (curTime - AutoRefill_PowerBombs_LastTime <= AUTOREFILL_DELAY)
                    return;
                Dolphin.MetroidPrime.PowerBombs++;
                AutoRefill_PowerBombs_LastTime = curTime;
            }

            if(type == 2)
            {
                Dolphin.MetroidPrime.PowerBombs = Dolphin.MetroidPrime.MaxPowerBombs;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            long curTime = GetCurTimeInMilliseconds();
            if (Dolphin.MetroidPrime.Health == Dolphin.MetroidPrime.MaxHealth)
            {
                Regenerate_Health_LastTime = curTime + REGEN_HEALTH_COOLDOWN;
                MessageBox.Show("You have all your HP!");
                return;
            }
            if (Dolphin.MetroidPrime.MaxHealth == 0)
                return;
            if (curTime - Regenerate_Health_LastTime <= REGEN_HEALTH_COOLDOWN)
            {
                DateTime remainingTime = new DateTime((REGEN_HEALTH_COOLDOWN - (curTime - Regenerate_Health_LastTime)) * TimeSpan.TicksPerMillisecond);
                MessageBox.Show("You can regenerate in " + (remainingTime.Minute == 0 ? "" : remainingTime.Minute + " minute" + (remainingTime.Minute > 1 ? "s " : " ")) + (remainingTime.Second == 0 ? "" : remainingTime.Second + " second" + (remainingTime.Second > 1 ? "s" : "")));
                return;
            }
            Dolphin.MetroidPrime.Health = Dolphin.MetroidPrime.MaxHealth;
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
            Dolphin.MetroidPrime.HaveMorphBall = morphBallChkBox.Checked;
            EditingInventory = false;
        }

        private void morphBallBombsChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            Dolphin.MetroidPrime.HaveMorphBallBombs = morphBallBombsChkBox.Checked;
            EditingInventory = false;
        }

        private void spiderBallChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            Dolphin.MetroidPrime.HaveSpiderBall = spiderBallChkBox.Checked;
            EditingInventory = false;
        }

        private void boostBallChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            Dolphin.MetroidPrime.HaveBoostBall = boostBallChkBox.Checked;
            EditingInventory = false;
        }

        private void powerBombsChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            if (missileLauncherChkBox.Checked)
            {
                using (var dialog = new PowerBombsCountDialog(this.checkBox1.Checked))
                    if (dialog.ShowDialog() == DialogResult.OK)
                        Dolphin.MetroidPrime.MaxPowerBombs = Dolphin.MetroidPrime.PowerBombs = dialog.PowerBombsCount;
            }
            else
                Dolphin.MetroidPrime.MaxPowerBombs = Dolphin.MetroidPrime.PowerBombs = 0;
            EditingInventory = false;
        }

        private void missileLauncherChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            if (missileLauncherChkBox.Checked)
            {
                using (var dialog = new MissileCountDialog(this.checkBox1.Checked))
                    if (dialog.ShowDialog() == DialogResult.OK)
                        Dolphin.MetroidPrime.MaxMissiles = Dolphin.MetroidPrime.Missiles = dialog.MissileCount;
            }
            else
                Dolphin.MetroidPrime.MaxMissiles = Dolphin.MetroidPrime.Missiles = 0;
            EditingInventory = false;
        }

        private void superMissileChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            Dolphin.MetroidPrime.HaveSuperMissile = superMissileChkBox.Checked;
            EditingInventory = false;
        }

        private void variaSuitChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            Dolphin.MetroidPrime.HaveVariaSuit = variaSuitChkBox.Checked;
            EditingInventory = false;
        }

        private void gravitySuitChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            Dolphin.MetroidPrime.HaveGravitySuit = gravitySuitChkBox.Checked;
            EditingInventory = false;
        }

        private void phazonSuitChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            Dolphin.MetroidPrime.HavePhazonSuit = phazonSuitChkBox.Checked;
            EditingInventory = false;
        }

        private void spaceJumpBootsChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            Dolphin.MetroidPrime.HaveSpaceJumpBoots = spaceJumpBootsChkBox.Checked;
            EditingInventory = false;
        }

        private void thermalVisorChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            Dolphin.MetroidPrime.HaveThermalVisor = thermalVisorChkBox.Checked;
            EditingInventory = false;
        }

        private void xrayVisorChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            Dolphin.MetroidPrime.HaveXRayVisor = xrayVisorChkBox.Checked;
            EditingInventory = false;
        }

        private void waveBeamChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            Dolphin.MetroidPrime.HaveWaveBeam = waveBeamChkBox.Checked;
            EditingInventory = false;
        }

        private void iceBeamChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            Dolphin.MetroidPrime.HaveIceBeam = iceBeamChkBox.Checked;
            EditingInventory = false;
        }

        private void plasmaBeamChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            Dolphin.MetroidPrime.HavePlasmaBeam = plasmaBeamChkBox.Checked;
            EditingInventory = false;
        }

        private void chargeBeamChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            Dolphin.MetroidPrime.HaveChargeBeam = chargeBeamChkBox.Checked;
            EditingInventory = false;
        }

        private void grappleBeamChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            Dolphin.MetroidPrime.HaveGrappleBeam = grappleBeamChkBox.Checked;
            EditingInventory = false;
        }

        private void wavebusterChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            Dolphin.MetroidPrime.HaveWavebuster = wavebusterChkBox.Checked;
            EditingInventory = false;
        }

        private void iceSpreaderChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            Dolphin.MetroidPrime.HaveIceSpreader = iceSpreaderChkBox.Checked;
            EditingInventory = false;
        }

        private void flamethrowerChkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditingInventory = true;
            Dolphin.MetroidPrime.HaveFlamethrower = flamethrowerChkBox.Checked;
            EditingInventory = false;
        }
    }
}
