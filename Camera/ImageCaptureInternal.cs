using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using DirectShowLib;

namespace LeopardCamera
{
    using System.Collections;

    /// <summary> Summary description for MainForm. </summary>
    public partial class ImageCapture
    {
        const byte XU_ERASE_REBOOT = (0x0F);

        public int RebootCamera()
        {
            byte value = 0x9b;
            int n = set_uvc_extension_property_value(m_capFilter, XU_ERASE_REBOOT, 0, value);
            if (n != 0) throw new Exception("Set mode Property Value Error.");
            return 0;
        }

        public int EraseEEPROM()
        {
            byte value = 0x9a;
            int n = set_uvc_extension_property_value(m_capFilter, XU_ERASE_REBOOT, 0, value);
            if (n != 0) throw new Exception("Set mode Property Value Error.");
            return 0;
        }

        public int SetSpiPortSelect(byte mode)
        {
            byte value = (byte)(0xa0 | (mode & 0x0f));
            int n = set_uvc_extension_property_value(m_capFilter, XU_ERASE_REBOOT, 0, value);
            if (n != 0) throw new Exception("Set mode Property Value Error.");
            return 0;
        }
    }
}