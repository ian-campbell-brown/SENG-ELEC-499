using UnityEngine;

public class AttenuationFilter : MonoBehaviour
{
    public float m_minDistance = 0;
    public float m_maxDistance = 100;
    public float m_scale = 100;
    public float m_attack = 0.001f;
    public AttenuationType m_attenuationType = AttenuationType.Logarithmic;

    private float m_setGain = 0;
    private float m_gain = 0;

    void FixedUpdate()
    {
        AudioListener listener = GameObject.FindObjectOfType<AudioListener>();
        if (listener == null)
        {
            m_setGain = 1;
            return;
        }

        float distance = Vector3.Distance(transform.position, listener.transform.position);
        float factor = Mathf.Clamp((distance - m_minDistance) / (m_maxDistance - m_minDistance), 0, 1);

        switch (m_attenuationType)
        {
            case AttenuationType.Linear:        m_setGain = ComputeLinear(factor); break;
            case AttenuationType.Logarithmic:   m_setGain = ComputeLogarithmic(factor); break;
            case AttenuationType.LogReverse:    m_setGain = ComputeLogReverse(factor); break;
            case AttenuationType.Inverse:       m_setGain = ComputeInverse(factor); break;
            default:                            m_setGain = 1; break;
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
        {
            if (i % channels == 0)
            {
                m_gain = Mathf.Lerp(m_gain, m_setGain, m_attack);
            }

            data[i] *= m_gain;
        }
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