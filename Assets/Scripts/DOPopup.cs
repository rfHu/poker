using UnityEngine;
using UIWidgets;
using DG.Tweening;

public enum AnimType {
	Up2Down,
	Left2Right,
	Right2Left
}

public class DOPopup : MonoBehaviour {
	public AnimType Animate;

    float duration = 0.15f;

	Vector2 beginPosition; 
	Vector2 endPosition;

	int modalKey;

	void Awake()
	{
		var rectTrans = GetComponent<RectTransform>();

		if (Animate == AnimType.Up2Down) {
			beginPosition = new Vector2(0, rectTrans.rect.height);
			endPosition = new Vector2(0, 0);
			rectTrans.anchoredPosition = beginPosition;
		} else if (Animate == AnimType.Left2Right) {

		} else if(Animate == AnimType.Right2Left) {

		}
	}

	public void Show(Canvas canvas) {
		transform.SetParent(canvas.transform, false);
		modalKey = ModalHelper.Open(this, null, new Color(0, 0, 0, 0), Close);
		transform.SetAsLastSibling();
		GetComponent<RectTransform>().DOAnchorPos(endPosition, duration);
	}

	public void  Close() {
		ModalHelper.Close(modalKey);
		GetComponent<RectTransform>().DOAnchorPos(beginPosition, duration).OnComplete(() => {
			Destroy(gameObject);
		});
	}
}
