using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// A simple asset class to allow empty texture buffers to be created and bound to objects as assets
public class SharedTexture2D : ScriptableObject
{
    [HideInInspector]
    public Texture2D m_source;

    public int m_width;
    public int m_height;

    public TextureFormat m_format = TextureFormat.ARGB32;

    public bool m_useMipmaps = true;
    public bool m_useLinearFiltering = false;

    public List<Material> m_materialBindings;

    void OnEnable()
    {
        m_source = new Texture2D(m_width, m_height, m_format, m_useMipmaps, m_useLinearFiltering);
        m_source.name = string.Format("{0} (shared)", this.name);

        foreach (Material mat in m_materialBindings)
            if (mat != null)
                mat.mainTexture = m_source;
    }

    [MenuItem("Assets/Create/SharedTexture2D")]
    public static void CreateAsset()
    {
        ScriptableObjectUtils.CreateAsset<SharedTexture2D>();
    }
}