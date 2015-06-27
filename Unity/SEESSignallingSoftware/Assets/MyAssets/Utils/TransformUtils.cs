using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public static class TransformUtils
{
    public static Vector3 TransformFOV(Vector3 point, float fov)
    {
        Vector3 result = point;

        result.x *= Mathf.Tan(Mathf.Deg2Rad * fov);
        result.y *= Mathf.Tan(Mathf.Deg2Rad * fov);

        return result;
    }
}
