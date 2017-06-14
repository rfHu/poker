﻿using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using System;
using DarkTonic.MasterAudio;
using System.Linq;

public class ChipsGo : MonoBehaviour {
	public Text TextNumber;

	private Seat theSeat;
	private bool hided = false;
	private Player player;
	private Tweener tween;

	public void SetChips(int chips) {
		TextNumber.text = _.Num2CnDigit(chips);
	}

	public void Create(int value, Seat seat, Player player) {
		theSeat = seat;
		this.player = player;
		addEvents();

		TweenCallback cb = () => {
			SetChips(value);
			TextNumber.gameObject.SetActive(true);
			
			theSeat.SeatPos.AsObservable().Subscribe((pos) => {
				GetComponent<RectTransform>().anchoredPosition = getVector(pos);
			}).AddTo(this);
		};

		if (player.ChipsChange) {
			G.PlaySound("chip");
			doTween().OnComplete(cb);
		} else {
			GetComponent<RectTransform>().anchoredPosition = getVector();
			cb();
		}
	}

	public void AddMore(Action callback, Seat seat, Player player) {
		theSeat = seat;
		this.player = player;
		addEvents();

		G.PlaySound("chip");
		doTween().OnComplete(() => {
			Destroy(gameObject);	
			callback();
		});
	}

	public void Hide() {
		if (hided || this == null) {
			return ;
		}

		hided = true;

		G.PlaySound("hechip");

		TextNumber.gameObject.SetActive(false);
		transform.SetParent(G.UICvs.transform, true);
		
		var duration = 0.4f;

		GetComponent<Image>().DOFade(0.3f, duration);
		GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, 350), duration)
		.OnComplete(() => {
			Destroy(gameObject);
		});
	}

	private void addEvents() {
		player.PrChips.Subscribe((value) => {
			if (value != 0) {
				return ;
			}

			Hide();
		}).AddTo(this);

		RxSubjects.Look.Subscribe((_) => {
			Destroy(gameObject);
		}).AddTo(this);
	}
	
	private Tweener doTween() {
		var pos = theSeat.GetPos();

		tween = GetComponent<RectTransform>()
		.DOAnchorPos(getVector(pos), 0.25f);

		return tween;
	}

	private Vector2 getVector(SeatPosition pos) {
		var vector = new Vector2(136, 20);

		if (pos == SeatPosition.Right) {
			vector = new Vector2(-136, 20);
		} else if (pos == SeatPosition.Top || pos == SeatPosition.TopLeft || pos == SeatPosition.TopRight) {
			vector = new Vector2(0, -180);
		} else if (pos == SeatPosition.Bottom) {
			vector = new Vector2(136, 0);
		}

		return vector;
	}

	private Vector2 getVector() {
		return getVector(theSeat.GetPos());
	}
}
