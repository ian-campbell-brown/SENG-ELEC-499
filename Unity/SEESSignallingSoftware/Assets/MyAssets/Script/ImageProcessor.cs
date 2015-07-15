using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ImageProcessor : MonoBehaviour
{
    public static ImageProcessor Instance;

    // In Millimeters
    public int m_depthRange = 4000;

    public SharedTexture2D m_filteredDepthImage;

    [HideInInspector]
    public short[] m_rawDepthData;

    [HideInInspector]
    public float[] m_rawDepthDataNormalized;

    public float m_nearest = 0.0f;
    public float m_furthest = 1.0f;

    // Use this for initialization
    void Start()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Two instances of signleton exist in this scene.", this);
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        int nPixels = m_filteredDepthImage.m_width * m_filteredDepthImage.m_height;
        m_rawDepthData = new short[nPixels];
        m_rawDepthDataNormalized = new float[nPixels];

        m_filteredDepthImage.name = "FilteredDepthImage";
        m_filteredDepthImage.m_source.wrapMode = TextureWrapMode.Clamp;
    }

    // Update is called once per frame
    void Update()
    {
        DepthImageManager dim = DepthImageManager.Instance;
        if (dim.m_mode == DepthImageManager.Mode.Kinect)
        {
            DepthWrapper dw = KinectSingleton.Instance.DepthWrapper;
            if (!dw.pollDepth())
                return;

            Array.Copy(dim.m_rawData, m_rawDepthData, m_rawDepthData.Length);
        }
        else if (dim.m_mode == DepthImageManager.Mode.RealSense)
        {
            for (int y = 0; y < dim.m_height / 2; ++y)
                for (int x = 0; x < dim.m_width / 2; ++x)
                    m_rawDepthData[x + y * dim.m_width / 2] = dim.m_rawData[2*x + 2*y * dim.m_width / 2];
        }
        m_depthRange = dim.m_depthRange;


        Color32[] pixels = new Color32[m_rawDepthData.Length];

        ExtractDepthPixels(pixels);
        //HighlightEdges(pixels);

        //ExtractRegions(pixels);
        //MarkRegions(pixels);

        //CullImageDepth(pixels);

        m_filteredDepthImage.m_source.SetPixels32(pixels);
        m_filteredDepthImage.m_source.Apply(false);
    }

    private void ExtractDepthPixels(Color32[] pixels)
    {
        m_furthest = 0.0f;
        m_nearest = 1.0f;

		for (int i = 0; i < m_rawDepthData.Length; i++)
		{
            short rawDepth = m_rawDepthData[i];
            float normalizedDepth = (float)rawDepth / m_depthRange;
            byte intensity = (byte)(normalizedDepth * 0xFF);

            // simplistic filtering for unreliable data points
            //if (rawDepth == 0)
            //    intensity = (byte)((float)pixels[i].r / 1.2f);

            pixels[i] = new Color32(intensity, intensity, intensity, 255);
            m_rawDepthDataNormalized[i] = normalizedDepth;
            m_furthest = Math.Max(m_furthest, normalizedDepth);

            if (normalizedDepth != 0.0f)
                m_nearest = Math.Min(m_nearest, normalizedDepth);
		}
    }

    private void HighlightEdges(Color32[] pixels)
    {
        int wsize = 1;
        int wwidth = 2 * wsize + 1;
        int threshold = 300;
        int width = m_filteredDepthImage.m_width;
        int height = m_filteredDepthImage.m_height;

        short[] window = new short[wwidth * wwidth];
        int iMid = window.Length / 2;

        for (int y1 = 0; y1 < height - wwidth; ++y1)
        {
            int y1Stride = y1 * width;
            for (int x1 = 0; x1 < width - wwidth; ++x1)
            {
                int wi = 0;
                for (int y2 = 0; y2 < wwidth; ++y2)
                {
                    int y2Stride = y2 * width;
                    for (int x2 = 0; x2 < wwidth; ++x2)
                    {
                        int i = y1Stride + y2Stride + x1 + x2;

                        window[wi++] = m_rawDepthData[i];
                    }
                }

                int refDepth = window[iMid];
                int value = -1;
                for (int i = 0; i < window.Length; ++i)
                {
                    int depth = window[i];
                    int slope = Math.Abs(refDepth - depth);

                    if (slope < threshold || depth == 0)
                        value++;
                }

                float fvalue = (float)value / (window.Length - 1);
                byte intensity = (byte)(fvalue > 0.8f ? 0 : 0xFF);
                //byte intensity = (byte)((1.0f - fvalue) * 255);

                if (intensity > 0)
                {
                    int pi = y1Stride + x1;

                    pixels[pi] = new Color32(0, intensity, 0, 255);
                }
            }
        }
    }

    private void ExtractRegions(Color32[] pixels)
    {
        int wsize = 4;
        int wwidth = 2 * wsize + 1;
        int threshold = 200;
        int width = m_filteredDepthImage.m_width;
        int height = m_filteredDepthImage.m_height;

        short[] window = new short[wwidth * wwidth];
        int iMid = window.Length / 2;

        int y1 = (height - wwidth) / 2;
        int y1Stride = y1 * width;
        for (int x1 = 0; x1 < width - wwidth; ++x1)
        {
            int wi = 0;
            for (int y2 = 0; y2 < wwidth; ++y2)
            {
                int y2Stride = y2 * width;
                for (int x2 = 0; x2 < wwidth; ++x2)
                {
                    int i = y1Stride + y2Stride + x1 + x2;

                    window[wi++] = m_rawDepthData[i];
                }
            }

            int refDepth = window[iMid];
            int value = -1;
            for (int i = 0; i < window.Length; ++i)
            {
                int depth = window[i];
                int slope = Math.Abs(refDepth - depth);

                if (slope < threshold || depth == 0)
                    value++;
            }

            float fvalue = (float)value / (window.Length - 1);
            byte intensity = (byte)(fvalue > 0.8f ? 0 : 0xFF);

            int pi = y1Stride + (x1 + wsize);

            pixels[pi] = new Color32(intensity, intensity, intensity, 255);
        }
    }

    private void MarkRegions(Color32[] pixels)
    {
        int wsize = 4;
        int wwidth = 2 * wsize + 1;
        int width = m_filteredDepthImage.m_width;
        int height = m_filteredDepthImage.m_height;

        int xStart = 0;
        bool inBlack = false;

        int y1 = (height - wwidth) / 2;
        int y1Stride = y1 * width;
        for (int x1 = 0; x1 < width; ++x1)
        {
            int i = y1Stride + x1;
            bool isBlack = pixels[i].r == 0;
            if (inBlack && (!isBlack || x1 == width - 1))
            {
                int regionLength = x1 - xStart;
                int xmark = xStart + regionLength / 2;

                if (regionLength > 0)
                {
                    for (int z = -2; z < 2; ++z)
                    {
                        Color32 c = new Color32(0, 0, 255, 255);
                        pixels[y1Stride - 2 * width + xmark + z] = c;
                        pixels[y1Stride - width + xmark + z] = c;
                        pixels[y1Stride + xmark + z] = c;
                        pixels[y1Stride + width + xmark + z] = c;
                        pixels[y1Stride + 2 * width + xmark + z] = c;
                    }

                    inBlack = false;
                }
            }
            else if (!inBlack && isBlack)
            {
                xStart = x1;

                inBlack = true;
            }
        }
    }

    public void CullImageDepth(Color32[] pixels)
    {
        float range = m_furthest;
        float culledRange = range * 0.75f;
        byte threshold = 128;

        for (int i = 0; i < m_rawDepthData.Length; i++)
        {
            if (pixels[i].r > threshold)
                pixels[i] = new Color32(0, 0, 0, 255);
        }
    }
}
