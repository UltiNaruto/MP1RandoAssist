using System.Reflection;
using System.Windows.Forms;

namespace MPRandoAssist
{
    public static class CheckBoxExtension
    {
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
