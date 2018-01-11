/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  
*****************************************************************************/

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using DirectShowLib;
using System.Drawing;

namespace LeopardCamera
{
    using System.Collections;

    // A delegate type for hooking up Frame received notifications.
    public delegate void FrameReceivedEventHandler(object sender, EventArgs e);

    /// <summary> Summary description for MainForm. </summary>
    public partial class ImageCapture : ISampleGrabberCB, IDisposable
    {
        // An event that clients can use to be notified whenever a frame is received
        public event FrameReceivedEventHandler ReceivedOneFrame;

        public int frameCount = 0;
        public int m_iExposure;

        public int iCount = 0, iPieceCount = 0;

        // resolution that the camera supports
        private int[,] m_Resolution = new int[128, 2];
        private int m_ResCnt = 0;
        private long[,] m_FrameRate = new long[128, 3];//for each resolution,3 framerates supported at most currently

        // list of cameras
        public List<DsDevice> m_cameraList;

        private int m_startX = 0;
        private int m_startY = 0;
        private int m_sizeX = 2592;
        private int m_sizeY = 1944;
        private UInt16 m_rGain = 0x80, m_grGain = 0x80, m_gbGain = 0x80, m_bGain = 0x80;
        private int m_exposure = -1;
        private int m_sensorMode = -1;
        private int m_roilevel = -1;

        public UInt16 mCameraModel = 0;
        public int OutWidth = 0;
        public int OutHeight = 0;

        // extension unit command definition
        const byte XU_MODE_SWITCH = (0x01);
        const byte XU_WINDOW_REPOSITION = (0x02);
        const byte XU_LED_MODES = (0x03);
        const byte XU_GAIN_CONTROL_RGB = (0x04);
        const byte XU_GAIN_CONTROL_A = (0x05);
        const byte XU_EXPOSURE_TIME = (0x06);
        const byte XU_UUID_HWFW_REV = (0x07);
        const byte XU_DEFECT_PIXEL_TABLE = (0x08);
        const byte XU_SOFT_TRIGGER = (0x09);
        const byte XU_TRIGGER_MODE = (0x0b);
        const byte XU_TRIGGER_DELAY_TIME = (0x0a);

        // for sensor register configuration into flash
        const byte XU_SENSOR_REGISTER_CONFIGURATION = (0x0c);

        const byte XU_EXTENSION_INFO = (0x0d);
        const byte XU_GENERIC_REG_RW = (0x0e);

        const byte XU_GENERIC_I2C_RW = (0x10);

        Mutex mut = new Mutex();

        #region Member variables
        /// <summary>Struct for keeping a fourcc to guid mapping for media types.</summary>
        struct name_guid
        {
            public string fourcc;
            public Guid guid;
        }
        private name_guid[] ng_array = new name_guid[5];
        /// <summary> graph builder interface. </summary>
        protected IFilterGraph2 m_FilterGraph = null;
        protected IFilterGraph2 m_FilterGraph1 = null;

        /// <summary>Used to snap picture on Still pin. </summary>
        public IBaseFilter m_capFilter = null;
        protected IAMCameraControl m_CamControl = null;
        protected IAMVideoProcAmp m_VidProcAmp = null;
        protected IAMVideoControl m_VidControl = null;
        public IMediaControl m_MediaCtrl = null;
        protected IMediaEvent m_MediaEvent = null;
        public IBaseFilter m_pRendererVideo = null;

        protected IPin m_pinStill = null;

        /// <summary> so we can wait for the async job to finish </summary>
        protected ManualResetEvent m_PictureReady = null;

        protected bool m_WantOne = false;

        /// <summary> Dimensions of the image, calculated once in constructor for perf. </summary>
        protected string mediatype;
        //private Guid m_mediatypeguid; //TDOD maybe used in future
        private int m_videoWidth;
        protected int m_videoHeight;
        protected int m_videoFormatIndex;
        protected int m_stride;
        protected bool m_verbose;
        protected int m_iWidth, m_iHeight;
        protected short m_iBPP;

        /// <summary> buffer for bitmap data.  Always released by caller</summary>
        protected IntPtr m_ipBuffer = IntPtr.Zero;
        protected IntPtr m_ipBuffer1 = IntPtr.Zero;
        protected int framelength = 0; // etron mjpeg camera
        public VideoProcAmpProperty[] videoProperties;
        public CameraControlProperty[] controlProperties;

        /// <summary>Virtual property media type translates 4CC code to Guid and back for limited set of properties</summary>
        public string mediaType
        {
            get
            {
                return "FOO";
            }
            set
            {

            }
        }

        /// <summary>A flag for verbosity to be injected.</summary>
        public bool verbose
        {
            get
            {
                return m_verbose;
            }
        }

        #endregion

        #region APIs
        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        protected static extern void CopyMemory(IntPtr Destination, IntPtr Source, [MarshalAs(UnmanagedType.U4)] int Length);
        #endregion

        public ImageCapture(string sDevicePath, out List<DsDevice> cameraList)
        {
            UpdateCameraList(sDevicePath, out cameraList);
        }

        public void UpdateCameraList(string sDevicePath, out List<DsDevice> cameraList)
        {
            // Get the graphbuilder object
            m_FilterGraph = new FilterGraph() as IFilterGraph2;
            m_FilterGraph1 = new FilterGraph() as IFilterGraph2;

            cameraList = new List<DsDevice>();

             ///<summary>This table maps character fourcc names with the media subtype GUIDs.  It will
            ///be used for interpreting the options passed in</summary>
            //ng_array = new name_guid[4];
            ng_array[0].fourcc = "NV12";
            ng_array[0].guid = MediaSubType.NV12;
            ng_array[1].fourcc = "RGB4";
            ng_array[1].guid = MediaSubType.RGB24;
            ng_array[2].fourcc = "YUY2";
            ng_array[2].guid = MediaSubType.YUY2;
            ng_array[3].fourcc = "YUYV";
            ng_array[3].guid = MediaSubType.YUYV;
            ng_array[4].fourcc = "MJPG";
            ng_array[4].guid = MediaSubType.MJPG;

            videoProperties = new VideoProcAmpProperty[] {VideoProcAmpProperty.Brightness,VideoProcAmpProperty.Contrast,
                VideoProcAmpProperty.Hue,VideoProcAmpProperty.Saturation,
                VideoProcAmpProperty.Sharpness,VideoProcAmpProperty.Gamma,
                VideoProcAmpProperty.ColorEnable,VideoProcAmpProperty.WhiteBalance,
                VideoProcAmpProperty.BacklightCompensation,VideoProcAmpProperty.Gain};
            controlProperties = new CameraControlProperty[] {CameraControlProperty.Pan,CameraControlProperty.Tilt,
                CameraControlProperty.Roll,CameraControlProperty.Zoom,CameraControlProperty.Exposure,
                CameraControlProperty.Iris,CameraControlProperty.Focus};

            DsDevice[] capDevices;

            // Get the collection of video devices
            capDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            if (capDevices.Length == 0)
                throw new Exception(string.Format("No video capture devices found!"));

            foreach (DsDevice camera in capDevices)
            {
                if (camera.DevicePath.Contains(sDevicePath.ToLower()) || camera.DevicePath.Contains("vid_29ab") || camera.DevicePath.Contains("vid_2a0b") || camera.DevicePath.Contains("vid_2b03")
                    || camera.DevicePath.Contains("vid_1e4e") || camera.DevicePath.Contains("vid_0568") || camera.DevicePath.Contains("vid_05a9"))
                {
                    if (camera.DevicePath.IndexOf("pid_00f2") != -1) { mCameraModel = 0x00f2; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00eb") != -1) { mCameraModel = 0x00eb; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00ec") != -1) { mCameraModel = 0x00ec; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00ed") != -1) { mCameraModel = 0x00ed; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00ee") != -1) { mCameraModel = 0x00ee; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00ef") != -1) { mCameraModel = 0x00ef; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00f4") != -1) { mCameraModel = 0x00f4; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00f5") != -1) { mCameraModel = 0x00f5; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00f7") != -1) { mCameraModel = 0x00f7; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00f8") != -1) { mCameraModel = 0x00f8; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00f9") != -1) { mCameraModel = 0x00f9; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00fa") != -1) { mCameraModel = 0x00fa; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00fb") != -1) { mCameraModel = 0x00fb; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00e9") != -1) { mCameraModel = 0x00e9; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00ea") != -1) { mCameraModel = 0x00ea; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00e8") != -1) { mCameraModel = 0x00e8; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00e6") != -1) { mCameraModel = 0x00e6; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00e5") != -1) { mCameraModel = 0x00e5; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00e4") != -1) { mCameraModel = 0x00e4; cameraList.Add(camera); }
					else if (camera.DevicePath.IndexOf("pid_f580") != -1) { mCameraModel = 0xf580; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00e3") != -1) { mCameraModel = 0x00e3; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00e2") != -1) { mCameraModel = 0x00e2; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00e1") != -1) { mCameraModel = 0x00e1; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00e0") != -1) { mCameraModel = 0x00e0; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00df") != -1) { mCameraModel = 0x00df; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00de") != -1) { mCameraModel = 0x00de; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00dd") != -1) { mCameraModel = 0x00dd; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00dc") != -1) { mCameraModel = 0x00dc; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_0568") != -1) { mCameraModel = 0x0568; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_1e4e") != -1) { mCameraModel = 0x1e4e; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00db") != -1) { mCameraModel = 0x00db; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00da") != -1) { mCameraModel = 0x00da; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00d8") != -1) { mCameraModel = 0x00d8; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00d9") != -1) { mCameraModel = 0x00d9; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00d7") != -1) { mCameraModel = 0x00d7; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00d6") != -1) { mCameraModel = 0x00d6; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00d5") != -1) { mCameraModel = 0x00d5; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00d4") != -1) { mCameraModel = 0x00d4; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00d3") != -1) { mCameraModel = 0x00d3; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_0581") != -1) { mCameraModel = 0x0581; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00d2") != -1) { mCameraModel = 0x00d2; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00d1") != -1) { mCameraModel = 0x00d1; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00d0") != -1) { mCameraModel = 0x00d0; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_0010") != -1) { mCameraModel = 0x0010; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00cf") != -1) { mCameraModel = 0x00cf; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00ce") != -1) { mCameraModel = 0x00ce; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00cb") != -1) { mCameraModel = 0x00cb; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00ca") != -1) { mCameraModel = 0x00ca; cameraList.Add(camera); }
                    else if (camera.DevicePath.IndexOf("pid_00c8") != -1) { mCameraModel = 0x00c8; cameraList.Add(camera); }

                }
            }
            
            m_cameraList = cameraList;
        }

        public void UpdateCameraInfo(DsDevice camera, string sDevicePath)
        {
            if (camera.DevicePath.Contains(sDevicePath.ToLower()))
            {
                if (camera.DevicePath.IndexOf("pid_00f2") != -1) { mCameraModel = 0x00f2; }
                else if (camera.DevicePath.IndexOf("pid_00eb") != -1) { mCameraModel = 0x00eb; }
                else if (camera.DevicePath.IndexOf("pid_00ec") != -1) { mCameraModel = 0x00ec; }
                else if (camera.DevicePath.IndexOf("pid_00ed") != -1) { mCameraModel = 0x00ed; }
                else if (camera.DevicePath.IndexOf("pid_00ef") != -1) { mCameraModel = 0x00ef; }
                else if (camera.DevicePath.IndexOf("pid_00f4") != -1) { mCameraModel = 0x00f4; }
                else if (camera.DevicePath.IndexOf("pid_00f5") != -1) { mCameraModel = 0x00f5;  }
                else if (camera.DevicePath.IndexOf("pid_00f7") != -1) { mCameraModel = 0x00f7; }
                else if (camera.DevicePath.IndexOf("pid_00f8") != -1) { mCameraModel = 0x00f8; }
                else if (camera.DevicePath.IndexOf("pid_00f9") != -1) { mCameraModel = 0x00f9; }
                else if (camera.DevicePath.IndexOf("pid_00fa") != -1) { mCameraModel = 0x00fa;  }
                else if (camera.DevicePath.IndexOf("pid_00fb") != -1) { mCameraModel = 0x00fb; }
                else if (camera.DevicePath.IndexOf("pid_00e9") != -1) { mCameraModel = 0x00e9; }
                else if (camera.DevicePath.IndexOf("pid_00ea") != -1) { mCameraModel = 0x00ea; }
                else if (camera.DevicePath.IndexOf("pid_00e8") != -1) { mCameraModel = 0x00e8; }
                else if (camera.DevicePath.IndexOf("pid_00e6") != -1) { mCameraModel = 0x00e6; }
                else if (camera.DevicePath.IndexOf("pid_00e5") != -1) { mCameraModel = 0x00e5; }
                else if (camera.DevicePath.IndexOf("pid_00e4") != -1) { mCameraModel = 0x00e4; }
				else if (camera.DevicePath.IndexOf("pid_f580") != -1) { mCameraModel = 0xf580; }
                else if (camera.DevicePath.IndexOf("pid_00e3") != -1) { mCameraModel = 0x00e3; }
                else if (camera.DevicePath.IndexOf("pid_00e2") != -1) { mCameraModel = 0x00e2; }
                else if (camera.DevicePath.IndexOf("pid_00e1") != -1) { mCameraModel = 0x00e1; }
                else if (camera.DevicePath.IndexOf("pid_00e0") != -1) { mCameraModel = 0x00e0; }
                else if (camera.DevicePath.IndexOf("pid_00df") != -1) { mCameraModel = 0x00df; }
                else if (camera.DevicePath.IndexOf("pid_00de") != -1) { mCameraModel = 0x00de; }
                else if (camera.DevicePath.IndexOf("pid_00dd") != -1) { mCameraModel = 0x00dd; }
                else if (camera.DevicePath.IndexOf("pid_00dc") != -1) { mCameraModel = 0x00dc; }
                else if (camera.DevicePath.IndexOf("pid_0568") != -1) { mCameraModel = 0x0568; }
                else if (camera.DevicePath.IndexOf("pid_1e4e") != -1) { mCameraModel = 0x1e4e; }
                else if (camera.DevicePath.IndexOf("pid_00db") != -1) { mCameraModel = 0x00db; }
                else if (camera.DevicePath.IndexOf("pid_00da") != -1) { mCameraModel = 0x00da; }
                else if (camera.DevicePath.IndexOf("pid_00d8") != -1) { mCameraModel = 0x00d8; }
                else if (camera.DevicePath.IndexOf("pid_00d9") != -1) { mCameraModel = 0x00d9; }
                else if (camera.DevicePath.IndexOf("pid_00d7") != -1) { mCameraModel = 0x00d7; }
                else if (camera.DevicePath.IndexOf("pid_00d6") != -1) { mCameraModel = 0x00d6; }
                else if (camera.DevicePath.IndexOf("pid_00d5") != -1) { mCameraModel = 0x00d5; }
                else if (camera.DevicePath.IndexOf("pid_00d4") != -1) { mCameraModel = 0x00d4; }
                else if (camera.DevicePath.IndexOf("pid_00d3") != -1) { mCameraModel = 0x00d3; }
                else if (camera.DevicePath.IndexOf("pid_0581") != -1) { mCameraModel = 0x0581; }
                else if (camera.DevicePath.IndexOf("pid_00d2") != -1) { mCameraModel = 0x00d2; }
                else if (camera.DevicePath.IndexOf("pid_00d1") != -1) { mCameraModel = 0x00d1; }
                else if (camera.DevicePath.IndexOf("pid_00d0") != -1) { mCameraModel = 0x00d0; }
                else if (camera.DevicePath.IndexOf("pid_0010") != -1) { mCameraModel = 0x0010; }
                else if (camera.DevicePath.IndexOf("pid_00cf") != -1) { mCameraModel = 0x00cf; }
                else if (camera.DevicePath.IndexOf("pid_00ce") != -1) { mCameraModel = 0x00ce; }
                else if (camera.DevicePath.IndexOf("pid_00cb") != -1) { mCameraModel = 0x00cb; }
                else if (camera.DevicePath.IndexOf("pid_00ca") != -1) { mCameraModel = 0x00ca; }
                else if (camera.DevicePath.IndexOf("pid_00c8") != -1) { mCameraModel = 0x00c8; }

            }
        }

        /// <summary>
        /// Construct Capture engine and start it running.
        /// </summary>
        /// <param name="iDeviceNum">The index of the desired device in the collection returned by device enumeration (default: 0)</param>
        /// <param name="iWidth">Image width in pixels (default: 640)</param>
        /// <param name="iHeight">Image height in pixels (default: 480)</param>
        /// <param name="iBPP">Image bits per pixel (default: 24)</param>
        /// <param name="hControl">Video display controls (default: null == no video)</param>
        public void SetupCamera(DsDevice cameraDevice, IntPtr parentWin, int iWidth = 640, int iHeight = 480, bool display = false, 
                        string mt = "", short iBPP = 24, long iTPF = 0, bool verbose = false )
        {
            m_sizeX = iWidth;
            m_sizeY = iHeight;

            m_verbose = verbose;
            mediatype = mt; // save mediatype string for later
            m_iWidth = iWidth;
            m_iHeight = iHeight;
            m_iBPP = iBPP;

            Console.WriteLine("device is {0}\r\n", cameraDevice.Name);

            try
            {
                if (mCameraModel == 0x0568) // etron 3d camera
                    SetupGraph1(cameraDevice, 320, 480, m_iBPP, iTPF, true, parentWin);

                SetupGraph(cameraDevice, m_iWidth, m_iHeight, m_iBPP, iTPF, display, parentWin);

                // tell the callback to ignore new images
                m_PictureReady = new ManualResetEvent(false);
            }
            catch (Exception e)
            {
                Dispose();
                throw e;
            }
        }

        /// <summary> release everything. </summary>
        public void Dispose()
        {
            CloseInterfaces();
            if (m_PictureReady != null)
            {
                m_PictureReady.Close();
            }

        }

        public void Stop()
        {
            if (m_FilterGraph1 != null)
            {
                IMediaControl mediaCtrl = m_FilterGraph1 as IMediaControl;
                // Stop the graph
                mediaCtrl.Stop();
            }
            if (m_FilterGraph != null)
            {
                IMediaControl mediaCtrl = m_FilterGraph as IMediaControl;
                // Stop the graph
                mediaCtrl.Stop();
            }
        }

        // Destructor
        ~ImageCapture()
        {
            Dispose();
        }

        /// <summary>
        /// Get the image from the Still pin.  The returned image can turned into a bitmap with
        /// Bitmap b = new Bitmap(cam.Width, cam.Height, cam.Stride, PixelFormat.Format24bppRgb, m_ip);
        /// If the image is upside down, you can fix it with
        /// b.RotateFlip(RotateFlipType.RotateNoneFlipY);
        /// </summary>
        /// <returns>Returned image copied into memory alloccated by caller in pBuffer</returns>
        public void Click(IntPtr pBuffer)
        {
            //Common.printMsg(20, "start filter graph: Run()");
            //int hr = m_MediaCtrl.Run();
            //Common.printMsg(10, string.Format("GraphRun return code hr = {0}", hr));

            m_PictureReady.Reset();
            m_WantOne = true;

            if (!m_PictureReady.WaitOne(Timeout.Infinite, false))
            {
                throw new Exception("Timeout waiting to get picture");
            }

            // Buffer was saved in m_ipBuffer by callback function BufferCB(); make a copy and return
            CopyMemory(pBuffer, m_ipBuffer, m_iHeight * m_iWidth * m_iBPP / 8);

        }

        public void Click_NoWait(IntPtr pBuffer)
        {
            // Buffer was saved in m_ipBuffer by callback function BufferCB(); make a copy and reture
            if (mCameraModel == 0x0568) // etron 3d camera
            {
#if false // 320
                if (m_ipBuffer != (IntPtr)0)
                {
                    for (int i = 0; i < m_iHeight; i++)
                    {
                        CopyMemory(pBuffer + i * (m_iWidth + 320) * m_iBPP / 8, m_ipBuffer + i * m_iWidth * m_iBPP / 8, m_iWidth * m_iBPP / 8);
                    }

                }

                if (m_ipBuffer1 != (IntPtr)0)
                {
                    for (int i = 0; i < 480; i++)
                    {
                        CopyMemory(pBuffer + i * (m_iWidth + 320) * m_iBPP / 8 + m_iWidth * m_iBPP / 8, m_ipBuffer1 + i * 320 * m_iBPP / 8, 320 * m_iBPP / 8);
                    }
                }
#else
                if (m_ipBuffer != (IntPtr)0)
                {
                    for (int i = 0; i < m_iHeight; i++)
                    {
                        CopyMemory(pBuffer + i * (m_iWidth + 640) * m_iBPP / 8, m_ipBuffer + i * m_iWidth * m_iBPP / 8, m_iWidth * m_iBPP / 8);
                    }

                }

                if (m_ipBuffer1 != (IntPtr)0)
                {
                    Byte[] val = new Byte[2];
                    for (int i = 0; i < 480; i++)
                    {
                        IntPtr tmp = pBuffer + i * (m_iWidth + 640) * m_iBPP / 8 + m_iWidth * m_iBPP / 8;
                        for (int j = 0; j < 640; j++)
                        {
                            Marshal.Copy(m_ipBuffer1 + i * 640 + j, val, 0, 1);
                            val[1] = 128;
                            Marshal.Copy(val, 0, tmp + j * 2, 2);
                        }
                    }
                }
#endif
            }
            else if (mCameraModel == 0x1e4e)
            {
                CopyMemory(pBuffer, m_ipBuffer, framelength);
            }
            else
            {
                CopyMemory(pBuffer, m_ipBuffer, m_iHeight * m_iWidth * m_iBPP / 8);
            }
        }

        /// <summary>
        /// Image width getter.
        /// </summary>
        /// <returns>Image Width</returns>
        public int Width
        {
            get
            {
                return m_videoWidth;
            }
        }
        /// <summary>
        /// Image height getter
        /// </summary>
        /// <returns>Image height</returns>
        public int Height
        {
            get
            {
                return 0;
            }
        }
        /// <summary>
        /// Image stride getter
        /// </summary>
        /// <remarks>The image stride is the number of bytes (usually pixels) in a row of the image.</remarks>
        /// <returns>Image Stride</returns>
        public int Stride
        {
            get
            {
                return m_stride;
            }
        }
        /// <summary>
        /// Convert fourcc string to MediaSubType by search of ng_array
        /// </summary>
        /// <param name="ms">string to look up in ng_array table</param>
        /// <returns>MediaSubType value that corresponds to string or MediaSubType.Null if not found.</returns>
        private System.Guid LookUpMediaSubtype(string ms)
        {
            foreach (name_guid ng in ng_array)
            {
                if (0 == string.Compare(ng.fourcc, ms))
                    return ng.guid;
            }
            return MediaSubType.Null; // didn't find it.
        }

        /// <summary>
        /// Look up fourcc code given media subtype guid
        /// </summary>
        /// <param name="mst"></param>
        /// DirectShow media subtype Guid
        /// <returns>fourcc code or empty string</returns>
        private string lookUpFourcc(Guid mst)
        {
            foreach (name_guid ng in ng_array)
            {
                if (ng.guid == mst)
                    return ng.fourcc;
            }
            byte[] fcc;
            fcc = mst.ToByteArray();

            return (System.Text.Encoding.ASCII.GetString(fcc).Substring(0, 4) + "?");
        }
        /// <summary> build the capture graph for grabber and start it. </summary>
        /// <param name="dev">Index of device in devices list</param>
        /// <param name="iWidth">Width of image in pixels</param>
        /// <param name="iHeight">Height of image in pixels</param>
        /// <param name="iBPP">Bits per pixel</param>
        /// <param name="hControl">Screen window control information</param>
        private void SetupGraph(DsDevice dev, int iWidth, int iHeight, short iBPP, long iTPF, bool display, IntPtr parentWin)
        {
            int hr;
            bool useFFDSHOW = false;
            Object ffdshow_capture=null;

            if (mCameraModel == 0x00da) // imx230 camera, 20M needs ffdshow to support
                useFFDSHOW = true;

            try
            {
                // add the video input device
                hr = m_FilterGraph.AddSourceFilterForMoniker(dev.Mon, null, dev.Name, out m_capFilter);

                DsError.ThrowExceptionForHR(hr);

                // Get a VideoProcAmp interface to the capture device
                m_VidProcAmp = m_capFilter as IAMVideoProcAmp;

                // Get a CameraControl interface to the capture device
                m_CamControl = m_capFilter as IAMCameraControl;

                // Get a control pointer (used in Click())
                //m_VidControl = capFilter as IAMVideoControl;
                m_VidControl = null;

                {
                    IPin pCaptureOut = DsFindPin.ByDirection(m_capFilter, PinDirection.Output, 0);

                    if (mCameraModel == 0x1e4e) //etron 2d mjpeg camera
                        mediatype = "MJPG";
                    SetConfigParms(pCaptureOut, mediatype, iWidth, iHeight, iBPP, iTPF);

                    ISampleGrabber sampleGrabber = new SampleGrabber() as ISampleGrabber;
                    ConfigureSampleGrabber(sampleGrabber, display);

                    // Get the default video renderer
                    IBaseFilter pRenderer = new NullRenderer() as IBaseFilter;
                    hr = m_FilterGraph.AddFilter(pRenderer, string.Format("NullRenderer {0}", 0));
                    DsError.ThrowExceptionForHR(hr);

                    SmartTee smartTee = null;
                    if (display)
                    {
                         // Get the default video renderer
                        m_pRendererVideo = new VideoRenderer() as IBaseFilter;
                        hr = m_FilterGraph.AddFilter(m_pRendererVideo, string.Format("VideoRenderer {0}", 0));

                        DsError.ThrowExceptionForHR(hr);

                        smartTee = new SmartTee();
                        hr = m_FilterGraph.AddFilter((IBaseFilter)smartTee, string.Format("smartTee {0}", 0));
                        DsError.ThrowExceptionForHR(hr);
                    }

                    // Add the sample grabber to the graph
                    hr = m_FilterGraph.AddFilter((IBaseFilter)sampleGrabber, string.Format("Ds.NET Grabber {0}", 0));
                    DsError.ThrowExceptionForHR(hr);

                    //Add AVI decompesser
                    Guid CLSID_AVIdecX = new Guid("{CF49D4E0-1115-11CE-B03A-0020AF0BA770}");
                    Object aviDecompressor = Activator.CreateInstance(Type.GetTypeFromCLSID(CLSID_AVIdecX));

                    hr = m_FilterGraph.AddFilter((IBaseFilter)aviDecompressor, string.Format("AVI Decompressor", 0));

                    if (useFFDSHOW)
                    {
                        //Add ffdshow raw filter
                        Guid CLSID_ffdshowrawfilter = new Guid("{0B390488-D80F-4A68-8408-48DC199F0E97}");
                        ffdshow_capture = Activator.CreateInstance(Type.GetTypeFromCLSID(CLSID_ffdshowrawfilter));
                        //Object ffdshow_preview = Activator.CreateInstance(Type.GetTypeFromCLSID(CLSID_ffdshowrawfilter));

                        hr = m_FilterGraph.AddFilter((IBaseFilter)ffdshow_capture, string.Format("ffdshow raw filter{0}", 0));
                        //hr = m_FilterGraph.AddFilter((IBaseFilter)ffdshow_preview, string.Format("ffdshow raw filter{0}", 1));
                    }

                    // Connect the Capture pin to the sample grabber
                    if (!display)
                    {
                        hr = m_FilterGraph.Connect(pCaptureOut, DsFindPin.ByDirection((IBaseFilter)sampleGrabber, PinDirection.Input, 0));

                        if (useFFDSHOW)
                        {
                            hr = m_FilterGraph.Connect(DsFindPin.ByDirection((IBaseFilter)sampleGrabber, PinDirection.Output, 0), DsFindPin.ByDirection((IBaseFilter)ffdshow_capture, PinDirection.Input, 0));
                            hr = m_FilterGraph.Connect(DsFindPin.ByDirection((IBaseFilter)ffdshow_capture, PinDirection.Output, 0), DsFindPin.ByDirection(pRenderer, PinDirection.Input, 0));
                        }
                        else
                        {


                        hr = m_FilterGraph.Connect(DsFindPin.ByDirection((IBaseFilter)sampleGrabber, PinDirection.Output, 0), DsFindPin.ByDirection((IBaseFilter)aviDecompressor, PinDirection.Input, 0));
                        hr = m_FilterGraph.Connect(DsFindPin.ByDirection((IBaseFilter)aviDecompressor, PinDirection.Output, 0), DsFindPin.ByDirection(pRenderer, PinDirection.Input, 0));

                        }
                    }
                    else
                    {
                        hr = m_FilterGraph.Connect(pCaptureOut, DsFindPin.ByDirection((IBaseFilter)sampleGrabber, PinDirection.Input, 0));
                        hr = m_FilterGraph.Connect(DsFindPin.ByDirection((IBaseFilter)sampleGrabber, PinDirection.Output, 0), DsFindPin.ByDirection((IBaseFilter)aviDecompressor, PinDirection.Input, 0));
                        hr = m_FilterGraph.Connect(DsFindPin.ByDirection((IBaseFilter)aviDecompressor, PinDirection.Output, 0), DsFindPin.ByDirection(m_pRendererVideo, PinDirection.Input, 0));

                        IVideoWindow videoWin = (IVideoWindow)m_pRendererVideo;

                        videoWin.put_Owner(parentWin);
                        videoWin.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipSiblings);
                    }

                    if (smartTee != null)
                    {
                        Marshal.ReleaseComObject(smartTee);
                    }

                    if (aviDecompressor != null)
                    {
                        Marshal.ReleaseComObject(aviDecompressor);
                    }

                    if (useFFDSHOW)
                    {
                        if (ffdshow_capture != null)
                        {
                            Marshal.ReleaseComObject(ffdshow_capture);
                        }
                    }

                    if (pRenderer != null)
                    {
                        Marshal.ReleaseComObject(pRenderer);
                    }

                    if (sampleGrabber != null)
                    {
                        Marshal.ReleaseComObject(sampleGrabber);
                    } 
                    
                    DsError.ThrowExceptionForHR(hr);
                }

                DsError.ThrowExceptionForHR(hr);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
            }
        }

        /// <summary> build the capture graph for grabber and start it. </summary>
        /// <param name="dev">Index of device in devices list</param>
        /// <param name="iWidth">Width of image in pixels</param>
        /// <param name="iHeight">Height of image in pixels</param>
        /// <param name="iBPP">Bits per pixel</param>
        /// <param name="hControl">Screen window control information</param>
        private void SetupGraph1(DsDevice dev, int iWidth, int iHeight, short iBPP, long iTPF, bool display, IntPtr parentWin)
        {
            int hr;

            try
            {
                // add the video input device
                hr = m_FilterGraph1.AddSourceFilterForMoniker(dev.Mon, null, dev.Name, out m_capFilter);

                DsError.ThrowExceptionForHR(hr);

                // Get a VideoProcAmp interface to the capture device
                m_VidProcAmp = m_capFilter as IAMVideoProcAmp;

                // Get a CameraControl interface to the capture device
                m_CamControl = m_capFilter as IAMCameraControl;

                // Get a control pointer (used in Click())
                //m_VidControl = capFilter as IAMVideoControl;
                m_VidControl = null;
                
                {
                    IPin pCaptureOut = DsFindPin.ByDirection(m_capFilter, PinDirection.Output, 1);

                    SetConfigParms(pCaptureOut, mediatype, 320, 480, iBPP, iTPF);

                    ISampleGrabber sampleGrabber = new SampleGrabber() as ISampleGrabber;
                    ConfigureSampleGrabber(sampleGrabber, display);

                    // Get the default video renderer
                    IBaseFilter pRenderer = new NullRenderer() as IBaseFilter;
                    hr = m_FilterGraph1.AddFilter(pRenderer, string.Format("NullRenderer {0}", 0));
                    DsError.ThrowExceptionForHR(hr);
                    SmartTee smartTee = null;

                    // Add the sample grabber to the graph
                    hr = m_FilterGraph1.AddFilter((IBaseFilter)sampleGrabber, string.Format("Ds.NET Grabber {0}", 0));
                    DsError.ThrowExceptionForHR(hr);

                    //Add AVI decompesser
                    Guid CLSID_AVIdecX = new Guid("{CF49D4E0-1115-11CE-B03A-0020AF0BA770}");
                    Object aviDecompressor = Activator.CreateInstance(Type.GetTypeFromCLSID(CLSID_AVIdecX));

                    hr = m_FilterGraph1.AddFilter((IBaseFilter)aviDecompressor, string.Format("AVI Decompressor", 0));

                    // Connect the Capture pin to the sample grabber
                    if (display)
                    {
                        hr = m_FilterGraph1.Connect(pCaptureOut, DsFindPin.ByDirection((IBaseFilter)sampleGrabber, PinDirection.Input, 0));
                        hr = m_FilterGraph1.Connect(DsFindPin.ByDirection((IBaseFilter)sampleGrabber, PinDirection.Output, 0), DsFindPin.ByDirection((IBaseFilter)aviDecompressor, PinDirection.Input, 0));
                        hr = m_FilterGraph1.Connect(DsFindPin.ByDirection((IBaseFilter)aviDecompressor, PinDirection.Output, 0), DsFindPin.ByDirection(pRenderer, PinDirection.Input, 0));
                    }

                    if (smartTee != null)
                    {
                        Marshal.ReleaseComObject(smartTee);
                    }

                    if (aviDecompressor != null)
                    {
                        Marshal.ReleaseComObject(aviDecompressor);
                    }

                    if (pRenderer != null)
                    {
                        Marshal.ReleaseComObject(pRenderer);
                    }

                    if (sampleGrabber != null)
                    {
                        Marshal.ReleaseComObject(sampleGrabber);
                    }

                    DsError.ThrowExceptionForHR(hr);
                }

                DsError.ThrowExceptionForHR(hr);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
            }
        }

        public void Run()
        {
            // Start the graph
            m_MediaCtrl = m_FilterGraph as IMediaControl;
            m_MediaEvent = m_FilterGraph as IMediaEvent;
            m_MediaCtrl.Run();

            // Start the graph
            m_MediaCtrl = m_FilterGraph1 as IMediaControl;
            m_MediaEvent = m_FilterGraph1 as IMediaEvent;
            m_MediaCtrl.Run();
        }

        /// <summary>
        /// Save the image size info from the SampleGrabber
        /// </summary>
        /// <remarks>Saves image width, height, and stride</remarks>
        /// <param name="sampGrabber">SampleGrabber filter member</param>
        private void SaveSizeInfo(ISampleGrabber sampGrabber)
        {
            int hr;

            // Get the media type from the SampleGrabber
            AMMediaType media = new AMMediaType();

            hr = sampGrabber.GetConnectedMediaType(media);
            DsError.ThrowExceptionForHR(hr);

            if ((media.formatType != FormatType.VideoInfo) || (media.formatPtr == IntPtr.Zero))
            {
                throw new NotSupportedException("Unknown Grabber Media Format");
            }

            // Grab the size info
            VideoInfoHeader videoInfoHeader = (VideoInfoHeader)Marshal.PtrToStructure(media.formatPtr, typeof(VideoInfoHeader));
            m_videoWidth = videoInfoHeader.BmiHeader.Width;
            m_videoHeight = videoInfoHeader.BmiHeader.Height;
            m_stride = (m_videoWidth * videoInfoHeader.BmiHeader.BitCount) / 8;

            DsUtils.FreeAMMediaType(media);
            media = null;
        }


        /// <summary>
        /// Configure SampleGrabber to only accept the format we want and to specify the type of callback
        /// </summary>
        /// <param name="sampGrabber">SampleGrabber to configure</param>
        private void ConfigureSampleGrabber(ISampleGrabber sampGrabber, bool display)
        {
            int hr;
            AMMediaType media = new AMMediaType();

            // Set the media type to Video/NV12
            media.majorType = MediaType.Video;
            media.subType = LookUpMediaSubtype(mediatype);
            media.formatType = FormatType.VideoInfo;
            hr = sampGrabber.SetMediaType(media);
            DsError.ThrowExceptionForHR(hr);

            DsUtils.FreeAMMediaType(media);
            media = null;

            //hr = sampGrabber.SetBufferSamples(true);
            //hr = sampGrabber.SetOneShot(true);
            // Configure the samplegrabber callbacks
            if (display)
            { // buffer mode
                hr = sampGrabber.SetBufferSamples(true);
                hr = sampGrabber.SetOneShot(false);
            }
            else // one shot mode
            {
                hr = sampGrabber.SetBufferSamples(false);
                hr = sampGrabber.SetOneShot(true);

            }
            hr = sampGrabber.SetCallback(this, 1);
            DsError.ThrowExceptionForHR(hr);
        }

        /// <summary>s
        /// Video Streaming configuration
        /// </summary>
        /// <remarks>Set the Framerate, and video size for capture PIN</remarks>
        /// <param name="pStill">Still frame pin of capture device</param>
        /// <param name="iWidth">Width of image</param>
        /// <param name="iHeight">Height of image</param>
        /// <param name="iBPP">bits per pixel</param>
        /// <param name="iTPF">max time per frame</param>

        private void SetConfigParms(IPin pStill, string fourcc, int iWidth, int iHeight, short iBPP, long iTPF)
        {
            int hr;

            IAMStreamConfig videoStreamConfig = pStill as IAMStreamConfig;
            if (null == videoStreamConfig)
                throw new Exception("Cannot obtain IAMStreamConfig");

            // Get number of possible combinations of format and size
            int capsCount, capSize;
            hr = videoStreamConfig.GetNumberOfCapabilities(out capsCount, out capSize);
            if ((hr != 0) && (verbose))
            {
                DsError.ThrowExceptionForHR(hr);
            }

            VideoInfoHeader vih = new VideoInfoHeader();
            VideoStreamConfigCaps vsc = new VideoStreamConfigCaps();
            IntPtr pSC = Marshal.AllocHGlobal(capSize);
            AMMediaType mt = null;
            int videoFormatIndex = -1;
            Guid wantsubtype = LookUpMediaSubtype(fourcc);

            for (int i = 0; i < capsCount; ++i)
            {
                // the video format is described in AMMediaType and VideoStreamConfigCaps
                hr = videoStreamConfig.GetStreamCaps(i, out mt, pSC);
                DsError.ThrowExceptionForHR(hr);

                // copy the unmanaged structures to managed in order to check the format
                Marshal.PtrToStructure(mt.formatPtr, vih);
                Marshal.PtrToStructure(pSC, vsc);

                // log the video format
                string capline = String.Format("{0}: {1} x {2} fps (min-max): {3}-{4}",
                              lookUpFourcc(mt.subType), vih.BmiHeader.Width, vih.BmiHeader.Height,
                              10000000 / vsc.MaxFrameInterval,
                              10000000 / vsc.MinFrameInterval);

                // check colorspace
                if (mt.subType == wantsubtype)
                {
                    // the video format has a range of supported frame rates (min-max)
                    // check the required frame rate and frame size
                    if ((vih.BmiHeader.Width == iWidth) &&
                        (vih.BmiHeader.Height == iHeight))
                    {
                        // remember the index of the video format that we’ll use
                        videoFormatIndex = i;
                        break;
                    }
                }
                DsUtils.FreeAMMediaType(mt);
            }

            // didn't find what we wanted
            if (videoFormatIndex < 0)
            {
                throw (new Exception(string.Format("Unable to find acceptable format for {0}, {1} x {2}", fourcc, iWidth, iHeight)));
            }

            try
            {
                m_videoFormatIndex = videoFormatIndex;
                hr = videoStreamConfig.GetStreamCaps(videoFormatIndex, out mt, pSC);
                DsError.ThrowExceptionForHR(hr);
                // explicitly set the framerate since the default may not what we want
                Marshal.PtrToStructure(mt.formatPtr, vih);

                if (iBPP > 0)
                {
                    
                }
                // if overriding the time per frame
                if (iTPF > 0)
                {
                    vih.AvgTimePerFrame = iTPF;
                }

                Marshal.StructureToPtr(vih, mt.formatPtr, false);
                hr = videoStreamConfig.SetFormat(mt);
                DsError.ThrowExceptionForHR(hr);
            }
            catch (Exception e)
            {
                throw (e);
            }
            finally
            {
                DsUtils.FreeAMMediaType(mt);
                Marshal.FreeHGlobal(pSC);
                mt = null;
            }
        }

        /// <summary>
        /// Find matching format/size parameters for camera
        /// </summary>
        /// <remarks>Cycles through the available formats for a camera looking for a match of the parameters.
        /// Some cameras are adaptable and will return something close, others aren't and will just barf if you
        /// force a set of parameters it doesn't directly support.  We take the conservative notion here and go
        /// for a perfect match or return null.</remarks>
        /// <param name="pStill">The still pin of the camera to use as a handle</param>
        /// <param name="ms">The mediatype string of the requested image format</param>
        /// <param name="iWidth">requested width of image in pixels</param>
        /// <param name="iHeight">requested height of image in pixels</param>
        /// <returns>int which is index of found format to set or -1 if not found</returns>
        private int FindFormat(IPin pStill, Guid ms, int iWidth, int iHeight)
        {
            VideoInfoHeader v;
            VideoStreamConfigCaps vsc = new VideoStreamConfigCaps();
            IAMStreamConfig videoStreamConfig = pStill as IAMStreamConfig;
            int videoFormatIndex = -1;
            int iCount = 0, iSize = 0;
            videoStreamConfig.GetNumberOfCapabilities(out iCount, out iSize);

            IntPtr TaskMemPointer = Marshal.AllocCoTaskMem(iSize);

            AMMediaType pmtConfig = null;
            for (int iFormat = 0; iFormat < iCount; iFormat++)
            {
                //IntPtr ptr = IntPtr.Zero;

                videoStreamConfig.GetStreamCaps(iFormat, out pmtConfig, TaskMemPointer);

                v = (VideoInfoHeader)Marshal.PtrToStructure(pmtConfig.formatPtr, typeof(VideoInfoHeader));
                if ((v.BmiHeader.Width == iWidth) && (v.BmiHeader.Height == iHeight) && (0 == pmtConfig.subType.CompareTo(ms)))
                {
                    videoFormatIndex = iFormat; //index of format we are interested in
                    break;
                }

            }

            Marshal.FreeCoTaskMem(TaskMemPointer); 
            DsUtils.FreeAMMediaType(pmtConfig);

            return videoFormatIndex; //index of acceptable block, if we found one.

        }

        /// <summary> Shut down capture </summary>
        /// <remarks>Shuts down the filter graph and releases all of the .COM
        /// storage.</remarks>
        public void CloseInterfaces()
        {
            int hr;

            try
            {
                if (m_FilterGraph != null)
                {
                    IMediaControl mediaCtrl = m_FilterGraph as IMediaControl;
                    // Stop the graph
                    hr = mediaCtrl.Stop();

                    if (m_pRendererVideo != null)
                    {
                        IVideoWindow videoWin = (IVideoWindow)m_pRendererVideo;
                        videoWin.put_Visible(OABool.False);
                        videoWin.put_Owner(IntPtr.Zero);

                        Marshal.ReleaseComObject(m_pRendererVideo);
                        m_pRendererVideo = null;
                    }
                }
                if (m_FilterGraph1 != null)
                {
                    IMediaControl mediaCtrl = m_FilterGraph1 as IMediaControl;
                    // Stop the graph
                    hr = mediaCtrl.Stop();

                    if (m_pRendererVideo != null)
                    {
                        IVideoWindow videoWin = (IVideoWindow)m_pRendererVideo;
                        videoWin.put_Visible(OABool.False);
                        videoWin.put_Owner(IntPtr.Zero);

                        Marshal.ReleaseComObject(m_pRendererVideo);
                        m_pRendererVideo = null;
                    }
                }
            }
            catch 
            {
            }

            if (m_FilterGraph != null)
            {
                Marshal.ReleaseComObject(m_FilterGraph);
                m_FilterGraph = null;
            }
            if (m_FilterGraph1 != null)
            {
                Marshal.ReleaseComObject(m_FilterGraph1);
                m_FilterGraph1 = null;
            }

            if (m_VidControl != null)
            {
                Marshal.ReleaseComObject(m_VidControl);
                m_VidControl = null;
            }

            if (m_pinStill != null)
            {
                Marshal.ReleaseComObject(m_pinStill);
                m_pinStill = null;
            }
        }

        /// <summary>
        /// SampleGraber Sample call back
        /// </summary>
        /// <remarks>This callback reportedly doesn't work, so we just release the buffer and punt</remarks>
        /// <param name="SampleTime">Time of sample</param>
        /// <param name="pSample">Sample instance</param>
        /// <returns>return code (always 0)</returns>
        int ISampleGrabberCB.SampleCB(double SampleTime, IMediaSample pSample)
        {
            Marshal.ReleaseComObject(pSample);
            return 0;
        }

        /// <summary>
        /// SampleGrabber buffer callback, COULD BE FROM FOREIGN THREAD.
        /// </summary>
        /// <param name="SampleTime">Time of sample</param>
        /// <param name="pBuffer">pointer to buffer holding sample</param>
        /// <param name="BufferLen">length, in bytes of buffer</param>
        /// <returns>return code (always 0)</returns>
        int ISampleGrabberCB.BufferCB(double SampleTime, IntPtr pBuffer, int BufferLen)
        {

            mut.WaitOne();

            frameCount++;

            if (BufferLen == 307200 && mCameraModel == 0x0568)  // for etron 3d camera
                m_ipBuffer1 = pBuffer;
            else
                m_ipBuffer = pBuffer;

            framelength = BufferLen;
            OutWidth = m_sizeX;
            OutHeight = m_sizeY;

            m_WantOne = false;
            m_PictureReady.Set();

            mut.ReleaseMutex();


            if (ReceivedOneFrame != null)
                ReceivedOneFrame(this, EventArgs.Empty);

            return 0;
        }

        public static void ShowFilterPropertyPage(IBaseFilter filter, IntPtr parent)
        {
            int hr = 0;
            FilterInfo filterInfo;
            DsCAUUID caGuid;
            object[] objs;

            if (filter == null)
                throw new ArgumentNullException("filter");

            hr = filter.QueryFilterInfo(out filterInfo);
            DsError.ThrowExceptionForHR(hr);

            if (filterInfo.pGraph != null)
                Marshal.ReleaseComObject(filterInfo.pGraph);

            hr = (filter as ISpecifyPropertyPages).GetPages(out caGuid);
            DsError.ThrowExceptionForHR(hr);

            try
            {
                objs = new object[1];
                objs[0] = filter;

                NativeMethods.OleCreatePropertyFrame(
                    parent, 0, 0,
                    filterInfo.achName,
                    objs.Length, objs,
                    caGuid.cElems, caGuid.pElems,
                    0, 0,
                    IntPtr.Zero
                    );
            }
            finally
            {
                Marshal.FreeCoTaskMem(caGuid.pElems);
            }
        }

        private static void ShowFilterPinPropertyPage(IPin pin, IntPtr parent)
        {
            int hr = 0;
            PinInfo pinInfo;
            DsCAUUID caGuid;
            object[] objs;

            if (pin == null) throw new ArgumentNullException("pin");

            {
                hr = pin.QueryPinInfo(out pinInfo);
                DsError.ThrowExceptionForHR(hr);

                if (pinInfo.filter != null)
                    Marshal.ReleaseComObject(pinInfo.filter);

                hr = (pin as ISpecifyPropertyPages).GetPages(out caGuid);
                DsError.ThrowExceptionForHR(hr);

                try
                {
                    objs = new object[1];
                    objs[0] = pin;

                    int n = NativeMethods.OleCreatePropertyFrame(
                        parent, 0, 0,
                        pinInfo.name,
                        objs.Length, objs,
                        caGuid.cElems, caGuid.pElems,
                        0, 0,
                        IntPtr.Zero
                        );
                }
                finally
                {
                    Marshal.FreeCoTaskMem(caGuid.pElems);
                }
            }
        }

        public void ShowCapturePinProperty(IntPtr parent_handle)
        {
            if (m_capFilter == null)
                throw new Exception("Camera is not initialized yet.");
            else
            {
                IPin camera_capture_0 = DsFindPin.ByDirection(m_capFilter, PinDirection.Output, 0);
                if (camera_capture_0 != null)
                {
                    ShowFilterPinPropertyPage(camera_capture_0, parent_handle);
                }
                Marshal.ReleaseComObject(camera_capture_0);
            }
        }

        public void ShowRendererProperty(IntPtr parent_handle)
        {
            if (m_pRendererVideo == null)
                throw new Exception("Renderer is not initialized yet.");
            else
            {
                ShowFilterPropertyPage(m_pRendererVideo, parent_handle);
            }
        }

        public void ShowCameraProperty(IntPtr parent_handle)
        {
            if (m_capFilter == null)
                throw new Exception("Camera is not initialized yet.");
            else
            {
                ShowFilterPropertyPage(m_capFilter, parent_handle);
            }
        }

        public void PrintCameraProperties()
        {
            int lValue;
            CameraControlFlags flags;

            foreach (CameraControlProperty prop in Enum.GetValues(typeof(CameraControlProperty)))
            {
                m_CamControl.Get(prop, out lValue, out flags);
            }
        }

        private Mutex uvcCmdMutex = new Mutex();
        public unsafe int WriteToUVCExtension(IBaseFilter filter, int property_id, IntPtr bytes, int length, out long length_returned)
        {
            uvcCmdMutex.WaitOne();
            try
            {
                var n = write_to_uvc_extension(filter, property_id, bytes, length, out length_returned);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to set UVC command" + e.ToString());
            }
            finally
            {
                uvcCmdMutex.ReleaseMutex();
            }
            return 0;
        }

        public unsafe int ReadFromUVCExtension(IBaseFilter filter, int property_id, IntPtr bytes, int length, out long length_returned)
        {
            int n=0;

            uvcCmdMutex.WaitOne();
            length_returned = 0;
            try
            {
                n = read_from_uvc_extension(filter, property_id, bytes, length, out length_returned);
                //if (length_returned != (long)length)
                //    throw new Exception("Failed to read UVC command");
            }
            catch (Exception e)
            {
                throw new Exception("Failed to read UVC command" + e.ToString());
            }
            finally
            {
                uvcCmdMutex.ReleaseMutex();
            }
            return n;
        }

        public int SetExposure(int iExp)
        {
            int hr=0;

            uvcCmdMutex.WaitOne();
            try
            {
                hr = m_CamControl.Set(CameraControlProperty.Exposure, iExp, CameraControlFlags.Manual);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to set Exposure " + e.ToString());
            }
            finally
            {
                uvcCmdMutex.ReleaseMutex();
            }

            return hr;
        }

        public int GetExposure()
        {
            int iExpValue;
            CameraControlFlags flags;

            uvcCmdMutex.WaitOne();
            try
            {
                m_CamControl.Get(CameraControlProperty.Exposure, out iExpValue, out flags);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to get Exposure " + e.ToString());
            }
            finally
            {
                uvcCmdMutex.ReleaseMutex();
            }

            return iExpValue;
        }

        public int SetLED(bool Left0, bool Left1, bool Right0, bool Right1)
        {
            byte value=0;

            if (Left0) value |= 0x04;
            if (Left1) value |= 0x08;
            if (Right0) value |= 0x01;
            if (Right1) value |= 0x02;

            uvcCmdMutex.WaitOne();
            try
            {
                int n = set_uvc_extension_property_value(m_capFilter, XU_LED_MODES, 0, value);
                if (n != 0) throw new Exception("Set mode Property Value Error.");
            }
            catch (Exception e)
            {
                throw new Exception("Failed to set LED " + e.ToString());
            }
            finally
            {
                uvcCmdMutex.ReleaseMutex();
            }
            return 0;
        }

        public int SoftTrigger()
        {
            byte[] bValue = new byte[2];
            long length_returned;

            bValue[0] = 0x00;
            bValue[1] = 0x00;

            unsafe
            {
                fixed (byte* pPtr = bValue)
                    WriteToUVCExtension(m_capFilter, XU_SOFT_TRIGGER, new IntPtr(pPtr), 2, out length_returned);
            }

            return 0;
        }

        public int TriggerEnable(bool ena,bool enb)
        {
            byte[] bValue = new byte[2];
            long length_returned;

            if (ena)
            {
                if (enb)
                    bValue[0] = 0x01;
                else
                    bValue[0] = 0x03;
            }
            else
                bValue[0] = 0x00;
            bValue[1] = 0x00;

            unsafe
            {
                fixed (byte* pPtr = bValue)
                    WriteToUVCExtension(m_capFilter, XU_TRIGGER_MODE, new IntPtr(pPtr), 2, out length_returned);
            }

            return 0;
        }

        public int SetRegRW(int rw_flag, int address, int value)
        {
            byte[] aValue = new byte[5];
            byte[] bValue = new byte[5];
            byte[] cValue = new byte[5];

            long length_returned;

            aValue[0] = (byte)rw_flag;
            aValue[1] = (byte)(address >> 8);
            aValue[2] = (byte)(address & 0xff);
            aValue[3] = (byte)(value >> 8);
            aValue[4] = (byte)(value & 0xff);

            unsafe
            {
                fixed (byte* pPtr = aValue)
                    WriteToUVCExtension(m_capFilter, XU_GENERIC_REG_RW, new IntPtr(pPtr), 5, out length_returned);
            }

            return 0;
        }

        // rw_flag: bit 7 0: read, 1 write. bit[6:0] regAddr bit width, 1: 8-bit regAddr, 2: 16-bit regAddr
        //          0x01 : write, regAddr is 8-bit; 0x02 : write, regAddr is 16-bit
        //          0x81 : read, regAddr is 8-bit; 0x81 : read, regAddr is 16-bit
        // bufCnt: length of regData, 1-256
        // subAddress: device I2C sub address, 8 bits
        // regAddr: register address
        // regData: data buf to write to the device, or data read from the device
        public int I2CRegRW(int rw_flag, int bufCnt, int subAddress, int regAddr, byte[] regData)
        {
            byte[] aValue = new byte[256+6];

            long length_returned;

            if (bufCnt > 256 || bufCnt <= 0)
                throw new Exception("I2CRegRW buf count should be > 0 & <= 256");

            aValue[0] = (byte)rw_flag;
            aValue[1] = (byte)(bufCnt-1);
            aValue[2] = (byte)(subAddress >> 8);
            aValue[3] = (byte)(subAddress & 0xff);
            aValue[4] = (byte)(regAddr >> 8);
            aValue[5] = (byte)(regAddr & 0xff);
            Array.Copy(regData, 0, aValue, 6, bufCnt);

            unsafe
            {
                fixed (byte* pPtr = aValue)
                    WriteToUVCExtension(m_capFilter, XU_GENERIC_I2C_RW, new IntPtr(pPtr), 256+6, out length_returned);
            }

            if ((rw_flag & 0x0080) == 0x0000) // read
            {

                unsafe
                {
                    fixed (byte* pPtr = aValue)
                        ReadFromUVCExtension(m_capFilter, XU_GENERIC_I2C_RW, new IntPtr(pPtr), 256 + 6, out length_returned);
                }

                Array.Copy(aValue, 6, regData, 0, bufCnt);
            }

            return 0;
        }

        public int GetREGStatus()
        {
            byte[] bExp = new byte[5];
            long length_returned;

            unsafe
            {
                fixed (byte* pPtr = bExp)
                    ReadFromUVCExtension(m_capFilter, XU_GENERIC_REG_RW, new IntPtr(pPtr), 5, out length_returned);
            }

            return (bExp[3] << 8) | bExp[4];
        }
        public int TriggerDelayTime(uint delayTime)
        {
            byte[] bValue = new byte[4];
            long length_returned;

            bValue[0] = (byte) delayTime;
            bValue[1] = (byte) (delayTime >> 8);
            bValue[2] = (byte)(delayTime >> 16);
            bValue[3] = (byte)(delayTime >> 24);

            unsafe
            {
                fixed (byte* pPtr = bValue)
                    WriteToUVCExtension(m_capFilter, XU_TRIGGER_DELAY_TIME, new IntPtr(pPtr), 4, out length_returned);
            }

            return 0;
        }

        public byte GetLEDStatus()
        {
            byte[] bExp = new byte[2];
            long length_returned;

            unsafe
            {
                fixed (byte* pPtr = bExp)
                    ReadFromUVCExtension(m_capFilter, XU_LED_MODES, new IntPtr(pPtr), 2, out length_returned);
            }

            return bExp[0];
        }

        public int SetPOS(int startX, int startY)
        {
            long length_returned;
            byte[] bROIParam = new byte[4];


//            if (startX + m_sizeX <= 2592 && startY + m_sizeY <= 1944
//                && startX >= 0 && startY >= 0)
            {

                m_startX = startX;
                m_startY = startY;

                //Console.WriteLine(String.Format("startX = {0}, startY = {1}", startX, startY));

                bROIParam[0] = (byte)(startX);
                bROIParam[1] = (byte)(startX >> 8);
                bROIParam[2] = (byte)(startY);
                bROIParam[3] = (byte)(startY >> 8);

                unsafe
                {
                    fixed (byte* pPtr = bROIParam)
                        WriteToUVCExtension(m_capFilter, XU_WINDOW_REPOSITION, new IntPtr(pPtr), 4, out length_returned);
                }
            }
            return 0;
        }

        public int SetRGBGain(UInt16 rGain, UInt16 grGain, UInt16 gbGain, UInt16 bGain)
        {
            long length_returned;
            byte[] bROIParam = new byte[8];

            if (m_rGain != rGain | m_grGain != grGain | m_gbGain != gbGain | m_bGain != bGain)
            {
                m_rGain = rGain;
                m_grGain = grGain;
                m_gbGain = gbGain;
                m_bGain = bGain;

                //Console.WriteLine(String.Format("rGain = {0}, grGain = {1}, gbGain = {2}, bGain = {3}", m_rGain, m_grGain, m_gbGain, m_bGain));

                bROIParam[0] = (byte)(m_rGain);
                bROIParam[1] = (byte)(m_rGain >> 8);
                bROIParam[2] = (byte)(m_grGain);
                bROIParam[3] = (byte)(m_grGain >> 8);
                bROIParam[4] = (byte)(m_gbGain);
                bROIParam[5] = (byte)(m_gbGain >> 8);
                bROIParam[6] = (byte)(m_bGain);
                bROIParam[7] = (byte)(m_bGain >> 8);

                unsafe
                {
                    fixed (byte* pPtr = bROIParam)
                        WriteToUVCExtension(m_capFilter, XU_GAIN_CONTROL_RGB, new IntPtr(pPtr), 8, out length_returned);
                }
            }
            return 0;
        }

        public int SetExposureExt(int exp)
        {
            long length_returned;
            byte[] bROIParam = new byte[2];

            if (m_exposure != exp)
            {
                m_exposure = exp;
                //Console.WriteLine(String.Format("set exposure time =" + m_exposure.ToString()));

                bROIParam[0] = (byte)(m_exposure);
                bROIParam[1] = (byte)(m_exposure >> 8);

                unsafe
                {
                    fixed (byte* pPtr = bROIParam)
                        WriteToUVCExtension(m_capFilter, XU_EXPOSURE_TIME, new IntPtr(pPtr), 2, out length_returned);
                }
            }
            return 0;
        }

        public int SetSensorMode(int mode)
        {
            long length_returned;
            byte[] bROIParam = new byte[2];

            if (m_sensorMode != mode)
            {
                m_sensorMode = mode;
                //Console.WriteLine(String.Format("set exposure time =" + m_exposure.ToString()));

                bROIParam[0] = (byte)(m_sensorMode);
                unsafe
                {
                    fixed (byte* pPtr = bROIParam)
                        WriteToUVCExtension(m_capFilter, XU_MODE_SWITCH, new IntPtr(pPtr), 2, out length_returned);
                }
            }
            return 0;
        }

        public int GetExpsosureExt()
        {
            byte[] bExp = new byte[2];
            long length_returned;

            unsafe
            {
                fixed (byte* pPtr = bExp)
                    ReadFromUVCExtension(m_capFilter, XU_EXPOSURE_TIME, new IntPtr(pPtr), 2, out length_returned);
            }

            m_exposure = ((int)bExp[1] << 8 | (int)bExp[0]) & 0x0000ffff;

            return m_exposure;
        }

        // write defect table
        public void WriteCamDefectPixelTable(byte[] defectTable)
        {
            long length_returned;

            unsafe
            {
                fixed (byte* pPtr = defectTable)
                    WriteToUVCExtension(m_capFilter, XU_DEFECT_PIXEL_TABLE, new IntPtr(pPtr), 33, out length_returned);
            }
        }

        // read defect table
        public void ReadCamDefectPixelTable(out byte[] defectTable)
        {
            long length_returned;
            defectTable = new byte[33];

            unsafe
            {
                fixed (byte* pPtr = defectTable)
                    ReadFromUVCExtension(m_capFilter, XU_DEFECT_PIXEL_TABLE, new IntPtr(pPtr), 33, out length_returned);
            }

        }


        // write sensor register configuration into flash
        public void WriteSensorRegisterConfToFlash(byte[] regConf)
        {
            long length_returned;

            unsafe
            {
                fixed (byte* pPtr = regConf)
                    WriteToUVCExtension(m_capFilter, XU_SENSOR_REGISTER_CONFIGURATION, new IntPtr(pPtr), 256, out length_returned);
            }
        }

        // read sensor register configutation from flash
        public void ReadSensorRegisterConfFromFlash(out byte[] regConf)
        {
            long length_returned;
            regConf = new byte[256];

            unsafe
            {
                fixed (byte* pPtr = regConf)
                    ReadFromUVCExtension(m_capFilter, XU_SENSOR_REGISTER_CONFIGURATION, new IntPtr(pPtr), 256, out length_returned);
            }

        }

        public void ReadCamUUIDnHWFWRev(out String uuid, out UInt16 HwRev, out UInt16 FwRev)
        {
            byte[] buf = new byte[36+9+4];
            byte[] uuidBuf = new byte[36+9];

            long length_returned;

            unsafe
            {
                fixed (byte* pPtr = buf)
                    ReadFromUVCExtension(m_capFilter, XU_UUID_HWFW_REV, new IntPtr(pPtr), 36+4+9, out length_returned);
            }

            HwRev = (UInt16)(buf[0] | (UInt16)buf[1] << 8);
            FwRev = (UInt16)(buf[2] | (UInt16)buf[3] << 8);

            for (int i = 0; i < (36+9); i++)
            {
                uuidBuf[i] = buf[4 + i];
            }

            uuid = System.Text.Encoding.Default.GetString(uuidBuf);
        
        }

        // some camera has FusiID
        public void ReadCamUUIDnHWFWRev(out String uuid, out UInt16 HwRev, out UInt16 FwRev, out String fuseid)
        {
            byte[] buf = new byte[36 + 9 + 4 + 16];
            byte[] uuidBuf = new byte[36 + 9];
            byte[] fuseId = new byte[16];

            long length_returned;

            unsafe
            {
                fixed (byte* pPtr = buf)
                    ReadFromUVCExtension(m_capFilter, XU_UUID_HWFW_REV, new IntPtr(pPtr), 36 + 4 + 9 + 16, out length_returned);
            }

            HwRev = (UInt16)(buf[0] | (UInt16)buf[1] << 8);
            FwRev = (UInt16)(buf[2] | (UInt16)buf[3] << 8);

            for (int i = 0; i < (36 + 9); i++)
            {
                uuidBuf[i] = buf[4 + i];
            }

            uuid = System.Text.Encoding.Default.GetString(uuidBuf);

            for (int i = 0; i < 16; i++)
            {
                fuseId[i] = buf[49 + i];
            }

            fuseid = System.Text.Encoding.Default.GetString(fuseId);

        }
        public void ReadROI_MAX_MIN(out UInt16 ROIX_Max, out UInt16 ROIX_Min, out UInt16 ROIY_Max, out UInt16 ROIY_Min)
        {
            byte[] buf = new byte[8];

            long length_returned;

            unsafe
            {
                fixed (byte* pPtr = buf)
                    ReadFromUVCExtension(m_capFilter, XU_EXTENSION_INFO, new IntPtr(pPtr), 8, out length_returned);
            }

            ROIX_Max = (UInt16)(buf[1] | buf[0] << 8);
            ROIX_Min = (UInt16)(buf[3] | buf[2] << 8);
            ROIY_Max = (UInt16)(buf[5] | buf[4] << 8);
            ROIY_Min = (UInt16)(buf[7] | buf[6] << 8);
        }
        public void SetROI_Level(int roi_level)
        {
            long length_returned;
            byte[] bROIParam = new byte[8];

            if (m_roilevel != roi_level)
            {
                m_roilevel = roi_level;
                //Console.WriteLine(String.Format("set exposure time =" + m_exposure.ToString()));

                bROIParam[0] = (byte)(m_roilevel);
                unsafe
                {
                    fixed (byte* pPtr = bROIParam)
                        WriteToUVCExtension(m_capFilter, XU_EXTENSION_INFO, new IntPtr(pPtr), 8, out length_returned);
                }
            }
        }
        public int SetGain(int iGain)
        {
            SetVideoProcAmpProperty(DirectShowLib.VideoProcAmpProperty.Gain, iGain);

            return 0;
        }

        public int GetGain()
        {
             return GetVideoProcAmpProperty(DirectShowLib.VideoProcAmpProperty.Gain);
        }
/*
        public void GetResolution(DsDevice camera, out int [,] resList, out int resCount)
        {
            int hr;

            // add the video input device
            hr = m_FilterGraph.AddSourceFilterForMoniker(camera.Mon, null, camera.Name, out m_capFilter);

            IPin pCaptureOut = DsFindPin.ByCategory(m_capFilter, PinCategory.Capture, 0);

            VideoInfoHeader v;

            IAMStreamConfig videoStreamConfig = pCaptureOut as IAMStreamConfig;

            int iCount = 0, iSize = 0;
            videoStreamConfig.GetNumberOfCapabilities(out iCount, out iSize);

            IntPtr TaskMemPointer = Marshal.AllocCoTaskMem(iSize);

            AMMediaType pmtConfig = null;
            m_ResCnt = 0;
            for (int iFormat = 0; iFormat < iCount; iFormat++)
            {
                IntPtr ptr = IntPtr.Zero;

                videoStreamConfig.GetStreamCaps(iFormat, out pmtConfig, TaskMemPointer);

                v = (VideoInfoHeader)Marshal.PtrToStructure(pmtConfig.formatPtr, typeof(VideoInfoHeader));
                if ( v.BmiHeader.Width != 0 && v.BmiHeader.Height != 0)
                {
                    m_Resolution[m_ResCnt,0] = v.BmiHeader.Width;
                    m_Resolution[m_ResCnt,1] = v.BmiHeader.Height;
                    m_ResCnt++;
                }

            }

            Marshal.FreeCoTaskMem(TaskMemPointer);
            DsUtils.FreeAMMediaType(pmtConfig);

            resList = m_Resolution;
            resCount = m_ResCnt;
        }
*/
        public void GetResolution(DsDevice camera, out int[,] resList, out int resCount, out long[,] framerateList, out int framerateCount)
        {
            int hr;

            int ret = 0;
            int framerate_cnt = 0;
            int tmp = 0;
            IntPtr framerates;

            // add the video input device
            hr = m_FilterGraph.AddSourceFilterForMoniker(camera.Mon, null, camera.Name, out m_capFilter);

            IPin pCaptureOut = DsFindPin.ByDirection(m_capFilter, PinDirection.Output, 0); ;

            VideoInfoHeader v;

            IAMStreamConfig videoStreamConfig = pCaptureOut as IAMStreamConfig;

            int iCount = 0, iSize = 0;
            videoStreamConfig.GetNumberOfCapabilities(out iCount, out iSize);

            IAMVideoControl videocontrol = m_capFilter as IAMVideoControl;

            IntPtr TaskMemPointer = Marshal.AllocCoTaskMem(iSize);

            AMMediaType pmtConfig = null;
            m_ResCnt = 0;
            for (int iFormat = 0; iFormat < iCount; iFormat++)
            {
                IntPtr ptr = IntPtr.Zero;

                videoStreamConfig.GetStreamCaps(iFormat, out pmtConfig, TaskMemPointer);

                v = (VideoInfoHeader)Marshal.PtrToStructure(pmtConfig.formatPtr, typeof(VideoInfoHeader));

                if (v.BmiHeader.Width != 0 && v.BmiHeader.Height != 0)
                {
                    //To get the corresponding framerates each resolution supports

                    Size temp = new Size(v.BmiHeader.Width, v.BmiHeader.Height);

                    //  ret = videocontrol.GetFrameRateList(pCaptureOut, 1 << m_ResCnt, temp, out framerate_cnt, out framerates);

                    ret = videocontrol.GetFrameRateList(pCaptureOut, 2 * m_ResCnt, temp, out framerate_cnt, out framerates);
                    if (framerate_cnt > 3)
                        framerate_cnt = 3;

                    tmp = framerate_cnt;
                    unsafe
                    {

                        int* bytes = (int*)framerates.ToPointer();
                        for (int i = 0; i < framerate_cnt; i++)
                        {
                            m_FrameRate[m_ResCnt, i] = *(bytes + 2 * i);
                            //m_FrameRate[m_ResCnt, 1] = *(bytes + 2);
                            //m_FrameRate[m_ResCnt, 2] = *(bytes + 4);
                        }
                    }


                    m_Resolution[m_ResCnt, 0] = v.BmiHeader.Width;
                    m_Resolution[m_ResCnt, 1] = v.BmiHeader.Height;
                    m_ResCnt++;
                }

            }

            Marshal.FreeCoTaskMem(TaskMemPointer);
            DsUtils.FreeAMMediaType(pmtConfig);

            framerateList = m_FrameRate;
            framerateCount = tmp;
            resList = m_Resolution;
            resCount = m_ResCnt;
        }

        public void GetCurFormat(out int width, out int height)
        {
            width = 0; height = 0;

            if (m_capFilter == null)
                throw new Exception("camera not initialized");

            IPin pCaptureOut = DsFindPin.ByDirection(m_capFilter, PinDirection.Output, 0); ;

            VideoInfoHeader v;

            IAMStreamConfig videoStreamConfig = pCaptureOut as IAMStreamConfig;

            AMMediaType pmt;
            videoStreamConfig.GetFormat(out pmt);

            v = (VideoInfoHeader)Marshal.PtrToStructure(pmt.formatPtr, typeof(VideoInfoHeader));

            width = v.BmiHeader.Width;
            height = v.BmiHeader.Height;
        }

        public int GetCameraControlProperty(CameraControlProperty propCam, out CameraControlFlags controlFlag)
        {
            int hr;
            int iVal;

            hr = m_CamControl.Get(propCam, out iVal, out controlFlag);
 
            return iVal;
        }

        public int SetCameraControlProperty(CameraControlProperty propCam, int iVal, CameraControlFlags controlFlag)
        {
            int hr;
            hr = m_CamControl.Set(propCam, iVal, controlFlag);

            CameraControlFlags flags;
            m_CamControl.Get(propCam, out iVal, out flags);

            return hr;
        }

        public void GetCameraControlPropertyRange(CameraControlProperty propCam, out int Min, out int Max, out int Step, out int Default)
        {
            int hr;
            CameraControlFlags flagVideo;

            hr = m_CamControl.GetRange(propCam, out Min, out Max, out Step, out Default, out flagVideo);
        }

        public int SetVideoProcAmpProperty(VideoProcAmpProperty propVideo, int iVal)
        {
            int hr;

            hr = m_VidProcAmp.Set(propVideo, iVal, VideoProcAmpFlags.Manual);

            VideoProcAmpFlags flagVideo;
            m_VidProcAmp.Get(propVideo, out iVal, out flagVideo);

            return hr;
        }

        public int GetVideoProcAmpProperty(VideoProcAmpProperty propVideo)
        {
            int hr;
            int iVal;
            VideoProcAmpFlags flagVideo;

            hr = m_VidProcAmp.Get(propVideo, out iVal, out flagVideo);

            return iVal;
        }

        public void GetVideoProcAmpPropertyRange(VideoProcAmpProperty propVideo, out int Min, out int Max, out int Step, out int Default)
        {
            int hr;
            VideoProcAmpFlags flagVideo;

            hr = m_VidProcAmp.GetRange(propVideo, out Min, out Max, out Step, out Default, out flagVideo);
        }

         #region PInvoke to unmanaged functions
        [DllImport("CAppLib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int get_uvc_extension_property_value(IBaseFilter filter, int property_id);

        [DllImport("CAppLib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int set_uvc_extension_property_value(IBaseFilter filter, int property_id, byte byte1, byte byte0);

        [DllImport("CAppLib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int write_to_uvc_extension(IBaseFilter filter, int property_id, IntPtr bytes, int length, out long length_returned);

        [DllImport("CAppLib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int read_from_uvc_extension(IBaseFilter filter, int property_id, IntPtr bytes, int length, out long ulBytesReturned);

        [DllImport("CAppLib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int raw_to_bmp(IntPtr in_bytes, IntPtr out_bytes, int width, int height, int bpp);
        #endregion
    }

    internal sealed class NativeMethods
    {
        private NativeMethods() { }

        [DllImport("ole32.dll")]
		public static extern int CreateBindCtx(int reserved, out UCOMIBindCtx ppbc);

        [DllImport("ole32.dll")]
		public static extern int MkParseDisplayName(UCOMIBindCtx pcb, [MarshalAs(UnmanagedType.LPWStr)] string szUserName, out int pchEaten, out UCOMIMoniker ppmk);

        [DllImport("oleaut32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int OleCreatePropertyFrame(
            [In] IntPtr hwndOwner,
            [In] int x,
            [In] int y,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpszCaption,
            [In] int cObjects,
            [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.IUnknown)] object[] ppUnk,
            [In] int cPages,
            [In] IntPtr pPageClsID,
            [In] int lcid,
            [In] int dwReserved,
            [In] IntPtr pvReserved
            );
    }
 

}
