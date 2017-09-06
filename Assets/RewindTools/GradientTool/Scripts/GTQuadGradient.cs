using UnityEngine;
using System.Collections;

namespace RWDTools
{
    [System.Serializable]
    public struct QuadGradient
    {
        public static QuadGradient Default { get { return new QuadGradient(Color.white, Color.black, Color.white, Color.black); } }

        public Color TopLeft;
        public Color TopRight;
        public Color BottomLeft;
        public Color BottomRight;

        public QuadGradient(Color topLeft, Color topRight, Color bottomLeft, Color bottomRight)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
        }
    }

    public class GTQuadGradient : GTGradient
	{
		public QuadGradient QuadGradient;

		public GTQuadGradient()
		{
		}
	}
}
