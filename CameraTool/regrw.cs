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
    public partial class RegRW_ModeSET : Form
    {
        public int WReg_Address = 0x0000;
        public int WReg_Value = 0x0000;
        public byte[] array_data = new byte[50];
        public int array_address = 0xf000;
        public int array_length = 15;

        public int RReg_Address = 0x0000;
        public int RReg_Value = 0x0000;

        public int sensormode = 0;
        public int roi_level = 0;
        public int ExpTime = 0;

        public bool Write_Triggered = false;
        public bool Read_Triggered = false;
        public bool Write_array_Triggered = false;
        public bool Read_array_Triggered = false;
        public bool ExpTime_Triggered = false;

        private int m_HexDec = 16; // true: Hex, false:Dec

        public int ROI_StartX
        {
            get
            {
                return m_ROI_StartX;
            }
            set
            {
                m_ROI_StartX = value;
                txtBoxStartX.Text = m_ROI_StartX.ToString();
            }
        }

        public int ROI_StartY
        {
            get
            {
                return m_ROI_StartY;
            }
            set
            {
                m_ROI_StartY = value;
                txtBoxStartY.Text = m_ROI_StartY.ToString();
            }
        }

        private int m_ROI_StartX = 12;
        private int m_ROI_StartY = 10;

        public RegRW_ModeSET()
        {
            InitializeComponent();
            SensorMode.SelectedIndex = 0;

            // Create the ToolTip and associate with the Form container.
            ToolTip toolTip1 = new ToolTip();

            // Set up the delays for the ToolTip.
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            // Force the ToolTip text to be displayed whether or not the form is active.
            toolTip1.ShowAlways = true;

            // Set up the ToolTip text for the Button and Checkbox.
            toolTip1.SetToolTip(this.btnFPGATriggerMode, "Trigger Mode \n\t[1:0] trigger_mode (default 1) \n\t [4:2] sensor parameter group update \n\t[6:5] fpga spi mode select");
            toolTip1.SetToolTip(this.btnFPGAExposureTime, "Exposure time for External trigger mode, in microsecond. 1-65535");
            toolTip1.SetToolTip(this.btnTriggerDelay, "Trigger Delay in microsecond. 1-65535");
            toolTip1.SetToolTip(this.btnFPGAFramePeriod, "Frame period in microsecond. 1-65535");

        }
        public void Update_TrackBarMinMax(int X_MAX,int X_MIN, int Y_MAX,int Y_MIN)
        {
            StartXtrackBar.Minimum = X_MIN;
            StartXtrackBar.Maximum = X_MAX;
            StartYtrackBar.Minimum = Y_MIN;
            StartYtrackBar.Maximum = Y_MAX;
        }
        public void Update_R_Value(int ret)
        {
            REG_Value.Text = GetHexDecString(RReg_Value);
        }

        private void Write_Click_1(object sender, EventArgs e)
        {
            WReg_Address = Convert.ToInt32(REG_Address.Text, m_HexDec);
            WReg_Value = Convert.ToInt32(REG_Value.Text, m_HexDec);
            Write_Triggered = true;
        }

        private void Read_Click_1(object sender, EventArgs e)
        {
            RReg_Address = Convert.ToInt32(REG_Address.Text, m_HexDec);
            RReg_Value = Convert.ToInt32(REG_Value.Text, m_HexDec);
            Read_Triggered = true;
        }

        private void R_Value_TextChanged(object sender, EventArgs e)
        {
            //R_Value.Text = CameraToolForm.
        }

        private void W_Address_TextChanged(object sender, EventArgs e)
        {

        }

        private void SensorMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            sensormode = SensorMode.SelectedIndex;
        }

        private void StartXtrackBar_Scroll(object sender, EventArgs e)
        {
            txtBoxStartX.Text = StartXtrackBar.Value.ToString();
        }

        private void StartYtrackBar_Scroll(object sender, EventArgs e)
        {
            txtBoxStartY.Text = StartYtrackBar.Value.ToString();
        }

        private void btnROI_ParameterOK_Click(object sender, EventArgs e)
        {
            int x, y;
            x = int.Parse(txtBoxStartX.Text);
            y = int.Parse(txtBoxStartY.Text);

            if (x < StartXtrackBar.Minimum)
                x = StartXtrackBar.Minimum;
            if (x > StartXtrackBar.Maximum)
                x = StartXtrackBar.Maximum;

            if (y < StartYtrackBar.Minimum)
                y = StartYtrackBar.Minimum;
            if (y > StartYtrackBar.Maximum)
                y = StartYtrackBar.Maximum;
//even
            if (x % 2 == 1)
            {
                    StartXtrackBar.Value = ROI_StartX = x - 1;
            }
            else
            {
                    StartXtrackBar.Value = ROI_StartX = x;
            }
//even
            if (y % 2 == 1)
            {
                    StartYtrackBar.Value = ROI_StartY = y - 1;
            }
            else
            {
                    StartYtrackBar.Value = ROI_StartY = y;
            }
            txtBoxStartX.Text = ROI_StartX.ToString();
            txtBoxStartY.Text = ROI_StartY.ToString();
        }

        private void ComboBox_ROI_Level_SelectedIndexChanged(object sender, EventArgs e)
        {
            roi_level = ComboBox_ROI_Level.SelectedIndex;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void btnPLL_Click(object sender, EventArgs e)
        {
            WReg_Address = 0xff53;
            RReg_Address = WReg_Address;
            REG_Address.Text = GetHexDecString(RReg_Address); 
            Read_Triggered = true;
        }

        private void btnGain_Click(object sender, EventArgs e)
        {
            WReg_Address = 0xff50;
            RReg_Address = WReg_Address;
            REG_Address.Text = GetHexDecString(RReg_Address);
            Read_Triggered = true;
        }

        private void btnExpL_Click(object sender, EventArgs e)
        {
            WReg_Address = 0xff2a;
            RReg_Address = WReg_Address;
            REG_Address.Text = GetHexDecString(RReg_Address);
            Read_Triggered = true;
        }

        private void btnExpH_Click(object sender, EventArgs e)
        {
            WReg_Address = 0xff2b;
            RReg_Address = WReg_Address;
            REG_Address.Text = GetHexDecString(RReg_Address);
            Read_Triggered = true;
        }

        private void btnBOffsetH_Click(object sender, EventArgs e)
        {
            WReg_Address = 0xff3c;
            RReg_Address = WReg_Address;
            REG_Address.Text = GetHexDecString(RReg_Address);
            Read_Triggered = true;
        }

        private void btnBOffsetL_Click(object sender, EventArgs e)
        {
            WReg_Address = 0xff3b;
            RReg_Address = WReg_Address;
            REG_Address.Text = GetHexDecString(RReg_Address);
            Read_Triggered = true;
        }

        private void btnOffsetH_Click(object sender, EventArgs e)
        {
            WReg_Address = 0xff62;
            RReg_Address = WReg_Address;
            REG_Address.Text = GetHexDecString(RReg_Address);
            Read_Triggered = true;
        }

        private void btnOffsetL_Click(object sender, EventArgs e)
        {
            WReg_Address = 0xff61;
            RReg_Address = WReg_Address;
            REG_Address.Text = GetHexDecString(RReg_Address);
            Read_Triggered = true;
        }

        private void btnExpMode_Click(object sender, EventArgs e)
        {
            WReg_Address = 0xff29;
            RReg_Address = WReg_Address;
            REG_Address.Text = GetHexDecString(RReg_Address);
            Read_Triggered = true;
        }

        private void btnFPGATriggerMode_Click(object sender, EventArgs e)
        {
            WReg_Address = 0x0000;
            RReg_Address = WReg_Address;
            REG_Address.Text = GetHexDecString(RReg_Address);
            Read_Triggered = true;
        }

        private void btnFPGAExposureTime_Click(object sender, EventArgs e)
        {
            WReg_Address = 0x0001;
            RReg_Address = WReg_Address;
            REG_Address.Text = GetHexDecString(RReg_Address);
            Read_Triggered = true;
        }

        private void btnTriggerDelay_Click(object sender, EventArgs e)
        {
            WReg_Address = 0x0002;
            RReg_Address = WReg_Address;
            REG_Address.Text = GetHexDecString(RReg_Address);
            Read_Triggered = true;
        }

        private void btnFPGAFramePeriod_Click(object sender, EventArgs e)
        {
            WReg_Address = 0x0003;
            RReg_Address = WReg_Address;
            REG_Address.Text = GetHexDecString(RReg_Address);
            Read_Triggered = true;
        }

        private void btnADCGain_Click(object sender, EventArgs e)
        {
            WReg_Address = 0xff64;
            RReg_Address = WReg_Address;
            REG_Address.Text = GetHexDecString(RReg_Address);
            Read_Triggered = true;
        }

        private void btnHexDec_Click(object sender, EventArgs e)
        {
            if (btnHexDec.Text == "HEX")
            {
                btnHexDec.Text = "DEC";
                m_HexDec = 10;
                REG_Address.Text = GetHexDecString(RReg_Address);
                REG_Value.Text = GetHexDecString(RReg_Value);
            }
            else
            {
                btnHexDec.Text = "HEX";
                m_HexDec = 16;
                REG_Address.Text = GetHexDecString(RReg_Address);
                REG_Value.Text = GetHexDecString(RReg_Value);
            }
        }

        private string GetHexDecString(int value)
        {
            if (m_HexDec == 10)
                return Convert.ToString(value, m_HexDec);
            else
                return "0x" + Convert.ToString(value, m_HexDec);
        }

        private bool IRSwitchOn = true;
        private void btnIRSwitch_Click(object sender, EventArgs e)
        {
            if (IRSwitchOn)
            {
                WReg_Address = 0xFF00;
                WReg_Value = 0x00;
                Write_Triggered = true;
                IRSwitchOn = false;
                btnIRSwitch.Text = "OFF";
            }
            else
            {
                WReg_Address = 0xFF00;
                WReg_Value = 0x01;
                Write_Triggered = true;
                IRSwitchOn = true;
                btnIRSwitch.Text = "ON";
            }
        }

        private void SerSet_Click(object sender, EventArgs e)
        {
            if (SerNum.Text.Length == 15)
            {
                string serialnum = SerNum.Text;
                byte[] array = System.Text.Encoding.ASCII.GetBytes(serialnum);

                //array_address = 0xf000;
                //array_length = array.Length;


                for (int i = 0; i < array.Length; i++)
                {
                    byte asciicode = (array[i]);
                    array_data[i] = asciicode;

                }
                MessageBox.Show("finishing");


                Write_array_Triggered = true;

                System.Threading.Thread.Sleep(1000);
            }
            else {
                MessageBox.Show("请输入15位数字");
            }
        }

        private void SerGet_Click(object sender, EventArgs e)
        {
            SerNum.Text = "";
            Read_array_Triggered = true;
            System.Threading.Thread.Sleep(1000);

            ASCIIEncoding ASCIITochar = new ASCIIEncoding();
            char[] ascii = ASCIITochar.GetChars((byte[])array_data);

            for (int i = 0; i < array_length; i++)
                SerNum.Text += ascii[i];

        }
    }
}
