using UnityEngine;
using System.Collections;

public class RuntimeLazyLoadExample : MonoBehaviour
{
	// Use this pattern to ensure gradients are only generated once, and as needed.

	Texture2D gradient;
	Texture2D Gradient
	{
		get {
			if (gradient == null)
			{
				gradient = RWDTools.GradientToolUtility.GenerateLinearGradientTexture(GTLinearGradient);
            }
			return gradient; }
		set {
			gradient = value; }
	}

	public RWDTools.GTLinearGradient GTLinearGradient;


	}
