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
	public float thinkTime = 15;
	public string Uid = "";
	public GameObject Cardfaces;
	public GameObject MyCards;
	public GameObject Stars;
	public Text WinNumber;
	public List<Card> ShowCards;
	public GameObject AvatarMask;

	private Text nameLabel;
	private Text scoreLabel;
	private GameObject countdown;
	private GameObject OPGo;
	private GameObject circle;
	private ChipsGo cgo; 
	private Player player;
	private float foldOpacity = 0.7f;
	private float animDuration = 0.4f;

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

		first.GetComponent<Card>().ShowServer(cards[0], true);
		second.GetComponent<Card>().ShowServer(cards[1], true);
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

	private bool isLeft() {
		return transform.parent.GetComponent<Seat>().IsLeft();
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
		player.PrChips.AsObservable().DistinctUntilChanged().Subscribe((value) => {
			if (value == 0) {
				return ;
			}

			setPrChips(value);
		}).AddTo(this);

		player.Bankroll.Subscribe((value) => {
			scoreLabel.text = value.ToString();
		}).AddTo(this);

		player.ActState.Subscribe((e) => {
			if (e == ActionState.None) {
				return ;
			}

			if (e == ActionState.Fold) {
				Fold();
			} else {
				MoveOut();
			}
		}).AddTo(this);

		player.Destroyed.AsObservable().Where((v) => v).Subscribe((_) => {
			Destroy(gameObject);
		}).AddTo(this);

		player.Cards.AsObservable().Where((cards) => cards != null && cards.Count == 2).Subscribe((cards) => {
			SeeCard(cards);
		}).AddTo(this);

		player.Winner.AsObservable().Where((winner) => winner != null).Subscribe((winner) => {
			var gain = winner.Gain();
			if (gain > 0) {
				Stars.SetActive(true);

				if (isSelf()) {
					WinImageGo.SetActive(true);
				}
			}
			
			WinNumber.transform.parent.gameObject.SetActive(true); 
			WinNumber.text = num2Text(gain);
			scoreLabel.gameObject.SetActive(false);

			if (!isSelf()) {
				showTheCards(winner.cards);
			}

			// 2s后隐藏动画
			Invoke("hideAnim", 2);			
		}).AddTo(this);

		RxSubjects.MoveTurn.Subscribe((e) => {
			var index = e.Data.Int("seat");
			
			if (index == Index) {
				TurnTo(e.Data);
			} else {
				MoveOut();
			}
		}).AddTo(this);

		// Gameover 应该清掉所有状态
		RxSubjects.GameOver.Subscribe((e) => {
			MoveOut();
		}).AddTo(this);
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
			ShowCards[0].transform.parent.gameObject.SetActive(true);

			// 显示手牌
			ShowCards[0].ShowServer(cards[0], true);
			ShowCards[1].ShowServer(cards[1], true);

			Cardfaces.SetActive(false);
		}
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
	}

	private void setPrChips(int value) {
		var chips = (GameObject)Instantiate(Resources.Load("Prefab/UpChip"));
		chips.transform.SetParent(transform, false);

		if (cgo == null) {
			cgo = chips.GetComponent<ChipsGo>();
			cgo.Create(value, isLeft());
		} else {
			chips.GetComponent<ChipsGo>().AddMore(() => {
				cgo.SetChips(value);
			}, isLeft());	
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

	private void TurnTo(Dictionary<string, object> dict) {
		if (isSelf()) {
			showOP(dict);
		} else {
			StartCoroutine(myTurn());				
		}
	}

	private IEnumerator myTurn(float elaspe = 0) {
		countdown.SetActive(true);
		activated = true;
		AvatarMask.SetActive(true);

		float time = thinkTime - elaspe;
		var mask = Avatar.GetComponent<CircleMask>();
		mask.SetTextColor(new Color(0, (float)255 / 255, (float)106 / 255));

		while (time > 0 && activated) {
			time = time - Time.deltaTime;
			
			var percent = Mathf.Min(1, time / thinkTime);
			countdown.GetComponent<ProceduralImage>().fillAmount = percent;
			mask.SetFillAmount(time);

			yield return new WaitForFixedUpdate();
		}

		activated = false;
		countdown.SetActive(false);
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

	private void showOP(Dictionary<string, object> data) {
		// 隐藏头像
		circle.SetActive(false);

		OPGo = (GameObject)Instantiate(Resources.Load("Prefab/OP"));	
		OPGo.transform.SetParent(transform, false);
		
		var op = OPGo.GetComponent<OP>();
		op.StartWithCmds(data);
	}
}
