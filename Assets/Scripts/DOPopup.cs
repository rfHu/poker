using UnityEngine;
using DG.Tweening;
using System;

public enum AnimType {
	Up2Down,
	Left2Right,
	Right2Left,
	Popup
}

[RequireComponent(typeof(CanvasGroup))]
public class DOPopup : MonoBehaviour {
	public AnimType Animate;

    float duration = 0.15f;

	Vector2 beginPosition; 
	Vector2 endPosition;

	private ModalHelper modal;   

	bool hasShow = false;

	void Awake()
	{
		var rectTrans = GetComponent<RectTransform>();

		// @TODO: 处理ContentSizeFitter

		switch(Animate) {
			case AnimType.Up2Down:
				beginPosition = new Vector2(0, rectTrans.rect.height);
				break;
			case AnimType.Left2Right:
				beginPosition = new Vector2(-rectTrans.rect.width, 0);
				break;
			case AnimType.Right2Left:
				beginPosition = new Vector2(rectTrans.rect.width, 0);
				break; 
			default:
				return;
		}

		endPosition = new Vector2(0, 0);
		rectTrans.anchoredPosition = beginPosition;
	}

	private static DOPopup instance;
	
	public void Show(Action close = null, bool modal = true, bool singleton = true) {
		if (hasShow) {
			return ;
		}

		if (instance != null && singleton) {
			instance.Close();
		}

		hasShow = true;

		transform.SetParent(G.DialogCvs.transform, false);

		if (modal) {
			this.modal = ModalHelper.Create();

			this.modal.Show(G.DialogCvs, () => {
				if (close != null) {
					close();
				}

				Close();
			});				
			transform.SetAsLastSibling();
		}

		switch(Animate) {
			case AnimType.Up2Down: case AnimType.Left2Right: case AnimType.Right2Left:
				GetComponent<RectTransform>().DOAnchorPos(endPosition, duration);
				break;
			case AnimType.Popup: 
				gameObject.Popup();
				break;
		}

		gameObject.SetActive(true);

		// 保存起来，singleton只允许出现一个
		if (singleton) {
			instance = this;
		}
	}

	public void ImmediateClose() {
		hideModal();
		Destroy(gameObject);
	}

	public void Close() {
		hideModal();

		Tween tween = null;

		switch(Animate) {
			case AnimType.Up2Down: case AnimType.Left2Right: case AnimType.Right2Left:
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

	private void hideModal() {
		if (modal != null) {
			modal.Hide();
		}
	}
}
