using UnityEngine;

namespace Utilities
{
    public static class ColorUtilities
    {
        public static Vector3 ToVector3(this Color c)
        {
            return new Vector3(c.r, c.g, c.b);
        }
        
        public static Vector4 ToVector4(this Color c)
        {
            return new Vector4(c.r, c.g, c.b, c.a);
        }

        public static Color ToColor(this Vector3 v)
        {
            return new Color(v.x, v.y, v.z, 1);
        }
        
        public static Color ToColor(this Vector4 v)
        {
            return new Color(v.x, v.y, v.z, v.w);
        }

        public static Color Intensify(this Color c, float intensity)
        {
            return (c.ToVector3() * Mathf.Pow(2,intensity)).ToColor();
        }

        public static Color AddOffset(this Color c, Vector3 offset)
        {
            return (c.ToVector3() + offset).ToColor();
        }
        
        public static Color AddOffset(this Color c, Vector4 offset)
        {
            return (c.ToVector4() + offset).ToColor();
        }
        
        public static Color AddOffset(this Color c, float offset)
        {
            return (c.ToVector3() + Vector3.one*offset).ToColor();
        }
    }
}