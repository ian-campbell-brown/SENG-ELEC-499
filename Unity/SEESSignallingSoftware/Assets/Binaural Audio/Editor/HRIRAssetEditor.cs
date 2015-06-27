using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HRIRAsset))]
public class HRIRAssetEditor : Editor
{
    public bool _ShowAzimuths = false;
    public bool _ShowElevations = false;

    public override void OnInspectorGUI()
    {
        HRIRAsset asset = (HRIRAsset)target;

        EditorGUILayout.LabelField("Subject Name", asset.SubjectName);
        EditorGUILayout.LabelField("HRIR Size", string.Format("{0} samples", asset.Samples));
        EditorGUILayout.Separator();

        EditorGUILayout.LabelField(" ");
        _ShowAzimuths = EditorGUI.Foldout(GUILayoutUtility.GetLastRect(), _ShowAzimuths, string.Format("Azimuths\t[{0}]", asset.Azimuths.Length), true);
        if (_ShowAzimuths)
            for (int i = 0; i < asset.Azimuths.Length; i++)
                EditorGUILayout.LabelField(string.Format("\tElement {0}", i), asset.Azimuths[i].ToString());

        EditorGUILayout.LabelField(" ");
        _ShowElevations = EditorGUI.Foldout(GUILayoutUtility.GetLastRect(), _ShowElevations, string.Format("Elevations\t[{0}]", asset.Elevations.Length), true);
        if (_ShowElevations)
            for (int i = 0; i < asset.Elevations.Length; i++)
                EditorGUILayout.LabelField(string.Format("\tElement {0}", i), asset.Elevations[i].ToString());
    }
}
