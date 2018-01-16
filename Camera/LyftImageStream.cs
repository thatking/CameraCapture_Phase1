using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace LeopardCamera
{
    public class LyftImageStream
    {
        string imLogDir; // image logging directory
        bool imLogEnabled = false;
        const string imPrefix = "Lyft_IMX390_";
        string createdDirName = "";

        public bool ImLogEnabled { get => imLogEnabled; set => imLogEnabled = value; }

        public LyftImageStream()
        {
            Debug.Print("Creating Image stream capture object");
            imLogDir = System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\";
        }

        public LyftImageStream(bool createDirectory)
        {
            Debug.Print("Creating image capture object with create dir param");
            imLogDir = System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\";
            createdDirName = imPrefix + DateTime.Now.ToString("yy-MM-dd-HH-mm-ss") + "\\";
            imLogDir += createdDirName;
            Debug.Print("Logging images to : " + imLogDir);

            if (!Directory.Exists(imLogDir))
                Directory.CreateDirectory(imLogDir);
        }

        public void StartImageCapture()
        {
            Debug.Print("Starting image capture");
            imLogEnabled = true;
        }

        public void StopImageCapture()
        {
            Debug.Print("Stopping image capture");
            imLogEnabled = false;
        }

        public void StoreImage(ref Bitmap bmp)
        {
            if (imLogEnabled)
            {
                string fileName = imLogDir + DateTime.Now.ToString("yy-MM-dd-HH-mm-ss") + ".bmp";
                bmp.Save(fileName, ImageFormat.Bmp);    // save uncompressed
            }
        }

        ~LyftImageStream()
        {
            ImLogEnabled = false;
        }

        
    }
}
