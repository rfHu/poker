using UnityEngine;
using UIWidgets;
using DG.Tweening;

public enum AnimType {
	Up2Down,
	Left2Right,
	Right2Left,
	Popup
}

public class DOPopup : MonoBehaviour {
	public AnimType Animate;

    float duration = 0.15f;

	Vector2 beginPosition; 
	Vector2 endPosition;

	int modalKey;

	void Start()
	{
		var rectTrans = GetComponent<RectTransform>();

		// @TODO: 处理ContentSizeFitter

		switch(Animate) {
			case AnimType.Up2Down:
				beginPosition = new Vector2(0, rectTrans.rect.height);
				endPosition = new Vector2(0, 0);
				rectTrans.anchoredPosition = beginPosition;
				break;
			default:
				break;
		}
	}

	public void Show(Canvas canvas) {
		transform.SetParent(canvas.transform, false);
		modalKey = ModalHelper.Open(this, null, new Color(0, 0, 0, 0), Close);
		transform.SetAsLastSibling();
        
		switch(Animate) {
			case AnimType.Up2Down: 
				GetComponent<RectTransform>().DOAnchorPos(endPosition, duration);
				break;
			case AnimType.Popup: 
				gameObject.Popup();
				break;
		}
	}

	public void Close() {
		ModalHelper.Close(modalKey);
		Tween tween = null;

		switch(Animate) {
			case AnimType.Up2Down:
				tween = GetComponent<RectTransform>().DOAnchorPos(beginPosition, duration);
				break;
			case AnimType.Popup:
				tween = gameObject.Popup(false);
				break;
			default:
				break;
		}

		if (tween == null) {
			Destroy(gameObject);
		} else {
			tween.OnComplete(() => {
				Destroy(gameObject);
			});
		}
	}
}
