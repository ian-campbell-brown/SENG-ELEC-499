using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class HRIRAsset : ScriptableObject
{
    [SerializeField] 
    private string _SubjectName = null;
    public string SubjectName { get { return _SubjectName; } }

    [SerializeField]
    private int _Samples = 0;
    public int Samples { get { return _Samples; } }

    [SerializeField]
    private float[] _Azimuths = null;
    public float[] Azimuths { get { return _Azimuths; } }

    [SerializeField]
    private float[] _Elevations = null;
    public float[] Elevations { get { return _Elevations; } }


    private float[, ,] _LeftData = null;
    private float[, ,] _RightData = null;

    [SerializeField] public HRIRTable _LeftDataSerialized = null;
    [SerializeField] public HRIRTable _RightDataSerialized = null;

    public void OnEnable()
    {
        if (_LeftDataSerialized != null)
            _LeftData = _LeftDataSerialized.GetHRIRData();

        if (_RightDataSerialized != null)
            _RightData = _RightDataSerialized.GetHRIRData();
    }

    public float[] GetLeftHrir(int iAzimuth, int iElevation)
    {
        float[] result = new float[_LeftData.GetLength(2)];
        for (int i = 0; i < result.Length; i++)
            result[i] = _LeftData[iAzimuth, iElevation, i];

        return result;
    }

    public float[] GetRightHrir(int iAzimuth, int iElevation)
    {
        float[] result = new float[_RightData.GetLength(2)];
        for (int i = 0; i < result.Length; i++)
            result[i] = _RightData[iAzimuth, iElevation, i];

        return result;
    }

    public int GetNearestAzimuthIndex(float azimuth)
    {
        return _Azimuths.Select((x, i) => new KeyValuePair<float, int>(Math.Abs(azimuth - x), i)).OrderBy(x => x.Key).First().Value;
    }

    public int GetNearestElevationIndex(float elevation)
    {
        return _Elevations.Select((x, i) => new KeyValuePair<float, int>(Math.Abs(elevation - x), i)).OrderBy(x => x.Key).First().Value;
    }

    public void UpdateAssetData(HRIRAsset newData)
    {
        _SubjectName = newData._SubjectName;
        _Samples = newData._Samples;
        _Azimuths = newData._Azimuths;
        _Elevations = newData._Elevations;
        _RightDataSerialized = newData._RightDataSerialized;
        _LeftDataSerialized = newData._LeftDataSerialized;
    }

    #region Static Members

    public static HRIRAsset CreateInstance(string subjectName, float[] azimuths, float[] elevations,
                                           double[, ,] leftData, double[, ,] rightData, int samples)
    {
        HRIRAsset asset = HRIRAsset.CreateInstance("HRIRAsset") as HRIRAsset;

        asset._SubjectName = subjectName;
        asset._Azimuths = azimuths;
        asset._Elevations = elevations;

        asset._LeftDataSerialized = new HRIRTable(leftData);
        asset._RightDataSerialized = new HRIRTable(rightData);
        asset._Samples = samples;

        return asset;
    }
    
    #endregion

    [Serializable]
    public class HRIRTable
    {
        [SerializeField]
        public float[] _Data = null;

        [SerializeField]
        public int _Length, _Width, _Height = 0;

        public HRIRTable()
        {

        }

        public HRIRTable(double[, ,] data)
        {
            _Width = data.GetLength(0);
            _Height = data.GetLength(1);
            _Length = data.GetLength(2);
            _Data = new float[_Width * _Height * _Length];

            for (int x = 0; x < _Width; x++)
                for (int y = 0; y < _Height; y++)
                    for (int z = 0; z < _Length; z++)
                    {
                        int xi = x * _Height * _Length;
                        int yi = y * _Length;
                        int zi = z;
                        int i = xi + yi + zi;

                        _Data[i] = (float)data[x, y, z];
                    }
        }

        public float[, ,] GetHRIRData()
        {
            float[, ,] result = new float[_Width, _Height, _Length];

            for (int x = 0; x < _Width; x++)
                for (int y = 0; y < _Height; y++)
                    for (int z = 0; z < _Length; z++)
                    {
                        int xi = x * _Height * _Length;
                        int yi = y * _Length;
                        int zi = z;
                        int i = xi + yi + zi;

                        result[x, y, z] = _Data[i];
                    }

            return result;
        }
    }
}