using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MatlabImport
{
    [DebuggerDisplay("Name={Name}, Width={Width}, Height={Height}")]
    public class MVector
    {
        public string Name { get { return _Name; } }
        private string _Name = null;

        public double[,] Data { get { return _Data; } }
        private double[,] _Data = null;

        public int Width { get { return _Data.GetLength(0); } }
        public int Height { get { return _Data.GetLength(1); } }

        public MVector(string name, double[,] data)
        {
            _Name = name;

            int width = data.GetLength(0);
            int height = data.GetLength(1);
            _Data = new double[width, height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    _Data[x, y] = data[x, y];
        }

        public MVector(string name, int width, int height)
        {
            _Name = name;
            _Data = new double[width, height];
        }

        public int SizeInMemory()
        {
            return Wrapper.SIZE + Encoding.ASCII.GetByteCount(_Name) + Width * Height * sizeof(double);
        }


        #region Static Members

        public static unsafe MVector[] FromFile(string path)
        {
            byte[] data = File.ReadAllBytes(path);
            return FromFile(data);
        }

        public static unsafe MVector[] FromFile(byte[] data)
        {
            fixed (byte* ptr = data)
                return FromFile(ptr, data.Length);
        }

        public static unsafe MVector[] FromFile(byte* ptr, int length)
        {
            List<MVector> result = new List<MVector>();
            int offset = 0;

            while (offset < length)
            {
                MVector vector = FromPtr(ptr + offset);

                result.Add(vector);

                offset += vector.SizeInMemory();
            }

            return result.ToArray();
        }

        public static unsafe MVector FromPtr(byte* ptr)
        {
            Wrapper* wrapper = (Wrapper*)ptr;

            int nameLen = wrapper->nameLen;
            sbyte* nameData = (sbyte*)(ptr + Wrapper.SIZE);
            string name = new string(nameData, 0, nameLen, Encoding.ASCII);

            int width = wrapper->width;
            int height = wrapper->height;
            double* data = (double*)(ptr + Wrapper.SIZE + nameLen);
            MVector result = new MVector(name, width, height);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    result.Data[x, y] = data[(y * width) + x];

            return result;
        }

        #endregion

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct Wrapper
        {
            public const int SIZE = 0x14;

            public int unk1;
            public int width;
            public int height;
            public int unk4;
            public int nameLen;
        }
    }
}
