using System;
using System.Runtime.InteropServices;
using DirectShowLib;

namespace LeopardCamera
{
    public abstract class AbstractCamera
    {
        public enum CameraModel { M031, M034, M114, V034, Stereo, AR0330, MT9P031, ICP1_AR0330,C570,C661,NULL };
        public enum SENSOR_DATA_MODE { YUV, RAW12, RAW10, RAW8 };

        private int image_alloc_stat = 0;   // number of image allocated in memory
      
        /// <summary>
        /// disable public default constructor
        /// </summary>
        protected AbstractCamera() { }

        //public static AbstractCamera CameraInit()
        //{
        //    //AbstractCamera camera;

        //    //camera = new CameraLP { SerialID = "" };
        //    //return camera;
        //}

        /// <summary>
        /// Allocate memory for a new image
        /// </summary>
        /// <param name="height">Image height</param>
        /// <param name="weight">Image width</param>
        /// <param name="bit">Image bit per pixel</param>
        /// <returns>Pointer to the unmanaged buffer</returns>
        /// 
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

        public virtual int ResetStream()
        {
            return 0;
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

            pBuffer = this.AllocNewImage(this.Height, this.Width, this.Bits);
            IntPtr image = pBuffer;
            this.CameraCaptureImageNoWait(image, out iWidth, out iHeight, out iBPP);

            retval = 0;
            return retval;
        }

        public virtual void Close()
        {
         }

        public int ImageAllocStat { get { return image_alloc_stat; } }

        public abstract void Open(DsDevice cameraDevice);
        public abstract int CameraCaptureImage(IntPtr pBuffer, out int iWidth, out int iHeight, out int iBPP);
        public abstract int CameraCaptureImageNoWait(IntPtr pBuffer, out int iWidth, out int iHeight, out int iBPP);
        public abstract int SetPOS(int startX, int startY);
        public abstract int SetRGBGain(UInt16 rGain, UInt16 grGain, UInt16 gbGain, UInt16 bGain);
        public abstract int SetLED(bool Left0, bool left1, bool right0, bool right1);
        public abstract int RebootCamera();
        public abstract int EraseEEPROM();
        public abstract void ReadCamUUIDnHWFWRev(out String uuid, out UInt16 HwRev, out UInt16 FwRev);
        public abstract void Run();

        public abstract int Exposure { get; set; }
        public abstract int ExposureExt { get; set; }
        public abstract int Gain { get; set; }
        public abstract int Width { get; set; }
        public abstract int Height { get; set; }
        public abstract int Bits { get; set; }
        public abstract CameraModel cameraModel { get; }
    }

}
