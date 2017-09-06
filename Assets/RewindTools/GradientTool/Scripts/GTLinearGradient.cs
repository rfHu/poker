using UnityEngine;
using System.Collections;

namespace RWDTools
{
    [System.Serializable]
    public struct LinearGradient
    {
        public static LinearGradient Default {
            get {
                LinearGradient defaultGradient = new LinearGradient(new Gradient(), 0);
                defaultGradient.Gradient.colorKeys = new GradientColorKey[2];
                defaultGradient.Gradient.colorKeys[0].color = Color.black;
                defaultGradient.Gradient.colorKeys[0].time = 0;
                defaultGradient.Gradient.colorKeys[1].color = Color.white;
                defaultGradient.Gradient.colorKeys[1].time = 1;
                defaultGradient.Gradient.alphaKeys = new GradientAlphaKey[2];
                defaultGradient.Gradient.alphaKeys[0].alpha = 1;
                defaultGradient.Gradient.alphaKeys[0].time = 0;
                defaultGradient.Gradient.alphaKeys[1].alpha = 1;
                defaultGradient.Gradient.alphaKeys[1].time = 1;
                return defaultGradient;
            }
        }

        public Gradient Gradient;
        public float Angle;

        public LinearGradient(Gradient gradient, float angle)
        {
            Gradient = gradient;
            Angle = angle;
        }
    }

	public class GTLinearGradient : GTGradient
	{
        public LinearGradient LinearGradient;

        public GTLinearGradient()
		{
		}
	}
}