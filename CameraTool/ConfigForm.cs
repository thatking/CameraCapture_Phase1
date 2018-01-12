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
    public partial class ConfigForm : Form
    {
        private Configuration configXML;

        private int m_CameraDefaultMode;
        public int CameraDefaultMode
        {
            get { return m_CameraDefaultMode; }
        }

        private bool m_FocusProcDownSampling;
        public bool FocusProcDownSampling
        {
            get { return m_FocusProcDownSampling; }
        }
        
        private bool m_GammaEna;
        public bool GammaEna
        {
            get { return m_GammaEna; }
        }

        private double m_GammaValue;
        public double GammaValue
        {
            get { return m_GammaValue; }
        }

        private bool m_RGBGainOffsetEna;
        public bool RGBGainOffsetEna
        {
            get { return m_RGBGainOffsetEna; }
        }

        private int m_RGain, m_GGain, m_BGain, m_ROffset, m_GOffset, m_BOffset;
        public int RGain { get { return m_RGain; } }
        public int GGain { get { return m_GGain; } }
        public int BGain { get { return m_BGain; } }
        public int ROffset { get { return m_ROffset; } }
        public int GOffset { get { return m_GOffset; } }
        public int BOffset { get { return m_BOffset; } }

        private bool m_RGB2RGBMatrixEna;
        public bool RGB2RGBMatrixEna
        {
            get { return m_RGB2RGBMatrixEna; }
        }

        private int m_MatrixRR, m_MatrixRG, m_MatrixRB, m_MatrixGR, m_MatrixGG, m_MatrixGB, m_MatrixBR, m_MatrixBG, m_MatrixBB;
        public int MartixRR { get { return m_MatrixRR; } }
        public int MartixRG { get { return m_MatrixRG; } }
        public int MartixRB { get { return m_MatrixRB; } }
        public int MartixGR { get { return m_MatrixGR; } }
        public int MartixGG { get { return m_MatrixGG; } }
        public int MartixGB { get { return m_MatrixGB; } }
        public int MartixBR { get { return m_MatrixBR; } }
        public int MartixBG { get { return m_MatrixBG; } }
        public int MartixBB { get { return m_MatrixBB; } }

        private int m_CaptureNum;
        public int CaptureNum
        {
            get { return m_CaptureNum; }
        }

        private bool m_CaptureRAW;
        public bool CaptureRAW
        {
            get { return m_CaptureRAW; }
        }

        private bool m_CaptureBMP;

        private void ConfigForm_Load(object sender, EventArgs e)
        {

        }

        public bool CaptureBMP
        {
            get { return m_CaptureBMP; }
        }

        private string m_RegisterSetting;
        public string RegisterSetting
        {
            get { return m_RegisterSetting; }
        }

        public ConfigForm()
        {
            InitializeComponent();
            configXML = new Configuration();
            loadConfigFile();
            updateGUI();
        }

        // event for changes happening
        public delegate void configUpdatedHandler(object sender, EventArgs e);
        public event configUpdatedHandler ConfigUpdated;

        private void loadConfigFile()
        {
            try
            {
                m_CameraDefaultMode = (int)Convert.ToInt32(configXML.CameraDefaultMode);

                if (configXML.FocusProcDownSampling.Contains("YES"))
                    m_FocusProcDownSampling = true;
                else
                    m_FocusProcDownSampling = false;

                if (configXML.GammaEna.Contains("YES"))
                    m_GammaEna = true;
                else
                    m_GammaEna = false;

                m_GammaValue = Convert.ToDouble(configXML.GammaValue);

                if (configXML.RGBGainOffsetEna.Contains("YES"))
                    m_RGBGainOffsetEna = true;
                else
                    m_RGBGainOffsetEna = false;

                string strGainOffset = configXML.RGBGainOffset;

                string[] items = strGainOffset.Split(',');
                if (items.Length != 6)
                {
                    MessageBox.Show("RGBGainOffset item error");
                    return;
                }
                m_RGain = (int)(Convert.ToInt32(items[0]) );
                m_GGain = (int)(Convert.ToInt32(items[1]) );
                m_BGain = (int)(Convert.ToInt32(items[2]) );
                m_ROffset = (int)(Convert.ToInt32(items[3]) );
                m_GOffset = (int)(Convert.ToInt32(items[4]) );
                m_BOffset = (int)(Convert.ToInt32(items[5]) );


                if (configXML.RGB2RGBMatrixEna.Contains("YES"))
                    m_RGB2RGBMatrixEna = true;
                else
                    m_RGB2RGBMatrixEna = false;

                string strRGB2RGBMatrix = configXML.RGB2RGBMatrix;

                items = strRGB2RGBMatrix.Split(',');
                if (items.Length != 9)
                {
                    MessageBox.Show("RGB2RGBMatrix item error");
                    return;
                }
                m_MatrixRR = (int)(Convert.ToInt32(items[0]) );
                m_MatrixRG = (int)(Convert.ToInt32(items[1]) );
                m_MatrixRB = (int)(Convert.ToInt32(items[2]) );
                m_MatrixGR = (int)(Convert.ToInt32(items[3]) );
                m_MatrixGG = (int)(Convert.ToInt32(items[4]) );
                m_MatrixGB = (int)(Convert.ToInt32(items[5]) );
                m_MatrixBR = (int)(Convert.ToInt32(items[6]) );
                m_MatrixBG = (int)(Convert.ToInt32(items[7]) );
                m_MatrixBB = (int)(Convert.ToInt32(items[8]) );

                m_CaptureNum = (int)(Convert.ToInt32(configXML.CaptureNum));

                if (configXML.CaptureRAW.Contains("YES"))
                {
                    m_CaptureRAW = true;
                    checkBoxCaptureRAW.Checked = true;
                }
                else
                {
                    m_CaptureRAW = false;
                    checkBoxCaptureRAW.Checked = false;
                }

                if (configXML.CaptureBMP.Contains("YES"))
                {
                    m_CaptureBMP = true;
                    checkBoxCaptureBMP.Checked = true;
                }
                else
                {
                    m_CaptureBMP = false;
                    checkBoxCaptureBMP.Checked = false;
                }

                m_RegisterSetting = configXML.RegisterSetting;
            }
            catch (Exception ex)
            {
                MessageBox.Show("configuration file contains error: " + ex.ToString());
            }
        }

        private void updateGUI()
        {
            textBoxCameraDefaultMode.Text = m_CameraDefaultMode.ToString();
            checkBoxFocusProcDownSample.Checked = m_FocusProcDownSampling;

            checkBoxGammaEna.Checked = m_GammaEna;
            textBoxGammaValue.Text = m_GammaValue.ToString("F1");

            checkBoxRGBGainOffsetEna.Checked = m_RGBGainOffsetEna;
            textBoxRedGain.Text = (m_RGain ).ToString();
            textBoxGreenGain.Text = (m_GGain ).ToString();
            textBoxBlueGain.Text = (m_BGain ).ToString();
            textBoxRedOffset.Text = (m_ROffset ).ToString();
            textBoxGreenOffset.Text = (m_GOffset ).ToString();
            textBoxBlueOffset.Text = (m_BOffset ).ToString();

            checkBoxRGB2RGBMatrixEna.Checked = m_RGB2RGBMatrixEna;
            textBoxRRGain.Text = (m_MatrixRR ).ToString();
            textBoxRGGain.Text = (m_MatrixRG ).ToString();
            textBoxRBGain.Text = (m_MatrixRB ).ToString();
            textBoxGRGain.Text = (m_MatrixGR ).ToString();
            textBoxGGGain.Text = (m_MatrixGG ).ToString();
            textBoxGBGain.Text = (m_MatrixGB ).ToString();
            textBoxBRGain.Text = (m_MatrixBR ).ToString();
            textBoxBGGain.Text = (m_MatrixBG ).ToString();
            textBoxBBGain.Text = (m_MatrixBB ).ToString();

            textBoxCaptureNum.Text = m_CaptureNum.ToString();
            checkBoxCaptureBMP.Checked = m_CaptureBMP;
            checkBoxCaptureRAW.Checked = m_CaptureRAW;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                m_CameraDefaultMode = (int)Convert.ToInt32(textBoxCameraDefaultMode.Text);
                m_FocusProcDownSampling = checkBoxFocusProcDownSample.Checked;

                m_GammaEna = checkBoxGammaEna.Checked;
                m_GammaValue = Convert.ToDouble(textBoxGammaValue.Text);

                m_RGBGainOffsetEna = checkBoxRGBGainOffsetEna.Checked;
                m_RGain = (int)(Convert.ToInt32(textBoxRedGain.Text) );
                m_GGain = (int)(Convert.ToInt32(textBoxGreenGain.Text) );
                m_BGain = (int)(Convert.ToInt32(textBoxBlueGain.Text) );
                m_ROffset = (int)(Convert.ToInt32(textBoxRedOffset.Text) );
                m_GOffset = (int)(Convert.ToInt32(textBoxGreenOffset.Text) );
                m_BOffset = (int)(Convert.ToInt32(textBoxBlueOffset.Text) );

                m_MatrixRR = (int)(Convert.ToInt32(textBoxRRGain.Text) );
                m_MatrixRG = (int)(Convert.ToInt32(textBoxRGGain.Text) );
                m_MatrixRB = (int)(Convert.ToInt32(textBoxRBGain.Text) );
                m_MatrixGR = (int)(Convert.ToInt32(textBoxGRGain.Text) );
                m_MatrixGG = (int)(Convert.ToInt32(textBoxGGGain.Text) );
                m_MatrixGB = (int)(Convert.ToInt32(textBoxGBGain.Text) );
                m_MatrixBR = (int)(Convert.ToInt32(textBoxBRGain.Text) );
                m_MatrixBG = (int)(Convert.ToInt32(textBoxBGGain.Text) );
                m_MatrixBB = (int)(Convert.ToInt32(textBoxBBGain.Text) );

                m_CaptureNum = (int)(Convert.ToInt32(textBoxCaptureNum.Text));

                m_CaptureRAW = checkBoxCaptureRAW.Checked;
                m_CaptureBMP = checkBoxCaptureBMP.Checked;

                // invoke the event to let the app know that some changes happened
                if (ConfigUpdated != null)
                    ConfigUpdated(this, e);

            }
            catch (Exception ex)
            {
                MessageBox.Show("input has error: " + ex.ToString());
            }

            configXML.CameraDefaultMode = textBoxCameraDefaultMode.Text;

            configXML.FocusProcDownSampling = m_FocusProcDownSampling ? "YES" : "NO";

            configXML.GammaEna = m_GammaEna ? "YES" : "NO";
            configXML.GammaValue = textBoxGammaValue.Text;

            configXML.RGBGainOffsetEna = m_RGBGainOffsetEna ? "YES" : "NO";
            configXML.RGBGainOffset = textBoxRedGain.Text + ", " +
                textBoxGreenGain.Text + ", " +
                textBoxBlueGain.Text + ", " +
                textBoxRedOffset.Text + ", " +
                textBoxGreenOffset.Text + ", " +
                textBoxBlueOffset.Text;

            configXML.RGB2RGBMatrixEna = m_RGB2RGBMatrixEna ? "YES" : "NO";
            configXML.RGB2RGBMatrix = textBoxRRGain.Text + ", " +
                textBoxRGGain.Text + ", " +
                textBoxRBGain.Text + ", " +
                textBoxGRGain.Text + ", " +
                textBoxGGGain.Text + ", " +
                textBoxGBGain.Text + ", " +
                textBoxBRGain.Text + ", " +
                textBoxBGGain.Text + ", " +
                textBoxBBGain.Text;

            configXML.CaptureNum = textBoxCaptureNum.Text;
            configXML.CaptureRAW = m_CaptureRAW ? "YES" : "NO";
            configXML.CaptureBMP = m_CaptureBMP ? "YES" : "NO";

        }

        private void checkBoxGammaEna_CheckedChanged(object sender, EventArgs e)
        {
            m_GammaEna = checkBoxGammaEna.Checked;
        }

        private void checkBoxRGBGainOffsetEna_CheckedChanged(object sender, EventArgs e)
        {
            m_RGBGainOffsetEna = checkBoxRGBGainOffsetEna.Checked;
        }

        private void checkBoxRGB2RGBMatrixEna_CheckedChanged(object sender, EventArgs e)
        {
            m_RGB2RGBMatrixEna = checkBoxRGB2RGBMatrixEna.Checked;
        }
    }
}
