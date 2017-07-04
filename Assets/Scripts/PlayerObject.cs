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
	public GameObject AvatarMask;
	public PlayerActGo PlayerAct;
	public Sprite[] ActSprites;
	public GameObject AllinGo;

	public Text ScoreLabel;
	public GameObject Countdown;
	public GameObject Circle;
	public GameObject AutoArea;
	public GameObject[] AutoOperas; 
	public GameObject[] Eyes; 

	private Transform OPTransform;
	private ChipsGo cgo; 
	private Player player;
    private float hideDuration = 0.3f; 
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

	public void MoveOut() {
		// @TODO: 未定位出什么错误，暂时这么处理
		if (this == null || AvatarMask == null) {
			return ;
		}

		activated = false;
		AvatarMask.SetActive(false);

		if (OPTransform != null) {
			PoolMan.Despawn(OPTransform);
		}

		Circle.SetActive(true); // 显示头像
		Avt.GetComponent<CircleMask>().Disable();
	}

	void OnDespawned()
	{
		if (OPTransform != null && GameData.Shared.MySeat == -1) {
			PoolMan.Despawn(OPTransform);
		}

        if (isSelf && GameData.Shared.MySeat == -1)
        {
            RxSubjects.Seating.OnNext(false);
        }

		if (turnCoroutine != null) {
			StopCoroutine(turnCoroutine);
		}

		disposables.Clear();
	}

	public void ShowPlayer(Player player, Transform parent) {
		Index = player.Index;
		Uid = player.Uid;

		this.player = player;

		if (isSelf) {
			RxSubjects.ChangeVectorsByIndex.OnNext(Index);
            RxSubjects.Seating.OnNext(true);
		} else {
			
		}

		if (GameData.Shared.InGame && !player.InGame) {
			setFolded();
		}


		GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
		transform.SetParent(parent, false);
		
		registerRxEvent();
	}

	public void AutoCheckOrFold() {
		toggleAutoBtns(0);	
	}

	public void AutoCall() {
		toggleAutoBtns(1);	
	}

	private void toggleAutoBtns(int index) {
		var valStr = player.Trust.SelectedFlag.Value;

		if (String.IsNullOrEmpty(valStr)) {
			valStr = "00";
		}
		
		var value = new System.Text.StringBuilder(valStr);

		value[index] = value[index] == '0' ? '1' : '0';
		value[index ^ 1] = '0';	

		player.Trust.SelectedFlag.Value = value.ToString();	
	}
	
	private void toggleEye(int index) {
		var value = new System.Text.StringBuilder(player.ShowCard.Value);

		// 这一手结束后，只能亮牌，不能关闭亮牌
		if (value[index] == '1' && gameover) {
			return ;
		}

		value[index] =  value[index] == '0' ? '1' : '0';

		player.ShowCard.Value = value.ToString();

		// 转换10进制
		var num = Convert.ToInt16(value.ToString(), 2); 

		// 发送请求
		Connect.Shared.Emit(new Dictionary<string, object> {
			{"f", "showcard"},
			{"args", new Dictionary<string, object> {
				{"showcard", num}
			}}
		});
	}

	public void ShowFirstCard() {
		toggleEye(0);	
	}

	public void ShowSecondCard() {
		toggleEye(1);
	}

	public void Fold() {
		MoveOut();

		if (isSelf) {
			// 图片灰掉
			darkenCards();
		} else {

		}

		setFolded();
	}

	private void darkenCards() {
		MyCards[0].GetComponent<Card>().Darken();
		MyCards[1].GetComponent<Card>().Darken();
	}

	

	private void setFolded() {
		if (isSelf) {
			darkenCards();	
		}

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

		// Gameover 应该清掉所有状态
		RxSubjects.GameOver.Subscribe((e) => {
			MoveOut();
			PlayerAct.gameObject.SetActive(false);
			AllinGo.SetActive(false);
			AutoArea.SetActive(false);
			gameover = true;

			if (cgo != null) {
			 	cgo.Hide();
			}
		}).AddTo(disposables);
		
		setupSelfEvents();		
	}

	private void setupSelfEvents() {
		if (!isSelf) {
			return ;
		}

		GameData.Shared.MaxFiveRank.Subscribe((value) => {
			var parent = CardDesc.transform.parent.gameObject;

			if (value == 0)
			{
				parent.SetActive(false);
				return;
			}

			parent.SetActive(true);
			CardDesc.text = Card.GetCardDesc(value);
		}).AddTo(disposables);

		player.ShowCard.Subscribe((value) => {
			if (value[0] == '1') {
				Eyes[0].SetActive(true);
			} else {
				Eyes[0].SetActive(false);
			}

			if (value[1] == '1') {
				Eyes[1].SetActive(true);
			} else {
				Eyes[1].SetActive(false);
			}
		}).AddTo(disposables);

		player.Trust.ShouldShow.Subscribe((show) => {
			AutoArea.SetActive(show);
		}).AddTo(disposables);

		player.Trust.CallNumber.Subscribe((num) => {
			var text = AutoOperas[1].transform.Find("Text").GetComponent<Text>();

			if (num == 0) {
				text.text = "自动让牌";
			} else if (num == -1) {
				text.text = "全下";
			} else if (num > 0) {
				text.text = String.Format("跟注\n<size=40>{0}</size>", _.Num2CnDigit<int>(num));
			}

			var flag = player.Trust.FlagString();
			if (flag == "01") {
				player.Trust.SelectedFlag.Value = "00";	
			}
		}).AddTo(disposables);

		player.Trust.SelectedFlag.Where((flags) => { return flags != null; }).Subscribe((flags) => {
			var ncolor = _.HexColor("#2196F300");
			var scolor = _.HexColor("#2196F3");
			
			var img0 = AutoOperas[0].GetComponent<ProceduralImage>();
			var img1 = AutoOperas[1].GetComponent<ProceduralImage>();

			if (flags[0] == '0') {
				img0.color = ncolor;
			} else {
				img0.color = scolor;
			}

			if (flags[1] == '0') {
				img1.color = ncolor;
			} else {
				img1.color = scolor;
			}
		}).AddTo(disposables);

		RxSubjects.Deal.Subscribe((_) => {
			player.Trust.Hide();
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
		go.GetComponent<CanvasGroup>().DOFade(0, hideDuration).OnComplete(() => {
			go.SetActive(false);

			if (callback != null) {
				callback();
			}
		});
	}

	private void turnTo(Dictionary<string, object> dict, int left, bool restore = false,int buyTimeCost = 10) {
		if (isSelf) {
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
	}

    IEnumerator turnCoroutine;

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
