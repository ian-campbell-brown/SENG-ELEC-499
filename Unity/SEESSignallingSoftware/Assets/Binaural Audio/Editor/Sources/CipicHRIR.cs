using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MatlabImport;

public class CipicHRIR
{
    public string Name { get { return _Name; } }
    private string _Name = null;

    public double[,] OnL { get { return _OnL; } }
    public double[,] OnR { get { return _OnR; } }
    public double[,] Itd { get { return _Itd; } }
    private double[,] _OnL = null;
    private double[,] _OnR = null;
    private double[,] _Itd = null;

    public double[, ,] LeftData { get { return _LeftData; } }
    public double[, ,] RightData { get { return _RightData; } }
    private double[, ,] _LeftData = null;
    private double[, ,] _RightData = null;

    public float[] Azimuths { get { return _Azimuths; } }
    public float[] Elevations { get { return _Elevations; } }
    private float[] _Azimuths = null;
    private float[] _Elevations = null;

    public int Samples { get { return _Samples; } }
    private int _Samples = 0;

    public CipicHRIR()
    {

    }

    #region Static Members
    
    public static CipicHRIR FromFile(string path)
    {
        MData[] data = MData.FromFile(path);

        return FromMData(data);
    }

    public static CipicHRIR FromMData(MData[] data)
    {
        MTable<double> onLTable = MTable<double>.FromMData(data[0]);
        MTable<double> onRTable = MTable<double>.FromMData(data[1]);
        MTable<double> itdTable = MTable<double>.FromMData(data[2]);
        MTable<double> leftHrirTable = MTable<double>.FromMData(data[4]);
        MTable<double> rightHrirTable = MTable<double>.FromMData(data[3]);
        MTable<short> nameTable = MTable<short>.FromMData(data[5]);
        CipicHRIR result = new CipicHRIR();
        result._OnL = new double[onLTable.Dimensions[0], onLTable.Dimensions[1]];
        result._OnR = new double[onRTable.Dimensions[0], onRTable.Dimensions[1]];
        result._Itd = new double[itdTable.Dimensions[0], itdTable.Dimensions[1]];
        result._LeftData = new double[leftHrirTable.Dimensions[0], leftHrirTable.Dimensions[1], leftHrirTable.Dimensions[2]];
        result._RightData = new double[rightHrirTable.Dimensions[0], rightHrirTable.Dimensions[1], rightHrirTable.Dimensions[2]];
        result._Name = new string(nameTable.Data.Select(x => (char)x).ToArray());
        result._Samples = leftHrirTable.Dimensions[2];
        result._Azimuths = GetAzimuths();
        result._Elevations = GetElevations();

        for (int y = 0; y < onLTable.Dimensions[1]; y++)
            for (int x = 0; x < onLTable.Dimensions[0]; x++)
                result._OnL[x, y] = onLTable.Data[y * onLTable.Dimensions[0] + x];

        for (int y = 0; y < onRTable.Dimensions[1]; y++)
            for (int x = 0; x < onRTable.Dimensions[0]; x++)
                result._OnR[x, y] = onRTable.Data[y * onRTable.Dimensions[0] + x];

        for (int y = 0; y < itdTable.Dimensions[1]; y++)
            for (int x = 0; x < itdTable.Dimensions[0]; x++)
                result._Itd[x, y] = itdTable.Data[y * itdTable.Dimensions[0] + x];

        for (int y = 0; y < leftHrirTable.Dimensions[1]; y++)
            for (int x = 0; x < leftHrirTable.Dimensions[0]; x++)
                for (int t = 0; t < leftHrirTable.Dimensions[2]; t++)
                    result._LeftData[x, y, t] = leftHrirTable.Data[t * leftHrirTable.Dimensions[1] * leftHrirTable.Dimensions[0] + y * leftHrirTable.Dimensions[0] + x];

        for (int y = 0; y < rightHrirTable.Dimensions[1]; y++)
            for (int x = 0; x < rightHrirTable.Dimensions[0]; x++)
                for (int t = 0; t < rightHrirTable.Dimensions[2]; t++)
                    result._RightData[x, y, t] = rightHrirTable.Data[t * rightHrirTable.Dimensions[0] * rightHrirTable.Dimensions[1] + y * rightHrirTable.Dimensions[0] + x];

        return result;
    }

    public static float[] GetAzimuths()
    {
        List<float> result = new List<float>();

        result.Add(-80);
        result.Add(-65);
        result.Add(-55);

        for (int i = -9; i <= 9; i++)
            result.Add(i * 5);

        result.Add(55);
        result.Add(65);
        result.Add(80);

        return result.ToArray();
    }

    public static float[] GetElevations()
    {
        List<float> result = new List<float>();

        for (int i = 0; i < 50; i++)
            result.Add(-45 + i * 5.625f);

        return result.ToArray();
    }
    
    #endregion
}