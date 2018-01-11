/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  
*****************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;
using DirectShowLib;

namespace LeopardCamera
{
    public partial class LPCamera 
    {
        public enum CameraModel { M031, M034, M114, V034, Stereo, AR0330, MT9P031, ICP1_AR0330, C570, 
                                    ICX285, AR1820, C661, T_SERIAL, IMX22x, OV10635, ov10640, OV8865, 
                                    OV13850, CMV300, ZED, OV7251, IMX226, ETRON3D, OV10823, IMX172,
                                    MLX75411, KEURIG_SPI, ISX017, AR0130_AP0100, AR023ZWDR, IMX230,
                                    IMX290, IMX185, OV2742, OV2685, PYTHON1300, IMX274, ETRON2D, IMX178,IMX377, RAA462113,
                                    IMX298, AR1335_ICP3, OV9712, AR0231, AR0144,IMX390, NULL
        };
        public enum SENSOR_DATA_MODE { YUV, RAW12, RAW10, RAW8, RAW8_DUAL, YUV_DUAL, JPEG };

        // Default values
        private const string LP_DEFAULT_ID_CYPRESS = @"VID_04b4";

        private const int LP_DEFAULT_IMAGE_HEIGHT = 1944;
        private const int LP_DEFAULT_IMAGE_WIDTH = 2592;
        private const string LP_DEFAULT_IMAGE_FORMAT = "YUY2";
        private const short LP_DEFAULT_IMAGE_BITS = 16;
        private const long LP_DEFAULT_IMAGE_TPF = 0;
        
        public ImageCapture m_capture = null;
        private string device;
        private int width;
        private int height;
        private string format;
        private short bpp;
        private long tpf;
        private bool opened;
        private DsDevice m_cameraDevice;
        public IVideoWindow rendererWin = null;
        private int image_alloc_stat = 0;   // number of image allocated in memory

        public List<DsDevice> cameraList;
        public int[,] ResList;
        public int ResCount;

        public long[,] FrameRateList;
        public int FrameRateCNT;
        public int framerateindex_target;
        public int resIndex_target;

        public int FrameCount { set {m_capture.frameCount = value;} get {return m_capture.frameCount;}}

        public LPCamera(string deviceID = LP_DEFAULT_ID_CYPRESS, string sFormat = LP_DEFAULT_IMAGE_FORMAT, int iWidth = LP_DEFAULT_IMAGE_WIDTH, 
                        int iHeight = LP_DEFAULT_IMAGE_HEIGHT, short iBPP = LP_DEFAULT_IMAGE_BITS, long iTPF =  LP_DEFAULT_IMAGE_TPF)
        {
            this.device = deviceID;
            this.format = sFormat;
            this.width = iWidth;
            this.height = iHeight;
            this.bpp = iBPP;
            this.tpf = iTPF;
            this.opened = false;

            m_capture = new ImageCapture(this.device, out cameraList);
        }

        ~LPCamera()
        {
            this.Close();
        }

        public void UpdateCameraList()
        {
            if (m_capture != null)
                m_capture.UpdateCameraList(this.device, out cameraList);
        }

        public void UpdateCameraInfo(DsDevice cameraDevice)
        {
            if (m_capture != null)
                m_capture.UpdateCameraInfo(cameraDevice, this.device);
        }

        public void Open(DsDevice cameraDevice, int resIndex, int frIndex)
        {
            try
            {
                m_cameraDevice = cameraDevice;
                framerateindex_target = frIndex;
                resIndex_target = resIndex;

                m_capture.GetResolution(cameraDevice, out ResList, out ResCount, out FrameRateList, out FrameRateCNT);

               // m_capture.GetResolution(cameraDevice, out ResList, out ResCount);
                UpdateCameraList();
                UpdateCameraInfo(cameraDevice);
                this.opened = true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void ShowCapturePinProperty(IntPtr parent_handle)
        {
            m_capture.ShowCapturePinProperty(parent_handle);
        }

        public void ShowRendererProperty(IntPtr parent_handle)
        {
            m_capture.ShowRendererProperty(parent_handle);
        }
     
        public void ShowCameraProperty(IntPtr parent_handle)
        {
            m_capture.ShowCameraProperty(parent_handle);
        }

        public void SetParam(int width, int height, bool display, IntPtr parentWin)
        {
            this.width = width;
            this.height = height;
            this.tpf = FrameRateList[resIndex_target, framerateindex_target];

            m_capture.SetupCamera(m_cameraDevice, parentWin, width, height, display, this.format, this.bpp, this.tpf);

            rendererWin = (IVideoWindow)m_capture.m_pRendererVideo;
        }

        public void Close()
        {
            if (this.opened)
            {
                try
                {
                    if (m_capture != null)
                        m_capture.Dispose(); // delete camera and graph
                }
                catch { }
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        protected IntPtr AllocNewImage(int height, int weight, int bit)
        {
            int newImageSize = height * weight * bit / 8;
            IntPtr newImage = Marshal.AllocCoTaskMem(newImageSize);
            image_alloc_stat++;

            return newImage;
        }

        public virtual void FreeImageBuffer(IntPtr ptr)
        {
            image_alloc_stat--;
            Marshal.FreeCoTaskMem(ptr);
        }

        public int CaptureImage(out IntPtr pBuffer, out int iWidth, out int iHeight, out int iBPP)
        {
            int retval = -1;
            iWidth = iHeight = iBPP = 0;
            pBuffer = IntPtr.Zero;

            pBuffer = this.AllocNewImage(this.Height, this.Width, this.Bits);
            IntPtr image = pBuffer;
            this.CameraCaptureImage(image, out iWidth, out iHeight, out iBPP);

            retval = 0;
            return retval;
        }

        public int CaptureImageNoWait(out IntPtr pBuffer, out int iWidth, out int iHeight, out int iBPP)
        {
            int retval = -1;
            iWidth = iHeight = iBPP = 0;
            pBuffer = IntPtr.Zero;

            if (cameraModel == CameraModel.ETRON3D)
                pBuffer = this.AllocNewImage(this.Height, this.Width + 640, this.Bits);
            else
                pBuffer = this.AllocNewImage(this.Height, this.Width, this.Bits);

            IntPtr image = pBuffer;
            this.CameraCaptureImageNoWait(image, out iWidth, out iHeight, out iBPP);

            retval = 0;
            return retval;
        }


        public void GetCurResolution(out int width, out int height)
        {
            if (m_capture == null)
                throw new Exception("camera not initialized");

            m_capture.GetCurFormat(out width, out height);
        }

        public void ReadCamUUIDnHWFWRev(out String uuid, out UInt16 HwRev, out UInt16 FwRev)
        {
            m_capture.ReadCamUUIDnHWFWRev(out uuid, out HwRev, out FwRev);
        }

        public void WriteCamDefectPixelTable(byte[] defectTable)
        {
            m_capture.WriteCamDefectPixelTable(defectTable);
        }

        public void ReadCamDefectPixelTable(out byte[] defectTable)
        {
            m_capture.ReadCamDefectPixelTable(out defectTable);
        }

        public void WriteSensorRegisterConfToFlash(byte[] regConf)
        {
            m_capture.WriteSensorRegisterConfToFlash(regConf);
        }

        public void ReadSensorRegisterConfFromFlash(out byte[] regConf)
        {
            m_capture.ReadSensorRegisterConfFromFlash(out regConf);
        }

        public void ReadCamUUIDnHWFWRev(out String uuid, out UInt16 HwRev, out UInt16 FwRev, out string FuseID)
        {
            m_capture.ReadCamUUIDnHWFWRev(out uuid, out HwRev, out FwRev, out FuseID);
        }

        public void ReadExtensionINFO(out UInt16 ROIX_Max, out UInt16 ROIX_Min, out UInt16 ROIY_Max, out UInt16 ROIY_Min)
        {
            m_capture.ReadROI_MAX_MIN(out ROIX_Max, out ROIX_Min, out ROIY_Max, out ROIY_Min);
        }
        public void SetROI_Level(int roi_level)
        {
            m_capture.SetROI_Level(roi_level);
        }
        public void Stop()
        {
            m_capture.Stop();
        }

        public void PrintProperties()
        {
            m_capture.PrintCameraProperties();
        }

        public int CameraCaptureImage(IntPtr pBuffer, out int iWidth, out int iHeight, out int iBPP)
        {
            iWidth = iHeight = iBPP = 0;
            if (m_capture == null)
            {
                return -1;
            }

            IntPtr m_ip = IntPtr.Zero;
            IntPtr m_rgb = IntPtr.Zero;
            do
            {
                m_capture.Click(pBuffer); // take the picture
            } while (pBuffer == IntPtr.Zero);

            if (pBuffer == System.IntPtr.Zero)
                return -1;

            iWidth = m_capture.OutWidth; // this.width;
            iHeight = m_capture.OutHeight; //this.height;
            iBPP = this.bpp;

            return 0;
        }

        public int CameraCaptureImageNoWait(IntPtr pBuffer, out int iWidth, out int iHeight, out int iBPP)
        {
            iWidth = iHeight = iBPP = 0;
            if (m_capture == null)
            {
                return -1;
            }

            IntPtr m_ip = IntPtr.Zero;
            IntPtr m_rgb = IntPtr.Zero;

            m_capture.Click_NoWait(pBuffer); // take the picture

            if (pBuffer == System.IntPtr.Zero)
                return -1;

            iWidth = m_capture.OutWidth; // this.width;
            iHeight = m_capture.OutHeight; //this.height;
            iBPP = this.bpp;

            return 0;
        }

        public string SerialID
        {
            get { return this.device; }
            set { this.device = value; }
        }

        // Auto Exposure enable/disable
        public bool AE
        {
            get
            {
                DirectShowLib.CameraControlFlags controlFlag;
                int exp = m_capture.GetCameraControlProperty(DirectShowLib.CameraControlProperty.Exposure, out controlFlag);
                if (controlFlag == CameraControlFlags.Auto)
                    return true;
                else
                    return false;
            }
            set
            {
                DirectShowLib.CameraControlFlags controlFlag;
                int exp = m_capture.GetCameraControlProperty(DirectShowLib.CameraControlProperty.Exposure, out controlFlag);
                
                if (value)
                {
                    m_capture.SetCameraControlProperty(CameraControlProperty.Exposure, exp, CameraControlFlags.Auto);
                }
                else
                {
                    m_capture.SetCameraControlProperty(CameraControlProperty.Exposure, exp, CameraControlFlags.Manual);
                }
            }
        }

        //exposure time, measured in time, value increased by 1, the time is double
        public int Exposure
        {
            get
            {
                DirectShowLib.CameraControlFlags controlFlag;
                return m_capture.GetCameraControlProperty(DirectShowLib.CameraControlProperty.Exposure, out controlFlag);
            }
            set
            {
                    m_capture.SetCameraControlProperty(DirectShowLib.CameraControlProperty.Exposure, value, CameraControlFlags.Manual);
            }
        }

        public void GetVideoProcAmpPropertyRange(VideoProcAmpProperty propVideo, out int Min, out int Max, out int Step, out int Default)
        {
            m_capture.GetVideoProcAmpPropertyRange(propVideo, out Min, out Max, out Step, out Default);
        }

        public void GetCameraControlPropertyRange(CameraControlProperty propVideo, out int Min, out int Max, out int Step, out int Default)
        {
            m_capture.GetCameraControlPropertyRange(propVideo, out Min, out Max, out Step, out Default);
        }

        public int SetSensorMode(int mode)
        {
            return m_capture.SetSensorMode(mode);
        }

        // exposure time, measured in Lines
        public int ExposureExt
        {
            get
            {
                //return m_capture.GetExposure();
                return m_capture.GetExpsosureExt();
            }
            set
            {
                m_capture.SetExposureExt(value);
            }
        }

        public void Run()
        {
            m_capture.Run();
        }

        public int Width
        {
            get { return this.width; }
            set { this.width = value; }
        }

        public int Height
        {
            get { return this.height; }
            set { this.height = value; }
        }

        public int Gain
        {
            get
            {
                return m_capture.GetGain();
            }
            set
            {
                m_capture.SetGain(value);
            }
        }

        public int Bits
        {
            get { return (int)this.bpp; }
            set { this.bpp = (short)value; }
        }

        public CameraModel cameraModel
        {
            get { 
                switch (m_capture.mCameraModel)
                {
                    case 0x00eb:
                        return CameraModel.T_SERIAL;
                    case 0x00ec:
                        return CameraModel.C661; 
                    case 0x00ed:
                        return CameraModel.AR1820;
                    case 0x00ee:
                        return CameraModel.ICX285;
                    case 0x00ef:
                        return CameraModel.C570;
                    case 0x00f2:
                        return CameraModel.ICP1_AR0330;
                    case 0x00f4:
                        return CameraModel.MT9P031;
                    case 0x00f5:
                        return CameraModel.Stereo;
                    case 0x00f7:
                        return CameraModel.V034;
                    case 0x00f8:
                        return CameraModel.M031;
                    case 0x00f9:
                        return CameraModel.M034;
                    case 0x00fa:
                        return CameraModel.M114;
                    case 0x00fb:
                        return CameraModel.AR0330;
                    case 0x00e9:
                        return CameraModel.IMX22x;
                    case 0x00ea:
                        return CameraModel.OV10635;
                    case 0x00e8:
                        return CameraModel.ov10640;
                    case 0x00e6:
                        return CameraModel.OV8865;
                    case 0x00e5:
                        return CameraModel.OV13850;
                    case 0x00e4:
                        return CameraModel.CMV300;
                    case 0xf580:
                        return CameraModel.ZED;
                    case 0x00e3:
                        return CameraModel.OV7251;
                    case 0x00e2:
                        return CameraModel.IMX226;
                    case 0x00e1:
                        return CameraModel.OV10823;
                    case 0x00e0:
                        return CameraModel.IMX172;
                    case 0x00df:
                        return CameraModel.MLX75411;
                    case 0x00de:
                        return CameraModel.KEURIG_SPI;
                    case 0x00dd:
                        return CameraModel.ISX017;
                    case 0x00dc:
                        return CameraModel.AR0130_AP0100;
                    case 0x0568:
                        return CameraModel.ETRON3D;
                    case 0x1e4e:
                        return CameraModel.ETRON2D;
                    case 0x00db:
                        return CameraModel.AR023ZWDR;
                    case 0x00da:
                        return CameraModel.IMX230;
                    case 0x00d8:
                        return CameraModel.IMX290;
                    case 0x00d9:
                        return CameraModel.IMX185;
                    case 0x00d7:
                        return CameraModel.OV2742;
                    case 0x00d6:
                        return CameraModel.OV2685;
                    case 0x00d5:
                        return CameraModel.PYTHON1300;
                    case 0x00d4:
                        return CameraModel.IMX274;
                    case 0x00d3:
                        return CameraModel.IMX178;
                    case 0x00d1:
                        return CameraModel.RAA462113;
                    case 0x00d0:
                        return CameraModel.IMX298;
                    case 0x0010:
                        return CameraModel.AR1335_ICP3;
                    case 0x00cf:
                        return CameraModel.OV9712;
                    case 0x00ce:
                        return CameraModel.IMX377;
                    case 0x00cb:
                        return CameraModel.AR0231;
                    case 0x00ca:
                        return CameraModel.AR0144;
                    case 0x00c8:
                        return CameraModel.IMX390;
                    default:
                        return CameraModel.NULL;
                }
            }
        }

        public int SetLED(bool Left0, bool Left1, bool Right0, bool Right1)
        {
            return m_capture.SetLED(Left0, Left1, Right0, Right1);
        }

        public int SetPOS(int startX, int startY)
        {
            return m_capture.SetPOS(startX, startY);
        }

        public int SetRGBGain(UInt16 rGain, UInt16 grGain, UInt16 gbGain, UInt16 bGain)
        {
            return m_capture.SetRGBGain(rGain, grGain, gbGain, bGain);
        }

        public int EnableTriggerMode(bool ena,bool enb)
        {
            return m_capture.TriggerEnable(ena,enb);
        }

        public int SetTriggerDelayTime(uint delayTime)
        {
            return m_capture.TriggerDelayTime(delayTime);
        }

        public int SoftTrigger()
        {
            return m_capture.SoftTrigger();
        }
        public int SetRegRW(int rw_flag, int address, int value)
        {
            return m_capture.SetRegRW(rw_flag, address, value);
        }

        // generic I2C read/write
        // rw_flag: bit 7 0: read, 1 write. bit[6:0] regAddr bit width, 1: 8-bit regAddr, 2: 16-bit regAddr
        //          0x01 : write, regAddr is 8-bit; 0x02 : write, regAddr is 16-bit
        //          0x81 : read, regAddr is 8-bit; 0x81 : read, regAddr is 16-bit
        // bufCnt: length of regData, 1-256
        // subAddress: device I2C sub address, 8 bits
        // regAddr: register address
        // regData: data buf to write to the device, or data read from the device
        public int I2CRegRW(int rw_flag, int bufCnt, int subAddress, int regAddr, byte[] regData)
        {
            return m_capture.I2CRegRW(rw_flag, bufCnt, subAddress, regAddr, regData);
        }

        public int GetREGStatus()
        {
            return m_capture.GetREGStatus();
        }
    }

}
