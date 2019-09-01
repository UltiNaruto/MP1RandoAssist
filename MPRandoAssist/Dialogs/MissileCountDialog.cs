using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MPRandoAssist
{
    public partial class MissileCountDialog : Form
    {
        internal byte MissileCount = 0;

        public MissileCountDialog()
        {
            InitializeComponent();
        }

        public MissileCountDialog(bool DarkMode)
        {
            InitializeComponent();
            if (DarkMode)
            {
                this.BackColor = this.button1.BackColor = this.button2.BackColor = this.numericUpDown1.BackColor = Color.Black;
                this.ForeColor = this.button1.ForeColor = this.button2.ForeColor = this.numericUpDown1.BackColor = Color.Gray;

            }
            else
            {
                this.BackColor = this.button1.BackColor = this.button2.BackColor = this.numericUpDown1.BackColor = Color.LightGoldenrodYellow;
                this.ForeColor = this.button1.ForeColor = this.button2.ForeColor = this.numericUpDown1.BackColor = Color.Black;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (numericUpDown1.Value % numericUpDown1.Increment != 0)
            {
                MessageBox.Show("Missile count must be a multiple of "+ numericUpDown1.Increment);
                return;
            }
            if (numericUpDown1.Value < numericUpDown1.Minimum || numericUpDown1.Value > numericUpDown1.Maximum)
            {
                MessageBox.Show("Missile count must be between " + numericUpDown1.Minimum + " and " + numericUpDown1.Maximum);
                return;
            }
            MissileCount = (byte)numericUpDown1.Value;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
