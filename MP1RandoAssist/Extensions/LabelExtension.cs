using System;
using System.Windows.Forms;

namespace MP1RandoAssist
{
    public static class LabelExtension
    {
        public static void SetTextSafely(this Label lbl, String text)
        {
            if (lbl.InvokeRequired)
                lbl.Invoke(new Action(() => lbl.SetTextSafely(text)));
            else
            {
                if (lbl.Text == text)
                    return;
                lbl.Text = text;
            }
        }
    }
}
