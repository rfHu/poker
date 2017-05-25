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
		transform.SetParent(G.UICvs.transform, false);
		GameData.Shared.DealerSeat.AsObservable().Subscribe(subs).AddTo(this);
	} 

	private void subs(int value) {
		if (value < 0 || value > seats.Count - 1) {
			gameObject.SetActive(false);
			return ;
		}

		gameObject.SetActive(true);

		if (cancel != null) {
			cancel.Dispose();
		}

		var seat = seats[value].GetComponent<Seat>();
		transform.SetParent(seat.transform, true);
		transform.SetAsFirstSibling();

		cancel = seat.GetComponent<Seat>().SeatPos.Subscribe(
			(pos) => {
				setPosition(pos);
			}
		).AddTo(this);
	}

	private void setPosition(SeatPosition pos) {
		var y = -65;
		var x = 90;

		if (pos == SeatPosition.Right) {
			x =  -90;
		}

		GetComponent<RectTransform>().DOAnchorPos(new Vector2(x, y), 0.15f);
	}
}
