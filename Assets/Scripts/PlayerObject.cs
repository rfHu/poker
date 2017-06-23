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
	public GameObject MyCards;
	public GameObject Stars;
    public GameObject WinNormal;
    public GameObject Win27;
	public Text WinNumber;
	public List<Card> ShowCards;
	public GameObject AvatarMask;
	public PlayerActGo PlayerAct;
	public Sprite[] ActSprites;
	public GameObject AllinGo;

	public Text NameLabel;
	public Text ScoreLabel;
	public GameObject Countdown;
	public GameObject Circle;
	public GameObject AutoArea;
	public GameObject[] AutoOperas; 
	public GameObject[] Eyes; 
	public GameObject BackGameBtn;

	private GameObject OPGo;
	private ChipsGo cgo; 
	private Player player;
	private float foldOpacity = 0.6f;
	private float animDuration = 0.5f;
    private float hideDuration = 0.3f; 
	private ActionState lastState;
	private bool gameover = false;
	private int actCardsNumber = 0;

	public Text CardDesc;
	public Text OthersCardDesc;

	public SpkTextGo SpkText;
	public GameObject Volume;
	public GameObject HandGo;
	public Text StateLabel;

	private Seat theSeat {
		get {
			return  transform.parent.GetComponent<Seat>();
		}
	}

	void Awake() {
		// 倒计时隐藏
		Countdown.SetActive(false);
	}

	public void SeeCard(List<int> cards) {
		var first = MyCards.transform.Find("First");
		var second = MyCards.transform.Find("Second");

		MyCards.SetActive(true);

		var state = player.SeeCardAnim;

		if (state) {
			first.GetComponent<Card>().ShowWithSound(cards[0], state);

			Observable.Timer(TimeSpan.FromSeconds(0.3)).Subscribe((_) => {
				second.GetComponent<Card>().ShowWithSound(cards[1], state);
			}).AddTo(this);
		} else {
			first.GetComponent<Card>().Show(cards[0], state);
			second.GetComponent<Card>().Show(cards[1], state);
		}
	}

	public void MoveOut() {
		// @TODO: 未定位出什么错误，暂时这么处理
		if (this == null || AvatarMask == null) {
			return ;
		}

		activated = false;
		AvatarMask.SetActive(false);

		if (OPGo != null) {
			Destroy(OPGo);
			OPGo = null;
		}

		Circle.SetActive(true); // 显示头像
		Avt.GetComponent<CircleMask>().Disable();
	}

	public void BackGame() {
		Connect.Shared.Emit(new Dictionary<string, object>{
			{"f", "ready"}
		});
	}

	void OnDestroy()
	{
		if (OPGo != null) {
			Destroy(OPGo);
		}

		if (SpkText != null) {
			Destroy(SpkText);
		}

		// OnDestory是异步的，存在时序的问题，所以要判断用户是否还在座位中
        if (isSelf() && GameData.Shared.MySeat == -1)
        {
            RxSubjects.Seating.OnNext(false);
        }
	}

	public void ShowPlayer(Player player, Transform parent) {
		Index = player.Index;
		Uid = player.Uid;
		SpkText.Uid = Uid;

		this.player = player;

		// 头像点击事件
		Avt.GetComponent<Avatar>().Uid = Uid;

		if (isSelf()) {
			NameLabel.gameObject.SetActive(false);
			RxSubjects.ChangeVectorsByIndex.OnNext(Index);
            RxSubjects.Seating.OnNext(true);
		} else if(player.InGame) { 
			Cardfaces.SetActive(true);
			NameLabel.gameObject.SetActive(true);
		}

		if (GameData.Shared.InGame && !player.InGame) {
			setFolded();
		}

		NameLabel.text = player.Name;
		ScoreLabel.text = _.Num2CnDigit<int>(player.Bankroll.Value);
		Avt.GetComponent<Avatar>().SetImage(player.Avatar);	

		GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
		transform.SetParent(parent.transform, false);
		
		// 隐藏坐下按钮
		var image = parent.gameObject.GetComponent<Image>();
		image.enabled = false;
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

		var canvas = G.UICvs;

		if (isSelf()) {
			// 图片灰掉
			darkenCards();
		} else {
			Cardfaces.transform.SetParent(canvas.transform, true);
			foldCards(Cardfaces);			
		}

		setFolded();
	}

	private void darkenCards() {
		MyCards.transform.Find("First").GetComponent<Card>().Darken();
		MyCards.transform.Find("Second").GetComponent<Card>().Darken();
	}

	private void dealAct(ActionState state) {
		lastState = state;

		setPlayerAct(true);
		PlayerAct.SetAct(state);

		if (state == ActionState.Allin) {
			AllinGo.SetActive(true);
		}
	}

	private void setPlayerAct(bool active, bool anim = true) {
		if (!active && isPersisState()) {
			return ;
		}

		var cvg = PlayerAct.GetComponent<CanvasGroup>();
		var targetValue = active ? 1 : 0;
		var duration = 0.1f;

		if (anim) {
			cvg.DOFade(targetValue, duration);
		} else {
			cvg.alpha = targetValue;
		}
	}

	private void setFolded() {
		if (isSelf()) {
			darkenCards();	
		}

		Circle.transform.Find("Avatar").GetComponent<CanvasGroup>().alpha = foldOpacity;
	}

	private bool isSelf() {
		return Uid == GameData.Shared.Uid;
	}

	private void registerRxEvent() {
		player.PrChips.AsObservable().Subscribe((value) => {
			if (value == 0) {
				return ;
			}

			setPrChips(value);
		}).AddTo(this);

		player.Bankroll.Subscribe((value) => {
			ScoreLabel.text = _.Num2CnDigit(value);
		}).AddTo(this);

		player.ActState.AsObservable().Subscribe((e) => {
			// 对象已被销毁，不应该执行
			if (this == null) {
				return ;
			}

			if (e == ActionState.None) {
				return ;
			}

			switch(e) {
				case ActionState.Check:
					G.PlaySound("check");
					break;
				case ActionState.Fold:
					// G.PlaySound("fold_boy");
					G.PlaySound("foldpai");
					break;
				case ActionState.Call:
					// G.PlaySound("call_boy");
					break;
				case ActionState.Allin:
					if (player.ActStateTrigger) {
						// G.PlaySound("allin_boy");
						G.PlaySound("allin");
					}
					break;
				case ActionState.Raise:
					// G.PlaySound("raise_boy");
					break;
				default:
					break;
			}

			if (e == ActionState.Fold) {
				Fold();
			} else {
				MoveOut();
			}

			dealAct(e);
			actCardsNumber = GameData.Shared.PublicCards.Count;	
		}).AddTo(this);

		player.Destroyed.AsObservable().Where((v) => v).Subscribe((_) => {
			Destroy(gameObject);
		}).AddTo(this);

		player.Cards.AsObservable().Where((cards) => {
			if (cards != null && cards.Count == 2) {
				if (cards[0] > 0 && cards[1] > 0) {
					return true;
				}
			}

			return false;
		}).Subscribe((cards) => {
			if (isSelf()) {
				SeeCard(cards);
			} else {
				showTheCards(cards, player.SeeCardAnim);
			}
		}).AddTo(this);

		player.OverData.AsObservable().Where((data) => data != null).Subscribe((data) => {
			var gain = data.Gain();
            var normalStar = data.award27 == -1;
			if (gain > 0) {
                Stars.SetActive(true);
                WinNormal.SetActive(normalStar);
                Win27.SetActive(!normalStar);

				if (isSelf()) {
					WinImageGo.SetActive(true);
				}
			}

			// 收回大于0，展示盈亏
			if (data.prize > 0) {
				WinNumber.transform.parent.gameObject.SetActive(true); 
				WinNumber.text = _.Number2Text(gain);
				ScoreLabel.transform.parent.gameObject.SetActive(false);
			}

			if (!isSelf()) {
				showTheCards(data.cards, true);
				showCardType(data.maxFiveRank);
			}

			// 4s后隐藏动画
			Invoke("hideAnim", 4);			
		}).AddTo(this);

		// 中途复原行动
		player.Countdown.AsObservable().Where((obj) => obj.seconds > 0).Subscribe((obj) => {
			turnTo(obj.data, obj.seconds, true);	
		}).AddTo(this);

		RxSubjects.MoveTurn.Subscribe((e) => {
			var index = e.Data.Int("seat");
			var dc = e.Data.Int("deal_card");
		
			if (index == Index) {
				turnTo(e.Data, GameData.Shared.ThinkTime);
				setPlayerAct(false, false);
			} else {
				MoveOut();

				// 刚发了牌
				if (dc == 1 || GameData.Shared.PublicCards.Count != actCardsNumber) {
					setPlayerAct(false);
				}
			}

			// 自动托管
			if (isSelf()) {
				player.SetTrust(e.Data.Dict("trust"));
			}
		}).AddTo(this);

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
		}).AddTo(this);
		
		theSeat.SeatPos.Subscribe((pos) => {
			fixChatPos(pos);
			PlayerAct.ChangePos(pos);
		}).AddTo(this);

		// fixChatPos(SeatPosition.Right);
		// Observable.Timer(TimeSpan.FromSeconds(5)).AsObservable().Subscribe((e) => {
		// 	SpkText.ShowMessage("快来看啊~~这时很长很长的文字很长很长的文字很长很长的问题很长的文字");
		// }).AddTo(this);

		if (isSelf()) {
			GameData.Shared.MaxFiveRank.Subscribe((value) => {
				var parent = CardDesc.transform.parent.gameObject;

				if (value == 0)
                {
                    parent.SetActive(false);
                    return;
                }

                parent.SetActive(true);
                CardDesc.text = Card.GetCardDesc(value);
            }).AddTo(this);

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
			}).AddTo(this);

			player.Trust.ShouldShow.Subscribe((show) => {
				AutoArea.SetActive(show);
			}).AddTo(this);

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
			}).AddTo(this);

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
			}).AddTo(this);

			RxSubjects.Deal.Subscribe((_) => {
				player.Trust.Hide();
			}).AddTo(this);
		}

		RxSubjects.ShowAudio.Where(isSelf).Subscribe((jsonStr) => {
			Volume.SetActive(true);
			ScoreLabel.gameObject.SetActive(false);
		}).AddTo(this);

		RxSubjects.HideAudio.Where(isSelf).Subscribe((_) => {
			Volume.SetActive(false);
			ScoreLabel.gameObject.SetActive(true);
		}).AddTo(this);

		RxSubjects.SendChat.Where(isSelf).Subscribe((jsonStr) => {
            var N = JSON.Parse(jsonStr);
            var text = N["text"].Value;
            SpkText.ShowMessage(text);
        }).AddTo(this);

		RxSubjects.ShowCard.Subscribe((e) => {
			var uid = e.Data.String("uid");
			if (uid != Uid) {
				return ;
			}

			var cards = e.Data.IL("cards");
			showTheCards(cards, true);
		}).AddTo(this);

		// 思考延时
		RxSubjects.Moretime.Subscribe((e) => {
			var model = e.Data.ToObject<MoreTimeModel>();

			if (model.uid != Uid) {
				return ;
			}

            if (!model.IsRound())
            {
                return;
            }

			if (isSelf()) {
				OPGo.GetComponent<OP>().Reset(model.total);
			} else {
				StopCoroutine(turnCoroutine);
                turnCoroutine = yourTurn(model.total);
				StartCoroutine(turnCoroutine);
			} 
		}).AddTo(this);

		player.Allin.Subscribe((allin) => {
			if (allin) {
				player.ActStateTrigger = false;
				player.ActState.OnNext(ActionState.Allin);
			}
		}).AddTo(this);

		player.PlayerStat.Subscribe((state) => {
			HandGo.SetActive(false);
			BackGameBtn.SetActive(false);

			var stateGo = StateLabel.transform.parent.gameObject;
			stateGo.SetActive(false);

			switch(state) {
				case PlayerState.Waiting: case PlayerState.Auditing:
					ScoreLabel.text = "<size=28>等待</size>";
					break;
				case PlayerState.Hanging:
					HandGo.SetActive(true);
					if (isSelf()) {
						BackGameBtn.SetActive(true);
						if (OPGo != null) {
							Destroy(OPGo);
						}
					}
					break;
				case PlayerState.Reserve:
					stateGo.SetActive(true);	
					if (isSelf()) {
						BackGameBtn.SetActive(true);
					}
					break;
				default: 
					break;
			}
		}).AddTo(this);

		IDisposable reserveCd = null; 
		player.ReservedCD.Subscribe((value) => {
			if (reserveCd != null) {
				reserveCd.Dispose();
			}

			if (value > 0) {
				setReserveCd(value);

				reserveCd = Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe((_) => {
					value = Mathf.Max(value - 1, 1);
					setReserveCd(value);
				}).AddTo(this);
			}
		}).AddTo(this);

		player.LastAct.Where((act) => !String.IsNullOrEmpty(act)).Subscribe((act) => {
			dealAct(act.ToActionEnum());	
		}).AddTo(this);

		RxSubjects.GainChip.Where((gainChip) => gainChip.Uid == Uid).Subscribe((gainChip) => {
			gainChip.Grp.ToPlayer(this);
		}).AddTo(this);
	}

	private void setReserveCd(int number) {
		var text = String.Format("留座<b>{0}s</b>", number);
		StateLabel.text = text;
	}

	private void fixChatPos(SeatPosition pos) {
		SpkText.ChangePos(pos);
	}

	private bool isSelf(String jsonStr) {
		var N = JSON.Parse(jsonStr);
		var uid = N["uid"].Value;
		return uid == Uid;
	}

	private bool isPersisState() {
		return lastState == ActionState.Allin || lastState == ActionState.Fold;
	}

	private void showTheCards(List<int> cards, bool anim) {
		if (cards.Count < 2 || isSelf()) {
			return ;
		}

		PlayerAct.gameObject.SetActive(false);		
	
		if (cards[0] > 0 || cards[1] > 0) {
			// 显示GameObject
			getShowCard().SetActive(true);

			// 显示手牌
			if (cards[0] > 0) {
				ShowCards[0].Show(cards[0], anim);
			} 

			if (cards[1] > 0) {
				ShowCards[1].Show(cards[1], anim);
			}

			if (Cardfaces != null) {
				Cardfaces.SetActive(false);
			}
		}
	}

	private GameObject getOtherCardGo() {
		return OthersCardDesc.transform.parent.gameObject;
	}

	private void showCardType(int maxFive) {
		var desc = Card.GetCardDesc(maxFive);

		if (string.IsNullOrEmpty(desc)) {
			return ;
		}

		getOtherCardGo().SetActive(true);
		OthersCardDesc.text = desc;
		NameLabel.gameObject.SetActive(false);
	}

	private GameObject getShowCard() {
		return ShowCards[0].transform.parent.gameObject;
	}

	private void hideAnim() {
		hideGo(Stars, () => {
			ScoreLabel.transform.parent.gameObject.SetActive(true);
		});

		hideGo(WinImageGo);	
		hideGo(getShowCard());	
		hideGo(getOtherCardGo());
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

	private void setPrChips(int value) {
		var chips = (GameObject)Instantiate(Resources.Load("Prefab/UpChip"));
		chips.transform.SetParent(transform, false);
		chips.transform.SetAsLastSibling();

		if (cgo == null) {
			cgo = chips.GetComponent<ChipsGo>();
			cgo.Create(value, theSeat, player);
		} else {
			chips.GetComponent<ChipsGo>().AddMore(() => {
				cgo.SetChips(value);
			}, theSeat, player);	
		}	
	}

	private void turnTo(Dictionary<string, object> dict, int left, bool restore = false) {
		if (isSelf()) {
			var op = showOP(dict, left);
			var flag = player.Trust.FlagString();
			var callNum = player.Trust.CallNumber.Value;

			if (flag == "10") { // 选中左边
				op.gameObject.SetActive(false);
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
					op.gameObject.SetActive(false);

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
		} else {
            turnCoroutine = yourTurn(left);
			StartCoroutine(turnCoroutine);				
		}
	}

    IEnumerator turnCoroutine;

	private IEnumerator yourTurn(float left) {
		Countdown.SetActive(true);
		activated = true;
		AvatarMask.SetActive(true);

        Countdown.GetComponent<Animator>().SetTrigger("1");
        Countdown.GetComponent<Animator>().speed = 1 / left;

		float time = left;
		float total = left.GetThinkTime();
		var mask = Avt.GetComponent<CircleMask>();
        mask.numberText.gameObject.SetActive(true);

		while (time > 0 && activated) {
			time = time - Time.deltaTime;
			
			var percent = Mathf.Min(1, time / total);
			var image = Countdown.GetComponent<ProceduralImage>();
			image.fillAmount = percent;
			mask.SetTextColor(image.color);
            mask.SetFillAmount(time / total, time); 

			yield return new WaitForFixedUpdate();
		}

		activated = false;
        Countdown.SetActive(false); 
	}

	private OP showOP(Dictionary<string, object> data, int left) {
		if (this == null) {
			return null;
		}
		
		// 隐藏头像
		Circle.SetActive(false);

		OPGo = G.Spawn("OP").gameObject;
		var op = OPGo.GetComponent<OP>();
		op.StartWithCmds(data, left);

		return op;
	}

	private void foldCards(GameObject go) {
		Ease ease = Ease.Flash;

		var rectTrans = go.GetComponent<RectTransform>();
		rectTrans.DOAnchorPos(new Vector2(0, 200), animDuration).SetEase(ease);

		var image = go.GetComponent<Image>();
		Tween tween; 

		if (image != null) {
			tween = image.DOFade(0.2f, animDuration).SetEase(ease);
		} else {
			var canvasGrp = go.GetComponent<CanvasGroup>();
			tween = canvasGrp.DOFade(0.2f, animDuration).SetEase(ease);
		}

		tween.OnComplete(() => {
			Destroy(go);
		});
	}
}
