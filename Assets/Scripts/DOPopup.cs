using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UI;
using UniRx;
using System.Collections;

public enum AnimType {
	Up2Down,
	Left2Right,
	Right2Left,
	Popup,
	Custom,
	Down2Up,
}

[RequireComponent(typeof(CanvasGroup))]
public class DOPopup : MonoBehaviour {
	public AnimType Animate;

    float duration = 0.15f;

	private Vector2 startPosition; 
	private Vector2 endPosition;

	private ModalHelper modalHelper;   

	private Action close = null;
	private bool modal = true;
	private bool singleton = true;  

	public Vector2 StartPosition;
	public Vector2 EndPosition;

	private static DOPopup instance;

	void Awake() {}

	private void autoFit() {
		var rectTrans = GetComponent<RectTransform>();
        endPosition = new Vector2(0, 0);

		switch(Animate) {
			case AnimType.Up2Down:
				startPosition = new Vector2(0, rectTrans.rect.height);
				break;
			case AnimType.Down2Up:
				startPosition = new Vector2(0, -rectTrans.rect.height);
				break; 
			case AnimType.Left2Right:
				startPosition = new Vector2(-rectTrans.rect.width, 0);
				break;
			case AnimType.Right2Left:
				startPosition = new Vector2(rectTrans.rect.width, 0);
				break; 
			case AnimType.Custom:
				startPosition = StartPosition;
				endPosition = EndPosition;			
				break; 
			default:
				return;
		}
		
		rectTrans.anchoredPosition = startPosition;
	}

	void startAnimation() {
		// yield return new WaitForFixedUpdate();

		autoFit();
		transform.SetParent(G.DialogCvs.transform, false);

		if (modal) {
			modalHelper = ModalHelper.Create();

			modalHelper.Show(G.DialogCvs, () => {
				if (close != null) {
					close();
				}

				Close();
			});				
			transform.SetAsLastSibling();
		}

		switch(Animate) {
			case AnimType.Up2Down: case AnimType.Left2Right: case AnimType.Right2Left: case AnimType.Custom: case AnimType.Down2Up:
				GetComponent<RectTransform>().DOAnchorPos(endPosition, duration);
				break;
			case AnimType.Popup: 
				gameObject.Popup();
				break;
		}

		// 保存起来，singleton只允许出现一个
		if (singleton) {
			instance = this;
		}
	}
	
	public void Show(Action close = null, bool modal = true, bool singleton = true) {
		this.close = close;
		this.modal = modal;
		this.singleton = singleton;

		if (instance != null && instance != this && singleton) {
			instance.Close();
		}

	}

	void OnSpawned() {
		despawned = false;
		startAnimation();
	}

	public void Show() {
		this.Show(close, modal, singleton);
	}

	public Tween Hide() {
		hideModal();

        Tween tween = null;

		switch(Animate) {
			case AnimType.Up2Down: case AnimType.Left2Right: case AnimType.Right2Left: case AnimType.Custom: case AnimType.Down2Up:
				tween = GetComponent<RectTransform>().DOAnchorPos(startPosition, duration);
				break;
			case AnimType.Popup:
				tween = gameObject.Popup(false);
				break;
			default:
				break;
		}

		return tween;
	}

	private bool despawned = false;

	public void Close() {
		if (despawned) {
			return ;
		}

		despawned = true;

		var tween = Hide();	

		if (tween == null) {
			release();
		} else {
			tween.OnComplete(() => {
				release();
			});
		}
	}

	private void release() {
		PoolMan.Despawn(transform);
	}

	private void hideModal() {
		if (modalHelper != null) {
			modalHelper.Despawn();
		}
	}
}
