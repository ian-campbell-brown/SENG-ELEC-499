using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ImageBinding : MonoBehaviour
{
    public SharedTexture2D m_sharedTexture;

    // Use this for initialization
    void Start()
    {
        RawImage image = this.GetComponent<RawImage>();
        if (image != null)
            image.texture = m_sharedTexture.m_source;

        Renderer renderer = this.GetComponent<Renderer>();
        if (renderer != null)
            renderer.material.mainTexture = m_sharedTexture.m_source;
	}

    // Update is called once per frame
    void Update()
    {

    }
}
