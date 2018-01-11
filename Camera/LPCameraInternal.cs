using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;
using DirectShowLib;

namespace LeopardCamera
{
    public partial class LPCamera 
    {
        public int RebootCamera()
        {
            return m_capture.RebootCamera();
        }

        public int EraseEEPROM()
        {
            return m_capture.EraseEEPROM();
        }

        public int SetSpiPortSelect(byte mode)
        {
            return m_capture.SetSpiPortSelect(mode);     
        }
	}
}