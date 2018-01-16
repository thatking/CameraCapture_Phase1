using System;
using System.Threading;
using System.Drawing;
using System.Diagnostics;
using AForge.Video.FFMPEG;

namespace LeopardCamera
{
    public class VideoCapture
    {
        string vLogFile;
        const string vLogFilePrefix = "Lyft_IMX390_";
        VideoFileWriter writer;
        bool writerIsOpen = false;
        bool inProgess = false;

        private static Mutex writerMutex = new Mutex();

        // Store videos in My Videos by default
        // TODO KB: do we need an option to change this
        string vLogFileDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyVideos) + "\\";

        public bool InProgess { get => inProgess;}

        public VideoCapture()
        {
            Debug.Print("Creating video capture obj");
            writer = new VideoFileWriter();
        }

        private void ResetAllFlags()
        {
            writerIsOpen = false;
            inProgess = false;
        }

        private void StartWriter(int width, int height)
        {
            Debug.Print("Default Directory: " + vLogFileDirectory);
            vLogFile = vLogFileDirectory + vLogFilePrefix + 
                        (DateTime.Now.ToString("yy-MM-dd-HH-mm-ss")) + ".avi";

            Debug.Print("Logging to " + vLogFile);

            writer.Open(vLogFile, width, height);

            writerIsOpen = true;
        }

        public void WriteVideoLogFrame(Bitmap frame)
        {
            if (!inProgess)
            {
                return;
            }

            // new log file each time the writer is being opened.
            if (!writerIsOpen)
            {
                StartWriter(frame.Width, frame.Height);
            }

            bool locked = writerMutex.WaitOne();
            if (locked)
            {
                try
                {
                    writer.WriteVideoFrame(frame);
                }
                catch (Exception e)
                {
                    Debug.Print("Failed to write frame. ");
                    throw e;
                }
                finally
                {
                    writerMutex.ReleaseMutex();
                }
            }
        }

        public void StartVideoCapture()
        {
            Debug.Print("Starting video capture..");
            inProgess = true;
        }

        public void StopVideoCapture()
        {
            Debug.Print("Stopping Video Capture");
            if (writerIsOpen)
            {
                bool locked = writerMutex.WaitOne();
                if (locked)
                {
                    inProgess = false;
                    writer.Close();
                    writerIsOpen = false;
                    writerMutex.ReleaseMutex();
                }
            }
        }

        ~VideoCapture()
        {
            StopVideoCapture();
        }
    }
}
