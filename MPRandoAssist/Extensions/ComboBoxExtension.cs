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
                return comboBox.SelectedIndex;
        }

        public static String GetText(this ComboBox comboBox)
        {
            if (comboBox.InvokeRequired)
                return (String)comboBox.Invoke(new Func<String>(() => comboBox.GetText()));
            else
                return comboBox.Text;
        }
    }
}
