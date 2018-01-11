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
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;


namespace EmguTool
{
    public class EmguDemo
    {
        public enum EmguDemoId { FontDemo, FDDemo, LPRDemo, DisableDemo };
       // private static CascadeClassifier face = new CascadeClassifier("haarcascade_frontalface_default.xml");
       // private static CascadeClassifier eye = new CascadeClassifier("haarcascade_eye.xml");
        private static CascadeClassifier face = null; // new CascadeClassifier("haarcascade_frontalface_default.xml");
        private static CascadeClassifier eye = null;//new CascadeClassifier("haarcascade_eye.xml");

        public static Bitmap EmguFontDemo(Bitmap inBmp)
        {
            Bitmap bitmap;

            Image<Bgr, Byte> img = new Image<Bgr, Byte>(inBmp);
            inBmp.Dispose();

            MCvFont font = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_COMPLEX, 1.0, 1.0);
            img.Draw("Test EmguCV font function!", ref font, new Point(25, 100), new Bgr(255, 0, 0));
            bitmap = img.ToBitmap();
            if (img != null)
                img.Dispose();

            return bitmap;
        }

        public static Bitmap EmguFDDemo(Bitmap inBmp)
        {
            Bitmap bitmap;

            if (face == null ||eye == null)
            {
                face = new CascadeClassifier("haarcascade_frontalface_default.xml");
                eye = new CascadeClassifier("haarcascade_eye.xml");
            }

            Image<Bgr, Byte> img = new Image<Bgr, Byte>(inBmp);
            inBmp.Dispose();

            Image<Gray, Byte> gray = img.Convert<Gray, Byte>();
            List<Rectangle> faces = new List<Rectangle>();
            List<Rectangle> eyes = new List<Rectangle>();


            // face detection
            Rectangle[] facesDetected = face.DetectMultiScale(gray, 1.1, 10, new Size(20, 20), Size.Empty);
            faces.AddRange(facesDetected);

            // eyes detection
            foreach (Rectangle f in facesDetected)
            {
                gray.ROI = f;
                Rectangle[] eyesDetected = eye.DetectMultiScale(gray, 1.1, 10, new Size(20, 20), Size.Empty);
                gray.ROI = Rectangle.Empty;

                foreach (Rectangle ey in eyesDetected)
                {
                    Rectangle eyeRect = ey;
                    eyeRect.Offset(f.X, f.Y);
                    eyes.Add(eyeRect);
                }
            }

            gray.Dispose();
 
            foreach (Rectangle face1 in faces)
            {
                img.Draw(face1, new Bgr(Color.Red), 2);
            }

            foreach (Rectangle eye1 in eyes)
            {
                img.Draw(eye1, new Bgr(Color.Blue), 2);
            }

            bitmap = img.ToBitmap();
            if (img != null)
                img.Dispose();

            return bitmap;
        }

        public static Bitmap EmguDemoRun(EmguDemoId index, Bitmap inbmp)
        { 
            if(index == EmguDemoId.FontDemo)
            {
                return EmguFontDemo(inbmp);
            }
            else if(index == EmguDemoId.FDDemo)
            {
                return EmguFDDemo(inbmp);
            }
            else if(index == EmguDemoId.LPRDemo)
            {
                return inbmp;
            }
            else
            {
                return inbmp;
            }
        }
    }
}