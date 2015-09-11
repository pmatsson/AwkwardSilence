using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoAwkwardSilence
{
    class CustomTabControl : TabControl
    {
        private const int TCM_ADJUSTRECT = 0x1328;

        protected override void WndProc(ref Message m)
        {
            // Hide the tab headers at run-time
            if (m.Msg == TCM_ADJUSTRECT && !DesignMode)
            {
                m.Result = (IntPtr)1;
                return;
            }

            // call the base class implementation
            base.WndProc(ref m);
        }
    }
}
