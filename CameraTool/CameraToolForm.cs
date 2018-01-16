/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  
*****************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Diagnostics;

using LeopardCamera;
using PluginInterface;

using EmguTool;

namespace CameraTool
{
    public partial class CameraToolForm : Form
    {
        internal LPCamera capture = null;
        internal VideoCapture vLog = null;
        internal LyftImageStream imLog = null;

        private enum CameraType { NO_CAMERA, CYPRESS_USB_BOOT, LEOPARD_CAMERA };
        private CameraType cameraList;

        // M034 modes for 720p
        private enum M034SensorMode { M720P_30_HDR_DLO = 0x06, M720P_30_HDR_MC, M720P_55_HDR_MC, M720P_55_HDR_DLO, M720P_30_SDR, M720P_60_SDR };
        private enum PIXEL_ORDER { GBRG = 0, GRBG, RGBG, BGGR };

        private string CameraUUID, FuseID;
        private UInt16 HwRev, FwRev;
        private UInt16 ROIX_MAX, ROIX_MIN, ROIY_MAX, ROIY_MIN;
        private bool MarkEn = false;

        private LeopardCamera.LPCamera.SENSOR_DATA_MODE m_SensorDataMode = LeopardCamera.LPCamera.SENSOR_DATA_MODE.YUV;

        private int m_CameraIndex = 0, m_ResolutionIndex = 0, m_FrameRateIndex = 0;

        private bool mRAWDisplay = true;
        private bool m_TriggerMode = false;
        private bool m_CaptureImages = false;
        private bool m_AutoExposure = false;
        private bool m_MonoSensor = false;
        private bool m_AutoTrigger = false;
        private int m_AutoTriggerCnt = 0, m_AutoTriggerPrevCnt = 0;
        private PIXEL_ORDER m_pixelOrder;
        private bool m_NoiseCalculationEna = false;

        private double m_ImageMean, m_RectMean, m_RectSTD, m_RectTN, m_RectFPN; // STD is total noise, TN is Temporal Noise, FPN is Fixed Pattern Noise
        private double m_PrevImageMean;
        private double dTargetMean;
        private double dTargetMeanFactor = 1.2;
        private int m_curExpTimeInLines=500; // exposure time, measured in Lines
        private int m_curExp=0;
        private int m_curGain=8;
        private int m_curExpXGain;
        private bool m_AE_done = false;
        private int m_PrevFrameCnt = 0;
        private bool m_Show_Anchors = false;

        private bool m_RW_REG_ModeSET = false;
        private int W_address = 0, W_value = 0;
        private int R_address = 0;
        private int ROI_StartX = 0;
        private int ROI_StartY = 0;

        public int R_value = 0;

        private int SensorMode = 0;
        private int Roi_Level = 0;

        private RegRW_ModeSET frmRegRW_MODESET;
        private ConfigForm configurationForm;
        private LyftConfigForm lyftConfigForm;

        // for Register Setting
        private string m_RegisterSetting;

        // for Camera
        private int m_CameraDefaultMode = 0;
        private bool m_FocusProcDownSample = true;

        // for RAW interpolation
        private bool m_GammaEna;
        private double m_GammaValue;
        private bool m_RGBGainOffsetEna;
        private int r_gain, g_gain, b_gain, r_offset, g_offset, b_offset;
        private bool m_RGB2RGBMatrixEna;
        private int matrix_rr, matrix_rg, matrix_rb, matrix_gr, matrix_gg, matrix_gb, matrix_br, matrix_bg, matrix_bb;

        private uint m_delayTime = 0;
        private SetTriggerDelay frmSetTriggerDelay;

        private CameraPropWin frmCameraPropWin;
        private bool m_curAE = false;

        private bool FrameDisconntinued = false;
        private bool FlashUpdateInProgress = false;

        private int flashUpdatePercentage = 0;

        private string captureFullFileName=null;

        // emguCV demo
        private EmguTool.EmguDemo.EmguDemoId m_EmguDemoId = EmguTool.EmguDemo.EmguDemoId.DisableDemo; 

        private LeopardPlugin m_selectedPlugin;
        private ICollection<LeopardPlugin> m_plugins;

        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        public CameraToolForm()
        {
            InitializeComponent();

            AddPluginMenu();

            frmRegRW_MODESET = new RegRW_ModeSET();
            configurationForm = new ConfigForm();
            lyftConfigForm = new LyftConfigForm();
            frmSetTriggerDelay = new SetTriggerDelay();
            frmCameraPropWin = new CameraPropWin();
            frmCameraPropWin.Gain.curValue = m_curGain;
            frmCameraPropWin.AE = m_curAE;
            frmCameraPropWin.Exposure.curValue = m_curExp;
            frmCameraPropWin.ExpTime = m_curExpTimeInLines;

            W_address = frmRegRW_MODESET.WReg_Address;
            W_value = frmRegRW_MODESET.WReg_Value;
            R_address = frmRegRW_MODESET.RReg_Address;
            R_value = frmRegRW_MODESET.RReg_Value;

            SensorMode = frmRegRW_MODESET.sensormode;

            timer.Tick += new EventHandler(timer_Tick); // Everytime timer ticks, timer_Tick will be called
            timer.Interval = (200);              
            timer.Enabled = true;                       // Enable the timer
            timer.Start();                              // Start the timer

            startTime = DateTime.Now;

            if (!File.Exists(i2cFileName))
            {
                createI2CFile();
            }

            // update once
            updateParamFromConfiguration(this, null);

            // add event for configure update on GUI
            configurationForm.ConfigUpdated += new ConfigForm.configUpdatedHandler(updateParamFromConfiguration);

            // add event handler for lyft cfg updates
            lyftConfigForm.ConfigUpdated += new LyftConfigForm.lyftCfgUpdateHandler(UpdateParamFromLyftConfig);

            // load default values from xml file.
            m_CameraDefaultMode = configurationForm.CameraDefaultMode;
            m_ResolutionIndex = m_CameraDefaultMode;

            DetectCamera();
        }

        private void AddPluginMenu()
        {

            m_plugins = LoadPlugins("Plugins");

            try
            {
                foreach (var item in m_plugins)
                {
                    ToolStripMenuItem NEW;
                    NEW = new ToolStripMenuItem(item.Name);
                    NEW.Text = item.Name;
                    NEW.Click += new EventHandler(pluginsStripMenuItemClick);
                    pluginsToolStripMenuItem.DropDown.Items.Add(NEW);
                }
            }
            catch
            {
            }
        }

        private void pluginsStripMenuItemClick(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem)  //Check On Click.
            {
                m_selectedPlugin = null;
                foreach (var plugin in m_plugins)
                {
                    if (plugin.Name == sender.ToString())
                    {
                        m_selectedPlugin = plugin;
                        m_selectedPlugin.SetCameraId(capture.cameraModel);
                        m_selectedPlugin.Initialize();

                        if (m_SensorDataMode != LPCamera.SENSOR_DATA_MODE.YUV
                            && m_SensorDataMode != LPCamera.SENSOR_DATA_MODE.YUV_DUAL)
                            m_AutoExposure = true; // enable auto exposure

                       // if (capture.cameraModel == LPCamera.CameraModel.IMX172)
                        {
                            mRAWDisplay = false;
                            ToolStripMenuItem item = (ToolStripMenuItem)noDisplayToolStripMenuItem;
                            item.Checked = true;
                        }
                        break;
                    }
                }

            }
        }

        private void PluginProcess(IntPtr pBuffer, int width, int height, int bpp,
            LeopardCamera.LPCamera.SENSOR_DATA_MODE sensorMode, bool monoSensor, int pixelOrder, int exposureTime, string cameraID, bool GammaEna, double gamma,
                                                 bool RGBGainEna, int r_gain, int g_gain, int b_gain, int r_offset, int g_offset, int b_offset,
                                                 bool RGB2RGBEna, int rr, int rg, int rb, int gr, int gg, int gb, int br, int bg, int bb)
        {
            if (m_selectedPlugin != null)
            {
                m_selectedPlugin.Process(pBuffer, width, height, bpp, sensorMode, monoSensor, pixelOrder, exposureTime, cameraID, GammaEna, gamma,
                                                 RGBGainEna, r_gain, g_gain, b_gain, r_offset, g_offset, b_offset,
                                                 RGB2RGBEna, rr, rg, rb, gr, gg, gb, br, bg, bb);
            }
        }

        private int preFrameCount = 0;
        private DateTime startTime, endTime;
        private DateTime triggerStartTime, triggerEndTime;
        void timer_Tick(object sender, EventArgs e)
        {
            if (capture != null)
            {
                endTime = DateTime.Now;
                TimeSpan ts = endTime - startTime;

                if (ts.TotalSeconds > 2)
                {
                    int fps = capture.FrameCount - preFrameCount;
                    preFrameCount = capture.FrameCount;

                    if (fps >= 0)
                        toolStripStatusLabelFPS.Text = ((double)fps * 1000 / (ts.Seconds * 1000 + ts.Milliseconds)).ToString("F1") + " fps";

                    startTime = DateTime.Now;
                }

                if (!m_SaveFileInProcess)
                    SaveCapturedImage(captureFullFileName);

                if (m_AutoExposure)
                {
                    if (capture.FrameCount - m_PrevFrameCnt > 3)
                    {
                        doAE();
                        m_PrevFrameCnt = capture.FrameCount;

                        statusToolStripStatusLabel.Text = "Mean: " + m_ImageMean.ToString("F1")
                                + "  exp: " + m_curExpTimeInLines.ToString()
                                + "  gain: " + m_curGain.ToString()
                                + "  AE doen:  " + m_AE_done.ToString();
                    }
                }
                else
                {
                    if (capture.FrameCount != m_PrevFrameCnt)
                        statusToolStripStatusLabel.Text = "Frame Count: " + capture.FrameCount.ToString();
                    m_PrevFrameCnt = capture.FrameCount;

                    if (FrameDisconntinued)
                        statusToolStripStatusLabel.Text += "FrmCnt disconnectinued";
                }

                // update the progress
                if (FlashUpdateInProgress)
                {
                    statusToolStripStatusLabel.Text = " Flash update at " + flashUpdatePercentage.ToString() + "%";
                }

                if (m_AutoTrigger)
                {
                    // captured one frame
                    if (m_AutoTriggerPrevCnt != m_AutoTriggerCnt)
                    {
                        TimeSpan tsAutoTrigger = triggerEndTime - triggerStartTime;
                        statusToolStripStatusLabel.Text +=
                            "  Capture Latency (including exposure time): " + (tsAutoTrigger.Seconds * 1000 + tsAutoTrigger.Milliseconds).ToString() + " ms";

                        m_AutoTriggerPrevCnt = m_AutoTriggerCnt;

                        capture.SoftTrigger();
                        triggerStartTime = DateTime.Now;
                    }
                }
                if (m_NoiseCalculationEna)
                {
                    toolStripStatusLabelMean.Text = m_RectMean.ToString("F1");
                    toolStripStatusLabelSTD.Text = m_RectSTD.ToString("F1");
                }
                else
                {
                    toolStripStatusLabelMean.Text = "-";
                    toolStripStatusLabelSTD.Text = "-";
                }
                toolStripStatusLabelTN.Text = "-";// m_RectTN.ToString("F1");
                //toolStripStatusLabelFPN.Text = "-";// m_RectFPN.ToString("F1");

                handleTriggerDelayTime();
                handleRegRW_MODESET();
                handleRegRW_aray();
                //handleCameraPropWin();

                parsePluginParam();
            }
            else
                toolStripStatusLabelFPS.Text = "0.0";
        }

        private void doAE()
        {
            if (m_AutoExposure && capture != null)
            {

                int maxExposureTime = capture.Height * 4;
                int minExposureTime = 50;
                int minGain = 8;
                int maxGain = 63;

                switch (capture.cameraModel)
                {
                    case LPCamera.CameraModel.ICX285:
                        minGain = 1;
                        maxGain = 8;
                        minExposureTime = 1;
                        break;
                    case LPCamera.CameraModel.IMX172:
                        minGain = 8;
                        maxGain = 48;
                        minExposureTime = 10;
                        maxExposureTime = 2990;
                        break;
                }

                // AE done
                if ((m_ImageMean > dTargetMean * 0.8 && m_ImageMean < dTargetMean * 1.2)
                    || ((m_curExpXGain >= maxExposureTime * maxGain) && (m_ImageMean < dTargetMean * 0.8))
                    || ((m_curExpXGain <= minExposureTime * minGain) && (m_ImageMean > dTargetMean * 1.2)))
                {
                    m_AE_done = true;
                    return;
                }

                m_AE_done = false;
                int calExpXGain = (int)(m_curExpXGain * (dTargetMean / m_ImageMean));
                m_curExpXGain = (m_curExpXGain + calExpXGain) / 2;

                m_PrevImageMean = m_ImageMean;

                if (m_curExpXGain >= maxExposureTime * minGain)
                {
                    m_curExpTimeInLines = maxExposureTime;
                    m_curGain = (int)(m_curGain * (dTargetMean / m_ImageMean));
                    if (m_curGain > maxGain)
                    {
                        m_curGain = maxGain;
                        //m_AE_done = true;
                    }
                    else if (m_curGain < minGain)
                    {
                        m_curGain = minGain;
                        //m_AE_done = false;
                    }
                }
                else if (m_curExpXGain < minExposureTime * minGain)
                {
                    m_curExpTimeInLines = minExposureTime;
                    m_curGain = minGain;
                    //m_AE_done = true;
                }
                else
                {
                    m_curGain = minGain;
                    m_curExpTimeInLines = (int)(m_curExpTimeInLines * (dTargetMean / m_ImageMean));
                    //m_AE_done = false;
                }

                m_curExpXGain = m_curExpTimeInLines * m_curGain;
                capture.ExposureExt = m_curExpTimeInLines;
                capture.Gain = m_curGain;
                frmRegRW_MODESET.ExpTime = m_curExpTimeInLines;
            }

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void updateDeviceInfo()
        {
            if (capture != null)
            {
                uUIDToolStripMenuItem.Text = "UUID: " + CameraUUID.ToString();
                firmwareRevToolStripMenuItem.Text = "Firmware Rev: " + FwRev.ToString();
                hardwareRevToolStripMenuItem.Text = "Hardware Rev: " + (HwRev & 0x0FFF).ToString("X4");
                dataFormatToolStripMenuItem.Text = "Data Format: " + m_SensorDataMode.ToString();
                devInfoToolStripMenuItem.Enabled = true;

                toolStripStatusLabelDevice.Text = capture.cameraList[m_CameraIndex].Name.ToString();
                toolStripStatusLabelHWRev.Text = (HwRev & 0x0FFF).ToString("X4");
                toolStripStatusLabelFWRev.Text = FwRev.ToString();
                toolStripStatusLabelRes.Text = capture.ResList[m_ResolutionIndex, 0].ToString() 
                                                + "X" + capture.ResList[m_ResolutionIndex, 1].ToString();
            }
            else
            {
                toolStripStatusLabelDevice.Text = "None";
                toolStripStatusLabelHWRev.Text = "0";
                toolStripStatusLabelFWRev.Text = "0";
                toolStripStatusLabelRes.Text = "";
                devInfoToolStripMenuItem.Enabled = false;
            }
        }

        private void updateDeviceResolution()
        {
            resolutionToolStripMenuItem.DropDown.Items.Clear();

            if (capture != null)
            {
                for (int i = 0; i < capture.ResCount; i++)
                {
                    ToolStripMenuItem NEW;
                    NEW = new ToolStripMenuItem(capture.ResList[i, 0].ToString() + "X" + capture.ResList[i, 1].ToString());
                    NEW.Text = capture.ResList[i, 0].ToString() + "X" + capture.ResList[i, 1].ToString();
                    NEW.Click += new EventHandler(resolutionStripMenuItemClick);
                    NEW.CheckOnClick = true;
                    resolutionToolStripMenuItem.DropDown.Items.Add(NEW);
                }

                // check the one that is being used
                ToolStripMenuItem item = (ToolStripMenuItem)resolutionToolStripMenuItem.DropDownItems[m_ResolutionIndex];
                item.Checked = true;
            }
        }
        private void updateDeviceFrameRate()
        {
            framerateToolStripMenuItem.DropDown.Items.Clear();

            if (capture != null)
            {
                for (int i = 0; i < capture.FrameRateCNT; i++)
                {
                    ToolStripMenuItem NEW;
                    long tmp = capture.FrameRateList[m_ResolutionIndex, i];
                    long a = 10000000 / tmp;
                    NEW = new ToolStripMenuItem(a.ToString() + "fps");
                    NEW.Text = a.ToString() + "fps";
                    NEW.Click += new EventHandler(framerateToolStripMenuItemClick);
                    NEW.CheckOnClick = true;
                    framerateToolStripMenuItem.DropDown.Items.Add(NEW);
                }

                // check the one that is being used
                ToolStripMenuItem item = (ToolStripMenuItem)framerateToolStripMenuItem.DropDownItems[m_FrameRateIndex];
                item.Checked = true;
            }
        }
        private void setupMenuAndInit(int width, int height)
        {
            ToolStripMenuItem item;

            if (capture != null)
            {
                if (capture.cameraModel == LPCamera.CameraModel.V034
                        || capture.cameraModel == LPCamera.CameraModel.M031
                        || capture.cameraModel == LPCamera.CameraModel.MT9P031
                        || capture.cameraModel == LPCamera.CameraModel.AR0330
                        || capture.cameraModel == LPCamera.CameraModel.Stereo
                        || capture.cameraModel == LPCamera.CameraModel.C570
                        || capture.cameraModel == LPCamera.CameraModel.C661
                        || capture.cameraModel == LPCamera.CameraModel.ICX285
                        || capture.cameraModel == LPCamera.CameraModel.AR1820 
                        || capture.cameraModel == LPCamera.CameraModel.IMX22x
                        || capture.cameraModel == LPCamera.CameraModel.ov10640
                        || capture.cameraModel == LPCamera.CameraModel.OV8865
                        || capture.cameraModel == LPCamera.CameraModel.OV13850
                        || capture.cameraModel == LPCamera.CameraModel.CMV300
                        || capture.cameraModel == LPCamera.CameraModel.OV7251
                        || capture.cameraModel == LPCamera.CameraModel.IMX226
                        || capture.cameraModel == LPCamera.CameraModel.OV10823
                        || capture.cameraModel == LPCamera.CameraModel.IMX172
                        || capture.cameraModel == LPCamera.CameraModel.MLX75411
						|| capture.cameraModel == LPCamera.CameraModel.KEURIG_SPI
                        || capture.cameraModel == LPCamera.CameraModel.IMX230
                        || capture.cameraModel == LPCamera.CameraModel.IMX290
                        || capture.cameraModel == LPCamera.CameraModel.IMX185
                        || capture.cameraModel == LPCamera.CameraModel.OV2742
                        || capture.cameraModel == LPCamera.CameraModel.PYTHON1300
                        || capture.cameraModel == LPCamera.CameraModel.IMX274
                        || capture.cameraModel == LPCamera.CameraModel.ETRON3D
                        || capture.cameraModel == LPCamera.CameraModel.IMX178
                        || capture.cameraModel == LPCamera.CameraModel.RAA462113
                        || capture.cameraModel == LPCamera.CameraModel.IMX298
                        || capture.cameraModel == LPCamera.CameraModel.OV9712
                        || capture.cameraModel == LPCamera.CameraModel.IMX377
                        || capture.cameraModel == LPCamera.CameraModel.AR0231
                        || capture.cameraModel == LPCamera.CameraModel.AR0144
                        || capture.cameraModel == LPCamera.CameraModel.IMX390)

                {
                    capture.SetParam(width, height, false, IntPtr.Zero);

                    captureImageToolStripMenuItem.Enabled = true;
                    triggerModeToolStripMenuItem.Enabled = true;
                    noDisplayToolStripMenuItem.Enabled = true;
                    softTriggerToolStripMenuItem.Enabled = false;
                    autoTriggerToolStripMenuItem.Enabled = false;
                    pixelOrderToolStripMenuItem.Enabled = true;
                    

                    // set initial exposure time to 500 lines
                    m_curExpTimeInLines = 500;
                    m_curGain = 8;

                    switch (capture.cameraModel)
                    {
                        case LPCamera.CameraModel.AR1820:
                            m_MonoSensor = false;
                            m_pixelOrder = PIXEL_ORDER.GBRG;

                            item = (ToolStripMenuItem)bGGRToolStripMenuItem;
                            item.Checked = true;
                            break;
                        case LPCamera.CameraModel.V034:
                            m_MonoSensor = true;
                            m_curExpTimeInLines = 100;
                            break;
                        case LPCamera.CameraModel.M031:
                            m_MonoSensor = false;
                            m_pixelOrder = PIXEL_ORDER.GBRG;

                            item = (ToolStripMenuItem)gBRGToolStripMenuItem;
                            item.Checked = true;
                            break;
                        case LPCamera.CameraModel.AR0144:
                            m_MonoSensor = false;
                            m_pixelOrder = PIXEL_ORDER.GBRG;

                            item = (ToolStripMenuItem)gBRGToolStripMenuItem;
                            item.Checked = true;
                            break;
                        case LPCamera.CameraModel.MLX75411:
                            m_MonoSensor = false;
                            m_pixelOrder = PIXEL_ORDER.BGGR;

                            item = (ToolStripMenuItem)gBRGToolStripMenuItem;
                            item.Checked = true;
                            break;
                        case LPCamera.CameraModel.MT9P031:
                            m_MonoSensor = false;
                            m_pixelOrder = PIXEL_ORDER.BGGR;

                            item = (ToolStripMenuItem)bGGRToolStripMenuItem;
                            item.Checked = true;
                            break;
                        case LPCamera.CameraModel.AR0330:
                            m_MonoSensor = true;
                            break;
                        case LPCamera.CameraModel.Stereo:
                            m_MonoSensor = true;
                            break;
                        case LPCamera.CameraModel.C570:
                            m_MonoSensor = true;
                            break;
                        case LPCamera.CameraModel.PYTHON1300:
                            m_MonoSensor = false;
                            m_pixelOrder = PIXEL_ORDER.GBRG;
                            item = (ToolStripMenuItem)gBRGToolStripMenuItem;
                            item.Checked = true;
                            m_curExpTimeInLines = 1000;
                            m_curGain = 32;
                            break;
                        case LPCamera.CameraModel.IMX274:
                            m_MonoSensor = false;
                            m_pixelOrder = PIXEL_ORDER.GRBG;
                            item = (ToolStripMenuItem)gRBGToolStripMenuItem1;
                            item.Checked = true;
                            m_curExpTimeInLines = 500;
                            m_curGain = 16;
                            break;
                        case LPCamera.CameraModel.CMV300:
                            m_MonoSensor = true;
                            break;
                        case LPCamera.CameraModel.C661:
                            m_curExpTimeInLines = 32;
                            m_MonoSensor = true;
                            break;	
                        case LPCamera.CameraModel.ICX285:
                            m_MonoSensor = true;
                            m_curExpTimeInLines = 500;
                            m_curGain = 1;
                            break;
                        case LPCamera.CameraModel.IMX22x:                        
                            m_MonoSensor = false;
                            m_pixelOrder = PIXEL_ORDER.GRBG;
                            break;
                        case LPCamera.CameraModel.ov10640:
                            m_MonoSensor = false;
                            m_pixelOrder = PIXEL_ORDER.RGBG;
                            item = (ToolStripMenuItem)rGBGToolStripMenuItem;
                            item.Checked = true;
                            break;
                        case LPCamera.CameraModel.OV8865:
                            m_MonoSensor = false;
                            m_pixelOrder = PIXEL_ORDER.RGBG;
                            item = (ToolStripMenuItem)rGBGToolStripMenuItem;
                            item.Checked = true;
                            break;
                        case LPCamera.CameraModel.OV13850:
                            m_MonoSensor = false;
                            m_pixelOrder = PIXEL_ORDER.RGBG;
                            item = (ToolStripMenuItem)rGBGToolStripMenuItem;
                            item.Checked = true;
                            break;
                        case LPCamera.CameraModel.OV7251:
                            m_MonoSensor = false;
                            m_pixelOrder = PIXEL_ORDER.RGBG;
                            item = (ToolStripMenuItem)rGBGToolStripMenuItem;
                            item.Checked = true;
                            break;
                        case LPCamera.CameraModel.IMX226:
                            m_MonoSensor = false;
                            m_pixelOrder = PIXEL_ORDER.RGBG;
                            item = (ToolStripMenuItem)rGBGToolStripMenuItem;
                            item.Checked = true;
                            break;
                        case LPCamera.CameraModel.OV10823:
                            m_MonoSensor = false;
                            m_pixelOrder = PIXEL_ORDER.RGBG;
                            item = (ToolStripMenuItem)rGBGToolStripMenuItem;
                            item.Checked = true;

                            m_curExpTimeInLines = 5000;
                            m_curGain = 32;
                            break;
                        case LPCamera.CameraModel.IMX230:
                            m_MonoSensor = false;
                            m_pixelOrder = PIXEL_ORDER.BGGR;
                            item = (ToolStripMenuItem)rGBGToolStripMenuItem;
                            item.Checked = true;

                            m_curExpTimeInLines = 3000;
                            m_curGain = 32;
                            break;
                        case LPCamera.CameraModel.IMX298:
                            m_MonoSensor = false;
                            m_pixelOrder = PIXEL_ORDER.BGGR;
                            item = (ToolStripMenuItem)rGBGToolStripMenuItem;
                            item.Checked = true;

                            m_curExpTimeInLines = 3000;
                            m_curGain = 32;
                            break;
                        case LPCamera.CameraModel.IMX290:
                            m_MonoSensor = false;
                            m_pixelOrder = PIXEL_ORDER.GRBG;                          

                            m_curExpTimeInLines = 1000;
                            m_curGain = 32;
                            break;
                        case LPCamera.CameraModel.IMX185:
                            m_MonoSensor = false;
                            m_pixelOrder = PIXEL_ORDER.GRBG;

                            m_curExpTimeInLines = 1000;
                            m_curGain = 32;
                            break;
                        case LPCamera.CameraModel.IMX178:
                            m_MonoSensor = false;
                            m_pixelOrder = PIXEL_ORDER.BGGR;
                            item = (ToolStripMenuItem)bGGRToolStripMenuItem;
                            item.Checked = true;

                            m_curExpTimeInLines = 1000;
                            m_curGain = 32;
                            break;
                        case LPCamera.CameraModel.IMX377:
                            m_MonoSensor = false;
                            m_pixelOrder = PIXEL_ORDER.RGBG;
                            item = (ToolStripMenuItem)bGGRToolStripMenuItem;
                            item.Checked = true;

                            m_curExpTimeInLines = 1000;
                            m_curGain = 32;
                            break;
                        case LPCamera.CameraModel.OV2742:
                            m_MonoSensor = false;
                            m_pixelOrder = PIXEL_ORDER.RGBG;
                            item = (ToolStripMenuItem)rGBGToolStripMenuItem;
                            item.Checked = true;
                            break;
                        case LPCamera.CameraModel.RAA462113:
                            m_MonoSensor = false;
                            m_pixelOrder = PIXEL_ORDER.RGBG;
                            item = (ToolStripMenuItem)rGBGToolStripMenuItem;
                            item.Checked = true;
                            break;
                        case LPCamera.CameraModel.AR0231:
                            m_MonoSensor = false;
                            m_pixelOrder = PIXEL_ORDER.GBRG;
                            item = (ToolStripMenuItem)rGBGToolStripMenuItem;
                            item.Checked = true;
                            break;
                        case LPCamera.CameraModel.IMX172:
                            m_MonoSensor = false;
                            m_pixelOrder = PIXEL_ORDER.GRBG;
                            item = (ToolStripMenuItem)rGBGToolStripMenuItem;
                            item.Checked = true;

                            m_curExpTimeInLines = 2000;
                            m_curGain = 16;

                            frmRegRW_MODESET.ROI_StartX = 66;
                            frmRegRW_MODESET.ROI_StartY = 10;
                            frmRegRW_MODESET.Show(); // pop up reg window to adjust X,Y offset

                            // put the window to up right corner
                            int xLocation = Screen.FromControl(this).Bounds.Width - frmRegRW_MODESET.Width;
                            frmRegRW_MODESET.Location = new Point(xLocation, 0);
                            break;
                        case LPCamera.CameraModel.OV9712:
                            m_MonoSensor = false;
                            m_pixelOrder = PIXEL_ORDER.BGGR;
                            item = (ToolStripMenuItem)rGBGToolStripMenuItem;
                            item.Checked = true;
                            break;
                        case LPCamera.CameraModel.IMX390:
                            m_MonoSensor = false;
                            m_pixelOrder = PIXEL_ORDER.GBRG;
                            item = (ToolStripMenuItem)rGBGToolStripMenuItem;
                            item.Checked = true;
                            break;
                    }

                    if (m_MonoSensor)
                    {
                        item = (ToolStripMenuItem)monoSensorToolStripMenuItem;
                        item.Checked = true;

                        pixelOrderToolStripMenuItem.Enabled = false;
                    }

                    // default emgu demo status is disabled
                    item = (ToolStripMenuItem)disableDemoToolStripMenuItem;
                    item.Checked = true;

                    capture.ExposureExt = m_curExpTimeInLines;
                    frmRegRW_MODESET.ExpTime = m_curExpTimeInLines;

                    capture.Gain = m_curGain;

                    m_curExpXGain = m_curExpTimeInLines * m_curGain;
                    m_PrevImageMean = 0;

                }
                else // YUV sensor
                {
                    capture.SetParam(width, height, true, pictBDisplay.Handle);

                    item = (ToolStripMenuItem)triggerModeToolStripMenuItem;
                    item.Checked = false;

                    item = (ToolStripMenuItem)monoSensorToolStripMenuItem;
                    item.Checked = false;

                    captureImageToolStripMenuItem.Enabled = true;
                    triggerModeToolStripMenuItem.Enabled = false;
                    noDisplayToolStripMenuItem.Enabled = false;
                    softTriggerToolStripMenuItem.Enabled = false;
                    autoTriggerToolStripMenuItem.Enabled = false;
                    pixelOrderToolStripMenuItem.Enabled = false;
                    monoSensorToolStripMenuItem.Enabled = false;
                    setTriggerDelayToolStripMenuItem.Enabled = false;
                    autoExposureSoftwareToolStripMenuItem.Enabled = false;

                    // sensor YUV mode disable emgu demo tool
                    EmguDemoToolStripMenuItem.Enabled = false;

                }

            }

            updateRegisterSettingFromConfiguration(capture.cameraModel);
        }

        private static Mutex OpenCameraMutex = new Mutex();
        private void openCameraByIndex(int index, int modeIndex)
        {
            int width, height;

            OpenCameraMutex.WaitOne();

            try
            {
                cameraPropertyToolStripMenuItem.Enabled = false;
                optionsToolStripMenuItem.Enabled = false;
                resolutionToolStripMenuItem.Enabled = false;
                framerateToolStripMenuItem.Enabled = false;
                lyftToolStripMenuItem.Enabled = false;

                CloseCamera();

                capture = new LPCamera();
                capture.Open(capture.cameraList[index], m_ResolutionIndex, m_FrameRateIndex);

                try
                {
                    vLog = new VideoCapture();
                }
                catch (Exception e)
                {
                    Debug.Print("Failed to open video capture: " + e.ToString());
                    throw e;
                }

                imLog = new LyftImageStream(true);  // create directory for now

                CameraUUID = "";
                FuseID = "";
                HwRev = 0;
                FwRev = 0;
                MarkEn = false; 

                try
                {
                    System.Threading.Thread.Sleep(500);
                    try
                    {
                        capture.ReadCamUUIDnHWFWRev(out CameraUUID, out HwRev, out FwRev);
                    }
                    catch
                    {
                        capture.ReadCamUUIDnHWFWRev(out CameraUUID, out HwRev, out FwRev, out FuseID);
                        MarkEn = true; 
                    }
                    capture.ReadExtensionINFO(out ROIX_MAX, out ROIX_MIN, out ROIY_MAX, out ROIY_MIN);
                    frmRegRW_MODESET.Update_TrackBarMinMax(ROIX_MAX,ROIX_MIN,ROIY_MAX,ROIY_MIN);

                }
                catch
                {
                }

                width = capture.ResList[modeIndex, 0];
                height = capture.ResList[modeIndex, 1];

                capture.m_capture.ReceivedOneFrame += new FrameReceivedEventHandler(onReceivedOneFrame);
                // Position video window in client rect of owner window
                pictBDisplay.Resize += new EventHandler(onPreviewWindowResize);
                
                pictureBoxCenter.Parent = pictBDisplay;
                pictureBoxTopLeft.Parent = pictBDisplay;
                pictureBoxTopRight.Parent = pictBDisplay;
                pictureBoxBottomLeft.Parent = pictBDisplay;
                pictureBoxBottomRight.Parent = pictBDisplay;

                onPreviewWindowResize(this, null);

                setupMenuAndInit(width, height);

                // the first 4 bits represents the sensor mode, 0x1 : RAW 8, 0x2: RAW 10, 0x3: RAW 12, 0x4: YUY2, 0x5: RAW8_DUAL
                if ((HwRev & 0xf000) == 0x1000)
                {
                    m_SensorDataMode = LeopardCamera.LPCamera.SENSOR_DATA_MODE.RAW8;
                    width = width * 2;
                }
                else if ((HwRev & 0xf000) == 0x2000)
                    m_SensorDataMode = LeopardCamera.LPCamera.SENSOR_DATA_MODE.RAW10;
                else if ((HwRev & 0xf000) == 0x3000)
                    m_SensorDataMode = LeopardCamera.LPCamera.SENSOR_DATA_MODE.RAW12;
                else if ((HwRev & 0xf000) == 0x4000)
                    m_SensorDataMode = LeopardCamera.LPCamera.SENSOR_DATA_MODE.YUV;
                else if ((HwRev & 0xf000) == 0x5000)
                    m_SensorDataMode = LeopardCamera.LPCamera.SENSOR_DATA_MODE.RAW8_DUAL;
                else if (capture.cameraModel == LPCamera.CameraModel.ZED)
                    m_SensorDataMode = LeopardCamera.LPCamera.SENSOR_DATA_MODE.YUV_DUAL;
                else if (capture.cameraModel == LPCamera.CameraModel.ETRON2D)
                    m_SensorDataMode = LeopardCamera.LPCamera.SENSOR_DATA_MODE.JPEG;

                width = width * (m_SensorDataMode == LeopardCamera.LPCamera.SENSOR_DATA_MODE.RAW8_DUAL ? 2 : 1);

                // ETRON3D camera is YUY2 data format, in order to handle it and display
                // receive it as raw10 data format, added depth image display area in the right form
                if (capture.cameraModel == LPCamera.CameraModel.ETRON3D)
                {
                    m_SensorDataMode = LeopardCamera.LPCamera.SENSOR_DATA_MODE.RAW10;
                    width += 640;
                }

                this.Width = width + 18;
                this.Height = height + 50 + pictBDisplay.Top + statusStrip2.Height;
                
                //if (capture.cameraModel == CameraLP.CameraModel.M034 && width == 1280 && height == 720)
                //    setupSensorMode();
                //else
                //    cmbSensorMode.Visible = false;

                capture.Run();

                m_PrevFrameCnt = capture.FrameCount;

                cameraPropertyToolStripMenuItem.Enabled = true;
                optionsToolStripMenuItem.Enabled = true;
                resolutionToolStripMenuItem.Enabled = true;
                framerateToolStripMenuItem.Enabled = true;
                lyftToolStripMenuItem.Enabled = true;

                // only ar0130_ap0100 camera supports flash update
                if (capture.cameraModel == LPCamera.CameraModel.AR0130_AP0100)
                    programFlashToolStripMenuItem.Enabled = true;
                else
                    programFlashToolStripMenuItem.Enabled = false;

                cameraList = CameraType.LEOPARD_CAMERA;

                updateDeviceInfo();
                updateDeviceResolution();
                updateDeviceFrameRate();
            }
            catch
            {
            }
            
            OpenCameraMutex.ReleaseMutex();
        }

        /// <summary> Resize the preview when the PreviewWindow is resized </summary>
        protected void onPreviewWindowResize(object sender, EventArgs e)
        {
            if (capture != null)
            {
                // if window size changed, disable rect drawing to avoid confusion
                drawingRect = false;

                if (capture.rendererWin != null)
                {
                    // Position video window in client rect of owner window
                    Rectangle rc = pictBDisplay.ClientRectangle;
                    capture.rendererWin.SetWindowPosition(0, 0, rc.Right, rc.Bottom);
                    pictureBoxCenter.Height = 10;
                    pictureBoxCenter.Width = 10;
                    pictureBoxCenter.Top = (rc.Bottom - rc.Top) / 2 - 25;
                    pictureBoxCenter.Left = (rc.Right - rc.Left) / 2 - 25;

                    pictureBoxTopLeft.Height = 10;
                    pictureBoxTopLeft.Width = 10;
                    pictureBoxTopLeft.Top = (rc.Bottom - rc.Top) / 3 - 25 -25 ;
                    pictureBoxTopLeft.Left = (rc.Right - rc.Left) / 3 - 25 - 25;

                    pictureBoxTopRight.Height = 10;
                    pictureBoxTopRight.Width = 10;
                    pictureBoxTopRight.Top = (rc.Bottom - rc.Top) / 3 - 25 -25;
                    pictureBoxTopRight.Left = (rc.Right - rc.Left) * 2 / 3 - 25 + 25;

                    pictureBoxBottomLeft.Height = 10;
                    pictureBoxBottomLeft.Width = 10;
                    pictureBoxBottomLeft.Top = (rc.Bottom - rc.Top) * 2 / 3 - 25 + 25;
                    pictureBoxBottomLeft.Left = (rc.Right - rc.Left) / 3 - 25 - 25;

                    pictureBoxBottomRight.Height = 10;
                    pictureBoxBottomRight.Width = 10;
                    pictureBoxBottomRight.Top = (rc.Bottom - rc.Top) * 2 / 3 - 25 + 25;
                    pictureBoxBottomRight.Left = (rc.Right - rc.Left) * 2 / 3 - 25 + 25;
                }
            }
        }

        #region DLL
        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth,
           int nHeight, IntPtr hObjSource, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);    

        [DllImport("gdi32.dll")]
        static extern bool StretchBlt(IntPtr hdcDest, int nXOriginDest,
            int nYOriginDest, int nWidthDest, int nHeightDest, IntPtr hdcSrc,
            int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc,
            TernaryRasterOperations dwRop);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        static extern bool SetStretchBltMode(IntPtr hdc, StretchMode iStretchMode);

        public enum StretchMode
        {
            STRETCH_ANDSCANS = 1,
            STRETCH_ORSCANS = 2,
            STRETCH_DELETESCANS = 3,
            STRETCH_HALFTONE = 4,
        }

        public enum TernaryRasterOperations
        {
            SRCCOPY = 0x00CC0020, /* dest = source*/
            SRCPAINT = 0x00EE0086, /* dest = source OR dest*/
            SRCAND = 0x008800C6, /* dest = source AND dest*/
            SRCINVERT = 0x00660046, /* dest = source XOR dest*/
            SRCERASE = 0x00440328, /* dest = source AND (NOT dest )*/
            NOTSRCCOPY = 0x00330008, /* dest = (NOT source)*/
            NOTSRCERASE = 0x001100A6, /* dest = (NOT src) AND (NOT dest) */
            MERGECOPY = 0x00C000CA, /* dest = (source AND pattern)*/
            MERGEPAINT = 0x00BB0226, /* dest = (NOT source) OR dest*/
            PATCOPY = 0x00F00021, /* dest = pattern*/
            PATPAINT = 0x00FB0A09, /* dest = DPSnoo*/
            PATINVERT = 0x005A0049, /* dest = pattern XOR dest*/
            DSTINVERT = 0x00550009, /* dest = (NOT dest)*/
            BLACKNESS = 0x00000042, /* dest = BLACK*/
            WHITENESS = 0x00FF0062, /* dest = WHITE*/
        };

        #endregion

        private byte[] imageArray, imageArrayPre;
        //private Bitmap imageBmp;
        private bool m_SaveFrameToFile = false;
        private bool m_SaveFileInProcess = false;
        private void SaveCapturedImage(string fileNamePrefix)
        {
            if (!m_SaveFrameToFile || m_SaveFileInProcess)
                return;

            m_SaveFileInProcess = true;
            m_SaveFrameToFile = false;

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            string fileName;

            if (fileNamePrefix == null || fileNamePrefix.Length == 0)
            {
                //openFileDialog1.InitialDirectory = "c:\\";
                if (m_SensorDataMode == LeopardCamera.LPCamera.SENSOR_DATA_MODE.YUV
                    || m_SensorDataMode == LeopardCamera.LPCamera.SENSOR_DATA_MODE.YUV_DUAL)
                    saveFileDialog1.Filter = "yuv files (*.yuv)|*.yuv|All files (*.*)|*.*";
                else
                    saveFileDialog1.Filter = "bmp files (*.bmp)|*.bmp|JPG files|*.jpg|All files (*.*)|*.*";

                //saveFileDialog1.Filter = "All files (*.*)|*.*";
                saveFileDialog1.FilterIndex = 1;
                saveFileDialog1.RestoreDirectory = true;
                if (FuseID != "")
                    saveFileDialog1.FileName = FuseID + "_" + DateTime.Now.ToString("yyyy-MM-dd");
                else
                    saveFileDialog1.FileName = CameraUUID + "exp_" + capture.ExposureExt.ToString();
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    fileName = Path.ChangeExtension(saveFileDialog1.FileName, null); // remove extension
                }
                else
                    return;
            }
            else
            {
                fileName = fileNamePrefix;
            }

            {
                if (configurationForm.CaptureBMP || configurationForm.CaptureRAW)
                {
                    System.IO.FileInfo file = new System.IO.FileInfo(fileName);
                    file.Directory.Create(); // If the directory doesn't exist, create it
                }

                int num = 0;
                int iSize = listIntPtr_Width * listIntPtr_Height * ( (listIntPtr_Bpp - 1 ) / 8 + 1) ;
                byte[] imageArray = new byte[iSize];

                foreach (var ptr in listIntPtr)
                {
                    Marshal.Copy(ptr, imageArray, 0, iSize);

                    Bitmap imageBmp = null;
                    
                    if (configurationForm.CaptureBMP)
                        imageBmp = CopyFrame(ptr, listIntPtr_Width, listIntPtr_Height, listIntPtr_Bpp);

                    Marshal.FreeHGlobal(ptr);
                    num++;
                    if (m_SensorDataMode == LeopardCamera.LPCamera.SENSOR_DATA_MODE.YUV
                        || m_SensorDataMode == LeopardCamera.LPCamera.SENSOR_DATA_MODE.YUV_DUAL)
                    {
                        if (configurationForm.CaptureRAW)
                            LeopardCamera.Tools.SaveRAWfile(imageArray, fileName + "_" + num.ToString() + ".raw");

                        if (configurationForm.CaptureBMP)
                        {
                            string bmpFileName = fileName + "_" + num.ToString() + ".bmp";
                            imageBmp.Save(bmpFileName, System.Drawing.Imaging.ImageFormat.Bmp);
                        }
                    }
                    else
                    {
                        if (configurationForm.CaptureBMP)
                        {
                            if (saveFileDialog1.FileName.Contains(".jpg"))
                            {
                                imageBmp.Save(fileName + "_" + num.ToString() + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                            }
                            else //if (saveFileDialog1.FileName.Contains(".bmp"))
                            {
                                imageBmp.Save(fileName + "_" + num.ToString() + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                            }
                        }

                        if (configurationForm.CaptureRAW)
                        {
                            string rawFileName = fileName + "_" + num.ToString() + ".raw";
                            LeopardCamera.Tools.SaveRAWfile(imageArray, rawFileName);
                        }
                    }

                    if (configurationForm.CaptureBMP)
                        imageBmp.Dispose();

                }
            }

            //imageBmp.Dispose();
            listIntPtr.Clear();
            m_SaveFrameToFile = false;
            m_SaveFileInProcess = false;
        }

        private int GetEmbeddedFrameCount(IntPtr pBuffer, int width, int height, int bpp)
        {
            int len = 4 * 5;
            byte[] imageArrayFrmCount = new byte[len];

            Marshal.Copy(pBuffer, imageArrayFrmCount, 0, len);

            int frameCount = (int)(imageArrayFrmCount[1] >> 4) << 12
                            | (int)(imageArrayFrmCount[1+4] >> 4) << 8
                            | (int)(imageArrayFrmCount[1+4*2] >> 4) << 4
                            | (int)(imageArrayFrmCount[1+4*3] >> 4);

            return frameCount;
        }

        private Bitmap CopyFrame(IntPtr pBuffer, int width, int height, int bpp)
        {
           // if (m_SaveFrameToFile)
           //     return;
            Bitmap imageBmp=null;

            //int iSize = width * height * ( (bpp - 1 ) / 8 + 1) ;
            //imageArray = new byte[iSize];

            //Marshal.Copy(pBuffer, imageArray, 0, iSize);

            if (m_SensorDataMode == LeopardCamera.LPCamera.SENSOR_DATA_MODE.YUV
                || m_SensorDataMode == LeopardCamera.LPCamera.SENSOR_DATA_MODE.YUV_DUAL)
                imageBmp = LeopardCamera.Tools.ConvrtYUV422BMP(pBuffer, width, height, MarkEn, (pictureBoxCenter.Top * height / pictBDisplay.Height) * width + pictureBoxCenter.Left * width / pictBDisplay.Width,
                                                                                       (pictureBoxTopLeft.Top * height / pictBDisplay.Height) * width + pictureBoxTopLeft.Left * width / pictBDisplay.Width,
                                                                                       (pictureBoxBottomLeft.Top * height / pictBDisplay.Height) * width + pictureBoxBottomLeft.Left * width / pictBDisplay.Width,
                                                                                       (pictureBoxTopRight.Top * height / pictBDisplay.Height) * width + pictureBoxTopRight.Left * width / pictBDisplay.Width,
                                                                                       (pictureBoxBottomRight.Top * height / pictBDisplay.Height) * width + pictureBoxBottomRight.Left * width / pictBDisplay.Width);

            else
            {
                imageBmp = LeopardCamera.Tools.ConvertBayer2BMP(pBuffer, width, height, bpp, (int)m_pixelOrder, 
                    m_GammaEna, m_GammaValue, 
                    m_RGBGainOffsetEna, r_gain, g_gain, b_gain, r_offset, g_offset, b_offset,
                    m_RGB2RGBMatrixEna, matrix_rr, matrix_rg, matrix_rb, matrix_gr, matrix_gg, matrix_gb, matrix_br, matrix_bg, matrix_bb,
                    m_MonoSensor, (m_SensorDataMode == LeopardCamera.LPCamera.SENSOR_DATA_MODE.RAW8_DUAL));

                if (m_Show_Anchors)
                    AddAnchorsToBmp(imageBmp);
            }

            //m_SaveFrameToFile = true;

            return imageBmp;
        }

        private void SavePreFrame(IntPtr pBuffer, int width, int height, int bpp)
        {

            int iSize = width * height * ((bpp - 1) / 8 + 1);
            imageArrayPre = new byte[iSize];

            Marshal.Copy(pBuffer, imageArrayPre, 0, iSize);
        }

        private void AddAnchorsToBmp(Bitmap bitmap)
        {
            System.Drawing.Graphics newGraphics = Graphics.FromImage(bitmap);

            SolidBrush redBrush = new SolidBrush(Color.Red);

            int box_X, box_Y, box_W=10, box_H = 10;

            box_Y = bitmap.Height / 2 - 25;
            box_X = bitmap.Width / 2 - 25;
            newGraphics.FillRectangle(redBrush, box_X, box_Y, box_W, box_H);

            box_Y = bitmap.Height / 3 - 25 - 25;
            box_X = bitmap.Width / 3 - 25 - 25;
            newGraphics.FillRectangle(redBrush, box_X, box_Y, box_W, box_H);

            box_Y = bitmap.Height / 3 - 25 - 25;
            box_X = bitmap.Width * 2 / 3 - 25 + 25;
            newGraphics.FillRectangle(redBrush, box_X, box_Y, box_W, box_H);

            box_Y = bitmap.Height * 2 / 3 - 25 + 25;
            box_X = bitmap.Width / 3 - 25 - 25;
            newGraphics.FillRectangle(redBrush, box_X, box_Y, box_W, box_H);

            box_Y = bitmap.Height * 2 / 3 - 25 + 25;
            box_X = bitmap.Width * 2 / 3 - 25 + 25;
            newGraphics.FillRectangle(redBrush, box_X, box_Y, box_W, box_H);            

        }

        private void DisplayImage(Bitmap bitmap, PictureBox pictbox)
        {
            System.Drawing.Graphics newGraphics = Graphics.FromImage(bitmap);

            if (this.pictBDisplay.Width != 0 && this.pictBDisplay.Height != 0)
            {
                if (drawingRect && m_boxRect.Width > 1 && m_boxRect.Height > 1)
                {
                    int iWidth = bitmap.Width;
                    int iHeight = bitmap.Height;

                    int box_X = m_boxRect.X * iWidth / this.pictBDisplay.Width;
                    int box_Y = m_boxRect.Y * iHeight / this.pictBDisplay.Height;
                    int box_W = m_boxRect.Width * iWidth / this.pictBDisplay.Width;
                    int box_H = m_boxRect.Height * iHeight / this.pictBDisplay.Height;

                    box_X = box_X < 0 ? 0 : box_X > iWidth ? iWidth : box_X;
                    box_Y = box_Y < 0 ? 0 : box_Y > iWidth ? iHeight : box_Y;
                    box_W = box_W < 1 ? 1 : box_W + box_X > iWidth ? iWidth - box_X : box_W;
                    box_H = box_H < 1 ? 1 : box_H + box_Y > iHeight ? iHeight - box_Y : box_H;

                    Pen pen = new Pen(Color.Red, 2);
                    newGraphics.DrawRectangle(pen, box_X, box_Y, box_W, box_H);
                }
            }

            System.Drawing.Graphics formGraphics = pictbox.CreateGraphics();

            IntPtr hbmp = bitmap.GetHbitmap();

            IntPtr pTarget = formGraphics.GetHdc();
            IntPtr pSource = CreateCompatibleDC(pTarget);
            IntPtr pOrig = SelectObject(pSource, hbmp);
            /*
            SetStretchBltMode(pTarget, StretchMode.STRETCH_DELETESCANS);

            StretchBlt(pTarget, 0,
                0, pictbox.Width, pictbox.Height, pSource,
                0, 0, bitmap.Width, bitmap.Height, TernaryRasterOperations.SRCCOPY);
            */
            BitBlt(pTarget, 0, 0, bitmap.Width, bitmap.Height, pSource, 0, 0, TernaryRasterOperations.SRCCOPY);

            IntPtr pNew = SelectObject(pSource, pOrig);
            DeleteObject(pNew);
            DeleteDC(pSource);
            formGraphics.ReleaseHdc(pTarget);
        }

        #region APIs
        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        protected static extern void CopyMemory(IntPtr Destination, IntPtr Source, [MarshalAs(UnmanagedType.U4)] int Length);
        #endregion

        private int PreEmbeddedFrmCnt = 0;
        private List<IntPtr> listIntPtr = new List<IntPtr>();
        private int listIntPtr_Width = 0, listIntPtr_Height = 0, listIntPtr_Bpp = 0;
        private int mCaptureFrameNum = 5;
        private int capturedFrame = 0;
        /// <summary> capture one frame and display it </summary>
        protected void onReceivedOneFrame(object sender, EventArgs e)
        {
            IntPtr pBuffer1 = IntPtr.Zero;
            IntPtr pBufferProc = IntPtr.Zero;

            int iWidth, iHeight, iBPP = 0;
            int dataWidth = 8;

            if (m_SaveFileInProcess) // saving image to disk, don't process 
                return;

            capture.CaptureImageNoWait(out pBuffer1, out iWidth, out iHeight, out iBPP);

            //Debug.Print("Captured image: " + iWidth.ToString() + "x" + iHeight.ToString() + " @" + iBPP.ToString() +
            //    " bpp, sensor Mode: " + m_SensorDataMode.ToString());

            iHeight = iHeight / 2 * 2;

            if (m_SensorDataMode == LeopardCamera.LPCamera.SENSOR_DATA_MODE.RAW12
                || m_SensorDataMode == LeopardCamera.LPCamera.SENSOR_DATA_MODE.RAW10)
            {
                int embeddedFrameCount = GetEmbeddedFrameCount(pBuffer1, iWidth, iHeight, iBPP);
                if (embeddedFrameCount != PreEmbeddedFrmCnt + 1
                    && PreEmbeddedFrmCnt != 0)
                    FrameDisconntinued = true;
                else
                    FrameDisconntinued = false;

                //Debug.Print("Embedded frame cnt: " + embeddedFrameCount.ToString());
                PreEmbeddedFrmCnt = embeddedFrameCount;
            }

            if (m_SensorDataMode == LeopardCamera.LPCamera.SENSOR_DATA_MODE.RAW8)
                iWidth = iWidth * 2;
            else if (m_SensorDataMode == LeopardCamera.LPCamera.SENSOR_DATA_MODE.RAW10)
                dataWidth = 10;
            else if (m_SensorDataMode == LeopardCamera.LPCamera.SENSOR_DATA_MODE.RAW12)
                dataWidth = 12;
            else if (m_SensorDataMode == LeopardCamera.LPCamera.SENSOR_DATA_MODE.RAW8_DUAL)
                dataWidth = 8;
            else if (m_SensorDataMode == LPCamera.SENSOR_DATA_MODE.YUV)
                dataWidth = 16;
            //Debug.Print("Data width: " + dataWidth.ToString());
            if (m_CaptureImages)
            {
                Debug.Print("m capture images true!");
                IntPtr pBufferTmp = Marshal.AllocHGlobal(iWidth * iHeight * ((dataWidth - 1) / 8 + 1));
                CopyMemory(pBufferTmp, pBuffer1, iWidth * iHeight * ((dataWidth - 1) / 8 + 1));

                listIntPtr.Add(pBufferTmp);

                capture.FreeImageBuffer(pBuffer1);

                capturedFrame++;

                if (capturedFrame >= mCaptureFrameNum)
                {
                    m_CaptureImages = false;

                    //CopyFrame(pBuffer1, iWidth, iHeight, dataWidth);
                    listIntPtr_Width = iWidth;
                    listIntPtr_Height = iHeight;
                    listIntPtr_Bpp = dataWidth;
                    m_SaveFrameToFile = true;
                }

                System.Threading.Thread.Sleep(InterFrameDelayTime);

                return;
            }

            // pbuffer will be changed by convert2BMP. make a copy and process
            if (m_selectedPlugin != null)
            {
                Debug.Print("m_selected plugin is not null");
                string cameraID;

                if (FuseID != "")
                    cameraID = FuseID;
                else
                    cameraID = CameraUUID;

                //if (capture.cameraModel == LPCamera.CameraModel.IMX172)
                if (m_FocusProcDownSample && iWidth > 1280 && iHeight > 720)
                {
                    if (capture.cameraModel == LPCamera.CameraModel.ETRON2D)
                    {
                        PluginProcess(pBuffer1, iWidth, iHeight, dataWidth, m_SensorDataMode, m_MonoSensor,
                                (int)m_pixelOrder, m_curExpTimeInLines, cameraID, m_GammaEna, m_GammaValue,
                                    m_RGBGainOffsetEna, r_gain, g_gain, b_gain, r_offset, g_offset, b_offset,
                                    m_RGB2RGBMatrixEna, matrix_rr, matrix_rg, matrix_rb, matrix_gr, matrix_gg, matrix_gb, matrix_br, matrix_bg, matrix_bb); 
                    }
                    else
                    {
                        pBufferProc = Marshal.AllocHGlobal(1280 * 720 * ((dataWidth - 1) / 8 + 1) * 2);
                        if (capture.cameraModel == LPCamera.CameraModel.IMX172)
                            LeopardCamera.Tools.ReframeTo720p(pBufferProc, pBuffer1, iWidth, iHeight, dataWidth);
                        else
                            LeopardCamera.Tools.ReframeTo720p_4corners(pBufferProc, pBuffer1, iWidth, iHeight, dataWidth);

                        if (m_AutoExposure)
                        {
                            int startx, starty;
                            int iSize = 150;

                            startx = (1280 - iSize) / 2;
                            starty = (720 - iSize) / 2;
                            m_ImageMean = LeopardCamera.Tools.CalcMean(pBufferProc, 1280, 720, startx, starty, iSize, dataWidth);
                            dTargetMean = (double)(0x01 << (dataWidth - 2)) * dTargetMeanFactor;
                        }

                        PluginProcess(pBufferProc, 1280, 720, dataWidth, m_SensorDataMode, m_MonoSensor,
                                (int)m_pixelOrder, m_curExpTimeInLines, cameraID, m_GammaEna, m_GammaValue,
                                    m_RGBGainOffsetEna, r_gain, g_gain, b_gain, r_offset, g_offset, b_offset,
                                    m_RGB2RGBMatrixEna, matrix_rr, matrix_rg, matrix_rb, matrix_gr, matrix_gg, matrix_gb, matrix_br, matrix_bg, matrix_bb);
                    }

                }
                else if (m_SensorDataMode == LeopardCamera.LPCamera.SENSOR_DATA_MODE.YUV
                    || m_SensorDataMode == LeopardCamera.LPCamera.SENSOR_DATA_MODE.YUV_DUAL
                    || m_SensorDataMode == LeopardCamera.LPCamera.SENSOR_DATA_MODE.RAW8_DUAL)
                {
                    pBufferProc = Marshal.AllocHGlobal(iWidth * iHeight * ((dataWidth - 1) / 8 + 1) * 2);
                    CopyMemory(pBufferProc, pBuffer1, iWidth * iHeight * ((dataWidth - 1) / 8 + 1) * 2);
                    PluginProcess(pBufferProc, iWidth, iHeight, dataWidth, m_SensorDataMode, m_MonoSensor,
                        (int)m_pixelOrder, m_curExpTimeInLines, cameraID, m_GammaEna, m_GammaValue,
                            m_RGBGainOffsetEna, r_gain, g_gain, b_gain, r_offset, g_offset, b_offset,
                            m_RGB2RGBMatrixEna, matrix_rr, matrix_rg, matrix_rb, matrix_gr, matrix_gg, matrix_gb, matrix_br, matrix_bg, matrix_bb);
                }
                else
                {
                    pBufferProc = Marshal.AllocHGlobal(iWidth * iHeight * ((dataWidth - 1) / 8 + 1));
                    CopyMemory(pBufferProc, pBuffer1, iWidth * iHeight * ((dataWidth - 1) / 8 + 1));
                    PluginProcess(pBufferProc, iWidth, iHeight, dataWidth, m_SensorDataMode, m_MonoSensor,
                        (int)m_pixelOrder, m_curExpTimeInLines, cameraID, m_GammaEna, m_GammaValue,
                            m_RGBGainOffsetEna, r_gain, g_gain, b_gain, r_offset, g_offset, b_offset,
                            m_RGB2RGBMatrixEna, matrix_rr, matrix_rg, matrix_rb, matrix_gr, matrix_gg, matrix_gb, matrix_br, matrix_bg, matrix_bb);
                }
                Marshal.FreeHGlobal(pBufferProc);
            }

            if (m_AutoTrigger)
            {
                Debug.Print("auto trigger true");
                triggerEndTime = DateTime.Now;
                m_AutoTriggerCnt++;
            }

            if (m_SensorDataMode != LeopardCamera.LPCamera.SENSOR_DATA_MODE.YUV
                && m_SensorDataMode != LeopardCamera.LPCamera.SENSOR_DATA_MODE.YUV_DUAL)
            {
                if (m_AutoExposure)
                {
                    Debug.Print("Auto exposure true");
                    int startx, starty;
                    int iSize = 256;

                    startx = (iWidth - iSize) / 2;
                    starty = (iHeight - iSize) / 2;
                    m_ImageMean = LeopardCamera.Tools.CalcMean(pBuffer1, iWidth, iHeight, startx, starty, iSize, dataWidth);
                    dTargetMean = (double)(0x01 << (dataWidth - 2)) * dTargetMeanFactor;
                }

                if (iWidth != 0 && iHeight != 0 && mRAWDisplay)
                {
                    Bitmap bitmap;
                    
                    if (this.pictBDisplay.Width != 0 && this.pictBDisplay.Height != 0
                        && m_NoiseCalculationEna)
                    {
                        int box_X = m_boxRect.X * iWidth / this.pictBDisplay.Width;
                        int box_Y = m_boxRect.Y * iHeight / this.pictBDisplay.Height;
                        int box_W = m_boxRect.Width * iWidth / this.pictBDisplay.Width;
                        int box_H = m_boxRect.Height * iHeight / this.pictBDisplay.Height;

                        box_X = box_X < 0 ? 0 : box_X > iWidth ? iWidth : box_X;
                        box_Y = box_Y < 0 ? 0 : box_Y > iWidth ? iHeight : box_Y;
                        box_W = box_W < 1 ? 1 : box_W + box_X > iWidth ? iWidth - box_X : box_W;
                        box_H = box_H < 1 ? 1 : box_H + box_Y > iHeight ? iHeight - box_Y : box_H;

                        if (drawingRect && m_boxRect.Height != 0 && m_boxRect.Width != 0) // calculate mean & STD
                        {
                            m_RectMean = LeopardCamera.Tools.CalcMean(pBuffer1, iWidth, iHeight, box_X, box_Y,
                                                                        box_W, box_H, dataWidth);
                            m_RectSTD = LeopardCamera.Tools.CalcSTD(pBuffer1, m_RectMean, iWidth, iHeight, box_X, box_Y,
                                                                        box_W, box_H, dataWidth);
                            //m_RectTN = LeopardCamera.Tools.CalcTemporalNoise(pBuffer1, imageArrayPre, iWidth, iHeight, box_X, box_Y,
                            //                                            box_W, box_H, dataWidth);
                            //m_RectFPN = m_RectSTD - m_RectTN;
                        }
                        else
                        {
                            m_RectMean = LeopardCamera.Tools.CalcMean(pBuffer1, iWidth, iHeight, box_X, box_Y,
                                                                        1, 1, dataWidth);
                            m_RectSTD = 0;
                            m_RectFPN = 0;
                            m_RectTN = 0;
                        }

                        //SavePreFrame(pBuffer1, iWidth, iHeight, dataWidth);
                    }

                    if (capture.cameraModel == LPCamera.CameraModel.ETRON3D)
                    {
                        bitmap = LeopardCamera.Tools.ConvrtYUV422BMP(pBuffer1, iWidth + 640, iHeight, false, 0, 0, 0, 0, 0);
                    }
                    else
                    {
                        //String s = String.Format("converting to bmp: {0}x{1} , dw {2}, pixOrder: {3}: mono: {4}, dual: {5}",
                        //    iWidth, iHeight, dataWidth, (int)m_pixelOrder, m_MonoSensor, (m_SensorDataMode == LeopardCamera.LPCamera.SENSOR_DATA_MODE.RAW8_DUAL));
                        //Debug.Print(s);
                        bitmap = LeopardCamera.Tools.ConvertBayer2BMP(pBuffer1, iWidth, iHeight, dataWidth, (int)m_pixelOrder, 
                                        m_GammaEna, m_GammaValue,
                                        m_RGBGainOffsetEna, r_gain, g_gain, b_gain, r_offset, g_offset, b_offset,
                                        m_RGB2RGBMatrixEna, matrix_rr, matrix_rg, matrix_rb, matrix_gr, matrix_gg, matrix_gb, matrix_br, matrix_bg, matrix_bb,
                                        m_MonoSensor, (m_SensorDataMode == LeopardCamera.LPCamera.SENSOR_DATA_MODE.RAW8_DUAL));
                    }

                    try
                    {
                        Bitmap bitmapDraw = bitmap;
                        //if (iWidth > pictBDisplay.Width || iHeight > pictBDisplay.Height)
                        {
                            Size size = new System.Drawing.Size(pictBDisplay.Width, pictBDisplay.Height);
                            bitmapDraw = (Bitmap)resizeImage(bitmap, size);
                        }

                        // emguCV demo
                        bitmap = EmguTool.EmguDemo.EmguDemoRun(m_EmguDemoId, bitmapDraw);
                        //bitmap = EmguTool.EmguDemo.EmguDemoRun(EmguDemo.EmguDemoId.FDDemo, bitmapDraw);
                        // KB this doesn't work

                        if (m_Show_Anchors)
                        {
                            AddAnchorsToBmp(bitmapDraw);
                        }

                        DisplayImage(bitmapDraw, pictBDisplay);

                        // log frame to videoLogFile if it's enabled
                        vLog.WriteVideoLogFrame(bitmapDraw);

                        // capture the raw unedited bitmap to file (if it's enabled)
                        imLog.StoreImage(ref bitmap);

                        bitmapDraw.Dispose();
                        bitmap.Dispose();
                    }
                    catch
                    {

                    }

                }
            }
            else //YUV
            {
                //if (m_CaptureOneImage)
                //{
                //    m_CaptureOneImage = false;
                //    CopyFrame(pBuffer1, iWidth, iHeight, dataWidth);
                //}

                if (m_NoiseCalculationEna)
                {
                    LeopardCamera.Tools.yuv422_TO_y(pBuffer1, pBuffer1, iWidth, iHeight);

                    m_RectMean = LeopardCamera.Tools.CalcMean(pBuffer1, iWidth, iHeight, 0, 0,
                                                  iWidth, iHeight, dataWidth);
                    m_RectSTD = LeopardCamera.Tools.CalcSTD(pBuffer1, m_RectMean, iWidth, iHeight, 0, 0,
                                                                iWidth, iHeight, dataWidth);
                    //m_RectTN = 0.0; LeopardCamera.Tools.CalcTemporalNoise(pBuffer1, imageArrayPre, iWidth, iHeight, 0, 0,
                    //                                             iWidth, iHeight, dataWidth);
                    //m_RectFPN = m_RectSTD - m_RectTN;

                    //SavePreFrame(pBuffer1, iWidth, iHeight, dataWidth);
                }
            }

            capture.FreeImageBuffer(pBuffer1);
            GC.Collect(); 
        }

        private Image resizeImage(Image imgToResize, Size size)
        {
            //int sourceWidth = imgToResize.Width;
            //int sourceHeight = imgToResize.Height;

            //float nPercent = 0;
            //float nPercentW = 0;
            //float nPercentH = 0;

            //nPercentW = ((float)size.Width / (float)sourceWidth);
            //nPercentH = ((float)size.Height / (float)sourceHeight);

            //if (nPercentH < nPercentW)
            //    nPercent = nPercentH;
            //else
            //    nPercent = nPercentW;

            int destWidth = size.Width;// (int)(sourceWidth * nPercent);
            int destHeight = size.Height;// (int)(sourceHeight * nPercent);

            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((Image)b);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();

            return (Image)b;
        }

        private void DevicesStripMenuItemClick(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem)  //Check On Click.
            {
                foreach (ToolStripMenuItem item in (((ToolStripMenuItem)sender).GetCurrentParent().Items))
                {
                    if (item == sender)
                    {
                        //txtNoteName.Text = item.Text;
                        item.Checked = true;

                        m_CameraIndex = (item.OwnerItem as ToolStripMenuItem).DropDownItems.IndexOf(item);
                        openCameraByIndex(m_CameraIndex, m_ResolutionIndex);
                    }
                    if ((item != null) && (item != sender))
                        item.Checked = false;
                }
            }
        }

        private void resolutionStripMenuItemClick(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem)  //Check On Click.
            {
                foreach (ToolStripMenuItem item in (((ToolStripMenuItem)sender).GetCurrentParent().Items))
                {
                    if (item == sender)
                    {
                        //txtNoteName.Text = item.Text;
                        item.Checked = true;

                        m_ResolutionIndex = (item.OwnerItem as ToolStripMenuItem).DropDownItems.IndexOf(item);
                        m_FrameRateIndex = 0;//always 0 when switching to another resolution
                        openCameraByIndex(m_CameraIndex, m_ResolutionIndex);
                        return;
                    }
                    if ((item != null) && (item != sender))
                        item.Checked = false;
                }
            }
        }
        private void framerateToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem)  //Check On Click.
            {
                foreach (ToolStripMenuItem item in (((ToolStripMenuItem)sender).GetCurrentParent().Items))
                {
                    if (item == sender)
                    {
                        //txtNoteName.Text = item.Text;
                        item.Checked = true;

                        m_FrameRateIndex = (item.OwnerItem as ToolStripMenuItem).DropDownItems.IndexOf(item);
                        // capture.framerateindex_curRes = m_FrameRateIndex;

                        openCameraByIndex(m_CameraIndex, m_ResolutionIndex);
                        return;
                    }
                    if ((item != null) && (item != sender))
                        item.Checked = false;
                }
            }
        }
        private void AddCamerasToMenu()
        {
            DevicesStripMenuItem.DropDown.Items.Clear();

            if (capture == null)
                return;

            if (capture.cameraList.Count == 0)
                return;

            for (int i = 0; i < capture.cameraList.Count; i++)
            {
                ToolStripMenuItem NEW;
                NEW = new ToolStripMenuItem(capture.cameraList[i].Name.ToString());
                NEW.Text = capture.cameraList[i].Name.ToString();
                NEW.Click += new EventHandler(DevicesStripMenuItemClick);
                NEW.CheckOnClick = true;
                DevicesStripMenuItem.DropDown.Items.Add(NEW);
            }

        }

        private void CloseCamera()
        {
            if (capture != null)
            {
                // Remove the Resize event handler
                pictBDisplay.Resize -= new EventHandler(onPreviewWindowResize);
                capture.m_capture.ReceivedOneFrame -= new FrameReceivedEventHandler(onReceivedOneFrame);

                capture.Stop();
                capture.Close();
            }
        }

        private void DetectCamera()
        {
            try
            {
                CloseCamera();
                capture = new LPCamera();
                AddCamerasToMenu();
                openCameraByIndex(m_CameraIndex, m_ResolutionIndex);

                ToolStripMenuItem item = (ToolStripMenuItem)DevicesStripMenuItem.DropDownItems[m_CameraIndex];
                item.Checked = true;
                item = (ToolStripMenuItem)resolutionToolStripMenuItem.DropDownItems[m_ResolutionIndex];
                item.Checked = true;

            }
            catch (Exception ex)
            {
                capture = null;

                CameraUUID = "";
                HwRev = 0;
                FwRev = 0;
                cameraList = CameraType.NO_CAMERA;

                AddCamerasToMenu();
                updateDeviceInfo();
                updateDeviceResolution();
                updateDeviceFrameRate();

                cameraPropertyToolStripMenuItem.Enabled = false;
                optionsToolStripMenuItem.Enabled = false;
                resolutionToolStripMenuItem.Enabled = false;
                triggerModeToolStripMenuItem.Enabled = false;
                autoTriggerToolStripMenuItem.Enabled = false;
                framerateToolStripMenuItem.Enabled = false;
                lyftToolStripMenuItem.Enabled = false;

                m_AutoTrigger = false;

                this.Width = 640;
                this.Height = 480;
            }

        }

        /// <summary>
        /// Windows Messages
        /// Defined in winuser.h from Windows SDK v6.1
        /// Documentation pulled from MSDN.
        /// For more look at: http://www.pinvoke.net/default.aspx/Enums/WindowsMessages.html
        /// </summary>
        public enum WM : uint
        {
            /// <summary>
            /// Notifies an application of a change to the hardware configuration of a device or the computer.
            /// </summary>
            DEVICECHANGE = 0x0219,
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_CLOSE = 0xF060;

            switch ((WM)m.Msg)
            {
                case WM.DEVICECHANGE:
                    //DetectCamera();
                    break;
            }

            if (m.Msg == WM_SYSCOMMAND)
            {
                if ((m.WParam.ToInt32() & 0xFFF0) == SC_CLOSE)
                {
                    CloseCamera();
                    if ( m_selectedPlugin != null)
                        m_selectedPlugin.Close();
                    m_selectedPlugin = null;
                }
            }

            base.WndProc(ref m);
        }

        private void cameraPropertyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (capture == null)
                throw new Exception("Camera is not initialized yet.");
            else
            {
                capture.ShowCameraProperty(this.Handle);
            }
        }

        private byte[] savedImageBuf = new byte[65536];
        private int bufIndex = 0;

        private void captureImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte packetLen = 0;
            byte[] imageBuf = new byte[65536];
            
            int validLen = 0;
            byte[] imageBufF;
            int[] regData = new int[512];
            /*
            string text = "// Register dump\n";
            // read 511 registers
            for (int i = 0; i < 512; i++)
            {
                capture.SetRegRW(0, i, R_value);

                System.Threading.Thread.Sleep(10);

                regData[i] = capture.GetREGStatus();
                text += "// " + i.ToString() + ",0x" + regData[i].ToString("X4") + ",\n";
            }

            
            System.IO.File.WriteAllText(@"C:\leon\regdump.txt", text);
            */
            // capture one image using uvc extension
            if (capture.cameraModel == LPCamera.CameraModel.KEURIG_SPI)
            {
                int maxCaptureLatency = 0;
                //while (true)
                {
                    Application.DoEvents();

                    //capture.SetRegRW(0, 0x0003, 0x0000); // capture image

                    capture.SetRegRW(0, 0x0007, 0x0000); // take image

                    System.Threading.Thread.Sleep(50);

                    int CaptureLatency = capture.GetREGStatus();
                    if (CaptureLatency > maxCaptureLatency)
                        maxCaptureLatency = CaptureLatency;

                    toolStripStatusLabelFPN.Text = "Latency = " + CaptureLatency.ToString() + " ms"
                                            + " Max Latency = " + maxCaptureLatency.ToString() + " ms";

                    capture.SetRegRW(0, 0x0008, 0x0000); // tranfer Image
                    {
                        byte[] imageData;
                        bufIndex = 0;
                        do
                        {
                            capture.ReadCamDefectPixelTable(out imageData);

                            // byte 33 is the valid date length in this packet
                            validLen = (int)imageData[32];
                            if (validLen != 0)
                                Array.Copy(imageData, 0, imageBuf, bufIndex, validLen);
                            else
                                break;

                            bufIndex += validLen;

                        } while (validLen != 0);

                        capture.SetRegRW(0, 0x0004, 0x0000); // end

                        imageBufF = new byte[bufIndex];
                        Array.Copy(imageBuf, 0, imageBufF, 0, bufIndex);
                        Array.Copy(imageBuf, 0, savedImageBuf, 0, bufIndex);

                        try
                        {
                            MemoryStream ms = new MemoryStream(imageBufF);
                            Image image = System.Drawing.Image.FromStream(ms);

                            Bitmap bitmap = new Bitmap(image);
                            DisplayImage(bitmap, pictBDisplay);

                            bitmap.Dispose();
                            statusToolStripStatusLabel.Text = "Image szie = " + bufIndex.ToString();
                        }
                        catch
                        {
                            statusToolStripStatusLabel.Text = "Image error";
                        }
                    }
                }

                SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                saveFileDialog1.Filter = "JPG files|*.jpg|All files (*.*)|*.*";

                saveFileDialog1.FilterIndex = 1;
                saveFileDialog1.RestoreDirectory = true;
                saveFileDialog1.FileName = CameraUUID + ".jpg";

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllBytes(saveFileDialog1.FileName, imageBufF);
                }

            }
            else
            {
                capturedFrame = 0;
                m_CaptureImages = true;
            }
        }


        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SofwareRevision rev = new SofwareRevision();

            string message =
                "Camera Tool for Leopard Imaging USB3.0 Cameras\n" +
                "Revision " + rev.revision.ToString() + "\n"
              + "Leopard Imaging, Inc. 2017";
            const string caption = "About";
            var result = MessageBox.Show(message, caption);

        }

        private void noDisplayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mRAWDisplay)
            {
                mRAWDisplay = false;
                ToolStripMenuItem item = (ToolStripMenuItem)noDisplayToolStripMenuItem;
                item.Checked = true;
            }
            else
            {
                mRAWDisplay = true;
                ToolStripMenuItem item = (ToolStripMenuItem)noDisplayToolStripMenuItem;
                item.Checked = false;
            }
        }
/*
        private void triggerModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (capture != null)
            {
                if (m_TriggerMode)
                {
                    capture.EnableTriggerMode(false);
                    m_TriggerMode = false;
                    ToolStripMenuItem item = (ToolStripMenuItem)triggerModeToolStripMenuItem;
                    item.Checked = false;

                    item = (ToolStripMenuItem)autoTriggerToolStripMenuItem;
                    item.Checked = false;

                    softTriggerToolStripMenuItem.Enabled = false;
                    captureImageToolStripMenuItem.Enabled = true;
                    autoTriggerToolStripMenuItem.Enabled = false;
                    m_AutoTrigger = false;
                }
                else
                {
                    capture.EnableTriggerMode(true);
                    m_TriggerMode = true;

                    ToolStripMenuItem item = (ToolStripMenuItem)triggerModeToolStripMenuItem;
                    item.Checked = true;

                    softTriggerToolStripMenuItem.Enabled = true;
                    captureImageToolStripMenuItem.Enabled = false;
                    autoTriggerToolStripMenuItem.Enabled = true;
                }
            }
        }
*/
        private void softTriggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (capture != null)
            {
                if (m_TriggerMode)
                {
                    // set exposure & gain before trigger
                    //capture.ExposureExt = 100;
                    //capture.Gain = 1;

                    capture.SoftTrigger();
                    m_CaptureImages = true;
                }

            }
        }

        private void monoSensorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (capture != null)
            {
                if (m_MonoSensor)
                {
                    m_MonoSensor = false;
                    ToolStripMenuItem item = (ToolStripMenuItem)monoSensorToolStripMenuItem;
                    item.Checked = false;

                    pixelOrderToolStripMenuItem.Enabled = true;
                }
                else
                {
                    m_MonoSensor = true;
                    ToolStripMenuItem item = (ToolStripMenuItem)monoSensorToolStripMenuItem;
                    item.Checked = true;

                    pixelOrderToolStripMenuItem.Enabled = false;
                }
            }
        }
        private void gBRGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem)  //Check On Click.
            {
                foreach (ToolStripMenuItem item in (((ToolStripMenuItem)sender).GetCurrentParent().Items))
                {
                    if (item == sender)
                    {
                        item.Checked = true;
                        m_pixelOrder = PIXEL_ORDER.GBRG;
                    }
                    if ((item != null) && (item != sender))
                        item.Checked = false;
                }
            }

        }

        private void gRBGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem)  //Check On Click.
            {
                foreach (ToolStripMenuItem item in (((ToolStripMenuItem)sender).GetCurrentParent().Items))
                {
                    if (item == sender)
                    {
                        item.Checked = true;
                        m_pixelOrder = PIXEL_ORDER.GRBG;
                    }
                    if ((item != null) && (item != sender))
                        item.Checked = false;
                }
            }
        }

        private void bGGRToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem)  //Check On Click.
            {
                foreach (ToolStripMenuItem item in (((ToolStripMenuItem)sender).GetCurrentParent().Items))
                {
                    if (item == sender)
                    {
                        item.Checked = true;
                        m_pixelOrder = PIXEL_ORDER.BGGR;
                    }
                    if ((item != null) && (item != sender))
                        item.Checked = false;
                }
            }
        }

        private void rGBGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem)  //Check On Click.
            {
                foreach (ToolStripMenuItem item in (((ToolStripMenuItem)sender).GetCurrentParent().Items))
                {
                    if (item == sender)
                    {
                        item.Checked = true;
                        m_pixelOrder = PIXEL_ORDER.RGBG;
                    }
                    if ((item != null) && (item != sender))
                        item.Checked = false;
                }
            }
        }

        private void autoTriggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_AutoTrigger)
            {
                m_AutoTrigger = false;
                softTriggerToolStripMenuItem.Enabled = true;

                ToolStripMenuItem item = (ToolStripMenuItem)autoTriggerToolStripMenuItem;
                item.Checked = false;
            }
            else
            {
                m_AutoTrigger = true;
                softTriggerToolStripMenuItem.Enabled = false;

                ToolStripMenuItem item = (ToolStripMenuItem)autoTriggerToolStripMenuItem;
                item.Checked = true;

                capture.SoftTrigger();
            }
        }

        private void showAnchorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_Show_Anchors)
            {
                m_Show_Anchors = false;

                ToolStripMenuItem item = (ToolStripMenuItem)showAnchorsToolStripMenuItem;
                item.Checked = false;

                pictureBoxCenter.Visible = false;
                pictureBoxTopLeft.Visible = false;
                pictureBoxTopRight.Visible = false;
                pictureBoxBottomLeft.Visible = false;
                pictureBoxBottomRight.Visible = false;
            }
            else
            {
                m_Show_Anchors = true;

                ToolStripMenuItem item = (ToolStripMenuItem)showAnchorsToolStripMenuItem;
                item.Checked = true;

                if (m_SensorDataMode == LeopardCamera.LPCamera.SENSOR_DATA_MODE.YUV
                    || m_SensorDataMode == LeopardCamera.LPCamera.SENSOR_DATA_MODE.YUV_DUAL)
                {
                    pictureBoxCenter.Visible = true;
                    pictureBoxTopLeft.Visible = true;
                    pictureBoxTopRight.Visible = true;
                    pictureBoxBottomLeft.Visible = true;
                    pictureBoxBottomRight.Visible = true;
                }
            }
        }

        private void regRWModeSETToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmRegRW_MODESET.Show();
        }

        public static ICollection<LeopardPlugin> LoadPlugins(string path)
        {
            string[] dllFileNames = null;

            if (Directory.Exists(path))
            {
                dllFileNames = Directory.GetFiles(path, "*.dll");

                ICollection<Assembly> assemblies = new List<Assembly>(dllFileNames.Length);
                foreach (string dllFile in dllFileNames)
                {
                    AssemblyName an = AssemblyName.GetAssemblyName(dllFile);
                    Assembly assembly = Assembly.Load(an);
                    assemblies.Add(assembly);
                }

                Type pluginType = typeof(LeopardPlugin);
                ICollection<Type> pluginTypes = new List<Type>();
                foreach (Assembly assembly in assemblies)
                {
                    if (assembly != null)
                    {
                        Type[] types = assembly.GetTypes();

                        foreach (Type type in types)
                        {
                            if (type.IsInterface || type.IsAbstract)
                            {
                                continue;
                            }
                            else
                            {
                                if (type.GetInterface(pluginType.FullName) != null)
                                {
                                    pluginTypes.Add(type);
                                }
                            }
                        }
                    }
                }

                ICollection<LeopardPlugin> plugins = new List<LeopardPlugin>(pluginTypes.Count);
                foreach (Type type in pluginTypes)
                {
                    LeopardPlugin plugin = (LeopardPlugin)Activator.CreateInstance(type);
                    plugins.Add(plugin);
                }

                return plugins;
            }

            return null;
        }

        private void fontDemoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem)  //Check On Click.
            {
                foreach (ToolStripMenuItem item in (((ToolStripMenuItem)sender).GetCurrentParent().Items))
                {
                    if (item == sender)
                    {
                        item.Checked = true;
                        m_EmguDemoId = EmguTool.EmguDemo.EmguDemoId.FontDemo;
                    }
                    if ((item != null) && (item != sender))
                        item.Checked = false;
                }
            }

        }

        private void fDDemoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem)  //Check On Click.
            {
                foreach (ToolStripMenuItem item in (((ToolStripMenuItem)sender).GetCurrentParent().Items))
                {
                    if (item == sender)
                    {
                        item.Checked = true;
                        m_EmguDemoId = EmguTool.EmguDemo.EmguDemoId.FDDemo;
                    }
                    if ((item != null) && (item != sender))
                        item.Checked = false;
                }
            }
        }

        private void disableDemoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem)  //Check On Click.
            {
                foreach (ToolStripMenuItem item in (((ToolStripMenuItem)sender).GetCurrentParent().Items))
                {
                    if (item == sender)
                    {
                        item.Checked = true;
                        m_EmguDemoId = EmguTool.EmguDemo.EmguDemoId.DisableDemo;
                    }
                    if ((item != null) && (item != sender))
                        item.Checked = false;
                }
            }
        }

        private void setTriggerDelayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmSetTriggerDelay.Show();
        }

        private void handleTriggerDelayTime()
        {
            if (frmSetTriggerDelay.DelayTime != m_delayTime)
            {
                m_delayTime = frmSetTriggerDelay.DelayTime;
                if (capture != null)
                    capture.SetTriggerDelayTime((uint)m_delayTime);
            }
        }
        private void handleRegRW_aray()
        {
            if (frmRegRW_MODESET.Write_array_Triggered)
            {
                for (int i = 0; i < frmRegRW_MODESET.array_length; i++)
                {
                    capture.SetRegRW(1, 0xf000+i, frmRegRW_MODESET.array_data[i]);
                    Thread.Sleep(50);
                }

                frmRegRW_MODESET.Write_array_Triggered = false;

            }
            if (frmRegRW_MODESET.Read_array_Triggered)
            {
                frmRegRW_MODESET.Read_array_Triggered = false;
                for (int i = 0; i < frmRegRW_MODESET.array_length; i++)
                {
                    R_address = 0xf000+i;

                    capture.SetRegRW(0, R_address, R_value);

                    //System.Threading.Thread.Sleep(500);
                    DateTime tempTime = DateTime.Now;
                    while (tempTime.AddMilliseconds(500).CompareTo(DateTime.Now) > 0)
                        Application.DoEvents();

                    R_value = capture.GetREGStatus();

                    frmRegRW_MODESET.array_data[i] = (byte)R_value;

                }

                frmRegRW_MODESET.Read_array_Triggered = false;
            }
        }
        private void handleRegRW_MODESET()
        {
            //only active when Write or Read btn is clicked
            if (frmRegRW_MODESET.Write_Triggered)
            {
                //if ((frmRegRW_MODESET.WReg_Address != W_address) || (frmRegRW_MODESET.WReg_Value != W_value))
                {
                    W_address = frmRegRW_MODESET.WReg_Address;
                    W_value = frmRegRW_MODESET.WReg_Value;

                    capture.SetRegRW(1, W_address, W_value);
                }
                frmRegRW_MODESET.Write_Triggered = false;
            }
            if (frmRegRW_MODESET.Read_Triggered)
            {

                frmRegRW_MODESET.Read_Triggered = false;

                R_address = frmRegRW_MODESET.RReg_Address;

                capture.SetRegRW(0, R_address, R_value);

                //System.Threading.Thread.Sleep(500);
                DateTime tempTime = DateTime.Now;
                while (tempTime.AddMilliseconds(500).CompareTo(DateTime.Now) > 0)
                    Application.DoEvents();

                R_value = frmRegRW_MODESET.RReg_Value = capture.GetREGStatus();
                frmRegRW_MODESET.Update_R_Value(R_value);

                //MessageBox.Show("read back:0x" + R_value.ToString("x"));
                frmRegRW_MODESET.Read_Triggered = false;		 
            }

            if (SensorMode != frmRegRW_MODESET.sensormode)
            {
                SensorMode = frmRegRW_MODESET.sensormode;
                //MessageBox.Show("SensorMode:" + SensorMode.ToString());
                capture.SetSensorMode(SensorMode);
            }
            if (Roi_Level != frmRegRW_MODESET.roi_level)
            {
                Roi_Level = frmRegRW_MODESET.roi_level;
                //MessageBox.Show("SensorMode:" + SensorMode.ToString());
                capture.SetROI_Level(Roi_Level);
            }
            if ((ROI_StartX != frmRegRW_MODESET.ROI_StartX) || (ROI_StartY != frmRegRW_MODESET.ROI_StartY))
            {
                ROI_StartX = frmRegRW_MODESET.ROI_StartX;
                ROI_StartY = frmRegRW_MODESET.ROI_StartY;
                capture.SetPOS(ROI_StartX, ROI_StartY);
            }
        }

        private void positiveEdgeToolStripMenuItem_Click(object sender, EventArgs e)
        {

            ToolStripMenuItem item1 = (ToolStripMenuItem)negativeEdgeToolStripMenuItem;

            if (capture != null)
            {
                if (m_TriggerMode)
                {
                    if (!item1.Checked)
                    {
                        capture.EnableTriggerMode(false, false);
                        m_TriggerMode = false;
                        ToolStripMenuItem item = (ToolStripMenuItem)positiveEdgeToolStripMenuItem;
                        item.Checked = false;

                        item = (ToolStripMenuItem)autoTriggerToolStripMenuItem;
                        item.Checked = false;

                        softTriggerToolStripMenuItem.Enabled = false;
                        captureImageToolStripMenuItem.Enabled = true;
                        autoTriggerToolStripMenuItem.Enabled = false;
                        m_AutoTrigger = false;
                    }
                }
                else
                {
                    capture.EnableTriggerMode(true,true);//positive edge
                    m_TriggerMode = true;

                    ToolStripMenuItem item = (ToolStripMenuItem)positiveEdgeToolStripMenuItem;
                    item.Checked = true;
                    item1.Checked = false;

                    softTriggerToolStripMenuItem.Enabled = true;
                    captureImageToolStripMenuItem.Enabled = false;
                    autoTriggerToolStripMenuItem.Enabled = true;
                }
            }
        }

        private void negativeEdgeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item1 = (ToolStripMenuItem)positiveEdgeToolStripMenuItem;

            if (capture != null)
            {
                if (m_TriggerMode)
                {
                    if(!item1.Checked)
                    {
                    capture.EnableTriggerMode(false,false);
                    m_TriggerMode = false;
                    ToolStripMenuItem item = (ToolStripMenuItem)negativeEdgeToolStripMenuItem;
                    item.Checked = false;

                    item = (ToolStripMenuItem)autoTriggerToolStripMenuItem;
                    item.Checked = false;

                    softTriggerToolStripMenuItem.Enabled = false;
                    captureImageToolStripMenuItem.Enabled = true;
                    autoTriggerToolStripMenuItem.Enabled = false;
                    m_AutoTrigger = false;
                    }
                }
                else
                {
                    capture.EnableTriggerMode(true,false);//negative edge
                    m_TriggerMode = true;

                    ToolStripMenuItem item = (ToolStripMenuItem)negativeEdgeToolStripMenuItem;
                   
                    item.Checked = true;
                    item1.Checked = false;

                    softTriggerToolStripMenuItem.Enabled = true;
                    captureImageToolStripMenuItem.Enabled = false;
                    autoTriggerToolStripMenuItem.Enabled = true;
                }
            }
        }

        private void cameraPropWinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (capture != null)
            {
                CameraProperty cGain = new CameraProperty();
                CameraProperty cExposure = new CameraProperty();

                capture.GetVideoProcAmpPropertyRange(DirectShowLib.VideoProcAmpProperty.Gain, out cGain.Min, out cGain.Max, out cGain.Step, out cGain.Default);
                if (capture.Gain < cGain.Min)
                    capture.Gain = cGain.Min;
                else if (capture.Gain > cGain.Max)
                    capture.Gain = cGain.Max;

                cGain.curValue = capture.Gain;

                // YUV cameras have AE information on it
                if (m_SensorDataMode == LeopardCamera.LPCamera.SENSOR_DATA_MODE.YUV
                    || m_SensorDataMode == LeopardCamera.LPCamera.SENSOR_DATA_MODE.YUV_DUAL)
                    m_curAE = capture.AE;
                else // non YUV cameras don't store AE information, so we restore it from m_AutoExposure
                    m_curAE = m_AutoExposure;

                capture.GetCameraControlPropertyRange(DirectShowLib.CameraControlProperty.Exposure, out cExposure.Min, out cExposure.Max, out cExposure.Step, out cExposure.Default);
                if (capture.Exposure < cExposure.Min)
                    capture.Exposure = cExposure.Min;
                else if (capture.Exposure > cExposure.Max)
                    capture.Exposure = cExposure.Max;

                cExposure.curValue = capture.Exposure;
                m_curExp = cExposure.curValue;

                frmCameraPropWin.AE = m_curAE;
                frmCameraPropWin.UpdateValue(cGain, m_curAE, cExposure, m_curExpTimeInLines);
            }
            frmCameraPropWin.Show();
        }

        private void handleCameraPropWin()
        {
            // update current gain from CameraPropWin
            if (m_curGain != frmCameraPropWin.Gain.curValue && !m_curAE)
            {
                capture.Gain = frmCameraPropWin.Gain.curValue;
                m_curGain = frmCameraPropWin.Gain.curValue;
            }

            // update current AE mode from CameraPropWin
            if (m_curAE != frmCameraPropWin.AE)
            {
                capture.AE = frmCameraPropWin.AE;
                m_curAE = frmCameraPropWin.AE;

                if (!m_curAE) // when it comes back to manual mode, set the parameters
                {
                    capture.Gain = frmCameraPropWin.Gain.curValue;
                    m_curGain = frmCameraPropWin.Gain.curValue;

                    capture.Exposure = frmCameraPropWin.Exposure.curValue;
                    m_curExp = frmCameraPropWin.Exposure.curValue;

                    m_curExpTimeInLines = frmCameraPropWin.ExpTime;
                    capture.ExposureExt = m_curExpTimeInLines;
                }
            }

            // to enable/disable auto exposure in software if it is not YUV 
            if (m_SensorDataMode != LeopardCamera.LPCamera.SENSOR_DATA_MODE.YUV
                && m_SensorDataMode != LeopardCamera.LPCamera.SENSOR_DATA_MODE.YUV_DUAL)
                m_AutoExposure = m_curAE;

            // update current expsoure from CameraPropWin
            if (m_curExp != frmCameraPropWin.Exposure.curValue && !m_curAE)
            {
                capture.Exposure = frmCameraPropWin.Exposure.curValue;
                m_curExp = frmCameraPropWin.Exposure.curValue;
            }

            // update current expsoure time ( lines) from CameraPropWin
            if (frmCameraPropWin.ExpTime != m_curExpTimeInLines && !m_curAE)
            {
                m_curExpTimeInLines = frmCameraPropWin.ExpTime;
                capture.ExposureExt = m_curExpTimeInLines;
            }
        }

        private Rectangle m_boxRect = new Rectangle( 0, 0, 0, 0 );
        private bool drawingRect = false, endofRect = false;
        private void pictBDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            toolStripStatusLabelPos.Text = e.Location.X + ":" + e.Location.Y + "." + pictBDisplay.Width.ToString() + ":" + pictBDisplay.Height.ToString();

            if (drawingRect && !endofRect)
            {
                if (e.Location.X > m_boxRect.X)
                {
                    m_boxRect.Width = e.Location.X - m_boxRect.X;
                }
                else
                {
                    m_boxRect.Width = m_boxRect.X - e.Location.X;
                    m_boxRect.X = e.Location.X;
                }

                if (e.Location.Y > m_boxRect.Y)
                {
                    m_boxRect.Height = e.Location.Y - m_boxRect.Y;
                }
                else
                {
                    m_boxRect.Height = m_boxRect.Y - e.Location.Y;
                    m_boxRect.Y = e.Location.Y;
                }
            }
            else if (m_boxRect.Width == 1 && m_boxRect.Height == 1)
            {
                m_boxRect.X = e.Location.X;
                m_boxRect.Y = e.Location.Y;
            }
        }

        private void pictBDisplay_MouseDown(object sender, MouseEventArgs e)
        {
            drawingRect = true;
            m_boxRect.X = e.Location.X;
            m_boxRect.Y = e.Location.Y;
            m_boxRect.Width = 1;
            m_boxRect.Height = 1;
        }

        private void pictBDisplay_MouseUp(object sender, MouseEventArgs e)
        {
            endofRect = true;
            if (m_boxRect.X == e.Location.X && m_boxRect.Y == e.Location.Y) // click on the same position, disable rect drawing
            {
                drawingRect = false;
                endofRect = false;
                m_boxRect.Width = 1;
                m_boxRect.Height = 1;
            }
        }

        private void parsePluginParam()
        {
            byte[] param;
            int pos = 0;

            if (capture == null)
                return;

            if (m_selectedPlugin != null)
            {
                param = m_selectedPlugin.SetParam();
                if (param != null)
                {
                    while(pos < param.Length)
                    {
                        switch ((PlugInParamType)param[pos])
                        {
                            case PlugInParamType.PI_SETGAIN:
                                pos++;
                                capture.Gain = param[pos];
                                break;
                            case PlugInParamType.PI_SETEXPOSURE:
                                pos++;
                                int exposureTime = ((int)param[pos] << 8) | (int)param[pos + 1];
                                pos+=2;
                                if (m_curExpTimeInLines != exposureTime)
                                {
                                    capture.ExposureExt = exposureTime;
                                    m_curExpTimeInLines = exposureTime;
                                }
                                break;
                            case PlugInParamType.PI_FPN:
                                pos++;
                                for (int i = 0; i < capture.Height; i++)
                                {
                                    int FPNvalue = ((int)param[pos] << 8) | (int)param[pos + 1];
                                    pos += 2;

                                    // write address to reg 5
                                    capture.SetRegRW(1, 5, i);
                                    // write data to reg 12
                                    capture.SetRegRW(1, 12, FPNvalue);
                                }

                                break;
                            default:
                                return;
                        }
                    }
                }
            }
        }

        private void CameraToolForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_selectedPlugin != null)
            {
                m_selectedPlugin.Close();
                m_selectedPlugin = null;
            }
        }

        private void autoExposureSoftwareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_AutoExposure)
            {
                m_AutoExposure = false;
                ToolStripMenuItem item = (ToolStripMenuItem)autoExposureSoftwareToolStripMenuItem;
                item.Checked = false;
            }
            else
            {
                m_AutoExposure = true;
                ToolStripMenuItem item = (ToolStripMenuItem)autoExposureSoftwareToolStripMenuItem;
                item.Checked = true;
            }

        }

        private void noiseCalculationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_NoiseCalculationEna)
            {
                m_NoiseCalculationEna = false;
                ToolStripMenuItem item = (ToolStripMenuItem)noiseCalculationToolStripMenuItem;
                item.Checked = false;
            }
            else
            {
                m_NoiseCalculationEna = true;
                ToolStripMenuItem item = (ToolStripMenuItem)noiseCalculationToolStripMenuItem;
                item.Checked = true;
            }
        }

        #region Flash update
        //  For AP0100 flash update
        //  data format : type, len, data[0], data[1] ...
        //  type: 1 byte, 0: soft reset, 1: read ( read length = data[0]). 2 : commands. 3 : update flash.
        //  len: length of data[x] ...

        private int RETRY_NUM = 10;

        private void programFlashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFile1 = new OpenFileDialog();
                openFile1.Filter = "bin files (*.bin)|*.bin|All files (*.*)|*.*";
                if (openFile1.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs = new FileStream(openFile1.FileName, FileMode.Open);
                    if (fs.Length > 0)
                    {
                        Byte[] fileBuf = new Byte[fs.Length];
                        fs.Read(fileBuf, 0, fileBuf.Length);

                        programFlash(fileBuf);
                    }
                    fs.Close();
                }
                soft_reset();
                Thread.Sleep(500);
                readFuseID();

                System.Media.SystemSounds.Beep.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private enum AP0100_CMD {
	        CMD_GET_LOCK 		= 0x8500,
	        CMD_LOCK_STATUS 	= 0x8501,
	        CMD_RELEASE_LOCK	= 0x8502,
	        CMD_WRITE		    = 0x8505,
	        CMD_ERASE_BLOCK		= 0x8506,
	        CMD_QUERY_DEV		= 0x8508,
	        CMD_FLASH_STATUS	= 0x8509,
	        CMD_CONFIG_DEV		= 0x850a,
            CMD_CCIMGR_GET_LOCK = 0x8D00,
            CMD_CCIMGR_LOCK_STATUS  = 0x8D01,
            CMD_CCIMGR_RELEASE_LOCK = 0x8D02,
            CMD_CCIMGR_READ     = 0x8D05,
            CMD_CCIMGR_STATUS   = 0x8D08,
        };

        private Byte [] buffer_s = new Byte [33];
        private int cmd_start_pos = 2;
        private int send_command(AP0100_CMD cmd, int time_out)
        {
            Byte[] r_buf = new Byte[8];
            int cmd_size = cmd_start_pos + 4;
            int retry;

            buffer_s[0] = 2; // type : 2, command
            buffer_s[1] = 4; // len : 4 bytes
            buffer_s[cmd_start_pos] = 0x00;
            buffer_s[cmd_start_pos + 1] = 0x40;
            buffer_s[cmd_start_pos + 2] = (byte)(((int)cmd >> 8) & 0x00ff);
            buffer_s[cmd_start_pos + 3] = (byte)((int)cmd & 0x00ff);

            retry = 0;
            while (retry < time_out)
            {
                capture.WriteCamDefectPixelTable(buffer_s);

                capture.ReadCamDefectPixelTable(out r_buf);

                if ((r_buf[0] == 0x00) && (r_buf[1] == 0x00)) // doorbell cleared
                {
                    break;
                }
                retry++;
                System.Threading.Thread.Sleep(10);
            }

            if (retry == time_out)
            {
                string erroMsg = "Error: " + cmd.ToString() + " didn't go through";
                throw new Exception(erroMsg);
            }

            return 0;
        }

        // software reset
        private void soft_reset()
        {
            byte[] buffer_s = new byte[4];

            buffer_s[0] = 0;
            buffer_s[1] = 0;
            buffer_s[2] = 0;
            buffer_s[3] = 0;
            capture.WriteCamDefectPixelTable(buffer_s);
        }

        // read data from register {addrH, addrL}
        private void read_data(byte addrH, byte addrL, int count, out byte[] r_buf)
        {
            byte[] buffer_s = new byte[4];

            buffer_s[0] = 1;
            buffer_s[1] = (byte)count;
            buffer_s[2] = addrH;
            buffer_s[3] = addrL;
            capture.WriteCamDefectPixelTable(buffer_s);

            capture.ReadCamDefectPixelTable(out r_buf);
        }

        private void write_raw_data(byte[] bufferIn)
        {
            capture.WriteCamDefectPixelTable(bufferIn);
        }

        private void write_data(byte[] buf, int buf_size, int pos)
        {

            if (buf_size > 16)
            {
                return;
            }

            buffer_s[0] = 3;
            buffer_s[1] = (byte)(buf_size + 7 + 2);
            buffer_s[cmd_start_pos + 0] = 0xfc;
            buffer_s[cmd_start_pos + 1] = 0x00;
            buffer_s[cmd_start_pos + 2] = (byte)((pos >> 24) & 0x00ff);
            buffer_s[cmd_start_pos + 3] = (byte)((pos >> 16) & 0x00ff);
            buffer_s[cmd_start_pos + 4] = (byte)((pos >> 8) & 0x00ff);
            buffer_s[cmd_start_pos + 5] = (byte)((pos) & 0x00ff);
            buffer_s[cmd_start_pos + 6] = (byte)(buf_size);

            for (int i = 0; i < buf_size; i++)
            {
                buffer_s[cmd_start_pos + 7 + i] = buf[pos+i];
            }

            write_raw_data(buffer_s);
        }

        private void programFlash(Byte[] buffer_bin)
        {
            int pos = 0;
            int page_remaining = 0;
            int steps = 0;
            byte[] r_buf;

            FlashUpdateInProgress = true;
            flashUpdatePercentage = 0;
            try
            {
                soft_reset();
                Thread.Sleep(800);
                read_data(0x00, 0x00, 2, out r_buf); // ID
                Thread.Sleep(50);
                send_command(AP0100_CMD.CMD_GET_LOCK, 5);
                Thread.Sleep(50);
                send_command(AP0100_CMD.CMD_LOCK_STATUS, 5);
                Thread.Sleep(50);
                buffer_s[0] = 2; // write data
                buffer_s[1] = (byte)(cmd_start_pos + 10);
                buffer_s[cmd_start_pos] = 0xfc; buffer_s[cmd_start_pos + 1] = 0x00;
                buffer_s[cmd_start_pos + 2] = 0x04; buffer_s[cmd_start_pos + 3] = 0x00;
                buffer_s[cmd_start_pos + 4] = 0x03; buffer_s[cmd_start_pos + 5] = 0x18;
                buffer_s[cmd_start_pos + 6] = 0x00; buffer_s[cmd_start_pos + 7] = 0x01;
                buffer_s[cmd_start_pos + 8] = 0x00; buffer_s[cmd_start_pos + 9] = 0x00;
                write_raw_data(buffer_s);
                Thread.Sleep(50);

                send_command(AP0100_CMD.CMD_CONFIG_DEV, 5);
                Thread.Sleep(50); 
                send_command(AP0100_CMD.CMD_RELEASE_LOCK, 5);
                Thread.Sleep(50);

                buffer_s[0] = 2; // write data
                buffer_s[1] = (byte)(cmd_start_pos + 18);
                buffer_s[cmd_start_pos] = 0xfc; buffer_s[cmd_start_pos + 1] = 0x00;
                buffer_s[cmd_start_pos + 2] = 0x00; buffer_s[cmd_start_pos + 3] = 0x00;
                buffer_s[cmd_start_pos + 4] = 0x00; buffer_s[cmd_start_pos + 5] = 0x00;
                buffer_s[cmd_start_pos + 6] = 0x00; buffer_s[cmd_start_pos + 7] = 0x00;
                buffer_s[cmd_start_pos + 8] = 0x00; buffer_s[cmd_start_pos + 9] = 0x00;
                buffer_s[cmd_start_pos + 10] = 0x00; buffer_s[cmd_start_pos + 11] = 0x00;
                buffer_s[cmd_start_pos + 12] = 0x00; buffer_s[cmd_start_pos + 13] = 0x00;
                buffer_s[cmd_start_pos + 14] = 0x00; buffer_s[cmd_start_pos + 15] = 0x00;
                buffer_s[cmd_start_pos + 16] = 0x00; buffer_s[cmd_start_pos + 17] = 0x00;
                write_raw_data(buffer_s);
                Thread.Sleep(50);

                send_command(AP0100_CMD.CMD_GET_LOCK, 5);
                Thread.Sleep(50); 
                send_command(AP0100_CMD.CMD_LOCK_STATUS, 5);
                Thread.Sleep(50); 
                send_command(AP0100_CMD.CMD_QUERY_DEV, 5);
                Thread.Sleep(50); 
                send_command(AP0100_CMD.CMD_FLASH_STATUS, 5);
                Thread.Sleep(50);
                read_data(0xfc, 0x00, 16, out r_buf);
                Thread.Sleep(50);

                buffer_s[0] = 2; // write data
                buffer_s[1] = (byte)(cmd_start_pos + 6);
                buffer_s[cmd_start_pos] = 0xfc; buffer_s[cmd_start_pos + 1] = 0x00;
                buffer_s[cmd_start_pos + 2] = 0x00; buffer_s[cmd_start_pos + 3] = 0x00;
                buffer_s[cmd_start_pos + 4] = 0x00; buffer_s[cmd_start_pos + 5] = 0x00;
                write_raw_data(buffer_s);

                // erase flash
                send_command(AP0100_CMD.CMD_ERASE_BLOCK, 5);
                Thread.Sleep(1000);
                send_command(AP0100_CMD.CMD_FLASH_STATUS, 1000);
                Thread.Sleep(50);

                int buf_size = buffer_bin.Length;
                int PACKET_SIZE = 11;
                pos = 0;

                while (pos < buf_size)
                {
                    if (buf_size - pos > PACKET_SIZE)
                    {
                        page_remaining = 0x0100 - (pos & 0x00ff);

                        if (page_remaining > PACKET_SIZE)
                        {
                            write_data(buffer_bin, PACKET_SIZE, pos);
                            pos += PACKET_SIZE;
                        }
                        else
                        {
                            write_data(buffer_bin, page_remaining, pos);
                            pos += page_remaining;
                        }
                    }
                    else
                    {
                        write_data(buffer_bin, buf_size - pos, pos);
                        pos = buf_size;
                    }
                    send_command(AP0100_CMD.CMD_WRITE, 50);
                    send_command(AP0100_CMD.CMD_FLASH_STATUS, 50);

                    if (pos > buf_size * steps / 100)
                    {
                        steps++;
                    }
                    flashUpdatePercentage = pos * 100 / buf_size;
                    Application.DoEvents();
                }

                Thread.Sleep(50);
                send_command(AP0100_CMD.CMD_RELEASE_LOCK, 5);
                Thread.Sleep(50);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                FlashUpdateInProgress = false;
            }
            return;
        }

        private void readFuseID()
        {
            byte[] r_buf;

            Thread.Sleep(50);
            send_command(AP0100_CMD.CMD_CCIMGR_GET_LOCK, 5);
            Thread.Sleep(50);
            send_command(AP0100_CMD.CMD_CCIMGR_LOCK_STATUS, 5);
            Thread.Sleep(50);

            buffer_s[0] = 2; // write data
            buffer_s[1] = (byte)(cmd_start_pos + 4);
            buffer_s[cmd_start_pos] = 0xfc; buffer_s[cmd_start_pos + 1] = 0x00;
            buffer_s[cmd_start_pos + 2] = 0x31; buffer_s[cmd_start_pos + 3] = 0xF4;
            write_raw_data(buffer_s);
            Thread.Sleep(50);

            buffer_s[0] = 2; // write data
            buffer_s[1] = (byte)(cmd_start_pos + 3);
            buffer_s[cmd_start_pos] = 0xfc; buffer_s[cmd_start_pos + 1] = 0x02;
            buffer_s[cmd_start_pos + 2] = 0x08; 
            write_raw_data(buffer_s);
            Thread.Sleep(50);

            send_command(AP0100_CMD.CMD_CCIMGR_READ, 5);
            Thread.Sleep(50); 
            send_command(AP0100_CMD.CMD_CCIMGR_STATUS, 5);
            Thread.Sleep(50);

            read_data(0xfc, 0x00, 8, out r_buf);
            Thread.Sleep(50); 
            
            FuseID = "";
            for (int i = 0; i < 8; i++)
            {
                FuseID += r_buf[i].ToString("X2");
            }

            return;
        }

        #endregion 

        private void configurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            configurationForm.Show();
        }

        private void LyftCfgMenuItem_Click(object sender, EventArgs e)
        {
            lyftConfigForm.ShowDialog();
        }

        private void updateParamFromConfiguration(object sender, EventArgs e)
        {
            m_FocusProcDownSample = configurationForm.FocusProcDownSampling;

            m_GammaEna = configurationForm.GammaEna;
            m_GammaValue = configurationForm.GammaValue;

            m_RGBGainOffsetEna = configurationForm.RGBGainOffsetEna;
            r_gain = configurationForm.RGain;
            g_gain = configurationForm.GGain;
            b_gain = configurationForm.BGain;
            r_offset = configurationForm.ROffset;
            g_offset = configurationForm.GOffset;
            b_offset = configurationForm.BOffset;

            m_RGB2RGBMatrixEna = configurationForm.RGB2RGBMatrixEna;
            matrix_rr = configurationForm.MartixRR;
            matrix_rg = configurationForm.MartixRG;
            matrix_rb = configurationForm.MartixRB;
            matrix_gr = configurationForm.MartixGR;
            matrix_gg = configurationForm.MartixGG;
            matrix_gb = configurationForm.MartixGB;
            matrix_br = configurationForm.MartixBR;
            matrix_bg = configurationForm.MartixBG;
            matrix_bb = configurationForm.MartixBB;

            mCaptureFrameNum = configurationForm.CaptureNum;
            
        }

        private void UpdateParamFromLyftConfig(object sender, EventArgs e)
        {
            Debug.Print("got {0} event from {1}", e.GetType(), sender.GetType());
            if (e.GetType() == typeof(System.Windows.Forms.MouseEventArgs))
            {
                if (sender.Equals(this.lyftConfigForm.Reset_Gain))
                {
                    m_RGBGainOffsetEna = false;
                    return;
                }

                if (sender.Equals(this.lyftConfigForm.Reset_Gamma))
                {
                    m_GammaEna = false;
                    return;
                }
            }

            m_RGBGainOffsetEna = true;
            if (lyftConfigForm.GainUpdated)
            {
                r_gain = lyftConfigForm.RGain;
                g_gain = lyftConfigForm.GGain;
                b_gain = lyftConfigForm.BGain;
            }
            if (lyftConfigForm.OffsetUpdated)
            {
                r_offset = lyftConfigForm.ROffset;
                g_offset = lyftConfigForm.GOffset;
                b_offset = lyftConfigForm.BOffset;
            }

            if (lyftConfigForm.GammaUpdated)
            {
                m_GammaValue = lyftConfigForm.Gamma;
            }
        }

        private void updateRegisterSettingFromConfiguration(LeopardCamera.LPCamera.CameraModel cameraid)
        {
            int registerNum = 64;
            int i = 0, j = 0;
            int xmlregcount = 0, flashregcount = 0;
            bool registerBegin = false;
            int readlength;
            int shouldupdate = 0;
            //int regAddr, regVal;
            int[] regAddr = new int[registerNum];
            int[] regVal = new int[registerNum];
            int[] outregAddr = new int[registerNum];
            int[] outregVal = new int[registerNum];
            string[,] data = new string[1, registerNum * 2];


            m_RegisterSetting = configurationForm.RegisterSetting;
            foreach (string word in m_RegisterSetting.Split(new char[] { ':', ' ', '{', '}', ';', ',', '(', ')' },
                        StringSplitOptions.RemoveEmptyEntries))
            {
                data[0, i] = word;
                i++;
            }


            // read sensor register configration from xml file
            for (j = 0; j < i; j++)
            {
                if (data[0, j] == cameraid.ToString())
                {
                    j++;
                    registerBegin = true;
                }

                if(registerBegin)
                {
                    if (data[0, j].Substring(0, 2) != "0x")
                        break;

                    regAddr[xmlregcount] = (int)(Convert.ToInt32(data[0, j], 16));
                    regVal[xmlregcount] = (int)(Convert.ToInt32(data[0, j + 1], 16));
                    xmlregcount++;
                    j += 1;
                 //   if(regAddr != 0xffff && regVal != 0xffff)
                   //     capture.SetRegRW(1, regAddr, regVal);
                }
            }

            byte[] buffer = new byte[256];
            int m = 64;// ((xmlregcount + 31) / 32) * 32;

            // flag
            buffer[0] = 0x11;
            buffer[1] = 0x22;
            buffer[2] = 0x33;
            buffer[3] = 0x44;

            // length : not include the first 8 bytes
            buffer[4] = (byte)xmlregcount;
            buffer[5] = (byte)(xmlregcount >> 8);
            buffer[6] = (byte)(xmlregcount >> 16);
            buffer[7] = (byte)(xmlregcount >> 24);

            for (i = 2; i < m; i++)
            {
                    int addr;
                    int val;

                    addr = regAddr[i-2];
                    val = regVal[i-2];

                    if (i <= xmlregcount)
                    {
                        if (addr != 0xffff && val != 0xffff)
                            capture.SetRegRW(1, addr, val);

                        buffer[4 * i] = (byte)addr;
                        buffer[4 * i + 1] = (byte)(addr >> 8);
                        buffer[4 * i + 2] = (byte)(val);
                        buffer[4 * i + 3] = (byte)(val >> 8);
                    }
                    else
                    {
                        buffer[4 * i] = 0xff;
                        buffer[4 * i + 1] = 0xff;
                        buffer[4 * i + 2] = 0xff;
                        buffer[4 * i + 3] = 0xff;
                    }

                if((i + 1) % 64 == 0)
                    capture.WriteSensorRegisterConfToFlash(buffer);
            }


           // byte[] outbuf1;
            //capture.ReadSensorRegisterConfFromFlash(out outbuf1);

#if false
            // there is no any register configuration to specifi
            if (xmlregcount == 0)
            { 
                byte[] buf = new byte[128];
                buf[0] = 0;
                buf[1] = 0;
                buf[2] = 0;
                buf[3] = 0;
                for (i = 4; i < 124; i++)
                    buf[i] = 0xff;

                capture.WriteSensorRegisterConfToFlash(buf);
                return;
            }

            // read sensor register configration from flash
            byte[] outbuf;
            capture.ReadSensorRegisterConfFromFlash(out outbuf);
            readlength = outbuf[0] | (outbuf[1] << 8) | (outbuf[2] << 16) | (outbuf[3] << 24);
            for (i = 4; i < 128; i++)
            { 
                int addr = outbuf[i] | outbuf[i + 1] << 8;
                int val = outbuf[i + 2] | outbuf[i + 3] << 8;
                outregAddr[flashregcount] = addr;
                outregVal[flashregcount] = val;
                flashregcount++;
                i += 4;
            }

            if (readlength > 31)
            {
                capture.ReadSensorRegisterConfFromFlash(out outbuf);
                for (i = 0; i < 128; i++)
                {
                    int addr = outbuf[i] | outbuf[i + 1] << 8;
                    int val = outbuf[i + 2] | outbuf[i + 3] << 8;
                    outregAddr[flashregcount] = addr;
                    outregVal[flashregcount] = val;
                    flashregcount++;
                    i += 4;
                }
            }

            // compare flash register and xml register
            for (i = 0; i < xmlregcount; i++ )
            {
                if (regAddr[xmlregcount] != outregAddr[xmlregcount] || regVal[xmlregcount] != outregVal[xmlregcount])
                {
                    shouldupdate = 1;
                    break;
                }
            }

            if (shouldupdate == 1)
            {
                byte[] buffer = new byte[128];
                int m = (xmlregcount + 1) / 32;
                int n = (xmlregcount + 1) % 32;

                for (i = 0; i < m; i++)
                {
                    if (i == 0)
                    {
                        buffer[0] = (byte)xmlregcount;
                        buffer[1] = (byte)(xmlregcount >> 8);
                        buffer[2] = (byte)(xmlregcount >> 16);
                        buffer[3] = (byte)(xmlregcount >> 24);
                    }

                    for (j = 0; j < 32; j++)
                    {
                        int addr;
                        int val;

                        addr = regAddr[i*32 + j];
                        val = regVal[i*32 + j];

                        if(addr != 0xffff && val != 0xffff)
                            capture.SetRegRW(1, addr, val);

                        buffer[4*j] = (byte)addr;
                        buffer[4 * j + 1] = (byte)(addr >> 8);
                        buffer[4 * j + 2] = (byte)(val);
                        buffer[4 * j + 3] = (byte)(val >> 8);
                    }

                    capture.WriteSensorRegisterConfToFlash(buffer);
                }
            }
#endif
        }

        private string i2cFileName = "BatchCmd.txt";
        private string i2cLogFile = "BatchCmdLog.txt";
        private void createI2CFile()
        {
            string i2cFileContent =   "# Batch Command file for CameraTool\r\n"
                                    + "# Copyright Leopard Imaging, Inc. 2017\r\n"
                                    + "# \r\n"
                                    + "# This file contains some commands, including I2C access, Delay, Capture.\r\n"
                                    + "# A line starting with a # is a comment line\r\n"
                                    + "# The first word of each line is a command, each line supports only one command\r\n"
                                    + "# Support for the following commands\r\n"
                                    + "#    Read, Write, SubAddress, RegAddress, RegAddrWidth, Delay\r\n"
                                    + "# \r\n"
                                    + "# command syntax:\r\n"
                                    + "# Read byte_count\r\n"
                                    + "# Write byte0 byte1 byte2 ... max 256 bytes\r\n"
                                    + "# SubAddress i2c_sub_address\r\n"
                                    + "# RegAddress register_address\r\n"
                                    + "# RegAddrWidth register_address_width(8 or 16)\r\n"
                                    + "# Delay delay_time_in_ms\r\n"
                                    + "# Capture frame_number file_name \r\n"
                                    + "# Display yes_or_no \r\n"
                                    + "# InterFrameDelay delay_time_in_ms_for_each_frame_during_capture\r\n"
                                    + "# \r\n"
                                    + "# examples: \r\n"
                                    + "# SubAddress 0x6c # device 8-bit I2C sub address is 0x6C \r\n"
                                    + "# RegAddrWidth 16 # 16-bit address \r\n"
                                    + "# RegAddress 0x000A # register address 0x000A \r\n"
                                    + "# Read 2 # read 2 bytes \r\n"
                                    + "# Write 0xA3 0x30 # write 0xA330 to register 0x000A \r\n"
                                    + "# Display no # Disable display to avoid frame drop during image capture \r\n"
                                    + "# Delay 100 # delay 100ms \r\n"
                                    + "# InterFrameDelay 10 # delay 10 ms after each frame is captured \r\n"
                                    + "# Capture 3 CapturedImage # capture 3 frames, the file name is CapturedImage_1.bmp, CapturedImage_2.bmp, and CapturedImage_3.bmp \r\n"
                                    + "# Display yes # enable Display\r\n";
            System.IO.File.WriteAllText(i2cFileName, i2cFileContent);
        }

        private void buttonBatchCmdLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            //openFileDialog1.InitialDirectory = "c:\\";
            //openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                i2cFileName = openFileDialog1.FileName;
                richTextBoxBatchCmd.AppendText("Loaded batch file: " + i2cFileName + "\r\n");
            }
        }

        private int InterFrameDelayTime = 0;
        private void buttonBatchCmdExec_Click(object sender, EventArgs e)
        {
            buttonBatchCmdExec.Enabled = false;

            int subAddr = 0, regAddr = 0, regAddrWidth = 16, delayTime = 0, readCount = 2, writeCount = 2;

            string[] i2cFileContent = System.IO.File.ReadAllLines(i2cFileName);

            foreach (var item in i2cFileContent)
            {
                string line = item.Trim();
                if (line.Contains('#'))
                {
                    int pos = line.IndexOf('#');
                    line = line.Remove(pos).Trim();
                }

                if (line.Length > 0)
                {
                    string[] words = line.Split(' ');

                    switch (words[0])
                    {
                        case "SubAddress":
                            if (words[1].Contains("0x"))
                                subAddr = (int)Convert.ToInt32(words[1], 16);
                            else
                                subAddr = (int)Convert.ToInt32(words[1], 10);
                            break;
                        case "RegAddress":
                            if (words[1].Contains("0x"))
                                regAddr = (int)Convert.ToInt32(words[1], 16);
                            else
                                regAddr = (int)Convert.ToInt32(words[1], 10);
                            break;
                        case "RegAddrWidth":
                            if (words[1].Contains("0x"))
                                regAddrWidth = (int)Convert.ToInt32(words[1], 16);
                            else
                                regAddrWidth = (int)Convert.ToInt32(words[1], 10);
                            break;
                        case "Delay":
                            if (words[1].Contains("0x"))
                                delayTime = (int)Convert.ToInt32(words[1], 16);
                            else
                                delayTime = (int)Convert.ToInt32(words[1], 10);

                            string delayString = "Delayed " + delayTime.ToString() + " ms\r\n";
                            System.IO.File.AppendAllText(i2cLogFile, delayString);
                            // update message window
                            richTextBoxBatchCmd.AppendText(delayString);

                            // sleep 10 ms each time, and let other threads run
                            while (delayTime > 10)
                            {
                                System.Threading.Thread.Sleep(10);
                                Application.DoEvents();
                                delayTime -= 10;
                            }
                            if (delayTime >= 0)
                                System.Threading.Thread.Sleep(delayTime);

                            break;
                        case "InterFrameDelay":
                            if (words[1].Contains("0x"))
                                InterFrameDelayTime = (int)Convert.ToInt32(words[1], 16);
                            else
                                InterFrameDelayTime = (int)Convert.ToInt32(words[1], 10);

                            string InterFrameDelayString = "Set Inter Frame Delay Time to " + InterFrameDelayTime.ToString() + " ms\r\n";
                            System.IO.File.AppendAllText(i2cLogFile, InterFrameDelayString);
                            // update message window
                            richTextBoxBatchCmd.AppendText(InterFrameDelayString);
                            break;
                        case "Read":
                            if (words[1].Contains("0x"))
                                readCount = (int)Convert.ToInt32(words[1], 16);
                            else
                                readCount = (int)Convert.ToInt32(words[1], 10);

                            if (readCount <= 0 || readCount > 256)
                                throw new Exception("I2C read count must be 1-256");

                            byte[] regBuf = new byte[256];

                            capture.I2CRegRW(regAddrWidth == 16 ? 0x02 : 0x01, readCount, subAddr, regAddr, regBuf);

                            string readString = "Read SubAddr=0x" + subAddr.ToString("X")
                                            + " RegAddr=0x" + regAddr.ToString("X")
                                            + " Count=" + readCount.ToString();

                            for (int i = 0; i < readCount; i++)
                            {
                                readString += " 0x" + regBuf[i].ToString("X2");
                            }

                            readString += "\r\n";
                            // update log file
                            System.IO.File.AppendAllText(i2cLogFile, readString);
                            // update message window
                            richTextBoxBatchCmd.AppendText(readString);
                            break;
                        case "Write":
                            byte[] regWBuf = new byte[256];

                            for (int i = 1; i < words.Length; i++)
                            {
                                if (words[i].Contains("0x"))
                                    regWBuf[i - 1] = (byte)Convert.ToByte(words[i], 16);
                                else
                                    regWBuf[i - 1] = (byte)Convert.ToByte(words[i], 10);
                            }

                            writeCount = words.Length - 1;

                            capture.I2CRegRW(regAddrWidth == 16 ? 0x82 : 0x81, writeCount, subAddr, regAddr, regWBuf);

                            string writeString = "Write SubAddr=0x" + subAddr.ToString("X")
                                            + " RegAddr=0x" + regAddr.ToString("X")
                                            + " Count=" + writeCount.ToString();

                            for (int i = 0; i < writeCount; i++)
                            {
                                writeString += " 0x" + regWBuf[i].ToString("X2");
                            }

                            writeString += "\r\n";
                            // update log file
                            System.IO.File.AppendAllText(i2cLogFile, writeString);
                            // update message window
                            richTextBoxBatchCmd.AppendText(writeString);
                            break;
                        case "Capture":
                            
                            if (words[1].Contains("0x"))
                                mCaptureFrameNum = (int)Convert.ToInt32(words[1], 16);
                            else
                                mCaptureFrameNum = (int)Convert.ToInt32(words[1], 10);

                            string captureString = "Start capturing " 
                                                    + mCaptureFrameNum.ToString() + " frames ... \r\n";
                            System.IO.File.AppendAllText(i2cLogFile, captureString);
                            // update message window
                            richTextBoxBatchCmd.AppendText(captureString);

                            captureFullFileName = words[2];

                            capturedFrame = 0;
                            m_CaptureImages = true;

                            while (m_CaptureImages || m_SaveFrameToFile || m_SaveFileInProcess)
                            {
                                Application.DoEvents();
                            }

                            captureString =  "Image Captured \r\n";
                            System.IO.File.AppendAllText(i2cLogFile, captureString);
                            // update message window
                            richTextBoxBatchCmd.AppendText(captureString);
                            break;
                        case "Display":
                            string displayString = "Error: Display syntax error \r\n";
                            
                            if (words[1].Contains("no"))
                            {
                                mRAWDisplay = false;
                                ToolStripMenuItem item1 = (ToolStripMenuItem)noDisplayToolStripMenuItem;
                                item1.Checked = true;
                                displayString = "Display Disabled \r\n";
                            }
                            else if (words[1].Contains("yes"))
                            {
                                mRAWDisplay = true;
                                ToolStripMenuItem item1 = (ToolStripMenuItem)noDisplayToolStripMenuItem;
                                item1.Checked = false;
                                displayString = "Display Enabled \r\n"; 
                            }
                            
                            System.IO.File.AppendAllText(i2cLogFile, displayString);
                            // update message window
                            richTextBoxBatchCmd.AppendText(displayString);

                            break;

                    }

                }

            }

            buttonBatchCmdExec.Enabled = true;

            MessageBox.Show(" operation done. Log is in BatchCmdLog.txt");


        }

        private void radioButtonI2CDataWidth16Bits_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void buttonI2CRead_Click(object sender, EventArgs e)
        {

            int subAddr, regAddr, regAddrWidth, readCount;
            byte[] regBuf = new byte[256];

            if (textBoxI2CAddr.Text.Contains("0x"))
                subAddr = (int)Convert.ToInt32(textBoxI2CAddr.Text, 16);
            else
                subAddr = (int)Convert.ToInt32(textBoxI2CAddr.Text, 10);

            if (radioButtonI2CAddrWidth8Bits.Checked)
                regAddrWidth = 8;
            else
                regAddrWidth = 16;

            if (radioButtonI2CDataWidth8Bits.Checked)
                readCount = 1;
            else if (radioButtonI2CDataWidth16Bits.Checked)
                readCount = 2;
            else
                readCount = 4;

            if (textBoxI2CRegAddr.Text.Contains("0x"))
                regAddr = (int)Convert.ToInt32(textBoxI2CRegAddr.Text, 16);
            else
                regAddr = (int)Convert.ToInt32(textBoxI2CRegAddr.Text, 10);
            try
            {
                capture.I2CRegRW(regAddrWidth == 16 ? 0x02 : 0x01, readCount, subAddr, regAddr, regBuf);
            }
            catch
            {
                MessageBox.Show("Read Error");
                return;
            }

            UInt32 regValue;

            if (radioButtonI2CDataWidth8Bits.Checked)
            {
                regValue = regBuf[0];
            }
            else if (radioButtonI2CDataWidth16Bits.Checked)
            {
                regValue = (UInt32)(regBuf[1] & 0x00FF) | (UInt32)((regBuf[0] << 8) & 0xFF00);
            }
            else
            {
                regValue = (UInt32)(regBuf[3] & 0x000000FF) | (UInt32)((regBuf[2] << 8) & 0x0000FF00)
                           | (UInt32)((regBuf[1] << 16) & 0x00FF0000) | (UInt32)((regBuf[0] << 24) & 0xFF000000);
            }

            if (I2CCmdHex)
            {
                textBoxI2CRegValue.Text = "0x" + regValue.ToString("X");
            }
            else
            {
                textBoxI2CRegValue.Text = regValue.ToString();
            }
            richTextBoxBatchCmd.AppendText("\r\n*Read: " + "SubAddr: 0x" + subAddr.ToString("X")
                                        + " Reg: 0x" + regAddr.ToString("X")
                                        + " Value: 0x" + regValue.ToString("X"));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            richTextBoxBatchCmd.Clear();
        }

        private bool I2CCmdHex = true;
        private UInt32 I2CSubAddr = 0, I2CRegAddr = 0, I2CRegValue = 0;
        private void buttonI2CCmdHexDec_Click(object sender, EventArgs e)
        {
            if (I2CCmdHex)
            {
                try
                {
                    I2CSubAddr = (UInt32)Convert.ToInt32(textBoxI2CAddr.Text, 16);
                }
                catch
                {
                    MessageBox.Show("I2C sub address format is wrong");
                    return;
                }

                try
                {
                    I2CRegAddr = (UInt32)Convert.ToInt32(textBoxI2CRegAddr.Text, 16);
                }
                catch
                {
                    MessageBox.Show("I2C reg address format is wrong");
                    return;
                }

                try
                {
                    I2CRegValue = (UInt32)Convert.ToInt32(textBoxI2CRegValue.Text, 16);
                }
                catch
                {
                    MessageBox.Show("I2C reg address format is wrong");
                    return;
                }
                textBoxI2CAddr.Text = I2CSubAddr.ToString();
                textBoxI2CRegAddr.Text = I2CRegAddr.ToString();
                textBoxI2CRegValue.Text = I2CRegValue.ToString();

                I2CCmdHex = false;
                buttonI2CCmdHexDec.Text = "Dec";
            }
            else
            {
                try
                {
                    I2CSubAddr = (UInt32)Convert.ToInt32(textBoxI2CAddr.Text, 10);
                }
                catch
                {
                    MessageBox.Show("I2C sub address format is wrong");
                    return;
                }

                try
                {
                    I2CRegAddr = (UInt32)Convert.ToInt32(textBoxI2CRegAddr.Text, 10);
                }
                catch
                {
                    MessageBox.Show("I2C reg address format is wrong");
                    return;
                }

                try
                {
                    I2CRegValue = (UInt32)Convert.ToInt32(textBoxI2CRegValue.Text, 10);
                }
                catch
                {
                    MessageBox.Show("I2C reg address format is wrong");
                    return;
                }
                textBoxI2CAddr.Text = "0x" + I2CSubAddr.ToString("X");
                textBoxI2CRegAddr.Text = "0x" + I2CRegAddr.ToString("X");
                textBoxI2CRegValue.Text = "0x" + I2CRegValue.ToString("X");

                I2CCmdHex = true;
                buttonI2CCmdHexDec.Text = "Hex";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int subAddr, regAddr, regAddrWidth, WriteCount, regValue;
            byte[] regWBuf = new byte[256];

            if (textBoxI2CAddr.Text.Contains("0x"))
                subAddr = (int)Convert.ToInt32(textBoxI2CAddr.Text, 16);
            else
                subAddr = (int)Convert.ToInt32(textBoxI2CAddr.Text, 10);

            if (radioButtonI2CAddrWidth8Bits.Checked)
                regAddrWidth = 8;
            else
                regAddrWidth = 16;

            if (radioButtonI2CDataWidth8Bits.Checked)
                WriteCount = 1;
            else if (radioButtonI2CDataWidth16Bits.Checked)
                WriteCount = 2;
            else
                WriteCount = 4;

            if (textBoxI2CRegAddr.Text.Contains("0x"))
                regAddr = (int)Convert.ToInt32(textBoxI2CRegAddr.Text, 16);
            else
                regAddr = (int)Convert.ToInt32(textBoxI2CRegAddr.Text, 10);

            if (textBoxI2CRegValue.Text.Contains("0x"))
                regValue = (int)Convert.ToInt32(textBoxI2CRegValue.Text, 16);
            else
                regValue = (int)Convert.ToInt32(textBoxI2CRegValue.Text, 10);
            

            for (int i = 0; i < WriteCount; i++)
            {
                regWBuf[i] = (Byte)(regValue >> ((WriteCount-i-1) * 8));
            }

            try
            {
                capture.I2CRegRW(regAddrWidth == 16 ? 0x82 : 0x81, WriteCount, subAddr, regAddr, regWBuf);
            }
            catch
            {
                MessageBox.Show("Read Error");
                return;
            }

            string writeString = "Write SubAddr=0x" + subAddr.ToString("X")
                            + " RegAddr=0x" + regAddr.ToString("X")
                            + " Count=" + WriteCount.ToString();

            for (int i = 0; i < WriteCount; i++)
            {
                writeString += " 0x" + regWBuf[i].ToString("X2");
            }

            writeString += "\r\n";
            // update message window
            richTextBoxBatchCmd.AppendText(writeString);
        }

        private void eEPROMWriteToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void selectFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "二进制文件|*.bin|C#文件|*.cs|所有文件|*.*";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FilterIndex = 1;
            int[] regData1 = new int[512];
            int i = 0x0;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                int WReg_Value = 0;
                int WReg_Address = 0xF000;
                FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open);
                BinaryReader r = new BinaryReader(fs);
                for (i = 0x0; i < 0x100; i++)
                {
                    regData1[i] = WReg_Value = r.ReadByte();
                    capture.SetRegRW(1, WReg_Address, WReg_Value);
                    Thread.Sleep(2);
                    WReg_Address++;
                }
                WReg_Address = 0xF000;
                for (i = 0x0; i < 0x100; i++)
                {
                    capture.SetRegRW(0, WReg_Address, R_value);
                    // R_address = frmRegRW_MODESET.RReg_Address;

                    // capture.SetRegRW(0, R_address, R_value);
                    //capture.SetRegRW(0, R_address, R_value);

                    //System.Threading.Thread.Sleep(500);

                    Thread.Sleep(2);
                    R_value = frmRegRW_MODESET.RReg_Value = capture.GetREGStatus();

                    frmRegRW_MODESET.Update_R_Value(R_value);
                    WReg_Address++;
                    if (R_value != regData1[i]) break;
                }
                if (i == 256) MessageBox.Show("写入成功！");
                else MessageBox.Show("写入失败！");
                r.Close();
                fs.Close();
            }
        }

        private void LyftRecordStartMenuItem_Click(object sender, EventArgs e)
        {
            if (vLog.InProgess)
            {
                MessageBox.Show("Another recording in progress. Click Stop then restart.");
                return;
            }
            vLog.StartVideoCapture();
        }

        private void LyftRecordStopMenuItem_Click(object sender, EventArgs e)
        {
            if (vLog.InProgess)
                vLog.StopVideoCapture();
            else
                MessageBox.Show("No recording in progress. Did you goof up?");
        }

        private void LyftImageStreamStart_Click(object sender, EventArgs e)
        {
            if (imLog.ImLogEnabled)
            {
                MessageBox.Show("Already capturing images..");
                return;
            }
            imLog.StartImageCapture();
        }

        private void LyftImageStreamStop_Click(object sender, EventArgs e)
        {
            if (imLog.ImLogEnabled)
                imLog.StopImageCapture();
            else
                MessageBox.Show("We weren't capturing images! Did you goof up?");
        }
    }
}
