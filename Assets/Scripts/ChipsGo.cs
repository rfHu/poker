using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using System;

public class ChipsGo : MonoBehaviour {
	public Text TextNumber;

	private Seat theSeat;

	void Awake() {
		RxSubjects.Deal.Subscribe((e) => {
			hideChips();
		}).AddTo(this);
	}

	public void SetChips(int chips) {
		TextNumber.text = chips.ToString();
	}

	public void Create(int value, Seat seat) {
		theSeat = seat;

		doTween().OnComplete(() => {
			SetChips(value);
			TextNumber.gameObject.SetActive(true);
		});

		theSeat.SeatPos.Subscribe((pos) => {
			GetComponent<RectTransform>().anchoredPosition = getVector(pos);
		}).AddTo(this);
	}

	public void AddMore(Action callback, Seat seat) {
		theSeat = seat;

		doTween().OnComplete(() => {
			Destroy(gameObject);	
			callback();
		});
	}

	private Tweener doTween() {
		var pos = theSeat.GetPos();

		return GetComponent<RectTransform>()
		.DOAnchorPos(getVector(pos), 0.4f);
	}

	private Vector2 getVector(SeatPosition pos) {
		var vector = new Vector2(80, 0);

		if (pos == SeatPosition.Right) {
			vector = new Vector2(-80, 0);
		} else if (pos == SeatPosition.Top) {
			vector = new Vector2(0, -120);
		}

		return vector;
	}

	private void hideChips() {
		transform.SetParent(G.Cvs.transform, true);
		
		var rect = GetComponent<RectTransform>();
		rect.DOAnchorPos(new Vector2(0, 250), 0.4f)
		.OnComplete(() => {
			Destroy(gameObject);
		});
	}
}
