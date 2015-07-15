using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RaySignaller : MonoBehaviour
{
    public int m_numAudioCues = 8;
    public int m_sideBuffer = 16;
    public GameObject m_prefAudioCue;
    public float m_freqBase = 0.2f;
    public float m_freqGrowth = 1.0f;

    private List<GameObject> m_audioCues = new List<GameObject>();

    void Start()
    {
        for (int i = 0; i < m_numAudioCues; ++i)
        {
            GameObject obj = GameObject.Instantiate(m_prefAudioCue, transform.position, transform.rotation) as GameObject;
            obj.transform.parent = this.transform;
            m_audioCues.Add(obj);
        }
    }

    void Update()
    {
        Texture2D depthImage = ImageProcessor.Instance.m_filteredDepthImage.m_source;

        for (int x = 0; x < m_numAudioCues; x++)
        {
            int buffer = m_sideBuffer;
            int stepX = (depthImage.width - buffer) / (m_numAudioCues - 1);
            //int stepY = (depthImage.height - buffer) / (m_numRows - 1);

            int xi = x * stepX + (buffer / 2);
            int yi = depthImage.height / 2;

            float distNormalized = depthImage.GetPixel(xi, yi).grayscale;

            float px = ((float)xi / depthImage.width) - 0.5f;
            float py = -0.2f;//((float)yi / depthImage.height) - 0.5f;
            float pz = distNormalized;

            float sz = Mathf.Tan(Mathf.Deg2Rad * 45) * pz * 2;
            Vector3 point = transform.TransformPoint(new Vector3(px * sz, py, pz));

            Transform cue = m_audioCues[x].transform;
            if (distNormalized > 0)
            {
                //float factor = (float)x / (m_numAudioCues - 1);
                //float offsetScale = 1 - (2 * Mathf.Abs(factor - 0.5f));

                float scale = (1.0f - distNormalized);

                cue.position = point;
                cue.GetComponent<AudioSource>().mute = false;
                cue.GetComponent<AudioSource>().pitch = m_freqBase + scale * m_freqGrowth;// +(offsetScale * 0.8f);

                cue.GetComponent<Renderer>().enabled = true;
            }
            else
            {
                cue.GetComponent<AudioSource>().mute = true;
                cue.position = point;
                cue.GetComponent<Renderer>().enabled = false;
            }
        }


        Color c = new Color(.85f, 0.0f, 0.0f);
        foreach (GameObject obj in m_audioCues)
        {
            Debug.DrawLine(transform.position, obj.transform.position, c);
        }
    }

    void OnDrawGizmosSelected()
    {
        float sz = Mathf.Tan(Mathf.Deg2Rad * 45);
        Vector3 frontLeft = transform.TransformPoint(new Vector3(-sz, 0, 1));
        Vector3 frontRight = transform.TransformPoint(new Vector3(sz, 0, 1));
        Vector3 centerBack = Vector3.zero;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(centerBack, frontLeft);
        Gizmos.DrawLine(centerBack, frontRight);
        Gizmos.DrawLine(frontLeft, frontRight);
    }
}
