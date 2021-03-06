﻿using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UI;
using UniRx;
using System.Collections;
using System.Collections.Generic;

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

	private Action onClickModal = null;
	private bool modal = true;
	private bool singleton = true;  
	private bool closeOnClick = true;
	private Color  modalColor = ModalHelper.DefaultColor;

	public Vector2 StartPosition;
	public Vector2 EndPosition;

	private static DOPopup instance;

	private bool hasShow = false;

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

	private void show() {
		showModalIfNeeded();

		var contentSizeFitter = GetComponent<ContentSizeFitter>();

		// 暂时兼容的写法
		if (contentSizeFitter != null && contentSizeFitter.enabled && Animate == AnimType.Up2Down) {
			// 移出屏幕外
			GetComponent<RectTransform>().anchoredPosition = new Vector2(-3000, 0);	

			Observable.TimerFrame(2, FrameCountType.EndOfFrame).Subscribe((_) => {
				animateIn();
			}).AddTo(this);
		} else {
			animateIn();
		}
	}

	private void animateIn() {
		autoFit();

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

	private void showModalIfNeeded() {
		if (modal) {
			modalHelper = ModalHelper.Create();

			modalHelper.Show(transform.parent, () => {
				if (onClickModal != null) {
					onClickModal();
				}

				if (closeOnClick) {
					Close();
				}			
			}, modalColor);				
			transform.SetAsLastSibling();
		}
	}

	public void ShowModal(Color color, Action onClickModal = null, bool closeOnClick = true) {
		this.modalColor = color;
		Show(onClickModal, true, true, closeOnClick, true);		
	}

	public void Show(Action onClickModal = null, bool modal = true, bool singleton = true, bool closeOnClick = true, bool _configModalColor = false) {
		this.onClickModal = onClickModal;
		this.modal = modal;
		this.singleton = singleton;
		this.closeOnClick = closeOnClick;

		transform.SetParent(G.DialogCvs, false);

		// 该字段只在内部使用
		if (!_configModalColor) {
			this.modalColor = ModalHelper.DefaultColor;
		}

		if (instance != null && instance != this && singleton) {
			instance.Close();
		}

		hasShow = true;
		show();
	}

	public Tween Hide() {
		if (!hasShow) {
			return null;
		}

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

	public void Close() {
		if (!hasShow) {
			return ;
		}

		var tween = Hide();	
		hasShow = false; // hide也要判断hasShow，所以逻辑要放到hide之后

		if (instance == this) {
			instance = null;
		}

		if (tween == null) {
			release();
		} else {
			tween.OnComplete(() => {
				release();
			});
		}
	}

	private void release() {
		if (PoolMan.IsSpawned(transform)) {
			PoolMan.Despawn(transform);
		} else if (!PoolMan.Contains(transform)) {
			Destroy(gameObject);
		}
	}

	private void hideModal() {
		if (modalHelper != null) {
			modalHelper.Despawn();
		}

		modalHelper = null;
	}
}
