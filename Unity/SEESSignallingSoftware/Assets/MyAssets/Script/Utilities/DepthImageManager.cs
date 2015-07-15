using UnityEngine;
using System.Collections;

using Kinect;
using RSUnityToolkit;

public class DepthImageManager : MonoBehaviour
{
    public static DepthImageManager Instance;

    public int m_width;
    public int m_height;

    public int m_depthRange = 1;

    [Range(0.0f, 90.0f)]
    public int m_fov = 45;

    [HideInInspector]
    public short[] m_rawData;

    public Mode m_mode = Mode.AutoDetect;

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

        SenseToolkitManager.Instance.SetSenseOption(SenseOption.SenseOptionID.VideoDepthStream);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_mode == Mode.AutoDetect)
        {
            if (KinectSensor.Instance != null)
            {
                m_width = 420;
                m_height = 320;
                m_depthRange = 4000;
                m_fov = 45;

                m_mode = Mode.Kinect;
            }
            else if (SenseToolkitManager.Instance.Initialized && SenseToolkitManager.Instance.ImageDepthOutput != null)
            {
                PXCMImage depthImage = SenseToolkitManager.Instance.ImageDepthOutput;
                m_width = depthImage.info.width;
                m_height = depthImage.info.height;
                m_depthRange = 1000;
                m_fov = 45;

                m_mode = Mode.RealSense;
            }
        }
        
        DepthWrapper kinectDepth = KinectSingleton.Instance.DepthWrapper;
        PXCMImage senseDepth = SenseToolkitManager.Instance.ImageDepthOutput;

        if (m_mode == Mode.Kinect)
        {
            if (kinectDepth.pollDepth())
            {
                m_rawData = kinectDepth.depthImg;
            }
        }
        else if (m_mode == Mode.RealSense)
        {
            PXCMImage.ImageData data;
            pxcmStatus stat = senseDepth.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.PixelFormat.PIXEL_FORMAT_DEPTH, out data);
            if (stat >= pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                m_rawData = data.ToShortArray(0, m_width * m_height);

                senseDepth.ReleaseAccess(data);
            }
        }
    }

    public enum Mode
    {
        AutoDetect,
        RealSense,
        Kinect
    }
}
