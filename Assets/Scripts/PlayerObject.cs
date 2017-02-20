using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.UI.ProceduralImage;
using System;
using DG.Tweening;
using UniRx;
using Extensions;

public class PlayerObject : MonoBehaviour {
	public int Index;
	public GameObject WinImageGo;
	public RawImage Avatar;
	public bool activated = false;
	public string Uid = "";
	public GameObject Cardfaces;
	public GameObject MyCards;
	public GameObject Stars;
	public Text WinNumber;
	public List<Card> ShowCards;
	public GameObject AvatarMask;
	public Image ActImage;
	// 0:看牌   1:加注    2:跟注    3:弃牌    4:All In
	public Sprite[] ActSprites;
	public GameObject AllinAnim;

	private Text nameLabel;
	private Text scoreLabel;
	private GameObject countdown;
	private GameObject OPGo;
	private GameObject circle;
	private ChipsGo cgo; 
	private Player player;
	private float foldOpacity = 0.7f;
	private float animDuration = 0.4f;

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

		// 倒计时隐藏
		countdown.SetActive(false);
	}

	public void SeeCard(List<int> cards) {
		var first = MyCards.transform.Find("First");
		var second = MyCards.transform.Find("Second");

		MyCards.SetActive(true);

		first.GetComponent<Card>().Show(cards[0], true);
		second.GetComponent<Card>().Show(cards[1], true);
	}

	public void MoveOut() {
		activated = false;
		AvatarMask.SetActive(false);

		if (OPGo != null) {
			Destroy(OPGo);
			OPGo = null;
			circle.SetActive(true); // 显示头像
		}

		Avatar.GetComponent<CircleMask>().Disable();
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

		if (isSelf()) {
			hideName();
			RxSubjects.ChangeVectorsByIndex.OnNext(Index);
		} else if(player.InGame) { 
			Cardfaces.SetActive(true);
		}

		if (GameData.Shared.InGame && !player.InGame) {
			setAlpha();
		}

		nameLabel.text = player.Name;
		scoreLabel.text = player.Bankroll.ToString();
		RawImage rawImage = Avatar.GetComponent<RawImage>();
		StartCoroutine(downloadAvatar(rawImage, player.Avatar));

		GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
		transform.SetParent(parent.transform, false);
		
		// 隐藏坐下按钮
		var image = parent.gameObject.GetComponent<Image>();
		image.enabled = false;
		registerRxEvent();
	}

	public void Fold() {
		MoveOut();

		var canvas = G.Cvs;

		if (isSelf()) {
			var copy = Instantiate(MyCards, canvas.transform, true);
			
			MyCards.GetComponent<CanvasGroup>().alpha = foldOpacity;
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

	public void HideAct() {
		ActImage.gameObject.SetActive(false);
	}

	private void dealAct(ActionState state) {
		var map = new Dictionary<ActionState, int>{
			{ActionState.Check, 0},
			{ActionState.Raise, 1},
			{ActionState.Call, 2},
			{ActionState.Fold, 3},
			{ActionState.Allin, 4}
		};	

		ActImage.gameObject.SetActive(true);
		ActImage.sprite = ActSprites[map[state]];

		if (state == ActionState.Allin) {
			AllinAnim.SetActive(true);
		}
	}

	private void setAlpha() {
		if (isSelf()) {
			MyCards.GetComponent<CanvasGroup>().alpha = foldOpacity;
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
			if (e == ActionState.None) {
				return ;
			}

			if (e == ActionState.Fold) {
				Fold();
			} else {
				MoveOut();
			}

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

		player.Winner.AsObservable().Where((winner) => winner != null).Subscribe((winner) => {
			var gain = winner.Gain();
			if (gain > 0) {
				Stars.SetActive(true);

				if (isSelf()) {
					WinImageGo.SetActive(true);
				}
			}

			// 收回大于0，要做筹码动画
			if (winner.prize > 0) {
				Invoke("doChipsAnim", 1f);
			}
			
			WinNumber.transform.parent.gameObject.SetActive(true); 
			WinNumber.text = num2Text(gain);
			scoreLabel.gameObject.SetActive(false);

			if (!isSelf() && !AllinAnim.activeSelf) {
				showTheCards(winner.cards);
			}

			// 2s后隐藏动画
			Invoke("hideAnim", 2);			
		}).AddTo(this);

		// 中途复原行动
		player.Countdown.AsObservable().Where((obj) => obj.seconds > 0).Subscribe((obj) => {
			var elaspe = GameData.Shared.ThinkTime - obj.seconds;
			turnTo(obj.data, elaspe);	
		});

		RxSubjects.MoveTurn.Subscribe((e) => {
			var index = e.Data.Int("seat");
			
			if (index == Index) {
				turnTo(e.Data, 0);
				ActImage.gameObject.SetActive(false);
			} else {
				MoveOut();
			}
		}).AddTo(this);

		// Gameover 应该清掉所有状态
		RxSubjects.GameOver.Subscribe((e) => {
			MoveOut();
			ActImage.gameObject.SetActive(false);
			AllinAnim.SetActive(false);

			if (cgo != null) {
			 	cgo.Hide();
			}
		}).AddTo(this);

		RxSubjects.Deal.Subscribe((e) => {
			ActImage.gameObject.SetActive(false);
		}).AddTo(this);

		theSeat.SeatPos.Subscribe((pos) => {
			var trans = ActImage.GetComponent<RectTransform>();
			var v = trans.anchoredPosition;
			var x = Math.Abs(v.x);

			if (pos == SeatPosition.Right) {
				trans.anchoredPosition = new Vector2(-x, v.y);	
			} else {
				trans.anchoredPosition = new Vector2(x, v.y);
			}
		}).AddTo(this);
	}

	private void doChipsAnim() {
		var grp = Pots.CloneChips();
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

	private GameObject getShowCard() {
		return ShowCards[0].transform.parent.gameObject;
	}

	private void hideAnim() {
		var duration = 0.3f;

		if (Stars.activeSelf) {
			Stars.GetComponent<CanvasGroup>().DOFade(0,duration).OnComplete(() => {
				scoreLabel.gameObject.SetActive(true);
				WinNumber.transform.parent.gameObject.SetActive(false);
				Stars.SetActive(false);
			});
		}

		if (WinImageGo.activeSelf) {
			WinImageGo.GetComponent<RawImage>().DOFade(0,duration).OnComplete(() => {
				WinImageGo.SetActive(false);
			});
		}

		var showCard = getShowCard();
		if (showCard.activeSelf) {
			showCard.GetComponent<CanvasGroup>().DOFade(0, duration).OnComplete(() => {
				showCard.SetActive(false);
			});
		}
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

	private void hideName() {
		nameLabel.gameObject.SetActive(false);
	}

	private IEnumerator<WWW> downloadAvatar(RawImage img, string url) {
		WWW www = new WWW(url);
		yield return www;
		img.texture = _.Circular(www.texture);
	}	

	private void turnTo(Dictionary<string, object> dict, int elaspe) {
		if (isSelf()) {
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
		var mask = Avatar.GetComponent<CircleMask>();
		mask.SetTextColor(new Color(0, (float)255 / 255, (float)106 / 255));

		while (time > 0 && activated) {
			time = time - Time.deltaTime;
			
			var percent = Mathf.Min(1, time / GameData.Shared.ThinkTime);
			countdown.GetComponent<ProceduralImage>().fillAmount = percent;
			mask.SetFillAmount(time);

			yield return new WaitForFixedUpdate();
		}

		activated = false;
		countdown.SetActive(false);
	}

	private void showOP(Dictionary<string, object> data, int elaspe) {
		// 隐藏头像
		circle.SetActive(false);

		OPGo = (GameObject)Instantiate(Resources.Load("Prefab/OP"));	
		OPGo.transform.SetParent(G.Cvs.transform, false);
		
		var op = OPGo.GetComponent<OP>();
		op.StartWithCmds(data, elaspe);
	}

	private void foldCards(GameObject go, Action callback = null) {
		var rectTrans = go.GetComponent<RectTransform>();
		rectTrans.DOAnchorPos(new Vector2(0, 0), animDuration);
		rectTrans.DOScale(new Vector2(0.5f, 0.5f), animDuration);

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
}
