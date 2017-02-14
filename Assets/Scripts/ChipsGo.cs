﻿using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using System;

public class ChipsGo : MonoBehaviour {
	public Text TextNumber;

	private bool isLeft;

	void Awake() {
		RxSubjects.Deal.Subscribe((e) => {
			hideChips();
		}).AddTo(this);
	}

	public void SetChips(int chips) {
		TextNumber.text = chips.ToString();
	}

	public void Create(int value, bool isLeft) {
		this.isLeft = isLeft;

		doTween().OnComplete(() => {
			SetChips(value);
			TextNumber.gameObject.SetActive(true);
		});
	}

	public void AddMore(Action callback, bool isLeft) {
		this.isLeft = isLeft;

		doTween().OnComplete(() => {
			Destroy(gameObject);	
			callback();
		});
	}

	private Tweener doTween() {
		var vector = isLeft ? new Vector2(80, 0) : new Vector2(-80, 0);

		return GetComponent<RectTransform>()
		.DOAnchorPos(vector, 0.4f);
	}

	void hideChips() {
		transform.SetParent(G.Cvs.transform, true);
		
		var rect = GetComponent<RectTransform>();
		rect.DOAnchorPos(new Vector2(0, 0), 0.4f)
		.OnComplete(() => {
			Destroy(gameObject);
		});
	}
}
