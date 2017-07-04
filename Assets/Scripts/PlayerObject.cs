using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.UI.ProceduralImage;
using System;
using DG.Tweening;
using UniRx;
using DarkTonic.MasterAudio;
using System.Linq;
using SimpleJSON;

public class PlayerObject : MonoBehaviour {
	public int Index;
	public GameObject WinImageGo;
	public GameObject Avt;
	public bool activated = false;
	public string Uid = "";
	public GameObject Cardfaces;
	public List<Transform> MyCards;
    public GameObject WinStars;
	public Text WinNumber;
	public PlayerActGo PlayerAct;
	public Sprite[] ActSprites;
	public GameObject AllinGo;

	public Text ScoreLabel;
	public GameObject Countdown;
	public GameObject Circle;
	public GameObject AutoArea;
	public GameObject[] AutoOperas; 
	

	private Transform OPTransform;
	private ChipsGo cgo; 
	private Player player;
	private ActionState lastState;
	private bool gameover = false;

	public Text CardDesc;
	

	private CompositeDisposable disposables = new CompositeDisposable();

	void OnSpawned() {
		Countdown.SetActive(false);
		Avt.GetComponent<CanvasGroup>().alpha = 1;	
		Avt.GetComponent<CircleMask>().Disable();
		Cardfaces.GetComponent<Image>().color = new Color(1, 1, 1);
		Cardfaces.GetComponent<RectTransform>().anchoredPosition = new Vector2(40, -20);
		AutoArea.SetActive(false);
		MyCards[0].parent.gameObject.SetActive(false);
	}

	public void SeeCard(List<int> cards) {
		MyCards[0].parent.gameObject.SetActive(true);

		var state = player.SeeCardAnim;

		if (state) {
			MyCards[0].GetComponent<Card>().ShowWithSound(cards[0], state);

			Observable.Timer(TimeSpan.FromSeconds(0.3)).Subscribe((_) => {
				MyCards[1].GetComponent<Card>().ShowWithSound(cards[1], state);
			}).AddTo(disposables);
		} else {
			MyCards[0].GetComponent<Card>().Show(cards[0], state);
			MyCards[1].GetComponent<Card>().Show(cards[1], state);
		}
	}

	// void OnDespawned()
	// {
	// 	if (OPTransform != null && GameData.Shared.MySeat == -1) {
	// 		PoolMan.Despawn(OPTransform);
	// 	}

    //     if (isSelf && GameData.Shared.MySeat == -1)
    //     {
    //         RxSubjects.Seating.OnNext(false);
    //     }
	// }

	public void ShowPlayer(Player player, Transform parent) {
		
	}
	


	

	private bool isSelf {
		get {
			return Uid == GameData.Shared.Uid;
		}
	}

	private void registerRxEvent() {
		player.Cards.AsObservable().Where((cards) => {
			if (cards != null && cards.Count == 2) {
				return cards[0] > 0 && cards[1] > 0;
			}

			return false;
		}).Subscribe((cards) => {
			if (isSelf) {
				SeeCard(cards);
			} else {
				// showTheCards(cards, player.SeeCardAnim);
			}
		}).AddTo(disposables);

		player.OverData.AsObservable().Where((data) => data != null).Subscribe((data) => {
			var gain = data.Gain();
			if (gain > 0) {
                WinStars.SetActive(true);

				if (isSelf) {
					WinImageGo.SetActive(true);
				}
			}

			// 收回大于0，展示盈亏
			if (data.prize > 0) {
				WinNumber.transform.parent.gameObject.SetActive(true); 
				WinNumber.text = _.Number2Text(gain);
				ScoreLabel.transform.parent.gameObject.SetActive(false);
			}

			if (!isSelf) {
				// showTheCards(data.cards, true);
				// showCardType(data.maxFiveRank);
			}

			// 4s后隐藏动画
			Invoke("hideAnim", 4);			
		}).AddTo(disposables);

	}

	private void hideAnim() {
		hideGo(WinStars, () => {
			ScoreLabel.transform.parent.gameObject.SetActive(true);
		});
		hideGo(WinImageGo);	
		// hideGo(getShowCard());	
		// hideGo(getOtherCardGo());
		hideGo(WinNumber.transform.parent.gameObject);
	}

	private void hideGo(GameObject go, Action callback = null) {
		if (!go.activeSelf) {
			return ;
		}
		go.GetComponent<CanvasGroup>().DOFade(0, 0.3f).OnComplete(() => {
			go.SetActive(false);

			if (callback != null) {
				callback();
			}
		});
	}

	private void turnTo(Dictionary<string, object> dict, int left, bool restore = false,int buyTimeCost = 10) {
			showOP(dict, left, buyTimeCost);

			var flag = player.Trust.FlagString();
			var callNum = player.Trust.CallNumber.Value;

			if (flag == "10") { // 选中左边
				PoolMan.Despawn(OPTransform);	
				var check = dict.Dict("cmds").Bool("check");

				if (check) {
					OP.OPS.Check();
 				} else {
					OP.OPS.Fold();
				 }
			} else if (flag == "01") { // 选中右边
				var data = dict.Dict("cmds");
				var call = data.Int("call");
				var check = data.Bool("check");

				if (callNum == -1 || (callNum == 0 && check) || (callNum == call && call != 0)) {
					PoolMan.Despawn(OPTransform);

					if (callNum == 0) {
						OP.OPS.Check();
					} else if (callNum == -1) {
						OP.OPS.AllIn();
					} else {
						OP.OPS.Call();
					}
				} else {
					if (!restore) {
						G.PlaySound("on_turn");
					}
				}
			} else {
				if (!restore) {
					G.PlaySound("on_turn");
				}
			}
			
			player.Trust.SelectedFlag.Value = "00";
	}

	private OP showOP(Dictionary<string, object> data, int left, int buyTimeCost = 10) {
		if (this == null) {
			return null;
		}
		
		// 隐藏头像
		Circle.SetActive(false);

		OPTransform = OP.Spawn();
		var op = OPTransform.GetComponent<OP>();
		op.StartWithCmds(data, left, buyTimeCost);

		return op;
	}
}
