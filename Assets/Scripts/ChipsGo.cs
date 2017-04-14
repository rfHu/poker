﻿using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using System;
using DarkTonic.MasterAudio;
using System.Linq;
using Extensions;

public class ChipsGo : MonoBehaviour {
	public Text TextNumber;

	private Seat theSeat;
	private bool hided = false;


	void Awake() {
		RxSubjects.Deal.AsObservable().Where((e) => {
			return e.Data.Dict("deals").ContainsKey("-1");
		}).Subscribe((e) => {
			Hide();
		}).AddTo(this);
	}

	public void SetChips(int chips) {
		TextNumber.text = _.Num2Text(chips);
	}

	public void Create(int value, Seat seat) {
		theSeat = seat;

		MasterAudio.PlaySound("chip");
		doTween().OnComplete(() => {
			SetChips(value);
			TextNumber.gameObject.SetActive(true);
			
			theSeat.SeatPos.AsObservable().Subscribe((pos) => {
				GetComponent<RectTransform>().anchoredPosition = getVector(pos);
			}).AddTo(this);
		});
	}

	public void AddMore(Action callback, Seat seat) {
		theSeat = seat;

		MasterAudio.PlaySound("chip");
		doTween().OnComplete(() => {
			Destroy(gameObject);	
			callback();
		});
	}

	public void Hide() {
		if (hided) {
			return ;
		}

		MasterAudio.PlaySound("hechip");

		hided = true;

		TextNumber.gameObject.SetActive(false);
		transform.SetParent(G.UICvs.transform, true);
		
		var rect = GetComponent<RectTransform>();
		rect.DOAnchorPos(new Vector2(-70, 250), 0.4f)
		.OnComplete(() => {
			Destroy(gameObject);
		});
	}
	
	private Tweener doTween() {
		var pos = theSeat.GetPos();

		return GetComponent<RectTransform>()
		.DOAnchorPos(getVector(pos), 0.4f);
	}

	private Vector2 getVector(SeatPosition pos) {
		var vector = new Vector2(120, 0);

		if (pos == SeatPosition.Right) {
			vector = new Vector2(-120, 0);
		} else if (pos == SeatPosition.Top || pos == SeatPosition.TopLeft || pos == SeatPosition.TopRight) {
			vector = new Vector2(0, -180);
		}

		return vector;
	}
}
