using UnityEngine;

public class AttenuationFilter : MonoBehaviour
{
    public float m_minDistance = 0;
    public float m_maxDistance = 100;
    public float m_scale = 100;
    public AttenuationType m_attenuationType = AttenuationType.Logarithmic;

    private float m_gain = 0;

    void Update()
    {
        AudioListener listener = GameObject.FindObjectOfType<AudioListener>();
        if (listener == null)
        {
            m_gain = 1;
            return;
        }


        float distance = Vector3.Distance(transform.position, listener.transform.position);
        float factor = Mathf.Clamp((distance - m_minDistance) / (m_maxDistance - m_minDistance), 0, 1);

        switch (m_attenuationType)
        {
            case AttenuationType.Linear:        m_gain = ComputeLinear(factor); break;
            case AttenuationType.Logarithmic:   m_gain = ComputeLogarithmic(factor); break;
            case AttenuationType.LogReverse:    m_gain = ComputeLogReverse(factor); break;
            case AttenuationType.Inverse:       m_gain = ComputeInverse(factor); break;
            default:                            m_gain = 1; break;
        }
    }

    private float ComputeLinear(float factor)
    {
        return 1 - factor;
    }

    private float ComputeLogarithmic(float factor)
    {
        if (factor == 0)
            return 1;

        return Mathf.Min(-Mathf.Log(factor) / m_scale, 1.0f);
    }

    private float ComputeLogReverse(float factor)
    {
        return 1 - ComputeLogarithmic(1 - factor);
    }

    private float ComputeInverse(float factor)
    {
        if (factor == 1)
            return 0;

        return Mathf.Min(((1 / factor) - 1) / m_scale, 1.0f);
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        for (int i = 0; i < data.Length; i++)
            data[i] *= m_gain;
    }

    public enum AttenuationType
    {
        None,
        Linear,
        Logarithmic,
        LogReverse,
        Inverse,
    }
}