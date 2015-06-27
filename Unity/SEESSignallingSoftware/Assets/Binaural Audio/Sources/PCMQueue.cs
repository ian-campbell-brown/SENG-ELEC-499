using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

    public class PCMQueue
    {
        private float[] _Buffer = null;
        private int _Length = 0;
        private int _ICurrent = 0;

        public int Length { get { return _Length; } }
        public float this[int index] { get { return _Buffer[(_ICurrent + index) % _Buffer.Length]; } set { _Buffer[(_ICurrent + index) % _Buffer.Length] = value; } }

        public PCMQueue(int length)
        {
            _Buffer = new float[length];
            _Length = length;
            _ICurrent = 0;
        }

        public void Enqueue(float value)
        {
            if (_ICurrent == 0)
                _ICurrent = _Length;

            _Buffer[--_ICurrent] = value;
        }

        public float Get(int index)
        {
            return _Buffer[(_ICurrent + index) % _Length];
        }

        public void Set(int index, float value)
        {
            _Buffer[(_ICurrent + index) % _Length] = value;
        }
    }
