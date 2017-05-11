using UnityEngine;
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
	private Player player;
	private Tweener tween;

	public void SetChips(int chips) {
		TextNumber.text = _.Num2CnDigit(chips);
	}

	public void Create(int value, Seat seat, Player player) {
		theSeat = seat;
		this.player = player;
		addEvents();

		MasterAudio.PlaySound("chip");
		doTween().OnComplete(() => {
			SetChips(value);
			TextNumber.gameObject.SetActive(true);
			
			theSeat.SeatPos.AsObservable().Subscribe((pos) => {
				GetComponent<RectTransform>().anchoredPosition = getVector(pos);
			}).AddTo(this);
		});
	}

	public void AddMore(Action callback, Seat seat, Player player) {
		theSeat = seat;
		this.player = player;
		addEvents();

		MasterAudio.PlaySound("chip");
		doTween().OnComplete(() => {
			Destroy(gameObject);	
			callback();
		});
	}

	public void Hide() {
		if (hided || this == null) {
			return ;
		}

		MasterAudio.PlaySound("hechip");

		hided = true;

		TextNumber.gameObject.SetActive(false);
		transform.SetParent(G.UICvs.transform, true);
		
		var rect = GetComponent<RectTransform>();
		rect.DOAnchorPos(new Vector2(-80, 250), 0.4f)
		.OnComplete(() => {
			Destroy(gameObject);
		});
	}

	private void addEvents() {
		player.PrChips.Subscribe((value) => {
			if (value != 0) {
				return ;
			}

			G.WaitSound(() => {
				Hide();
			});
		}).AddTo(this);
	}
	
	private Tweener doTween() {
		var pos = theSeat.GetPos();

		tween = GetComponent<RectTransform>()
		.DOAnchorPos(getVector(pos), 0.4f);

		return tween;
	}

	private Vector2 getVector(SeatPosition pos) {
		var vector = new Vector2(120, 20);

		if (pos == SeatPosition.Right) {
			vector = new Vector2(-120, 20);
		} else if (pos == SeatPosition.Top || pos == SeatPosition.TopLeft || pos == SeatPosition.TopRight) {
			vector = new Vector2(0, -180);
		} else if (pos == SeatPosition.Bottom) {
			vector = new Vector2(120, 0);
		}

		return vector;
	}
}
