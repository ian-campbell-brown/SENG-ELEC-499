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
    public short[] m_rawDepthData = null;

    [HideInInspector]
    public float[] m_rawDepthDataNormalized = null;

    // Use this for initialization
    void Start()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Two instances of signleton exist in this scene.", this);
            Destroy(this.gameObject);
        }
        Instance = this;

        m_rawDepthData = new short[0];

        m_filteredDepthImage.name = "FilteredDepthImage";
    }

    // Update is called once per frame
    void Update()
    {
        DepthWrapper dw = KinectSingleton.Instance.DepthWrapper;
        if (!dw.pollDepth())
            return;

        m_rawDepthData = dw.depthImg.Reverse().ToArray();
        Array.Resize(ref m_rawDepthDataNormalized, m_rawDepthData.Length);

        ProcessDepthData();
    }

    private void ProcessDepthData()
    {
        Color32[] pixels = m_filteredDepthImage.m_source.GetPixels32();
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
		}

        m_filteredDepthImage.m_source.SetPixels32(pixels);
        m_filteredDepthImage.m_source.Apply(false);
    }
}
