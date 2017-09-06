using UnityEngine;
using System.Collections;
namespace RWDTools
{
    public class GradientToolUtility : MonoBehaviour
	{
        public static Texture2D GenerateRadialGradientTexture(GTRadialGradient GTRadialGradient)
        {
            return GenerateRadialGradientTexture(GTRadialGradient.RadialGradient, (int)GTRadialGradient.Size.x, (int)GTRadialGradient.Size.y, GTRadialGradient.FilterMode);
        }

        public static Texture2D GenerateRadialGradientTexture(RadialGradient radialGradient, int width, int height, FilterMode filterMode = FilterMode.Bilinear)
        {
            Material RadialMat = new Material(Shader.Find("GradientTool/S_RadialGradient"));
           
            RadialMat.SetVector("_Anchor", new Vector4(radialGradient.Anchor.x, radialGradient.Anchor.y, radialGradient.Scale, (float)width/(float)height));
            RadialMat.SetFloat("_UseHSV", (radialGradient.UseHSV ? 1 : 0));
            RadialMat.SetVector("_Inner", radialGradient.Inner);
            RadialMat.SetVector("_Outer", radialGradient.Outer);

            Texture2D inputTex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            RenderTexture rTex = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
            Graphics.Blit(inputTex, rTex, RadialMat);

            Texture2D returnTex = new Texture2D(rTex.width, rTex.height, TextureFormat.ARGB32, false);
            RenderTexture.active = rTex;

            returnTex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            returnTex.Apply();
            
            returnTex.filterMode = filterMode;

            RenderTexture.ReleaseTemporary(rTex);
            RenderTexture.active = null;
            
            return returnTex;
        }

        public static Texture2D GenerateQuadGradientTexture(GTQuadGradient GTQuadGradient)
		{
			return GenerateQuadGradientTexture(GTQuadGradient.QuadGradient,(int) GTQuadGradient.Size.x,(int) GTQuadGradient.Size.y, GTQuadGradient.FilterMode);
		}

		public static Texture2D GenerateQuadGradientTexture(QuadGradient quadGradient, int width, int height, FilterMode filterMode = FilterMode.Bilinear)
		{
			Texture2D newTex = new Texture2D(width, height);

			for (int x = 0; x < newTex.width; x++)
			{
				for (int y = 0; y < newTex.height; y++)
				{
					Vector2 normPosition = new Vector2((float)x / width, (float)y / height);
					Color topColor = Color.Lerp(quadGradient.TopLeft, quadGradient.TopRight, normPosition.x);
					Color botColor = Color.Lerp(quadGradient.BottomLeft, quadGradient.BottomRight, normPosition.x);
					newTex.SetPixel(x, y, Color.Lerp(botColor, topColor, normPosition.y));
				}
			}

			newTex.Apply();
			newTex.filterMode = filterMode;
			return newTex;
		}

		public static Texture2D GenerateLinearGradientTexture(GTLinearGradient GTLinearGradient)
		{
			return GenerateLinearGradientTexture(GTLinearGradient.LinearGradient,(int) GTLinearGradient.Size.x,(int) GTLinearGradient.Size.y, GTLinearGradient.FilterMode);
        }

		public static Texture2D GenerateLinearGradientTexture(LinearGradient linearGradient, int width, int height, FilterMode filterMode = FilterMode.Bilinear)
		{
			Texture2D newTex = new Texture2D(width, height);
			for (int x = 0; x < newTex.width; x++)
			{
				for (int y = 0; y < newTex.height; y++)
				{
					Vector2 normPosition = new Vector2((float)x / width, (float)y / height);
					normPosition = Rotate(normPosition - new Vector2(0.5f, 0.5f), (linearGradient.Angle-90)) + new Vector2(0.5f, 0.5f);
					newTex.SetPixel(x, y, linearGradient.Gradient.Evaluate(normPosition.x));
				}
			}

			newTex.Apply();

			newTex.filterMode = filterMode;

			return newTex;
		}

		public static Vector2 Rotate(Vector2 v, float degrees)
		{
			float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
			float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

			float tx = v.x;
			float ty = v.y;
			v.x = (cos * tx) - (sin * ty);
			v.y = (sin * tx) + (cos * ty);
			return v;
		}
	}
}