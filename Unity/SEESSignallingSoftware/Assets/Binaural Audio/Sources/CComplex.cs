using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

[DebuggerDisplay("{_r} + i{_i}")]
public struct CComplex
{
    public static readonly CComplex Zero = new CComplex(0, 0);
    public static readonly CComplex One = new CComplex(1);
    public static readonly CComplex I = new CComplex(0, 1);

    public float _r;
    public float _i;

    public static CComplex operator +(CComplex c1, CComplex c2) { return CComplex.Add(c1, c2); }
    public static CComplex operator -(CComplex c1, CComplex c2) { return CComplex.Subtract(c1, c2); }
    public static CComplex operator *(CComplex c1, CComplex c2) { return CComplex.Multiply(c1, c2); }
    public static CComplex operator +(CComplex c, float d) { return CComplex.Add(c, d); }
    public static CComplex operator -(CComplex c, float d) { return CComplex.Subtract(c, d); }
    public static CComplex operator *(CComplex c, float d) { return CComplex.Multiply(c, d); }
    public static CComplex operator -(CComplex c) { return CComplex.Negate(c); }
    public static implicit operator CComplex(PComplex c) { return new CComplex(c); }

    public CComplex(float r) : this(r, 0) { }
    public CComplex(float r, float i)
    {
        _r = r;
        _i = i;
    }

    public CComplex(PComplex c)
    {
        _r = c._m * (float)Math.Cos(c._a);
        _i = c._m * (float)Math.Sin(c._a);
    }

    public float GetMagnitude()
    {
        return (float)Math.Sqrt(_r * _r + _i * _i);
    }

    public float GetAngle()
    {
        return (float)Math.Atan2(_i, _r);
    }

    public static CComplex Add(CComplex c1, CComplex c2)
    {
        return new CComplex(c1._r + c2._r, c1._i + c2._i);
    }

    public static CComplex Add(CComplex c, float d)
    {
        return new CComplex(c._r + d, c._i);
    }

    public static CComplex Subtract(CComplex c1, CComplex c2)
    {
        return new CComplex(c1._r - c2._r, c1._i - c2._i);
    }

    public static CComplex Subtract(CComplex c, float d)
    {
        return new CComplex(c._r - d, c._i);
    }

    public static CComplex Multiply(CComplex c1, CComplex c2)
    {
#if PREFER_ADDITION
        // float a = c1._r;
        // float b = c1._i;
        // float c = c2._r;
        // float d = c2._i;
        // float k1 = a * (c + d);
        // float k2 = d * (a + b);
        // float k3 = c * (b - a);

        float k1 = c1._r * (c2._r + c2._i);
        float k2 = c2._i * (c1._r + c1._i);
        float k3 = c2._r * (c1._i - c1._r);

        return new CComplex(k1 - k2, k1 + k3);   
#else
        // float a = c1._r;
        // float b = c1._i;
        // float c = c2._r;
        // float d = c2._i;
        // return new CComplex(a * c - b * d, b * c + a * d);

        return new CComplex(c1._r * c2._r - c1._i * c2._i, c1._i * c2._r + c1._r * c2._i);
#endif
    }

    public static CComplex Multiply(CComplex c, float d)
    {
        return new CComplex(c._r * d, c._i * d);
    }

    public static CComplex Negate(CComplex c)
    {
        return new CComplex(-c._r, -c._i);
    }

    public static CComplex Conjugate(CComplex c)
    {
        return new CComplex(c._r, -c._i);
    }
}
