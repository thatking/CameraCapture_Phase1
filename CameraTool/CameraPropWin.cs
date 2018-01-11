/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  
*****************************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CameraTool
{
    public partial class CameraPropWin : Form
    {
        public CameraProperty Gain, Exposure;
        public bool AE;

        // exposure time, measured in Lines
        public int ExpTime = 0;

        public CameraPropWin()
        {
            InitializeComponent();
            trackBarGain.Scroll += new System.EventHandler(this.trackBarGain_Scroll);
            trackBarExposure.Scroll += new System.EventHandler(this.trackBarExposure_Scroll);
            Gain = new CameraProperty();
            Exposure = new CameraProperty();
        }

        public void UpdateValue(CameraProperty gain, bool AEMode, CameraProperty exposure, int exposureTime)
        {
            Gain = gain;
            trackBarGain.Minimum = Gain.Min;
            trackBarGain.Maximum = Gain.Max;

            textBoxGain.Text = Gain.curValue.ToString();
            trackBarGain.Value = Gain.curValue;

            AE = AEMode;
            if (AE)
                checkBoxAE.Checked = true;
            else
                checkBoxAE.Checked = false;

            Exposure = exposure;
            trackBarExposure.Minimum = Exposure.Min;
            trackBarExposure.Maximum = Exposure.Max;
            trackBarExposure.Value = Exposure.curValue;

            textBoxExposure.Text = trackBarExposure.Value.ToString();

            ExpTime = exposureTime;
            txtBoxExpTime.Text = ExpTime.ToString();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void trackBarGain_Scroll(object sender, System.EventArgs e)
        {
            textBoxGain.Text = trackBarGain.Value.ToString();
            Gain.curValue = trackBarGain.Value;
        }

        private void trackBarExposure_Scroll(object sender, System.EventArgs e)
        {
            textBoxExposure.Text = trackBarExposure.Value.ToString();
            Exposure.curValue = trackBarExposure.Value;
        }

        private void textBoxGain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter )
            {
                try
                {
                    trackBarGain.Value = Convert.ToInt32(textBoxGain.Text, 10);
                    Gain.curValue = trackBarGain.Value;
                }
                catch
                {
                    textBoxGain.Text = trackBarGain.Value.ToString();
                }
            }
        }

        private void checkBoxAE_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAE.Checked)
            {
                AE = true;
                textBoxExposure.Enabled = false;
                textBoxGain.Enabled = false;
                trackBarExposure.Enabled = false;
                trackBarGain.Enabled = false;
                buttonSet.Enabled = false;
                txtBoxExpTime.Enabled = false;
            }
            else
            {
                AE = false;
                textBoxExposure.Enabled = true;
                textBoxGain.Enabled = true;
                trackBarExposure.Enabled = true;
                trackBarGain.Enabled = true;
                buttonSet.Enabled = true;
                txtBoxExpTime.Enabled = true;
            }
        }

        private void textBoxExposure_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    trackBarExposure.Value = Convert.ToInt32(textBoxExposure.Text, 10);
                    Exposure.curValue = trackBarExposure.Value;
                }
                catch
                {
                    textBoxExposure.Text = trackBarExposure.Value.ToString();
                }
            }
        }

        private void buttonSet_Click(object sender, EventArgs e)
        {
            try
            {
                ExpTime = Convert.ToInt32(txtBoxExpTime.Text, 10);
            }
            catch
            {
            }
        }

    }

    public class CameraProperty
    {
        public int curValue;
        public int Min;
        public int Max;
        public int Step;
        public int Default;
    }

}
