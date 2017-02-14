using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using System;

public class ChipsGo : MonoBehaviour {
	public Text TextNumber;

	private SeatPosition pos;

	void Awake() {
		RxSubjects.Deal.Subscribe((e) => {
			hideChips();
		}).AddTo(this);
	}

	public void SetChips(int chips) {
		TextNumber.text = chips.ToString();
	}

	public void Create(int value, SeatPosition pos) {
		this.pos = pos;

		doTween().OnComplete(() => {
			SetChips(value);
			TextNumber.gameObject.SetActive(true);
		});
	}

	public void AddMore(Action callback, SeatPosition pos) {
		this.pos = pos;

		doTween().OnComplete(() => {
			Destroy(gameObject);	
			callback();
		});
	}

	private Tweener doTween() {
		var vector = new Vector2(80, 0);

		if (pos == SeatPosition.Right) {
			vector = new Vector2(-80, 0);
		} else if (pos == SeatPosition.Top) {
			vector = new Vector2(0, -120);
		}

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
