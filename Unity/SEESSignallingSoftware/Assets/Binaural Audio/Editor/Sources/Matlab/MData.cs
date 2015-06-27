using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MatlabImport
{
    public class MData
    {
        private Type _HeaderType = typeof(int);
        private Type _DataType = null;

        public object[] Data { get { return _Data; } }
        private object[] _Data = null;

        public MData(int count, Type dataType)
        {
            _Data = new object[count];
            _DataType = dataType;
        }

        public MData(object[] data)
            :this(data, typeof(int))
        {
            
        }

        private MData(object[] data, Type headerType)
        {
            _Data = data;

            if (data.Length == 0)
                throw new ArgumentException();

            _DataType = data[0].GetType();

            if (data.Any(x => x.GetType() != _DataType))
                throw new ArgumentException();

            _HeaderType = headerType;
        }

        public int SizeInMemory()
        {
            if (_DataType == typeof(MData))
            {
                return (2 * Marshal.SizeOf(_HeaderType)) + (_Data.OfType<MData>().Sum(x => x.SizeInMemory()));
            }
            else
            {
                if (_DataType == typeof(int))
                    return (2 * Marshal.SizeOf(_HeaderType)) + ((_Data.Length * Marshal.SizeOf(_DataType) + 0x7) & ~0x7);

                return (2 * Marshal.SizeOf(_HeaderType)) + ((_Data.Length * Marshal.SizeOf(_DataType) + 0x3) & ~0x3);
            }
        }

        #region Static Members

        public static unsafe MData[] FromFile(string path)
        {
            byte[] data = File.ReadAllBytes(path);
            return FromFile(data);
        }

        public static unsafe MData[] FromFile(byte[] data)
        {
            fixed (byte* ptr = data)
                return FromFile(ptr, data.Length);
        }

        public static unsafe MData[] FromFile(byte* ptr, int length)
        {
            string header = new string((sbyte*)ptr, 0, 0x7C, Encoding.ASCII);
            // short unk1 = *(short*)(ptr + 0x7C);
            // short unk2 = *(short*)(ptr + 0x7E);
            
            if (!header.ToUpper().StartsWith("MATLAB"))
                throw new Exception("Could not parse Matlab structure file.");

            return FromPtr(ptr + 0x80, length - 0x80);
        }

        public static unsafe MData[] FromPtr(byte* ptr, int length)
        {
            List<MData> result = new List<MData>();
            int offset = 0;

            while (offset < length)
            {
                MData data = FromPtr(ptr + offset);

                result.Add(data);

                offset += data.SizeInMemory();
            }

            return result.ToArray();
        }

        public static unsafe MData FromPtr(byte* ptr)
        {
            Type headerType = null;
            int* iData = (int*)ptr;
            short* sData = (short*)ptr;
            byte* data = ptr;
            int type = 0;
            int size = 0;
            
            if (sData[1] == 0)
            {
                type = iData[0];
                size = iData[1];
                data += 2 * sizeof(int);
                headerType = typeof(int);
            }
            else
            {
                type = sData[0];
                size = sData[1];
                data += 2 * sizeof(short);
                headerType = typeof(short);
            }
            
            switch (type)
            {
                case 0x01: return new MData(GetData<byte>(data, size), headerType);
                case 0x04: return new MData(GetData<short>(data, size), headerType);
                case 0x05: return new MData(GetData<int>(data, size), headerType);
                case 0x06: return new MData(GetData<long>(data, size), headerType);
                case 0x09: return new MData(GetData<double>(data, size), headerType);
                case 0x0E: return new MData((object[])FromPtr(data, size), headerType);
                default: throw new Exception(string.Format("Unrecognized Matlab data type {0}.", type));
            }
        }

        private static unsafe object[] GetData<T>(byte* ptr, int size) where T : struct
        {
            int sizeOf = Marshal.SizeOf(typeof(T));
            object[] result = new object[size / sizeOf];

            for (int i = 0; i < result.Length; i++)
                result[i] = (T)Marshal.PtrToStructure(new IntPtr(ptr + (i * sizeOf)), typeof(T));

            return (object[])result;
        }

        #endregion
    }
}
