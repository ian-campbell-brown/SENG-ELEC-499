using UnityEngine;
using System.Collections;
using System.Linq;

public class VolumeDetector : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (ImageProcessor.Instance.m_rawDepthData.Length == 0)
            return; // Image data isn't ready yet.

        SignallingVolume[] volumes = GetSignallingVolumes();
        foreach (SignallingVolume volume in volumes)
        {
            UpdateVolume(volume);
        }
    }


    private SignallingVolume[] GetSignallingVolumes()
    {
        return FindObjectsOfType(typeof(SignallingVolume)).Cast<SignallingVolume>().ToArray();
    }

    void UpdateVolume(SignallingVolume volume)
    {
        float[] rawDepthData = ImageProcessor.Instance.m_rawDepthDataNormalized;

        Bounds bounds = volume.GetDepthImageBounds();

        int xmin = Mathf.RoundToInt(bounds.min.x);
        int xmax = Mathf.RoundToInt(bounds.max.x);
        int ymin = Mathf.RoundToInt(bounds.min.y);
        int ymax = Mathf.RoundToInt(bounds.max.y);
        float zmin = bounds.min.z / 600;
        float zmax = bounds.max.z / 600;

        volume.m_value = 0;
        for (int y = ymin; y < ymax; ++y)
        {
            int yi = y * 320;
            for (int x = xmin; x < xmax; ++x)
            {
                int xi = x;
                int i = xi + yi;

                float depth = rawDepthData[i];
                if (depth <= zmax && depth >= zmin)
                    volume.m_value++;
            }
        }
    }
}
