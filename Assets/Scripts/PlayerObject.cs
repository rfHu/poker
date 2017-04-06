using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.UI.ProceduralImage;
using System;
using DG.Tweening;
using UniRx;
using Extensions;
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
	public Text WinNumber;
	public List<Card> ShowCards;
	public GameObject AvatarMask;
	public PlayerActGo PlayerAct;
	public Sprite[] ActSprites;
	public GameObject AllinGo;

	private Text nameLabel;
	private Text scoreLabel;
	private GameObject countdown;
	private GameObject OPGo;
	private GameObject circle;
	private ChipsGo cgo; 
	private Player player;
	private float foldOpacity = 0.6f;
	private float animDuration = 0.4f;
    private float hideDuration = 0.3f; 
    private DOTweenAnimation countdownColorAni;
	private ActionState lastState;

	public Text CardDesc;
	public Text OthersCardDesc;

	public SpkTextGo SpkText;
	public GameObject Volume;

	private Seat theSeat {
		get {
			return  transform.parent.GetComponent<Seat>();
		}
	}

	void Awake() {
		var info = transform.Find("Info");

		nameLabel = info.Find("Name").GetComponent<Text>();
		scoreLabel = info.Find("Coins").Find("Text").GetComponent<Text>();

		circle = info.Find("Circle").gameObject;
		countdown = circle.transform.Find("Countdown").gameObject;

        countdownColorAni = countdown.GetComponent<DOTweenAnimation>();

		// 倒计时隐藏
		countdown.SetActive(false);

		SpkText.Uid = Uid;
	}

	public void SeeCard(List<int> cards) {
		var first = MyCards.transform.Find("First");
		var second = MyCards.transform.Find("Second");

		MyCards.SetActive(true);

		var state = GameData.Shared.SeeCardState;
		first.GetComponent<Card>().Show(cards[0], state);
		second.GetComponent<Card>().Show(cards[1], state);
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
			circle.SetActive(true); // 显示头像
		}

		Avt.GetComponent<CircleMask>().Disable();
	}

	void OnDestroy()
	{
		if (OPGo != null) {
			Destroy(OPGo);
		}
	}

	public void ShowPlayer(Player player, Transform parent) {
		Index = player.Index;
		Uid = player.Uid;

		this.player = player;

		// 头像点击事件
		Avt.GetComponent<Avatar>().Uid = Uid;

		if (isSelf()) {
			nameLabel.gameObject.SetActive(false);
			RxSubjects.ChangeVectorsByIndex.OnNext(Index);
		} else if(player.InGame) { 
			Cardfaces.SetActive(true);
		}

		if (GameData.Shared.InGame && !player.InGame) {
			setFolded();
		}

		nameLabel.text = player.Name;
		scoreLabel.text = player.Bankroll.ToString();
		Avt.GetComponent<Avatar>().SetImage(player.Avatar);	

		GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
		transform.SetParent(parent.transform, false);
		
		// 隐藏坐下按钮
		var image = parent.gameObject.GetComponent<Image>();
		image.enabled = false;
		registerRxEvent();
	}

	public void Fold() {
		MoveOut();

		var canvas = G.UICvs;

		if (isSelf()) {
			var copy = Instantiate(MyCards, canvas.transform, true);
			
			// 图片灰掉
			darkenCards();

			MyCards.SetActive(false);

			foldCards(copy, () => {
				if (MyCards != null) {
					MyCards.SetActive(true);
				}
			});
		} else {
			Cardfaces.transform.SetParent(canvas.transform, true);
			foldCards(Cardfaces);			
		}

		transform.Find("Info").GetComponent<CanvasGroup>().alpha = foldOpacity;
	}

	private void darkenCards() {
		MyCards.transform.Find("First").GetComponent<Card>().Darken();
		MyCards.transform.Find("Second").GetComponent<Card>().Darken();
	}

	private void dealAct(ActionState state) {
		setPlayerAct(true);
		PlayerAct.SetAct(state);

		if (state == ActionState.Allin) {
			AllinGo.SetActive(true);
		}
	}

	private void setPlayerAct(bool active) {
		if (!active && isPersisState()) {
			return ;
		}

		PlayerAct.gameObject.SetActive(active);

		if (!isSelf()) {
			nameLabel.gameObject.SetActive(!active);
		}
	}

	private void setFolded() {
		if (isSelf()) {
			darkenCards();	
		}

		transform.Find("Info").GetComponent<CanvasGroup>().alpha = foldOpacity;
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
			scoreLabel.text = value.ToString();
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
					MasterAudio.PlaySound("check");
					break;
				case ActionState.Fold:
					MasterAudio.PlaySound("foldpai");
					break;
				default:
					break;
			}

			if (e == ActionState.Fold) {
				Fold();
			} else {
				MoveOut();
			}

			lastState = e;
			dealAct(e);
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
				showTheCards(cards);
			}
		}).AddTo(this);

		player.OverData.AsObservable().Where((data) => data != null).Subscribe((data) => {
			var gain = data.Gain();
			if (gain > 0) {
				Stars.SetActive(true);

				if (isSelf()) {
					WinImageGo.SetActive(true);
				}
			}

			// 收回大于0，要做筹码动画，同时展示盈亏
			if (data.prize > 0) {
				Invoke("doChipsAnim", 1f);
				WinNumber.transform.parent.gameObject.SetActive(true); 
				WinNumber.text = num2Text(gain);
				scoreLabel.transform.parent.gameObject.SetActive(false);
			}

			if (!isSelf()) {
				if (!AllinGo.activeSelf) {
					showTheCards(data.cards);
				}
				showCardType(data.maxFiveRank);
			}

			// 4s后隐藏动画
			Invoke("hideAnim", 4);			
		}).AddTo(this);

		// 中途复原行动
		player.Countdown.AsObservable().Where((obj) => obj.seconds > 0).Subscribe((obj) => {
			var elaspe = Math.Max(GameData.Shared.ThinkTime - obj.seconds, 0);
			turnTo(obj.data, elaspe);	
		}).AddTo(this);

		RxSubjects.MoveTurn.Subscribe((e) => {
			G.waitSound(() => {
				if (this == null) {
					return ;
				}
				
				var index = e.Data.Int("seat");
				var dc = e.Data.Int("deal_card");

				// 刚发了牌
				if (dc == 1) {
					setPlayerAct(false);
				}
			
				if (index == Index) {
					turnTo(e.Data, 0);
					setPlayerAct(false);
				} else {
					MoveOut();
				}
			});
		}).AddTo(this);

		// Gameover 应该清掉所有状态
		RxSubjects.GameOver.Subscribe((e) => {
			MoveOut();
			PlayerAct.gameObject.SetActive(false);
			AllinGo.SetActive(false);

			if (cgo != null) {
			 	cgo.Hide();
			}
		}).AddTo(this);
		
		theSeat.SeatPos.Subscribe((pos) => {
			fixVoicePos(pos);	
			fixChatPos(pos);
		}).AddTo(this);

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
		}

		RxSubjects.ShowAudio.Where(isSelf).Subscribe((jsonStr) => {
			Volume.SetActive(true);
		}).AddTo(this);

		RxSubjects.HideAudio.Where(isSelf).Subscribe((uid) => {
			Volume.SetActive(false);
		}).AddTo(this);

		RxSubjects.SendChat.Where(isSelf).Subscribe((jsonStr) => {
            var N = JSON.Parse(jsonStr);
            var text = N["text"].Value;
            SpkText.ShowMessage(text);
        }).AddTo(this); 
	}

	private void fixVoicePos(SeatPosition pos) {
		if (!Volume.activeSelf) {
			return ;
		}

		var trans = Volume.GetComponent<RectTransform>();
		var v = trans.anchoredPosition;
		var x = Math.Abs(v.x);

		if (pos == SeatPosition.Right) {
			trans.anchoredPosition = new Vector2(-x, v.y);	
			trans.localScale = new Vector2(-1, 1);
		} else {
			trans.anchoredPosition = new Vector2(x, v.y);
			trans.localScale = new Vector2(1, 1);
		}
	}

	private void fixChatPos(SeatPosition pos) {
		if (!SpkText.gameObject.activeSelf) {
			return ;
		}

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

	private void doChipsAnim() {
		var grp = Pots.CloneChipsHideSource();
        grp.ToPlayer(this);
	}

	private string num2Text(int num) {
		if (num <= 0) {
			return num.ToString();
		}

		return "+" + num.ToString();
	}

	private void showTheCards(List<int> cards) {
		if (cards.Count < 2) {
			return ;
		}
	
		if (cards[0] > 0 && cards[1] > 0) {
			// 显示GameObject
			getShowCard().SetActive(true);

			// 显示手牌
			ShowCards[0].Show(cards[0], true);
			ShowCards[1].Show(cards[1], true);

			Cardfaces.SetActive(false);
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
		nameLabel.gameObject.SetActive(false);
	}

	private GameObject getShowCard() {
		return ShowCards[0].transform.parent.gameObject;
	}

	private void hideAnim() {
		hideGo(Stars, () => {
			scoreLabel.transform.parent.gameObject.SetActive(true);
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

		if (cgo == null) {
			cgo = chips.GetComponent<ChipsGo>();
			cgo.Create(value, theSeat);
		} else {
			chips.GetComponent<ChipsGo>().AddMore(() => {
				cgo.SetChips(value);
			}, theSeat);	
		}	
	}

	private void turnTo(Dictionary<string, object> dict, int elaspe) {
		if (isSelf()) {
			MasterAudio.PlaySound("on_turn");
			showOP(dict, elaspe);
		} else {
			StartCoroutine(yourTurn(elaspe));				
		}
	}

	private IEnumerator yourTurn(float elaspe) {
		countdown.SetActive(true);
		activated = true;
		AvatarMask.SetActive(true);

		float time = GameData.Shared.ThinkTime - elaspe;
		var mask = Avt.GetComponent<CircleMask>();

        PlayCountdownAni(elaspe);

		while (time > 0 && activated) {
			time = time - Time.deltaTime;
			
			var percent = Mathf.Min(1, time / GameData.Shared.ThinkTime);
			var image = countdown.GetComponent<ProceduralImage>();
			image.fillAmount = percent;
			mask.SetTextColor(image.color);
            mask.SetFillAmount(time); 

			yield return new WaitForFixedUpdate();
		}

		activated = false;
        countdown.SetActive(false); 
	}

	private void showOP(Dictionary<string, object> data, int elaspe) {
		if (this == null) {
			return ;
		}
		
		// 隐藏头像
		circle.SetActive(false);

		OPGo = (GameObject)Instantiate(Resources.Load("Prefab/OP"));	
		var op = OPGo.GetComponent<OP>();
		op.StartWithCmds(data, elaspe, () => {
			circle.SetActive(true);
		});
	}

	private void foldCards(GameObject go, Action callback = null) {
		var rectTrans = go.GetComponent<RectTransform>();
		rectTrans.DOAnchorPos(new Vector2(0, 200), animDuration);
		rectTrans.DOScale(new Vector2(0.9f, 0.9f), animDuration);

		var image = go.GetComponent<Image>();
		Tween tween; 

		if (image != null) {
			tween = image.DOFade(0, animDuration);
		} else {
			var canvasGrp = go.GetComponent<CanvasGroup>();
			tween = canvasGrp.DOFade(0, animDuration);
		}

		tween.OnComplete(() => {
			Destroy(go);
			if (callback != null) {
				callback();
			}
		});
	}

    private void PlayCountdownAni(float elaspe)
    {
        List<Tween> tweens = countdownColorAni.GetTweens();
        for (int i = 0; i < tweens.Count; i++)
        {
            tweens[i].Goto(elaspe);
            tweens[i].Play();
        }
    }
}
