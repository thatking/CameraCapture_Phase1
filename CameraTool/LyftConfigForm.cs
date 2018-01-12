using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace CameraTool
{
    public partial class LyftConfigForm : Form
    {
        int rGain = 0;
        int gGain = 0;
        int bGain = 0;
        int rOffset = 0;
        int gOffset = 0;
        int bOffset = 0;

        bool gainUpdated = false;
        bool offsetUpdated = false;

        public delegate void lyftCfgUpdateHandler(object sender, EventArgs e);
        public event lyftCfgUpdateHandler ConfigUpdated;

        public int RGain { get => rGain; set => rGain = value; }
        public int GGain { get => gGain; set => gGain = value; }
        public int BGain { get => bGain; set => bGain = value; }
        public bool GainUpdated { get => gainUpdated; set => gainUpdated = value; }
        public bool OffsetUpdated { get => offsetUpdated; set => offsetUpdated = value; }
        public int ROffset { get => rOffset; set => rOffset = value; }
        public int GOffset { get => gOffset; set => gOffset = value; }
        public int BOffset { get => bOffset; set => bOffset = value; }

        public LyftConfigForm()
        {
            InitializeComponent();
        }

       private void ResetGains()
        {
            rGain = 0;
            bGain = 0;
            gGain = 0;
            gainUpdated = false;

            // reset GUI box markers to start position
            RedGain.Value = 0;
            GreenGain.Value = 0;
            BlueGain.Value = 0;
        }

        private void ResetOffsets()
        {
            ROffset = 0;
            BOffset = 0;
            GOffset = 0;
            offsetUpdated = false;

            // reset GUI Box markers to start position
            RedOffset.Value = 0;
            GreenOffset.Value = 0;
            BlueOffset.Value = 0;
        }

        private void SendEvent()
        {
            if (ConfigUpdated != null)
            {
                ConfigUpdated(this, null);
            }
        }

        private void RedOffset_Scroll(object sender, ScrollEventArgs e)
        {
            ROffset = e.NewValue;
            offsetUpdated = true;
            SendEvent();
        }

        private void GreenOffset_Scroll(object sender, ScrollEventArgs e)
        {
            GOffset = e.NewValue;
            offsetUpdated = true;
            SendEvent();
        }

        private void BlueOffset_Scroll(object sender, ScrollEventArgs e)
        {
            BOffset = e.NewValue;
            offsetUpdated = true;
            SendEvent();
        }

        private void RedGain_Scroll(object sender, ScrollEventArgs e)
        {
            Debug.Print("Got Event: " + e.Type + " newval: " + e.NewValue + " scroll orient" + e.ScrollOrientation);

            this.AutoScroll = true;
            rGain = e.NewValue;
            gainUpdated = true;
            SendEvent();
        }

        private void GreenGain_Scroll(object sender, ScrollEventArgs e)
        {
            gGain = e.NewValue;
            gainUpdated = true;
            SendEvent();
        }

        private void BlueGain_Scroll(object sender, ScrollEventArgs e)
        {
            bGain = e.NewValue;
            gainUpdated = true;
            SendEvent();
        }

        private void Reset_Gain_Click(object sender, EventArgs e)
        {
            ResetGains();
        }

        private void Reset_Offset_Click(object sender, EventArgs e)
        {
            ResetOffsets();
        }
        
    }
}
