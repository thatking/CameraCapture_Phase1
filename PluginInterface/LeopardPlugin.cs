/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  
*****************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeopardCamera;

namespace PluginInterface
{
    public enum PlugInParamType { PI_SETGAIN, PI_SETEXPOSURE, PI_FPN }; 

    public interface LeopardPlugin
    {
        string Name { get; }
        void Process(IntPtr pBuffer, int width, int height, int bpp, LeopardCamera.LPCamera.SENSOR_DATA_MODE sensorMode,
                    bool monoSensor, int pixelOrder, int exposureTime, string cameraID,  bool GammaEna, double gamma, 
			            						 bool RGBGainEna, int r_gain, int g_gain, int b_gain, int r_offset, int g_offset, int b_offset,
						            			 bool RGB2RGBEna, int rr,int rg,int rb,int gr,int gg,int gb,int br,int bg,int bb);

        // param is a list of pairs. The pair is composed of a byte (PlugInParamType) followed by 
        //       certain number of bytes
        // PI_SETGAIN : 1-byte gain
        // PI_SETEXPOSURE : 2-byte exposure time
        // PI_FPN : 2*height bytes of FPN table
        byte[] SetParam();

        void SetCameraId(LeopardCamera.LPCamera.CameraModel cameraid);
        void Initialize();
        void Close();
    }
}
