using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using System;

public class ChipsGo : MonoBehaviour {
	public Text TextNumber;

	void Awake() {
		RxSubjects.Deal.Subscribe((e) => {
			hideChips();
		}).AddTo(this);
	}

	public void SetChips(int chips) {
		TextNumber.text = chips.ToString();
	}

	public void Create(int value) {
		doTween().OnComplete(() => {
			SetChips(value);
		});
	}

	public void AddMore(Action callback) {
		TextNumber.enabled = false;
		doTween().OnComplete(() => {
			Destroy(gameObject);	
			callback();
		});
	}

	private Tweener doTween() {
		return GetComponent<RectTransform>()
		.DOAnchorPos(new Vector2(80, 0), 0.4f);
	}

	void hideChips() {
		var canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
		transform.SetParent(canvas.transform, true);
		
		var rect = GetComponent<RectTransform>();
		rect.DOAnchorPos(new Vector2(0, 0), 0.4f)
		.OnComplete(() => {
			Destroy(gameObject);
		});
	}
}
