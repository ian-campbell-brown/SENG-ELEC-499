using UnityEngine;
using System.Collections;

public class VolumeSignalObject : MonoBehaviour
{

    public SignallingVolume m_owner = null;

    private Vector3 m_scale;

    // Use this for initialization
    void Start()
    {
        m_scale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 ownerPosition = m_owner.transform.position;

        float z = ownerPosition.z / 60;
        float sz = 2 * z * Mathf.Tan(Mathf.Deg2Rad * 45);

        transform.position = Vector3.Scale(ownerPosition, new Vector3(sz, sz, 1));

        transform.localScale = Vector3.Lerp(transform.localScale, m_scale, 0.3f);
    }

    public void PlayEntryCue(AudioClip clip)
    {
        transform.localScale = Vector3.one * 2f;

        AudioSource source = GetComponent<AudioSource>();
        source.clip = clip;
        source.Play();
    }

    public void PlayExitCue(AudioClip clip)
    {
        transform.localScale = Vector3.one * 0.5f;

        AudioSource source = GetComponent<AudioSource>();
        source.clip = clip;
        source.Play();
    }
}
