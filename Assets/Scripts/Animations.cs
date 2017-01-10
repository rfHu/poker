using UnityEngine;
using DG.Tweening;

public class Animations {
	public static void popup(GameObject target) {
		float duration = 1f;
		var canvasGroup = target.GetComponent<CanvasGroup>();

		// FadeIn 
		canvasGroup.alpha = 0;
		canvasGroup.DOFade(1, duration);
		
		// 由小变大
		var rectTransform = target.GetComponent<RectTransform>();
		rectTransform.localScale = new Vector2((float)0.8, (float)0.8);
		rectTransform.DOScale(new Vector3(1, 1, 1), duration);
	} 
}