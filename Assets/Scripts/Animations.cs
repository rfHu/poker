using UnityEngine;
using DG.Tweening;

public static class Animations {
	public static Tween Popup(this GameObject gameObject, bool show = true) {
		float duration = 0.2f;
		var canvasGroup = gameObject.GetComponent<CanvasGroup>();
		var rectTransform = gameObject.GetComponent<RectTransform>();

		if (canvasGroup == null || rectTransform == null) {
			return null;
		}

		rectTransform.anchoredPosition = new Vector2(0, 0);

		Tween tween;

		if (show) {
            // FadeIn 
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1, duration);

            // 由小变大
            rectTransform.localScale = new Vector3((float)0.8, (float)0.8, 1);
            tween = rectTransform.DOScale(new Vector3(1, 1, 1), duration);
		} else {
			canvasGroup.DOFade(0, duration);
        	tween = rectTransform.DOScale(new Vector3((float)0.8, (float)0.8, 1), duration);
		}

		return tween;
	} 
}