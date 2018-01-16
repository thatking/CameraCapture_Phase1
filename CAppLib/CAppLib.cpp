/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  
*****************************************************************************/

#include <stdio.h>
#include <windows.h>
#include <commdlg.h>
//#include <streams.h>
#include <initguid.h>
#include <Vidcap.h>
#include <ks.h>
#include <ksmedia.h>
#include <ksproxy.h>
#include <math.h>

#include "raw2bmp.c"

// {78E321E1-C8AC-40A5-8AC9-75A2A02C74FB}
DEFINE_GUID(GUID_EXTENSION_UNIT_DESCRIPTOR, 
0x78e321e1, 0xc8ac, 0x40a5, 0x8a, 0xc9, 0x75, 0xa2, 0xa0, 0x2c, 0x74, 0xfb);

extern "C"
{
////////////// GAMMA_CORRECTION ///////////////////////
static unsigned short linear_to_gamma[65536];
static double gammaValue = -1;
static int gBPP = 0;

// create gamma table
static void initGammaTable(double gamma, int bpp)
{
    int result;
	double dMax;
	int iMax;

	if (bpp > 12)
		return;

	dMax = pow(2, (double)bpp);
	iMax = (int)dMax;
	
    for (int i = 0; i < iMax; i++) {
        result = (int)(pow((double)i/dMax, 1.0/gamma)*dMax);

        linear_to_gamma[i] = result;
    }

	gammaValue = gamma;
	gBPP = bpp;

}

static void gammaCorrection(BYTE* in_bytes, BYTE* out_bytes, int width, int height, int bpp, double gamma)
{
	int i;
	WORD *srcShort;
	WORD *dstShort;

	if (gamma != gammaValue || gBPP != bpp)
		initGammaTable(gamma, bpp);

	if (bpp > 8)
	{
		srcShort = (WORD *) (in_bytes);
		dstShort = (WORD *) (out_bytes);
		for (i=0; i<width*height; i++)
			*dstShort++ = linear_to_gamma[*srcShort++];
	}
	else
	{
		for (i=0; i<width*height; i++)
			*out_bytes++ = (BYTE)linear_to_gamma[*in_bytes++];
	}

}
////////////// GAMMA_CORRECTION ///////////////////////

// input image: two images interlaced pixel by pixel. 
//              In each WORD, the MSB 8bits are for the left image, the LSB 8bits are for the right image
// output image: two image side by side, each pixel is one byte.
__declspec(dllexport) void convDualImage(BYTE* in_bytes, BYTE* out_bytes, int width, int height)
{
	int i, j;
	BYTE *srcShort;
	BYTE *dstShortL;
	BYTE *dstShortR;

	srcShort = (BYTE *) (in_bytes);
	dstShortL = (BYTE *) (out_bytes);
	dstShortR = dstShortL + width;

	for (i=0; i<height; i++)
	{
		for (j=0; j<width; j++)
		{
			*dstShortL++ = *srcShort++;
			*dstShortR++ = *srcShort++;
		}
		dstShortL += width;
		dstShortR += width;
	}
}

static void rgb2rgb(BYTE* in_bytes, BYTE* out_bytes, int width, int height, int bpp,
					int rr,int rg,int rb,int gr,int gg,int gb,int br,int bg,int bb)
{
	int i, j;
	int r_in, g_in, b_in, r_out, g_out, b_out;

	for (i=0; i<height; i++)
		for (j=0; j<width; j++)
		{
			b_in = *in_bytes++; 
			g_in = *in_bytes++; 
			r_in = *in_bytes++;

			r_out = (rr * r_in + rg * g_in + rb * b_in) / 256;
			g_out = (gr * r_in + gg * g_in + gb * b_in) / 256;
			b_out = (br * r_in + bg * g_in + bb * b_in) / 256;

			*out_bytes++ = (b_out > 255) ? 255 : ( (b_out < 0) ? 0 : b_out);
			*out_bytes++ = (g_out > 255) ? 255 : ( (g_out < 0) ? 0 : g_out);
			*out_bytes++ = (r_out > 255) ? 255 : ( (r_out < 0) ? 0 : r_out);
		}
}

__declspec(dllexport) int get_uvc_extension_property_value(IBaseFilter* camera, int property_id)
{
	HRESULT hr; 
	IKsTopologyInfo *pKsTopologyInfo;
	hr = camera->QueryInterface(__uuidof(IKsTopologyInfo), (void **) &pKsTopologyInfo);
	
	DWORD numberOfNodes;
	hr = pKsTopologyInfo->get_NumNodes(&numberOfNodes);

	DWORD i;	GUID nodeGuid;
	for (i = 0; i < numberOfNodes; i++)
	{
		if (SUCCEEDED(pKsTopologyInfo->get_NodeType(i, &nodeGuid)))
		{
			if ( nodeGuid == KSNODETYPE_DEV_SPECIFIC )
			{ // Found the extension node
				DWORD pNodeId = i; 
				IKsNodeControl *pUnk;
				IKsControl *pKsControl;
				BYTE buf[100];

				// create node instance
				hr = pKsTopologyInfo->CreateNodeInstance( i, __uuidof(IUnknown), (VOID**) &pUnk );
				hr = pUnk->QueryInterface( __uuidof(IKsControl),  (VOID**)&pKsControl );

				KSP_NODE  s;	ULONG  ulBytesReturned;
				buf[0] = 0x00;	buf[1] = 0x00;

				// this is guid of our device extension unit
				s.Property.Set = GUID_EXTENSION_UNIT_DESCRIPTOR;
				s.Property.Id = property_id;
				s.Property.Flags = KSPROPERTY_TYPE_GET | KSPROPERTY_TYPE_TOPOLOGY;
				s.NodeId = i;
				hr = pKsControl->KsProperty( (PKSPROPERTY) &s, sizeof(s),&buf[0], 2, &ulBytesReturned );

				int ret = ((int)buf[1])<<8 | buf[0];
				return ret;
			}
		}
	}

	return -1;
}

__declspec(dllexport) int set_uvc_extension_property_value(IBaseFilter* camera, int property_id, byte byte1, byte byte0)
{
	HRESULT hr; 
	IKsTopologyInfo *pKsTopologyInfo;
	hr = camera->QueryInterface(__uuidof(IKsTopologyInfo), (void **) &pKsTopologyInfo);
	
	DWORD numberOfNodes;	hr = pKsTopologyInfo->get_NumNodes(&numberOfNodes);

	DWORD i;	GUID nodeGuid;
	for (i = 0; i < numberOfNodes; i++)
	{
		if (SUCCEEDED(pKsTopologyInfo->get_NodeType(i, &nodeGuid)))
		{
			if ( nodeGuid == KSNODETYPE_DEV_SPECIFIC )
			{ // Found the extension node
				DWORD pNodeId = i; 
				IKsNodeControl *pUnk;
				IKsControl *pKsControl;
				BYTE buf[100];

				// create node instance
				hr = pKsTopologyInfo->CreateNodeInstance( i, __uuidof(IUnknown), (VOID**) &pUnk );
				hr = pUnk->QueryInterface( __uuidof(IKsControl),  (VOID**)&pKsControl );

				KSP_NODE  s;	ULONG  ulBytesReturned;
				buf[1] = byte1; buf[0] = byte0;

				// this is guid of our device extension unit
				s.Property.Set = GUID_EXTENSION_UNIT_DESCRIPTOR;
				s.Property.Id = property_id;
				s.Property.Flags = KSPROPERTY_TYPE_SET | KSPROPERTY_TYPE_TOPOLOGY;
				s.NodeId = i;
				hr = pKsControl->KsProperty( (PKSPROPERTY) &s, sizeof(s),&buf[0], 2, &ulBytesReturned );

				if (hr == S_OK)	return 0;
				else return -1;
			}
		}
	}

	return -1;
}

__declspec(dllexport) int read_from_uvc_extension(IBaseFilter* camera, int property_id, BYTE* bytes, int length, ULONG* ulBytesReturned)
{
	HRESULT hr; 
	IKsTopologyInfo *pKsTopologyInfo;
	hr = camera->QueryInterface(__uuidof(IKsTopologyInfo), (void **) &pKsTopologyInfo);
	
	DWORD numberOfNodes;
	hr = pKsTopologyInfo->get_NumNodes(&numberOfNodes);

	DWORD i;	GUID nodeGuid;
	for (i = 0; i < numberOfNodes; i++)
	{
		if (SUCCEEDED(pKsTopologyInfo->get_NodeType(i, &nodeGuid)))
		{
			if ( nodeGuid == KSNODETYPE_DEV_SPECIFIC )
			{ // Found the extension node
				DWORD pNodeId = i; 
				IKsNodeControl *pUnk;
				IKsControl *pKsControl;

				// create node instance
				hr = pKsTopologyInfo->CreateNodeInstance( i, __uuidof(IUnknown), (VOID**) &pUnk );
				hr = pUnk->QueryInterface( __uuidof(IKsControl),  (VOID**)&pKsControl );

				KSP_NODE  s;	

				// this is guid of our device extension unit
				s.Property.Set = GUID_EXTENSION_UNIT_DESCRIPTOR;
				s.Property.Id = property_id;
				s.Property.Flags = KSPROPERTY_TYPE_GET | KSPROPERTY_TYPE_TOPOLOGY;
				s.NodeId = i;
				hr = pKsControl->KsProperty( (PKSPROPERTY) &s, sizeof(s),bytes, length, ulBytesReturned );

				return 0;
			}
		}
	}

	return -1;
}

__declspec(dllexport) int write_to_uvc_extension(IBaseFilter* camera, int property_id, BYTE* bytes, int length, ULONG* ulBytesReturned)
{
	HRESULT hr; 
	IKsTopologyInfo *pKsTopologyInfo;
	hr = camera->QueryInterface(__uuidof(IKsTopologyInfo), (void **) &pKsTopologyInfo);
	
	DWORD numberOfNodes;	hr = pKsTopologyInfo->get_NumNodes(&numberOfNodes);

	DWORD i;	GUID nodeGuid;
	for (i = 0; i < numberOfNodes; i++)
	{
		if (SUCCEEDED(pKsTopologyInfo->get_NodeType(i, &nodeGuid)))
		{
			if ( nodeGuid == KSNODETYPE_DEV_SPECIFIC )
			{ // Found the extension node
				DWORD pNodeId = i; 
				IKsNodeControl *pUnk;
				IKsControl *pKsControl;

				// create node instance
				hr = pKsTopologyInfo->CreateNodeInstance( i, __uuidof(IUnknown), (VOID**) &pUnk );
				hr = pUnk->QueryInterface( __uuidof(IKsControl),  (VOID**)&pKsControl );

				KSP_NODE  s;	//ULONG  ulBytesReturned;

				// this is guid of our device extension unit
				s.Property.Set = GUID_EXTENSION_UNIT_DESCRIPTOR;
				s.Property.Id = property_id;
				s.Property.Flags = KSPROPERTY_TYPE_SET | KSPROPERTY_TYPE_TOPOLOGY;
				s.NodeId = i;
				hr = pKsControl->KsProperty( (PKSPROPERTY) &s, sizeof(s), bytes, length, ulBytesReturned );

				if (hr == S_OK)	return 0;
				else return -1;
			}
		}
	}

	return -1;
}

__declspec(dllexport) int raw_to_bmp(BYTE* in_bytes, BYTE* out_bytes, int width, int height, int bpp, int pixel_order,
									 bool GammaEna, double gamma, 
									 bool RGBGainEna, int r_gain, int g_gain, int b_gain, int r_offset, int g_offset, int b_offset,
									 bool RGB2RGBEna, int rr,int rg,int rb,int gr,int gg,int gb,int br,int bg,int bb
									 )
{
	int i,j;
	int shift = bpp - 8;
	int maxValue = ( 1 << bpp) - 1;
	int itmp;
	unsigned short tmp;
	BYTE* ptr = in_bytes;
	BYTE* src = in_bytes;
	WORD* srcWord = (WORD *) in_bytes;
	unsigned short * wPtr = (unsigned short *)(in_bytes);
	unsigned short * wSrc = (unsigned short *)in_bytes;
	//unsigned int ctr = 0;

	/*
	printf("width: %d, height %d, bpp %d, pixel_order: %d, gammaEna: %d, gamma: %lf "
		"RgbGainEna: %d, rgain %d, ggain %d, bgain %d, roff %d, goff %d, boff %d "
		"RGB2RGBEna: %d, rr %d, rg %d, rb %d, gr %d, gg %d gb %d, br %d bg %d, bb %d\n",
		width, height, bpp, pixel_order, GammaEna, gamma,
		RGBGainEna, r_gain, g_gain, b_gain, r_offset, g_offset, b_offset,
		RGB2RGBEna, rr, rg, rb, gr, gg, gb, br, bg, bb);
	*/

	ptr = in_bytes;
	src = in_bytes;
	if (bpp > 8)
	{
		 srcWord = (WORD *) in_bytes;
		// convert 16bit bayer to 8bit bayer

		 if (RGBGainEna)
		 {
			switch (pixel_order)
			{
			case 0: // GRBG
				for (i=0; i<height; i+=2)
				{
					for (j=0; j<width; j+=2) // odd line
					{
						tmp = (*srcWord++);
						if (tmp < maxValue)
						{
							itmp = tmp + g_offset;
							itmp = itmp * g_gain / 256;
							if (itmp < 0) itmp = 0;
							else if (itmp > maxValue) itmp = maxValue;
							*ptr++ = (BYTE) (itmp >> shift);
						}
						else
							*ptr++ = 0xff; 

						tmp = (*srcWord++);
						if (tmp < maxValue)
						{
							itmp = tmp + r_offset;
							itmp = itmp * r_gain / 256;
							if (itmp < 0) itmp = 0;
							else if (itmp > maxValue) itmp = maxValue;
							*ptr++ = (BYTE) (itmp >> shift);
						}
						else
							*ptr++ = 0xff; 
					}
					for (j=0; j<width; j+=2) // even line
					{
						tmp = (*srcWord++);
						if (tmp < maxValue)
						{
							itmp = tmp + b_offset;
							itmp = itmp * b_gain / 256;
							if (itmp < 0) itmp = 0;
							else if (itmp > maxValue) itmp = maxValue;
							*ptr++ = (BYTE) (itmp >> shift);
						}
						else
							*ptr++ = 0xff; 

						tmp = (*srcWord++);
						if (tmp < maxValue)
						{
							itmp = tmp + g_offset;
							itmp = itmp * g_gain / 256;
							if (itmp < 0) itmp = 0;
							else if (itmp > maxValue) itmp = maxValue;
							*ptr++ = (BYTE) (itmp >> shift);
						}
						else
							*ptr++ = 0xff; 
					}
				}
				break;
			case 1: //GBRG
				for (i=0; i<height; i+=2)
				{
					for (j=0; j<width; j+=2) // odd line
					{
						tmp = (*srcWord++);
						if (tmp < maxValue)
						{
							itmp = tmp + g_offset;
							itmp = itmp * g_gain / 256;
							if (itmp < 0) itmp = 0;
							else if (itmp > maxValue) itmp = maxValue;
							*ptr++ = (BYTE) (itmp >> shift);
						}
						else
							*ptr++ = 0xff; 

						tmp = (*srcWord++);
						if (tmp < maxValue)
						{
							itmp = tmp + b_offset;
							itmp = itmp * b_gain / 256;
							if (itmp < 0) itmp = 0;
							else if (itmp > maxValue) itmp = maxValue;
							*ptr++ = (BYTE) (itmp >> shift);
						}
						else
							*ptr++ = 0xff; 
					}
					for (j=0; j<width; j+=2) // even line
					{
						tmp = (*srcWord++);
						if (tmp < maxValue)
						{
							itmp = tmp + r_offset;
							itmp = itmp * r_gain / 256;
							if (itmp < 0) itmp = 0;
							else if (itmp > maxValue) itmp = maxValue;
							*ptr++ = (BYTE) (itmp >> shift);
						}
						else
							*ptr++ = 0xff; 

						tmp = (*srcWord++);
						if (tmp < maxValue)
						{
							itmp = tmp + g_offset;
							itmp = itmp * g_gain / 256;
							if (itmp < 0) itmp = 0;
							else if (itmp > maxValue) itmp = maxValue;
							*ptr++ = (BYTE) (itmp >> shift);
						}
						else
							*ptr++ = 0xff; 
					}
				}
				break;
			case 3: //BGGR
				for (i=0; i<height; i+=2)
				{
					for (j=0; j<width; j+=2) // odd line
					{
						tmp = (*srcWord++);
						if (tmp < maxValue)
						{
							itmp = tmp + b_offset;
							itmp = itmp * b_gain / 256;
							if (itmp < 0) itmp = 0;
							else if (itmp > maxValue) itmp = maxValue;
							*ptr++ = (BYTE) (itmp >> shift);
						}
						else
							*ptr++ = 0xff; 

						tmp = (*srcWord++);
						if (tmp < maxValue)
						{
							itmp = tmp + g_offset;
							itmp = itmp * g_gain / 256;
							if (itmp < 0) itmp = 0;
							else if (itmp > maxValue) itmp = maxValue;
							*ptr++ = (BYTE) (itmp >> shift);
						}
						else
							*ptr++ = 0xff; 
					}
					for (j=0; j<width; j+=2) // even line
					{
						tmp = (*srcWord++);
						if (tmp < maxValue)
						{
							itmp = tmp + g_offset;
							itmp = itmp * g_gain / 256;
							if (itmp < 0) itmp = 0;
							else if (itmp > maxValue) itmp = maxValue;
							*ptr++ = (BYTE) (itmp >> shift);
						}
						else
							*ptr++ = 0xff; 

						tmp = (*srcWord++);
						if (tmp < maxValue)
						{
							itmp = tmp + r_offset;
							itmp = itmp * r_gain / 256;
							if (itmp < 0) itmp = 0;
							else if (itmp > maxValue) itmp = maxValue;
							*ptr++ = (BYTE) (itmp >> shift);
						}
						else
							*ptr++ = 0xff; 
					}
				}
				break;
			case 2: //RGGB
				for (i=0; i<height; i+=2)
				{
					for (j=0; j<width; j+=2) // odd line
					{
						tmp = (*srcWord++);
						if (tmp < maxValue)
						{
							itmp = tmp + r_offset;
							itmp = itmp * r_gain / 256;
							if (itmp < 0) itmp = 0;
							else if (itmp > maxValue) itmp = maxValue;
							*ptr++ = (BYTE) (itmp >> shift);
						}
						else
							*ptr++ = 0xff; 
						
						tmp = (*srcWord++);
						if (tmp < maxValue)
						{
							itmp = tmp + g_offset;
							itmp = itmp * g_gain / 256;
							if (itmp < 0) itmp = 0;
							else if (itmp > maxValue) itmp = maxValue;
							*ptr++ = (BYTE) (itmp >> shift);
						}
						else
							*ptr++ = 0xff; 
						
					}
					for (j=0; j<width; j+=2) // even line
					{
						tmp = (*srcWord++);
						if (tmp < maxValue)
						{
							itmp = tmp + g_offset;
							itmp = itmp * g_gain / 256;
							if (itmp < 0) itmp = 0;
							else if (itmp > maxValue) itmp = maxValue;
							*ptr++ = (BYTE) (itmp >> shift);
						}
						else
							*ptr++ = 0xff; 
						
						tmp = (*srcWord++);
						if (tmp < maxValue)
						{
							itmp = tmp + b_offset;
							itmp = itmp * b_gain / 256;
							if (itmp < 0) itmp = 0;
							else if (itmp > maxValue) itmp = maxValue;
							*ptr++ = (BYTE) (itmp >> shift);
						}
						else
							*ptr++ = 0xff; 
					}
				}
				break;
			}
		 }
		 else  // direct shift data
		 {
			for (i=0; i<height; i++)
				for (j=0; j<width; j++)
				{
					tmp = (*srcWord++) >> shift;
					*ptr++ = (BYTE) tmp;
				}
		 }
		 //printf("Shift ctr: %d\n", ctr);
	}

	src = in_bytes;
	bayer_to_rgb24(src, out_bytes, width, height, pixel_order);

	if (RGB2RGBEna)
	{
		src = out_bytes;
		rgb2rgb(src, src, width, height, bpp, rr, rg, rb, gr, gg, gb, br, bg, bb);
	}

	if (GammaEna)
	{
		src = out_bytes;
		gammaCorrection(src, src, width*3, height, 8, gamma);
	}

	return 0;
}

__declspec(dllexport) int raw_to_bmp_mono(BYTE* in_bytes, BYTE* out_bytes, int width, int height, int bpp,
									 bool GammaEna, double gamma)
{
	int i,j;
	int shift = bpp - 8;
	unsigned short tmp;
	BYTE* dst = out_bytes;
	BYTE* src = in_bytes;
	WORD* srcWord = (WORD *) in_bytes;

	if (bpp > 8)
	{
		 srcWord = (WORD *) in_bytes;
		// convert 16bit bayer to 8bit bayer
		for (i=0; i<height; i++)
			for (j=0; j<width; j++)
			{
				tmp = (*srcWord++) >> shift;
				*dst++ = (BYTE) tmp;
				*dst++ = (BYTE) tmp;
				*dst++ = (BYTE) tmp;
			}
	}
	else
	{
		for (i=0; i<height; i++)
			for (j=0; j<width; j++)
			{
				tmp = (*src++);
				*dst++ = (BYTE) tmp;
				*dst++ = (BYTE) tmp;
				*dst++ = (BYTE) tmp;
			}

	}

	if (GammaEna)
	{
		src = out_bytes;
		gammaCorrection(src, src, width * 3, height, 8, gamma); // bmp has 3 color elements, so * 3
	}
	return 0;
}

__declspec(dllexport) double calc_mean(BYTE* in_bytes, int width, int height, int bpp, int startX, int startY, int iSize)
{
	WORD *srcWord;
	BYTE *srcByte;
	int i, j;
	double dSum=0.0;

	if (bpp > 8)
	{
		srcWord = (WORD *) in_bytes;
		srcWord += startY * width + startX;
		for (i=0; i<iSize; i++)
		{
			for (j=0; j<iSize; j++)
			{
				dSum += (double) (*srcWord++);
			}
			srcWord += width;
		}
	}
	else
	{
		srcByte = (BYTE *) in_bytes;
		srcByte += startY * width + startX;
		for (i=0; i<iSize; i++)
		{
			for (j=0; j<iSize; j++)
			{
				dSum += (double) (*srcByte++);
			}
			srcByte += width;
		}
	}

	return dSum / iSize / iSize;
}

enum MY_COLOR {
	MY_RED = 0,
	MY_GREEN,
	MY_BLUE,
	MY_YELLOW
};

__declspec(dllexport) int yuv422_to_bmp_mono(BYTE* in_bytes, BYTE* out_bytes, int width, int height)
{
	int i,j;
	unsigned short tmp;
	BYTE* dst = out_bytes;
	BYTE* src = in_bytes;

	{
		for (i=0; i<height; i++)
			for (j=0; j<width; j++)
			{
				tmp = (*src++);
				*src++;
				*dst++ = (BYTE) tmp;
				*dst++ = (BYTE) tmp;
				*dst++ = (BYTE) tmp;
			}

	}
	return 0;
}

static int convert_yuv_to_rgb_pixel(int y, int u, int v)
{
        int pixel32 = 0;
		BYTE *pixel = (BYTE *)&pixel32;
        int r, g, b;
        b = (int)(y + (1.370705 * (v-128)));
        g = (int)(y - (0.698001 * (v-128)) - (0.337633 * (u-128)));
        r = (int)(y + (1.732446 * (u-128)));
        if(r > 255) r = 255;
        if(g > 255) g = 255;
        if(b > 255) b = 255;
        if(r < 0) r = 0;
        if(g < 0) g = 0;
        if(b < 0) b = 0;
        pixel[0] = r ;
        pixel[1] = g ;
        pixel[2] = b ;
        return pixel32;
}

__declspec(dllexport) int convert_yuv_to_rgb_buffer(BYTE *yuv, BYTE *rgb, int width, int height, BOOL mark_en, int Center_picturebox_offset,int left_top,int left_bottom,int right_top,int right_bottom )
{
       int in, out = 0;
       int pixel_16;
       BYTE pixel_24[3];
       int pixel32;

        int y0, u, y1, v;

        for(in = 0; in < width * height * 2; in += 4)
        {
                pixel_16 =
                                yuv[in + 3] << 24 |
                                yuv[in + 2] << 16 |
                                yuv[in + 1] <<  8 |
                                yuv[in + 0];
                y0 = (pixel_16 & 0x000000ff);
                u  = (pixel_16 & 0x0000ff00) >>  8;
                y1 = (pixel_16 & 0x00ff0000) >> 16;
                v  = (pixel_16 & 0xff000000) >> 24;
                pixel32 = convert_yuv_to_rgb_pixel(y0, u, v);
                pixel_24[0] = (pixel32 & 0x000000ff);
                pixel_24[1] = (pixel32 & 0x0000ff00) >> 8;
                pixel_24[2] = (pixel32 & 0x00ff0000) >> 16;
                rgb[out++] = pixel_24[0];
                rgb[out++] = pixel_24[1];
                rgb[out++] = pixel_24[2];
                pixel32 = convert_yuv_to_rgb_pixel(y1, u, v);
                pixel_24[0] = (pixel32 & 0x000000ff);
                pixel_24[1] = (pixel32 & 0x0000ff00) >> 8;
                pixel_24[2] = (pixel32 & 0x00ff0000) >> 16;
                rgb[out++] = pixel_24[0];
                rgb[out++] = pixel_24[1];
                rgb[out++] = pixel_24[2];
        }

		if (mark_en)
		{
			//mark the picturebox,modify the rgb data 10*10 pixels
			for(int z = 0; z < 10 ; z++)
			{ 
				  for(int j = 0;j < 30; j = j+3)
				  {
						rgb[3*Center_picturebox_offset+j] = 0;
						rgb[3*Center_picturebox_offset+j+1] = 0;
						rgb[3*Center_picturebox_offset+j+2] = 255;
				  }
				  Center_picturebox_offset = Center_picturebox_offset +width;//picturebox_offset is for pixel offset
			}

					//mark the picturebox,modify the rgb data 10*10 pixels
			for(int z = 0; z < 10 ; z++)
			{ 
				  for(int j = 0;j < 30; j = j+3)
				  {
						rgb[3*left_top+j] = 0;
						rgb[3*left_top+j+1] = 0;
						rgb[3*left_top+j+2] = 255;
				  }
				  left_top = left_top +width;//picturebox_offset is for pixel offset
			}
					//mark the picturebox,modify the rgb data 10*10 pixels
			for(int z = 0; z < 10 ; z++)
			{ 
				  for(int j = 0;j < 30; j = j+3)
				  {
						rgb[3*left_bottom+j] = 0;
						rgb[3*left_bottom+j+1] = 0;
						rgb[3*left_bottom+j+2] = 255;
				  }
				  left_bottom = left_bottom +width;//picturebox_offset is for pixel offset
			}
					//mark the picturebox,modify the rgb data 10*10 pixels
			for(int z = 0; z < 10 ; z++)
			{ 
				  for(int j = 0;j < 30; j = j+3)
				  {
						rgb[3*right_top+j] = 0;
						rgb[3*right_top+j+1] = 0;
						rgb[3*right_top+j+2] = 255;
				  }
				  right_top = right_top +width;//picturebox_offset is for pixel offset
			}
			//mark the picturebox,modify the rgb data 10*10 pixels
			for(int z = 0; z < 10 ; z++)
			{ 
				  for(int j = 0;j < 30; j = j+3)
				  {
						rgb[3*right_bottom+j] = 0;
						rgb[3*right_bottom+j+1] = 0;
						rgb[3*right_bottom+j+2] = 255;
				  }
				  right_bottom = right_bottom +width;//picturebox_offset is for pixel offset
			}
		}
        return 0;

}

// remove all Cb & Cr, only leave y in the data
__declspec(dllexport) int yuv422_to_y(BYTE* in_bytes, BYTE* out_bytes, int width, int height)
{
	int i,j;
	BYTE* dst = out_bytes;
	BYTE* src = in_bytes;

	{
		for (i=0; i<height; i++)
			for (j=0; j<width; j++)
			{
				*dst++ = (*src++);
				*src++;
			}

	}
	return 0;
}

__declspec(dllexport) int dual_raw8_to_y(BYTE* in_bytes, int width, int height)
{
	int i,j;
	BYTE* line_ptr = in_bytes;
	BYTE* left = in_bytes;
	BYTE* right = in_bytes + width/2;

	BYTE *one_line = new BYTE[width];

	for (i=0; i<height; i++)
	{
		memcpy(one_line, line_ptr, width);
		for (j=0; j<width/2; j++)
		{
			*left++ = one_line[2*j];
			*right++ = one_line[2*j+1];
		}
		left += width/2;
		right += width/2;
		line_ptr += width;
	}

	delete(one_line);

	return 0;
}

__declspec(dllexport) int mono_to_y(BYTE* in_bytes, BYTE* out_bytes, int width, int height, int iBits)
{
	int i,j;
	BYTE* dst = out_bytes;
	short* src = (short *)in_bytes;
	int shift_bit = (iBits - 8);

	if (shift_bit <= 0)
		return 0;

	{
		for (i=0; i<height; i++)
			for (j=0; j<width; j++)
			{
				*dst++ = (*src++ >> shift_bit);
			}

	}
	return 0;
}

__declspec(dllexport) int bayer_to_y(BYTE* in_bytes, BYTE* out_bytes, int width, int height, int iBits)
{
	int i,j;
	BYTE* dst = out_bytes;
	short* src = (short *)in_bytes;
	int shift_bit = (iBits - 8);
	double grTotal=0, gbTotal=0, rTotal=0, bTotal=0;
	double grAvg, gbAvg, rAvg, bAvg;
	double grGain, gbGain, rGain=0, bGain=0;
	short pixel, maxValue;

	if (shift_bit <= 0)
		return 0;

	maxValue = 1 << iBits;
	{
		for (i=0; i<height/2; i++)
		{
			for (j=0; j<width/2; j++)
			{
				grTotal += *src++;
				rTotal += *src++;
			}
			for (j=0; j<width/2; j++)
			{
				bTotal += *src++;
				gbTotal += *src++;
			}
		}

		grAvg = grTotal / ((height / 2) * (width/2)) ;
		gbAvg = gbTotal / ((height / 2) * (width/2)) ;
		rAvg = rTotal / ((height / 2) * (width/2)) ;
		bAvg = bTotal / ((height / 2) * (width/2)) ;

		grGain = ( grAvg + gbAvg + rAvg + bAvg) / grAvg / 4;
		gbGain = ( grAvg + gbAvg + rAvg + bAvg) / gbAvg / 4;
		rGain = ( grAvg + gbAvg + rAvg + bAvg) / rAvg / 4;
		bGain = ( grAvg + gbAvg + rAvg + bAvg) / bAvg / 4;

		src = (short *)in_bytes;

		for (i=0; i<height/2; i++)
		{
			for (j=0; j<width/2; j++)
			{
				pixel = *src++;
				pixel = (short)(pixel * grGain);
				if (pixel > maxValue) pixel = maxValue;
				*dst = pixel >> shift_bit;

				pixel = *src++;
				pixel = (short)(pixel * rGain);
				if (pixel > maxValue) pixel = maxValue;
				*dst = pixel >> shift_bit;
			}
			for (j=0; j<width/2; j++)
			{
				pixel = *src++;
				pixel = (short)(pixel * bGain);
				if (pixel > maxValue) pixel = maxValue;
				*dst = pixel >> shift_bit;

				pixel = *src++;
				pixel = (short)(pixel * gbGain);
				if (pixel > maxValue) pixel = maxValue;
				*dst = pixel >> shift_bit;
			}
		}

	}
	return 0;
}

__declspec(dllexport) int y_SobleOperator(BYTE *in_buf, int iWidth, int iHeight, int startX, int startY, int iSize)
        {
	    int i,j;
            int HFFactor = 0;
            int w = iSize;
            int h = iSize;
            int x, y;
            unsigned char x1, x2, x3, x4, x5, x6;
            unsigned char y1, y2, y3, y4, y5, y6;

            unsigned char *inImage = in_buf + startY * iWidth + startX;

            for (i = 1; i < w - 1; i++)
                for (j = 1; j < h - 1; j++)
                {
                    x1 = *(inImage + (i - 1)*iWidth + j - 1);
                    x2 = *(inImage + (i - 1)*iWidth + j + 1);
                    x3 = *(inImage + (i)*iWidth + j - 1);
                    x4 = *(inImage + (i)*iWidth + j + 1);
                    x5 = *(inImage + (i + 1)*iWidth + j - 1);
                    x6 = *(inImage + (i + 1)*iWidth + j + 1);


                    y1 = *(inImage + (i - 1)*iWidth + j - 1);
                    y2 = *(inImage + (i - 1)*iWidth + j);
                    y3 = *(inImage + (i - 1)*iWidth + j + 1);
                    y4 = *(inImage + (i + 1)*iWidth + j - 1);
                    y5 = *(inImage + (i + 1)*iWidth + j);
                    y6 = *(inImage + (i + 1)*iWidth + j + 1);

                    x = -x1 + x2 - 2 * x3 + 2 * x4 - x5 + x6;
                    y = -y1 - 2 * y2 - y3 + y4 + 2 * y5 + y6;

                    HFFactor += x * x + y * y;
                }
            return HFFactor/iSize/iSize;
}

__declspec(dllexport) int y_CalcMean(BYTE *in_buf, int iWidth, int iHeight, int startX, int startY, int iSize)
        {
            int i, j;
            int dTotal = 0, dMean = 0;

            int startPos = startY * iWidth + startX;

            {
                for (i = 0; i < iSize; i++)
                {
                    for (j = 0; j < iSize; j++)
                    {
                        dTotal += in_buf[startPos + (i * iWidth + j)];
                    }
                }
            }

            dMean = dTotal / iSize / iSize;

            return (dMean);
        }

__declspec(dllexport) int y_MTF(BYTE *in_buf, int iWidth, int iHeight, int startX, int startY, int iSize)
        {
            int i, j;
            BYTE dPixel;
			int  dMax=0, dMin=0;
			BYTE maxPixels[10], minPixels[10];

            int startPos = startY * iWidth + startX;

			for (i = 0; i < 10; i++)
			{
				maxPixels[i] = 0;
				minPixels[i] = 255; // 8 bits, max 255
			}

            
            for (i = 0; i < iSize; i++)
            {
                for (j = 0; j < iSize; j++)
                {
                    dPixel = in_buf[startPos + (i * iWidth + j)];
					
					for (int k = 0; k < 10; k++)
					{
						if (dPixel > maxPixels[k])
						{
							maxPixels[k] = dPixel;
							break;
						}

						if (dPixel < minPixels[k])
						{
							minPixels[k] = dPixel;
							break;
						}

					}
                }
			}
            
			for (i = 5; i < 10; i++)
			{
				dMax += maxPixels[i];
				dMin += minPixels[i];
			}

			if (dMax+dMin != 0)
				return ((dMax-dMin)*1000/(dMax+dMin));
			else
				return 1000;
	}

__declspec(dllexport) void swapByte(BYTE *in_buf, int iWidth, int iHeight, int dataWidth)
	{
		BYTE tmp;
		int bPP = dataWidth > 8 ? 2 : 1; // bytes per pixel
		int iSize = iWidth * iHeight * bPP;

		BYTE *src = in_buf;
		BYTE *src_1 = in_buf+1;

		while (iSize > 0)
		{
			tmp = *src;
			*src = *src_1;
			*src_1 = tmp;

			src += 2;
			src_1 += 2;

			iSize -= 2;
		}

	}

// crop 5 blocks of the original image to form a 1280x720 image
__declspec(dllexport) void reframeTo720p(BYTE *out_buf, BYTE *in_buf, int iWidth, int iHeight, int dataWidth)
        {
            int i;
			int winWidth = 1280/3;
			int winHeight = 720/3;
			int bPP = dataWidth > 8 ? 2 : 1; // bytes per pixel

			BYTE *in_tmp, *out_tmp;
			
			//ZeroMemory(out_tmp, 1280*2*720); wont this crash?

			// top window
			out_tmp = out_buf;
			in_tmp = in_buf + (( ( iWidth - winWidth) / 2) & 0xFFFE) * bPP;
			for (i=0; i<winHeight; i++)
			{
				memcpy(out_tmp + i*1280*bPP, in_tmp + i*iWidth*bPP, winWidth*bPP);
			}

			// left window
			out_tmp = out_buf + (720/3) * 2 * 1280 * bPP;
			in_tmp = in_buf + ( iHeight - 720/3) / 2 * iWidth * bPP ;
			for (i=0; i<winHeight; i++)
			{
				memcpy(out_tmp + i*1280*bPP, in_tmp + i*iWidth*bPP, winWidth*bPP);
			}

			// center window
			out_tmp = out_buf + (720/3) * 1280 * bPP + 1280/3*bPP;
			in_tmp = in_buf + ((( iHeight - 720/3) / 2) & 0xFFFE) * iWidth * bPP 
							+ (((iWidth - 1280/3)/2) & 0xFFFE) *bPP;
			for (i=0; i<winHeight; i++)
			{
				memcpy(out_tmp + i*1280*bPP, in_tmp + i*iWidth*bPP, winWidth*bPP);
			}

			// right window
			out_tmp = out_buf + 1280/3 * 2 * bPP;
			in_tmp = in_buf + ((( iHeight - 720/3) / 2) & 0xFFFE) * iWidth * bPP 
							+ (((iWidth - 1280/3)) & 0xFFFE)*bPP;
			for (i=0; i<winHeight; i++)
			{
				memcpy(out_tmp + i*1280*bPP, in_tmp + i*iWidth*bPP, winWidth*bPP);
			}

			// bottom window
			out_tmp = out_buf + (720/3) * 2 * 1280 * bPP + 1280/3*2*bPP;
			in_tmp = in_buf + (( iHeight - 720/3) & 0xFFFE) *  iWidth * bPP 
								+ (((iWidth - 1280/3)/2)& 0xFFFE) *bPP;
			for (i=0; i<winHeight; i++)
			{
				memcpy(out_tmp + i*1280*bPP, in_tmp + i*iWidth*bPP, winWidth*bPP);
			}

        }

// crop 5 blocks of the original image to form a 1280x720 image
// top-left, top-right, center, bottom-left, bottom-right
__declspec(dllexport) void reframeTo720p_4corners(BYTE *out_buf, BYTE *in_buf, int iWidth, int iHeight, int dataWidth)
        {
            int i;
			int winWidth = 1280/3;
			int winHeight = 720/3;
			int bPP = dataWidth > 8 ? 2 : 1; // bytes per pixel

			BYTE *in_tmp, *out_tmp;
			
			//ZeroMemory(out_tmp, 1280*2*720);   wont this crash??

			// top left window
			out_tmp = out_buf;
			in_tmp = in_buf;
			for (i=0; i<winHeight; i++)
			{
				memcpy(out_tmp + i*1280*bPP, in_tmp + i*iWidth*bPP, winWidth*bPP);
			}

			// bottom left window
			out_tmp = out_buf + winHeight * 2 * 1280 * bPP;
			in_tmp = in_buf + ( iHeight - winHeight) * iWidth * bPP ;
			for (i=0; i<winHeight; i++)
			{
				memcpy(out_tmp + i*1280*bPP, in_tmp + i*iWidth*bPP, winWidth*bPP);
			}

			// center window
			out_tmp = out_buf + winHeight * 1280 * bPP + winWidth*bPP;
			in_tmp = in_buf + ((( iHeight - winHeight) / 2) & 0xFFFE) * iWidth * bPP 
							+ (((iWidth - winWidth)/2) & 0xFFFE) *bPP;
			for (i=0; i<winHeight; i++)
			{
				memcpy(out_tmp + i*1280*bPP, in_tmp + i*iWidth*bPP, winWidth*bPP);
			}

			// top right window
			out_tmp = out_buf + winWidth * 2 * bPP;
			in_tmp = in_buf + (((iWidth - winWidth)) & 0xFFFE)*bPP;
			for (i=0; i<winHeight; i++)
			{
				memcpy(out_tmp + i*1280*bPP, in_tmp + i*iWidth*bPP, winWidth*bPP);
			}

			// bottom right window
			out_tmp = out_buf + winHeight * 2 * 1280 * bPP + winWidth*2*bPP;
			in_tmp = in_buf + (( iHeight - winHeight) & 0xFFFE) *  iWidth * bPP 
								+ (((iWidth - winWidth))& 0xFFFE) *bPP;
			for (i=0; i<winHeight; i++)
			{
				memcpy(out_tmp + i*1280*bPP, in_tmp + i*iWidth*bPP, winWidth*bPP);
			}

        }

}