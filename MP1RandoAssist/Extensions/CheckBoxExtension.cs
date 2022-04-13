using System;
using System.Reflection;
using System.Windows.Forms;

namespace MP1RandoAssist
{
    public static class CheckBoxExtension
    {
        public static bool IsChecked(this CheckBox chBox)
        {
            if (chBox.InvokeRequired)
                return (bool)chBox.Invoke(new Func<bool>(() => chBox.Checked));
            else
                return chBox.Checked;
        }

        public static void SetChecked(this CheckBox chBox, bool check)
        {
            if (chBox.Checked == check)
                return;
            typeof(CheckBox).GetField("checkState", BindingFlags.NonPublic |
                                                    BindingFlags.Instance)
                            .SetValue(chBox, check ? CheckState.Checked :
                                                     CheckState.Unchecked);
            chBox.Invalidate();
        }
    }
}
