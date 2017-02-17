using System.Collections.Generic;
using UnityEngine;
using UniRx;
using DG.Tweening;
using System;

public class Dealer : MonoBehaviour {
	private List<GameObject> seats;
	private IDisposable cancel;

	void Awake()
	{
		gameObject.SetActive(false);	
	}

	public void Init(List<GameObject> seats) {
		this.seats = seats;
		transform.SetParent(G.Cvs.transform, false);
		GameData.Shared.DealerSeat.AsObservable().Subscribe(subs).AddTo(this);
	} 

	private void subs(int value) {
		if (value < 0) {
			gameObject.SetActive(false);
			return ;
		}

		gameObject.SetActive(true);

		if (cancel != null) {
			cancel.Dispose();
		}

		var seat = seats[value].GetComponent<Seat>();
		cancel = seat.GetComponent<Seat>().SeatPos.Subscribe(
			(pos) => {
				var anchor = seat.GetComponent<RectTransform>().anchoredPosition;
				setPosition(pos, anchor);
			}
		).AddTo(this);
	}

	private void setPosition(SeatPosition pos, Vector2 position) {
		var y = position.y - 45;
		var x = position.x + 70;

		if (pos == SeatPosition.Right) {
			x =  position.x - 70;
		}

		GetComponent<RectTransform>().DOAnchorPos(new Vector2(x, y), 0.15f);
	}
}
