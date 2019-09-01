using System;
using System.Windows.Forms;

namespace MPRandoAssist
{
    public static class ComboBoxExtension
    {
        public static int GetSelectedIndex(this ComboBox comboBox)
        {
            if (comboBox.InvokeRequired)
                return (int)comboBox.Invoke(new Func<int>(() => comboBox.GetSelectedIndex()));
            else
            {
                return comboBox.SelectedIndex;
            }
        }
    }
}
