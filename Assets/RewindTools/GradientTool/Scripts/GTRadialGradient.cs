using UnityEngine;
using System.Collections;

namespace RWDTools
{
    [System.Serializable]
    public struct RadialGradient
    {
        public static RadialGradient Default {  get { return new RadialGradient(Color.white, Color.black, Vector2.zero, 1, false);  } }

        public Color Inner;
        public Color Outer;
        public Vector2 Anchor;
        public float Scale;
        public bool UseHSV;

        public RadialGradient(Color inner, Color outer, Vector2 anchor, float scale, bool useHSV)
        {
            Inner = inner;
            Outer = outer;
            Anchor = anchor;
            Scale = scale;
            UseHSV = useHSV;
        }
    }

    public class GTRadialGradient : GTGradient
    {
        public RadialGradient RadialGradient;

        public GTRadialGradient()
        {
        }
    }
}

