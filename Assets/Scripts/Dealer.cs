using System.Collections;
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
		GameData.Shared.DealerSeat.AsObservable().Where((value) => value >= 0).Subscribe(subs).AddTo(this);
	} 

	private void subs(int value) {
		gameObject.SetActive(true);
		setPosition(value);

		if (cancel != null) {
			cancel.Dispose();
		}

		cancel = seats[value].GetComponent<Seat>().SeatPos.Subscribe(
			(pos) => {
				setPosition(pos);
			}
		).AddTo(this);
	}

	private void setPosition(int index) {
		var seat = seats[index].GetComponent<Seat>();
		var pos = seat.GetPos();
		setPosition(pos);	
	}

	private void setPosition(SeatPosition pos) {
		var position = gameObject.GetComponent<RectTransform>().anchoredPosition;
		var y = position.y - 45;
		var x = position.x + 70;

		if (pos == SeatPosition.Right) {
			x =  position.x - 70;
		}

		GetComponent<RectTransform>().DOAnchorPos(new Vector2(x, y), 0.15f);
	}
}
