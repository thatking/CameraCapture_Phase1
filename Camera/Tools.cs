/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  
*****************************************************************************/

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace LeopardCamera
{
    public class Tools
    {

        private static int pre_startX = 0, pre_startY = 0, pre_iSizeX = 0, pre_iSizeY = 0;

        public static Bitmap ConvertBayer2BMP(IntPtr ptr, int iInWidth, int iHeight, int iBits, int pixelOrder, 
            									 bool GammaEna, double gamma, 
			            						 bool RGBGainEna, int r_gain, int g_gain, int b_gain, int r_offset, int g_offset, int b_offset,
						            			 bool RGB2RGBEna, int rr,int rg,int rb,int gr,int gg,int gb,int br,int bg,int bb,
                                                bool Mono, bool Dual)
        {
            int iWidth = iInWidth * (Dual ? 2 : 1);
            int iSize = iWidth * iHeight * 2;

            int iPadding = (iWidth * 3) % 4;
            if (iPadding != 0)  // padding to make Bitmap.stride a multiple of 4
                iPadding = 4 - iPadding;

            Bitmap bmp = new Bitmap(iWidth, iHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                        ImageLockMode.WriteOnly, bmp.PixelFormat);
            IntPtr rgb = bmpData.Scan0;

            unsafe
            {
                if (Dual)
                {
                    IntPtr ptr_t = Marshal.AllocHGlobal(iWidth*iHeight);
                    //Debug.Print("Deinterlacing dual img");
                    convDualImage(ptr, ptr_t, iInWidth, iHeight);
                    if (!Mono)
                        raw_to_bmp(ptr_t, rgb, iWidth, iHeight, iBits, pixelOrder,
                                    GammaEna, gamma,  // gamma enable & gamma value
			            			RGBGainEna, r_gain, g_gain, b_gain, r_offset, g_offset, b_offset,
						            RGB2RGBEna, rr,rg,rb,gr,gg,gb,br,bg,bb);
                    else
                        raw_to_bmp_mono(ptr_t, rgb, iWidth, iHeight, iBits, (gamma != 1.0), gamma);

                    Marshal.FreeHGlobal(ptr_t);
                }
                else
                {
                    if (!Mono)
                    {
                        //Debug.Print("calling raw to bmp...");
                        raw_to_bmp(ptr, rgb, iWidth, iHeight, iBits, pixelOrder,
                                    GammaEna, gamma,  // gamma enable & gamma value
                                    RGBGainEna, r_gain, g_gain, b_gain, r_offset, g_offset, b_offset,
                                    RGB2RGBEna, rr, rg, rb, gr, gg, gb, br, bg, bb);
                    }
                    else
                    {

                        raw_to_bmp_mono(ptr, rgb, iWidth, iHeight, iBits, GammaEna, gamma);
                    }
                }
            }

            bmp.UnlockBits(bmpData);

            return bmp;
        }

        public static void ConvertBayer2y(IntPtr ptr, int iInWidth, int iHeight, int iBits, int pixelOrder, double gamma, bool Mono, bool Dual)
        {
            int iWidth = iInWidth * (Dual ? 2 : 1);
            int iSize = iWidth * iHeight * 2;

            IntPtr dst = ptr;

            unsafe
            {
                if (Dual)
                {
                    dual_raw8_to_y(ptr, iWidth, iHeight);
                }
                else
                {
                    if (Mono)
                    {
                        mono_to_y(ptr, dst, iWidth, iHeight, iBits);
                    }
                    else
                        bayer_to_y(ptr, dst, iWidth, iHeight, iBits);
                }
            }

        }

        public static Bitmap ConvrtYUV422BMPmono(IntPtr ptr, int iInWidth, int iHeight)
        {
            int iWidth = iInWidth ;
            int iSize = iWidth * iHeight * 3 / 2;

            int iPadding = (iWidth * 3) % 4;
            if (iPadding != 0)  // padding to make Bitmap.stride a multiple of 4
                iPadding = 4 - iPadding;

            Bitmap bmp = new Bitmap(iWidth, iHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                        ImageLockMode.WriteOnly, bmp.PixelFormat);
            IntPtr rgb = bmpData.Scan0;

            unsafe
            {
                yuv422_to_bmp_mono(ptr, rgb, iWidth, iHeight);
            }

            bmp.UnlockBits(bmpData);

            return bmp;
        }

        public static Bitmap ConvrtYUV422BMP(IntPtr ptr, int iInWidth, int iHeight,bool mark_en,int offset,int topleft ,int bottomleft,int topright,int bottomright)
        {
            int iWidth = iInWidth;
            int iSize = iWidth * iHeight * 3 / 2;

            int iPadding = (iWidth * 3) % 4;
            if (iPadding != 0)  // padding to make Bitmap.stride a multiple of 4
                iPadding = 4 - iPadding;

            Bitmap bmp = new Bitmap(iWidth, iHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                        ImageLockMode.WriteOnly, bmp.PixelFormat);
            IntPtr rgb = bmpData.Scan0;

            unsafe
            {
                convert_yuv_to_rgb_buffer(ptr, rgb, iWidth, iHeight,mark_en, offset,topleft,bottomleft,topright,bottomright);
            }

            bmp.UnlockBits(bmpData);

            return bmp;
        }

        public static void SaveRAWfile(IntPtr ptr, int iSize, String fileName)
        {
            byte[] arrBytes = new byte[iSize];
            Marshal.Copy(ptr, arrBytes, 0, iSize);

            System.IO.FileStream file = new System.IO.FileStream(fileName, System.IO.FileMode.Create);
            file.Write(arrBytes, 0, iSize);
            file.Close();
        }

        public static void SaveRAWfile(byte[] arrBytes, String fileName)
        {
            System.IO.FileStream file = new System.IO.FileStream(fileName, System.IO.FileMode.Create);
            file.Write(arrBytes, 0, arrBytes.Length);
            file.Close();
        }

        public static double CalcTemporalNoise(IntPtr ptr, byte[] arrBytesPre, int iWidth,
                                int iHeight, int startX, int startY, int iSizeX, int iSizeY, int iBPP)
        {
            double dValueNew = 0, dValuePre = 0, dTN = 0, dTotal = 0;
            double dDiffMean = 0;
            int dataWidth = (iBPP - 1) / 8 + 1;
            byte[] arrBytes = new byte[iWidth * iHeight * dataWidth];
            Marshal.Copy(ptr, arrBytes, 0, iWidth * iHeight * dataWidth);

            if (arrBytes == null || arrBytesPre == null)
                return 0;

            if (arrBytes.Length != arrBytesPre.Length)
                return 0;

            // only do calculation if the location and the size stay the same
            if (pre_startX == startX && pre_startY == startY
                && pre_iSizeX == iSizeX && pre_iSizeY == iSizeY
                && iSizeX > 1 && iSizeY > 1)
            {

                int startPos = (startY * iWidth + startX) * dataWidth;

                // get the mean value of the difference of the new & previous image
                if (dataWidth == 2) // 16 bit mode
                {
                    for (int i = 0; i < iSizeY; i++)
                    {
                        for (int j = 0; j < iSizeX; j++)
                        {
                            dValueNew = (double)(arrBytes[startPos + (i * iWidth + j) * 2] | arrBytes[startPos + (i * iWidth + j) * 2 + 1] << 8);
                            dValuePre = (double)(arrBytesPre[startPos + (i * iWidth + j) * 2] | arrBytesPre[startPos + (i * iWidth + j) * 2 + 1] << 8);
                            dDiffMean += (dValueNew - dValuePre) / iSizeY / iSizeX;
                        }
                    }
                }
                else // 8 bit mode
                {
                    for (int i = 0; i < iSizeY; i++)
                    {
                        for (int j = 0; j < iSizeX; j++)
                        {
                            dValueNew = (double)arrBytes[startPos + (i * iWidth + j)];
                            dValuePre = (double)arrBytesPre[startPos + (i * iWidth + j)];
                            dDiffMean += (dValueNew - dValuePre) / iSizeY / iSizeX;
                        }
                    }
                }

                // Temporal Noise is the STD of the difference of these two images
                if (dataWidth == 2) // 16 bit mode
                {
                    for (int i = 0; i < iSizeY; i++)
                    {
                        for (int j = 0; j < iSizeX; j++)
                        {
                            dValueNew = (double)(arrBytes[startPos + (i * iWidth + j) * 2] | arrBytes[startPos + (i * iWidth + j) * 2 + 1] << 8);
                            dValuePre = (double)(arrBytesPre[startPos + (i * iWidth + j) * 2] | arrBytesPre[startPos + (i * iWidth + j) * 2 + 1] << 8);
                            dTotal += (dValueNew - dValuePre - dDiffMean) * (dValueNew - dValuePre - dDiffMean);
                        }
                    }
                }
                else // 8 bit mode
                {
                    for (int i = 0; i < iSizeY; i++)
                    {
                        for (int j = 0; j < iSizeX; j++)
                        {
                            dValueNew = (double)arrBytes[startPos + (i * iWidth + j)];
                            dValuePre = (double)arrBytesPre[startPos + (i * iWidth + j)];
                            dTotal += (dValueNew - dValuePre - dDiffMean) * (dValueNew - dValuePre - dDiffMean);
                        }
                    }
                }

                dTN = Math.Sqrt(dTotal / iSizeX / iSizeY / 2);

                return (dTN);
            }

            pre_startX = startX;
            pre_startY = startY;
            pre_iSizeX = iSizeX;
            pre_iSizeY = iSizeY;

            return 0;
        }

        public static double CalcSTD(IntPtr ptr, byte[] arrayRef, double dMean, int iWidth,
                                int iHeight, int startX, int startY, int iSizeX, int iSizeY, int iBPP)
        {
            double dValue = 0, dSTD = 0, dTotal = 0;
            int dataWidth = (iBPP - 1) / 8 + 1;
            byte[] arrBytes = new byte[iWidth * iHeight * dataWidth];
            Marshal.Copy(ptr, arrBytes, 0, iWidth * iHeight * dataWidth);

            int startPos = (startY * iWidth + startX) * dataWidth;

            if (dataWidth == 2) // 16 bit mode
            {
                for (int i = 0; i < iSizeY; i++)
                {
                    for (int j = 0; j < iSizeX; j++)
                    {
                        dValue = (double)(arrBytes[startPos + (i * iWidth + j) * 2] | arrBytes[startPos + (i * iWidth + j) * 2 + 1] << 8)
                            - (double)(arrayRef[startPos + (i * iWidth + j) * 2] | arrayRef[startPos + (i * iWidth + j) * 2 + 1] << 8);
                        dTotal += (dValue - dMean) * (dValue - dMean);
                    }
                }
            }
            else // 8 bit mode
            {
                for (int i = 0; i < iSizeY; i++)
                {
                    for (int j = 0; j < iSizeX; j++)
                    {
                        dValue = (double)arrBytes[startPos + (i * iWidth + j)]
                                 - (double)arrayRef[startPos + (i * iWidth + j)];
                        dTotal += (dValue - dMean) * (dValue - dMean);
                    }
                }
            }

            dSTD = Math.Sqrt(dTotal / iSizeX / iSizeY);

            return (dSTD);
        }

        public static double CalcSTD(IntPtr ptr, double dMean, int iWidth, 
                                        int iHeight, int startX, int startY, int iSizeX, int iSizeY, int iBPP)
        {
            double dValue = 0, dSTD = 0, dTotal = 0;
            int dataWidth = (iBPP - 1) / 8 + 1;
            byte[] arrBytes = new byte[iWidth * iHeight * dataWidth];
            Marshal.Copy(ptr, arrBytes, 0, iWidth * iHeight * dataWidth);

            int startPos = (startY * iWidth + startX) * dataWidth;

            if (dataWidth == 2) // 16 bit mode
            {
                for (int i = 0; i < iSizeY; i++)
                {
                    for (int j = 0; j < iSizeX; j++)
                    {
                        dValue = (double)(arrBytes[startPos + (i * iWidth + j) * 2] | arrBytes[startPos + (i * iWidth + j) * 2 + 1] << 8);
                        dTotal += (dValue - dMean) * (dValue - dMean);
                    }
                }
            }
            else // 8 bit mode
            {
                for (int i = 0; i < iSizeY; i++)
                {
                    for (int j = 0; j < iSizeX; j++)
                    {
                        dValue = (double)arrBytes[startPos + (i * iWidth + j)];
                        dTotal += (dValue - dMean) * (dValue - dMean);
                    }
                }
            }

            dSTD = Math.Sqrt(dTotal / iSizeX / iSizeY);

            return (dSTD);
        }

        public static double CalcMean(byte[] arrBytes, int iWidth, int iHeight, int startX, int startY, int iSizeX, int iSizeY, int iBPP)
        {
            double dTotal = 0, dMean = 0;
            int dataWidth = (iBPP - 1) / 8 + 1;

            int startPos = (startY * iWidth + startX) * dataWidth;

            if (dataWidth == 2) // 16 bit mode
            {
                for (int i = 0; i < iSizeY; i++)
                {
                    for (int j = 0; j < iSizeX; j++)
                    {
                        dTotal += (double)(arrBytes[startPos + (i * iWidth + j) * 2] | arrBytes[startPos + (i * iWidth + j) * 2 + 1] << 8);
                    }
                }
            }
            else // 8 bit mode
            {
                for (int i = 0; i < iSizeY; i++)
                {
                    for (int j = 0; j < iSizeX; j++)
                    {
                        dTotal += (double)arrBytes[startPos + (i * iWidth + j)];
                    }
                }
            }

            dMean = dTotal / iSizeX / iSizeY;

            return (dMean);
        }

        public static double CalcMean(IntPtr ptr, byte[] arrayRef, int iWidth, int iHeight, int startX, int startY, int iSizeX, int iSizeY, int iBPP)
        {
            double dTotal = 0, dMean = 0;
            int dataWidth = (iBPP - 1) / 8 + 1;
            byte[] arrBytes = new byte[iWidth * iHeight * dataWidth];
            Marshal.Copy(ptr, arrBytes, 0, iWidth * iHeight * dataWidth);

            int startPos = (startY * iWidth + startX) * dataWidth;

            if (dataWidth == 2) // 16 bit mode
            {
                for (int i = 0; i < iSizeY; i++)
                {
                    for (int j = 0; j < iSizeX; j++)
                    {
                        dTotal += (double)(arrBytes[startPos + (i * iWidth + j) * 2] | arrBytes[startPos + (i * iWidth + j) * 2 + 1] << 8)
                                            - (double)(arrayRef[startPos + (i * iWidth + j) * 2] | arrayRef[startPos + (i * iWidth + j) * 2 + 1] << 8);
                    }
                }
            }
            else // 8 bit mode
            {
                for (int i = 0; i < iSizeY; i++)
                {
                    for (int j = 0; j < iSizeX; j++)
                    {
                        dTotal += (double)arrBytes[startPos + (i * iWidth + j)]
                            - (double)arrayRef[startPos + (i * iWidth + j)];
                    }
                }
            }

            dMean = dTotal / iSizeX / iSizeY;

            return (dMean);
        }

        public static double CalcMean(IntPtr ptr, int iWidth, int iHeight, int startX, int startY, int iSizeX, int iSizeY, int iBPP)
        {
            double dTotal = 0, dMean = 0;
            int dataWidth = (iBPP - 1) / 8 + 1;
            byte[] arrBytes = new byte[iWidth * iHeight * dataWidth];
            Marshal.Copy(ptr, arrBytes, 0, iWidth * iHeight * dataWidth);

            int startPos = (startY * iWidth + startX) * dataWidth;

            if (dataWidth == 2) // 16 bit mode
            {
                for (int i = 0; i < iSizeY; i++)
                {
                    for (int j = 0; j < iSizeX; j++)
                    {
                        dTotal += (double)(arrBytes[startPos + (i * iWidth + j) * 2] | arrBytes[startPos + (i * iWidth + j) * 2 + 1] << 8);
                    }
                }
            }
            else // 8 bit mode
            {
                for (int i = 0; i < iSizeY; i++)
                {
                    for (int j = 0; j < iSizeX; j++)
                    {
                        dTotal += (double)arrBytes[startPos + (i * iWidth + j)];
                    }
                }
            }

            dMean = dTotal / iSizeX / iSizeY;

            return (dMean);
        }

        public static double CalcMean(IntPtr ptr, int iWidth, int iHeight, int startX, int startY, int iSize, int iBPP)
        {
            double dTotal = 0, dMean = 0;
            int dataWidth = ( iBPP -1) / 8 + 1;
            byte[] arrBytes = new byte[iWidth * iHeight * dataWidth];
            Marshal.Copy(ptr, arrBytes, 0, iWidth * iHeight * dataWidth);

            int startPos = (startY * iWidth + startX) * dataWidth;

            if (dataWidth == 2) // 16 bit mode
            {
                for (int i = 0; i < iSize; i++)
                {
                    for (int j = 0; j < iSize; j++)
                    {
                        dTotal += (double)(arrBytes[startPos + (i * iWidth + j) * 2] | arrBytes[startPos + (i * iWidth + j) * 2 + 1] << 8);
                    }
                }
            }
            else // 8 bit mode
            {
                for (int i = 0; i < iSize; i++)
                {
                    for (int j = 0; j < iSize; j++)
                    {
                        dTotal += (double)arrBytes[startPos + (i * iWidth + j)];
                    }
                }
            }

            dMean = dTotal / iSize / iSize;

            return (dMean);
        }

        public static int SobleOperator(IntPtr in_buf, int iWidth, int iHeight, int startX, int startY, int iSize)
        {
            unsafe
            {
                return y_SobleOperator(in_buf, iWidth, iHeight, startX, startY, iSize);
            }
        }

        public static int CalcMean(IntPtr in_buf, int iWidth, int iHeight, int startX, int startY, int iSize)
        {
            unsafe
            {
                return y_CalcMean(in_buf, iWidth, iHeight, startX, startY, iSize);
            }
        }

        public static int MTF(IntPtr in_buf, int iWidth, int iHeight, int startX, int startY, int iSize)
        {
            unsafe
            {
                return y_MTF(in_buf, iWidth, iHeight, startX, startY, iSize);
            }
        }

        public static int yuv422_TO_y(IntPtr in_bytes, IntPtr out_bytes, int width, int height)
        {
            unsafe
            {
                return yuv422_to_y(in_bytes, out_bytes, width, height);
            }
        }

        public static void ReframeTo720p(IntPtr out_buf, IntPtr in_buf, int iWidth, int iHeight, int dataWidth)
        {
            unsafe
            {
                reframeTo720p(out_buf, in_buf, iWidth, iHeight, dataWidth);
            }
        }

        public static void ReframeTo720p_4corners(IntPtr out_buf, IntPtr in_buf, int iWidth, int iHeight, int dataWidth)
        {
            unsafe
            {
                reframeTo720p_4corners(out_buf, in_buf, iWidth, iHeight, dataWidth);
            }
        }
        

        #region PInvoke to unmanaged functions
        [DllImport("CAppLib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int raw_to_bmp(IntPtr in_bytes, IntPtr out_bytes, int width, int height, int bpp, int pixel_order,
									 bool GammaEna, double gamma, 
									 bool RGBGainEna, int r_gain, int g_gain, int b_gain, int r_offset, int g_offset, int b_offset,
									 bool RGB2RGBEna, int rr,int rg,int rb,int gr,int gg,int gb,int br,int bg,int bb
									 );

        [DllImport("CAppLib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int raw_to_bmp_mono(IntPtr in_bytes, IntPtr out_bytes, int width, int height, int bpp,
									 bool GammaEna, double gamma);

        [DllImport("CAppLib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern double calc_mean(IntPtr in_bytes, int width, int height, int bpp, int startX, int startY, int iSize);

        [DllImport("CAppLib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void convDualImage(IntPtr in_bytes, IntPtr out_bytes, int width, int height);

        [DllImport("CAppLib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int yuv422_to_bmp_mono(IntPtr in_bytes, IntPtr out_bytes, int width, int height);

        [DllImport("CAppLib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int y_SobleOperator(IntPtr in_buf, int iWidth, int iHeight, int startX, int startY, int iSize);

        [DllImport("CAppLib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int y_CalcMean(IntPtr in_buf, int iWidth, int iHeight, int startX, int startY, int iSize);

        [DllImport("CAppLib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int y_MTF(IntPtr in_buf, int iWidth, int iHeight, int startX, int startY, int iSize);

        [DllImport("CAppLib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int yuv422_to_y(IntPtr in_bytes, IntPtr out_bytes, int width, int height);

        [DllImport("CAppLib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int dual_raw8_to_y(IntPtr in_bytes, int width, int height);

        [DllImport("CAppLib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int mono_to_y(IntPtr in_bytes, IntPtr out_bytes, int width, int height, int iBits);

        [DllImport("CAppLib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int bayer_to_y(IntPtr in_bytes, IntPtr out_bytes, int width, int height, int iBits);

        [DllImport("CAppLib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int convert_yuv_to_rgb_buffer(IntPtr yuv, IntPtr rgb, int width, int height, bool mark_en, int Center_picturebox_offset, int left_top, int left_bottom, int right_top, int right_bottom);

        [DllImport("CAppLib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void reframeTo720p(IntPtr out_buf, IntPtr in_buf, int iWidth, int iHeight, int dataWidth);

        [DllImport("CAppLib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void reframeTo720p_4corners(IntPtr out_buf, IntPtr in_buf, int iWidth, int iHeight, int dataWidth);

        #endregion
    }
}

