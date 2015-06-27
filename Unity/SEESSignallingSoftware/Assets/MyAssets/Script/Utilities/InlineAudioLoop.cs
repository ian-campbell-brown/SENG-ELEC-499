using UnityEngine;
using System.Collections;
using System.Linq;

public class InlineAudioLoop : MonoBehaviour {

    public int StartTimeSamples = 0;
    public int EndTimeSamples = 0;

    private bool m_loop = false;
    private bool m_stopAudio = false;
    private int m_channels = 1;
    private int m_currentSample = 0;
    private float[] m_audioSamples = null;
        
    private AudioClip m_bufferClip = null;

	// Use this for initialization
	void Awake () {
        if (!enabled || GetComponent<AudioSource>().clip == null)
            return;

        SetupLoopedAudio(GetComponent<AudioSource>().clip);

        if (GetComponent<AudioSource>().playOnAwake)
            GetComponent<AudioSource>().Play();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (GetComponent<AudioSource>().clip == null)
            return;

        if (GetComponent<AudioSource>().clip == m_bufferClip)
        {
            m_loop = GetComponent<AudioSource>().loop;

            if (m_stopAudio)
                GetComponent<AudioSource>().Stop();
        }
        else
        {
            SetupLoopedAudio(GetComponent<AudioSource>().clip);
        }
	}

    private void SetupLoopedAudio(AudioClip clip)
    {
        m_loop = false;
        m_stopAudio = false;
        m_channels = clip.channels;
        m_currentSample = 0;
        m_audioSamples = GetAudioClipSamples(clip);
        m_bufferClip = AudioClip.Create("LoopBuffer", 1, clip.channels, clip.frequency, true, OnAudioRead, OnAudioSetPosition);

        GetComponent<AudioSource>().clip = m_bufferClip;
    }

    private float[] GetAudioClipSamples(AudioClip clip)
    {
        float[] buffer = new float[clip.samples * clip.channels];

        clip.GetData(buffer, 0);

        return buffer;
    }

    private void OnAudioRead(float[] data)
    {
        int startSamples = StartTimeSamples * m_channels;
        int endSamples = Mathf.Min(EndTimeSamples * m_channels, m_audioSamples.Length);
        int loopedSamples = Mathf.Max(endSamples - startSamples, 1 * m_channels);

        if (m_loop && m_currentSample > endSamples)
            m_currentSample = startSamples + ((m_currentSample - startSamples) % loopedSamples);

        for (int i = 0; i < data.Length; i++)
        {
            int iSample = m_currentSample + i;
            int iLooped = startSamples + ((iSample - startSamples) % loopedSamples);

            if (iSample < endSamples)
                data[i] = m_audioSamples[iSample];
            else if (m_loop)
                data[i] = m_audioSamples[iLooped];
            else if (iSample < m_audioSamples.Length)
                data[i] = m_audioSamples[iSample];
            else
                data[i] = 0;
        }

        m_currentSample += data.Length;

        // Add a buffer of 0x8000 samples to prevent the last samples
        // of audio from being cut off when the AudioSource is stopped.
        if (!m_loop && m_currentSample >= m_audioSamples.Length + 0x8000)
            m_stopAudio = true;
    }

    private void OnAudioSetPosition(int position)
    {
        m_currentSample = position;

        if (m_currentSample < 0)
            m_currentSample = 0;

        if (!m_loop && m_currentSample >= m_audioSamples.Length)
            m_stopAudio = true;
    }
}
