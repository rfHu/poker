using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using System;
using System.Linq;

public class ChipsGo : MonoBehaviour {
	public Text TextNumber;

	private Seat theSeat;
	private bool hided = false;
	private Player player;
	private Tweener tween;

	void OnDespawned() {
		this.Dispose();	
		TextNumber.gameObject.SetActive(false);
	}

	void OnSpawned() {
		hided = false;
		GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
		GetComponent<Image>().color = new Color(1, 1, 1);
	}

	public void SetChips(int chips) {
		TextNumber.text = _.Num2CnDigit(chips);
	}

	public void Create(Seat seat, Player player) {
		theSeat = seat;
		this.player = player;
		addEvents();

		TweenCallback cb = () => {
			TextNumber.gameObject.SetActive(true);
			
			theSeat.SeatPos.AsObservable().Subscribe((pos) => {
				GetComponent<RectTransform>().anchoredPosition = getVector(pos);
			}).AddTo(this);
		};


		if (player.ChipsChange)  {
			G.PlaySound("chip");

			if (getPrev() != null) {
				doTween().OnComplete(() => {
					PoolMan.Despawn(transform);
				});
			} else {
				doTween().OnComplete(cb);
			}
		}  else {
			GetComponent<RectTransform>().anchoredPosition = getVector();
			cb();
		}
	}

	private ChipsGo getPrev() {
		var sbIndex = transform.GetSiblingIndex();
		return transform.parent.GetChild(sbIndex - 1).GetComponent<ChipsGo>();
	}

	public void Hide() {
		if (hided || !PoolMan.IsSpawned(transform)) {
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
			PoolMan.Despawn(transform);
		});
	}

	private void addEvents() {
		player.PrChips.Subscribe((value) => {
			if (value == 0) {
				Hide();
			} else {
				SetChips(value);
			}
		}).AddTo(this);

		player.Destroyed.Subscribe((flag) => {
			if (flag) {
				PoolMan.Despawn(transform);
			}
		}).AddTo(this);
	}
	
	private Tweener doTween() {
		var pos = theSeat.GetPos();

		tween = GetComponent<RectTransform>()
		.DOAnchorPos(getVector(pos), 0.3f);

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
