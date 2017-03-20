using UnityEngine;
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


	void Awake() {
		RxSubjects.Deal.Subscribe((e) => {
			G.waitSound(() => {
				if (this == null) {
					return ;
				}

				Hide();
			});			
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
		var vector = new Vector2(80, 0);

		if (pos == SeatPosition.Right) {
			vector = new Vector2(-80, 0);
		} else if (pos == SeatPosition.Top) {
			vector = new Vector2(0, -120);
		}

		return vector;
	}
}
