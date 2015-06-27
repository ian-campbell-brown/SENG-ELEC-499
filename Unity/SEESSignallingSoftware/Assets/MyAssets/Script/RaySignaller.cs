using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RaySignaller : MonoBehaviour
{
    public int m_numAudioCues = 8;
    public GameObject m_prefAudioCue;

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

        for (int i = 0; i < m_numAudioCues; i++)
        {
            int step = depthImage.width / (m_numAudioCues - 1);

            int xi = i * step;
            int yi = depthImage.height / 2;
            
            float distNormalized = depthImage.GetPixel(xi, yi).grayscale;

            float x = ((float)xi / depthImage.width) - 0.5f;
            float y = 0.0f;
            float z = distNormalized;

            float sz = Mathf.Tan(Mathf.Deg2Rad * 45) * z * 2;
            Vector3 point = transform.TransformPoint(new Vector3(x * sz, y, z));

            Transform cue = m_audioCues[i].transform;
            if (distNormalized > 0)
            {
                cue.position = point;
                cue.GetComponent<AudioSource>().mute = false;
                cue.GetComponent<AudioSource>().pitch = 0.2f + ((0.5f - Mathf.Abs(((float)i / m_numAudioCues) - 0.5f)) * 1.6f);

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
