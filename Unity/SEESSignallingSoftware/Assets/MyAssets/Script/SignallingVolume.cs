using UnityEngine;
using System.Collections;

public class SignallingVolume : MonoBehaviour
{
    public bool m_isOn = false;

    public int m_value = 0;
    public int m_valueThreshold = 1000;

    public float m_timeout = 0.3f;
    private float m_offTime = 0.0f;

    public Color m_onColor = Color.blue;
    private Color m_offColor;
    public AudioClip m_entryCue;
    public AudioClip m_exitCue;

    public VolumeSignalObject m_signalObject;

    

    // Use this for initialization
    void Start()
    {
        m_offColor = GetComponent<Renderer>().material.color;
        m_signalObject = GameObject.Instantiate<VolumeSignalObject>(m_signalObject);
        m_signalObject.m_owner = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_value > m_valueThreshold)
        {
            if (m_offTime > 0.0f)
            {
                m_signalObject.PlayEntryCue(m_entryCue);
            }

            m_isOn = true;
            m_offTime = 0.0f;
        }
        else
        {
            m_offTime += Time.deltaTime;

            if (m_isOn)
            {
                m_signalObject.PlayExitCue(m_exitCue);
            }

            if (m_offTime >= m_timeout)
            {
                m_isOn = false;
            }
        }

        if (m_isOn)
            GetComponent<Renderer>().material.color = m_onColor;
        else
            GetComponent<Renderer>().material.color = m_offColor;
    }

    void OnDrawGizmos()
    {
        Vector3 spaceSz = new Vector3(320, 240, 600) / 10;
        Bounds bounds = new Bounds(transform.position, transform.lossyScale);
        bounds.center += new Vector3(spaceSz.x, spaceSz.y) / 2;
        bounds.min = Vector3.Min(Vector3.Max(bounds.min, Vector3.zero), spaceSz);
        bounds.max = Vector3.Min(Vector3.Max(bounds.max, Vector3.zero), spaceSz);
        bounds.center -= new Vector3(spaceSz.x, spaceSz.y) / 2;

        Color c = m_onColor;
        c.a = 1.0f;

        Gizmos.color = c;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }

    public Bounds GetDepthImageBounds()
    {
        Vector3 spaceSz = new Vector3(320, 240, 600);
        Bounds bounds = new Bounds(transform.position * 10, transform.localScale * 10);
        bounds.center += new Vector3(spaceSz.x, spaceSz.y) / 2;
        bounds.min = Vector3.Min(Vector3.Max(bounds.min, Vector3.zero), spaceSz);
        bounds.max = Vector3.Min(Vector3.Max(bounds.max, Vector3.zero), spaceSz);

        return bounds;
    }
}
