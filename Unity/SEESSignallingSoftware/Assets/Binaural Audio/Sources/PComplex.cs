using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

[DebuggerDisplay("m = {_m} a = {_a}")]
public struct PComplex
{
    public static readonly PComplex Zero = new PComplex(0, 0);
    public static readonly PComplex One = new PComplex(1, 0);
    public const float PI = 3.14159f;
    public const float PI2 = 2 * 3.14159f;

    public float _m;
    public float _a;

    public static PComplex operator *(PComplex c1, PComplex c2) { return PComplex.Multiply(c1, c2); }
    public static PComplex operator *(PComplex c, float d) { return PComplex.Multiply(c, d); }
    public static PComplex operator /(PComplex c1, PComplex c2) { return PComplex.Divide(c1, c2); }
    public static PComplex operator /(PComplex c, float d) { return PComplex.Divide(c, d); }
    public static implicit operator PComplex(CComplex c) { return new PComplex(c); }

    public PComplex(float m) : this(m, 0) { }
    public PComplex(float m, float a)
    {
        _m = m;
        _a = a;
    }

    public PComplex(CComplex c)
    {
        _m = (float)Math.Sqrt(c._r * c._r + c._i * c._i);
        _a = (float)Math.Atan2(c._i, c._r);
    }

    public float GetReal()
    {
        return (float)(_m * Math.Cos(_a));
    }

    public float GetImaginary()
    {
        return (float)(_m * Math.Sin(_a));
    }

    public static PComplex Multiply(PComplex c1, PComplex c2)
    {
        return new PComplex(c1._m * c2._m, c1._a + c2._a);
    }

    public static PComplex Multiply(PComplex c, float d)
    {
        return new PComplex(c._m * d, c._a);
    }

    public static PComplex Divide(PComplex c1, PComplex c2)
    {
        return new PComplex(c1._m / c2._m, c1._a - c2._a);
    }

    public static PComplex Divide(PComplex c, float d)
    {
        return new PComplex(c._m / d, c._a);
    }

    public static PComplex Invert(PComplex c)
    {
        return new PComplex(1.0f / c._m, -c._a);
    }

    public static PComplex Normalize(PComplex c)
    {
        c._a += (c._m < 0 ? PI : 0);
        c._m *= (c._m < 0 ? -1 : 1);

        c._a = (float)Math.IEEERemainder(c._a, 2 * PI);
        c._a += (c._a < 0 ? 2 * PI : 0);

        return c;
    }
}
