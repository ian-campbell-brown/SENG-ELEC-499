using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MatlabImport
{
    [DebuggerDisplay("Name={Name}")]
    public class MTable<T>
    {
        public string Name { get { return _Name; } }
        private string _Name = null;

        public long Unk { get { return _Unk; } }
        private long _Unk = 0;

        public int[] Dimensions { get { return _Dimensions; } }
        private int[] _Dimensions = null;

        public T[] Data { get { return _Data; } }
        private T[] _Data = null;

        public MTable(string name, int[] dimensions, long unk, T[] data)
        {
            _Name = name;
            _Dimensions = dimensions;
            _Unk = unk;
            _Data = data;
        }

        #region Static Members

        public static MTable<T> FromMData(MData data)
        {
            MData[] innerData = data.Data.Cast<MData>().ToArray();

            long unk = (long)innerData[0].Data[0];
            int[] dimensions = innerData[1].Data.Cast<int>().ToArray();
            T[] resultData = innerData[3].Data.Select(x => (T)x).ToArray();
            string name = new string(innerData[2].Data.Select(x => (char)(byte)x).ToArray());

            return new MTable<T>(name, dimensions, unk, resultData);
        }

        #endregion
    }
}
