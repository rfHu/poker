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

	bool show = false;

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
	
	public void Show(Action close = null, bool modal = true) {
		if (show) {
			return ;
		}

		show = true;

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
