using UnityEngine;
using System.Collections;
using System.Linq;


public class PointCloudVisualizer : MonoBehaviour
{
    public bool m_showBounds = false;
    public bool m_perspective = false;

    [Range(0.0f, 90.0f)]
    public float m_fov = 45.0f;

    public SharedTexture2D m_depthImage;

    public GameObject m_prefPointMesh;

    public Material m_shaderMaterial;

    // Use this for initialization
    void Start()
    {
        m_shaderMaterial = new Material(m_shaderMaterial);
        m_shaderMaterial.SetTexture("_MainTex", m_depthImage.m_source);

        CreateMeshes();
    }

    void Update()
    {
        m_shaderMaterial.SetFloat("_ProjectionOn", (m_perspective ? 1.0f : 0.0f));
        m_shaderMaterial.SetFloat("_ProjectionFOV", m_fov);
    }

    void CreateMeshes()
    {
        int width = m_depthImage.m_width;
        int height = m_depthImage.m_height;

        int numPoints = width * height;
        int numPointsPerMesh = 20000;
        int numMeshes = (numPoints + numPointsPerMesh - 1) / numPointsPerMesh;

        Vector3[][] points = new Vector3[numMeshes][];
        Vector2[][] uvs = new Vector2[numMeshes][];
        int[] indices = Enumerable.Range(0, numPointsPerMesh).ToArray();

        for (int i = 0; i < numMeshes; ++i)
        {
            points[i] = new Vector3[numPointsPerMesh];
            uvs[i] = new Vector2[numPointsPerMesh];
        }

        for (int i = 0; i < numPoints; ++i)
        {
            int iMesh = i / numPointsPerMesh;
            int iIndex = i % numPointsPerMesh;

            int xi = i % width;
            int yi = i / width;
            float x = ((float)xi / width) - 0.5f;
            float y = ((float)yi / height) - 0.5f;
            float u = (float)xi / width;
            float v = (float)yi / height;

            points[iMesh][iIndex] = new Vector3(x, y, 0);
            uvs[iMesh][iIndex] = new Vector2(u, v);
        }

        for (int i = 0; i < numMeshes; ++i)
        {
            GameObject meshObj = GameObject.Instantiate(m_prefPointMesh, transform.position, transform.rotation) as GameObject;
            Mesh mesh = new Mesh();

            mesh.vertices = points[i];
            mesh.uv = uvs[i];
            mesh.SetIndices(indices, MeshTopology.Points, 0);
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one);

            meshObj.transform.parent = this.transform;
            meshObj.transform.localScale = Vector3.one;
            meshObj.hideFlags |= HideFlags.NotEditable;
            meshObj.GetComponent<MeshFilter>().mesh = mesh;
            meshObj.GetComponent<Renderer>().material = m_shaderMaterial;
        }
    }

    void OnDrawGizmos()
    {
        if (m_showBounds)
            OnDrawGizmosSelected();
    }

    void OnDrawGizmosSelected()
    {
        if (m_perspective)
        {
            float sz = Mathf.Tan(Mathf.Deg2Rad * m_fov);
            
            Vector3 origin = transform.TransformPoint(new Vector3(0, 0, -0.5f));
            Vector3 tl = transform.TransformPoint(new Vector3(-sz, sz, 0.5f));
            Vector3 tr = transform.TransformPoint(new Vector3(sz, sz, 0.5f));
            Vector3 bl = transform.TransformPoint(new Vector3(-sz, -sz, 0.5f));
            Vector3 br = transform.TransformPoint(new Vector3(sz, -sz, 0.5f));
            

            Gizmos.color = new Color32(134, 209, 131, 255);
            DrawPyramid(origin, tl, tr, bl, br);

            
            
            float sz2 = (1 - 0.8f) * Mathf.Tan(Mathf.Deg2Rad * m_fov);
            Vector3 itl = transform.TransformPoint(new Vector3(-sz2, sz2, -0.3f));
            Vector3 itr = transform.TransformPoint(new Vector3(sz2, sz2, -0.3f));
            Vector3 ibl = transform.TransformPoint(new Vector3(-sz2, -sz2, -0.3f));
            Vector3 ibr = transform.TransformPoint(new Vector3(sz2, -sz2, -0.3f));

            Gizmos.color = new Color32(209, 209, 131, 255);
            DrawQuad(itl, itr, ibl, ibr);
        }
        else
        {
            Gizmos.color = new Color32(134, 209, 131, 255);
            Gizmos.DrawWireCube(transform.position, transform.lossyScale);

            
            Vector3 itl = transform.TransformPoint(new Vector3(-0.5f, 0.5f, -0.3f));
            Vector3 itr = transform.TransformPoint(new Vector3(0.5f, 0.5f, -0.3f));
            Vector3 ibl = transform.TransformPoint(new Vector3(-0.5f, -0.5f, -0.3f));
            Vector3 ibr = transform.TransformPoint(new Vector3(0.5f, -0.5f, -0.3f));

            Gizmos.color = new Color32(209, 209, 131, 255);
            DrawQuad(itl, itr, ibl, ibr);
        }


    }

    void DrawPyramid(Vector3 origin, Vector3 topLeft, Vector3 topRight, Vector3 botLeft, Vector3 botRight)
    {
        Gizmos.DrawLine(origin, topLeft);
        Gizmos.DrawLine(origin, topRight);
        Gizmos.DrawLine(origin, botLeft);
        Gizmos.DrawLine(origin, botRight);

        DrawQuad(topLeft, topRight, botLeft, botRight);
    }

    void DrawQuad(Vector3 topLeft, Vector3 topRight, Vector3 botLeft, Vector3 botRight)
    {
        Gizmos.DrawLine(botLeft, topLeft);
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, botRight);
        Gizmos.DrawLine(botRight, botLeft);
    }
}
