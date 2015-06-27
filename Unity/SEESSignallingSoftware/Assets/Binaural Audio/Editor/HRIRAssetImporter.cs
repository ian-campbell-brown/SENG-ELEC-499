using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

using MatlabImport;

public class HRIRAssetImporter : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
		foreach (string path in importedAssets.Where(x => IsMatlabFile(x)))
        {
            string hrirPath = Path.ChangeExtension(path, ".asset");
            HRIRAsset old = AssetDatabase.LoadAssetAtPath(hrirPath, typeof(HRIRAsset)) as HRIRAsset;
            HRIRAsset hrir = TryFromFile(path);

            if (old != null && hrir != null)
                old.UpdateAssetData(hrir);
            else if (hrir != null)
                AssetDatabase.CreateAsset(hrir, hrirPath);
            else
                AssetDatabase.DeleteAsset(hrirPath);
        }

        foreach (string path in deletedAssets.Where(x => IsMatlabFile(x)))
        {
            string hrirPath = Path.ChangeExtension(path, ".asset");
            AssetDatabase.DeleteAsset(hrirPath);
        }
        
        var movePairs = movedAssets.Select((x, i) => new KeyValuePair<string, string>(x, movedFromAssetPaths[i])).ToArray();
        foreach (var pair in movePairs.Where(x => IsMatlabFile(x.Value)))
        {
            string matSrc = pair.Value;
            string matDest = pair.Key;
            string hrirSrc = Path.ChangeExtension(matSrc, ".asset");
            string hrirDest = Path.ChangeExtension(matDest, ".asset");

            if (IsMatlabFile(matDest))
                AssetDatabase.MoveAsset(hrirSrc, hrirDest);
            else
                AssetDatabase.DeleteAsset(hrirSrc);
        }
	}

    public static bool IsMatlabFile(string path)
    {
        return Path.GetExtension(path).Equals(".mat", StringComparison.InvariantCultureIgnoreCase);
    }

    private static HRIRAsset TryFromFile(string path)
    {
        try
        {
            return HRIRAssetFromFile(path);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static HRIRAsset HRIRAssetFromFile(string path)
    {
        CipicHRIR hrir = CipicHRIR.FromFile(path);

        return HRIRAsset.CreateInstance(hrir.Name, hrir.Azimuths, hrir.Elevations, hrir.LeftData, hrir.RightData, hrir.Samples);
    }
}
