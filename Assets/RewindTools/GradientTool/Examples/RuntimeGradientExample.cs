using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using RWDTools;

// This is an example of how to use GradientTool at runtime to generate a gradient and apply it to a UI RawImage component.

[RequireComponent(typeof(RawImage))]
public class RuntimeGradientExample : MonoBehaviour
{
	[SerializeField]
	GTGradient GTGradient;

	Texture2D gradTexture;

	void OnEnable() 
	{
		RawImage rawImage = GetComponent<RawImage>();
        if (rawImage == null || GTGradient == null)
            return;

        if (GTGradient is GTLinearGradient)
        {
            gradTexture = GradientToolUtility.GenerateLinearGradientTexture((GTLinearGradient) GTGradient);
        }
        if (GTGradient is GTQuadGradient)
        {
            gradTexture = GradientToolUtility.GenerateQuadGradientTexture((GTQuadGradient)GTGradient);
        }
        if (GTGradient is GTRadialGradient)
        {
            gradTexture = GradientToolUtility.GenerateRadialGradientTexture((GTRadialGradient)GTGradient);
        }

        gradTexture.hideFlags = HideFlags.HideAndDontSave;

        rawImage.texture = gradTexture;
	}

	void OnDisable()
	{
		DestroyImmediate(gradTexture);
    } 

	[ContextMenu("Generate")]
	void Generate()
    {
		OnEnable();
    }

}
