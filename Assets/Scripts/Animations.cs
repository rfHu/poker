using UnityEngine;
using DG.Tweening;

public class Animations {
	public static void ShowPopup(GameObject target) {
		float duration = 0.2f;
		var canvasGroup = target.GetComponent<CanvasGroup>();
		var rectTransform = target.GetComponent<RectTransform>();

        // FadeIn 
        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1, duration);

        // 由小变大
        rectTransform.localScale = new Vector2((float)0.8, (float)0.8);
        rectTransform.DOScale(new Vector3(1, 1, 1), duration);
	} 

	public static void HidePopup(GameObject target) {
		float duration = 0.2f;
		var canvasGroup = target.GetComponent<CanvasGroup>();
		var rectTransform = target.GetComponent<RectTransform>();

		canvasGroup.DOFade(0, duration);
        rectTransform.DOScale(new Vector3((float)0.8, (float)0.8, 1), duration);
    }
}