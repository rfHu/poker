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
	
	Text nameLabel;
	Text scoreLabel;
	GameObject countdown;

	public RawImage Avatar;
	public bool activated = false;
	public float thinkTime = 15;
	public string Uid = "";

	public GameObject Cardfaces;
	public GameObject MyCards;

	GameObject OPGo;
	GameObject circle;

	private ChipsGo cgo; 

	private Player player;

	void Awake() {
		var info = transform.Find("Info");

		nameLabel = info.Find("Name").GetComponent<Text>();
		scoreLabel = info.Find("Coins").Find("Text").GetComponent<Text>();

		circle = info.Find("Circle").gameObject;
		countdown = circle.transform.Find("Countdown").gameObject;

		// 倒计时隐藏
		countdown.SetActive(false);
	}

	private void registerRxEvent() {
		RxSubjects.Ready.Subscribe((e) => {
			var data = e.Data;

			if (data.Int("where") != Index) {
				return ;
			}

			var bankroll = data.Int("bankroll");
			scoreLabel.text = bankroll.ToString();
		}).AddTo(this);

		RxSubjects.MoveTurn.Subscribe((e) => {
			var index = e.Data.Int("seat");
			
			if (index == Index) {
				turnTo(e.Data);
			} else {
				moveOut();
			}
		}).AddTo(this);

		if (Uid == GameData.Shared.Uid) {
			RxSubjects.SeeCard.Subscribe((e) => {
				if (Uid != GameData.Shared.Uid) {
					return ;
				}

				var cards = e.Data.IL("cards");

				int[] cvs = new int[]{
					Card.CardIndex(cards[0]),
					Card.CardIndex(cards[1])
				};

				var first = MyCards.transform.Find("First");
				var second = MyCards.transform.Find("Second");

				MyCards.SetActive(true);

				first.GetComponent<Card>().Show(cvs[0]);
				second.GetComponent<Card>().Show(cvs[1]);
			}).AddTo(this);	
		}

		player.PrChips.AsObservable().DistinctUntilChanged().Subscribe((value) => {
			if (value == 0) {
				return ;
			}

			setPrChips(value);
		}).AddTo(this);

		RxSubjects.Fold.Subscribe((e) => {
			var index = e.Data.Int("args");

			if (index == Index) {
				fold();
			}
		}).AddTo(this);

		RxSubjects.Call.Subscribe(act).AddTo(this);

		RxSubjects.AllIn.Subscribe(act).AddTo(this);

		RxSubjects.Check.Subscribe(act).AddTo(this);

		RxSubjects.Raise.Subscribe(act).AddTo(this);
	}

	private void act(RxData e) {
		var mop = e.Data.ToObject<Mop>();

		if (mop.seat != Index) {
			return ;
		}	

		player.PrChips.Value = mop.pr_chips;
		moveOut();
	}

	void setPrChips(int value) {
		var chips = (GameObject)Instantiate(Resources.Load("Prefab/UpChip"));
		chips.transform.SetParent(transform, false);

		if (cgo == null) {
			cgo = chips.GetComponent<ChipsGo>();
			cgo.Create(value);
		} else {
			chips.GetComponent<ChipsGo>().AddMore(() => {
				cgo.SetChips(value);
			});	
		}	
	}

	void hideName() {
		nameLabel.gameObject.SetActive(false);
	}

	// public void AddScore(int score) {
	// 	var has = Convert.ToInt32(scoreLabel.text);
	// 	scoreLabel.text = (has + score).ToString();
	// }

	void moveOut() {
		activated = false;

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

		if (Uid == GameData.Shared.Uid) {
			hideName();
			RxSubjects.ChangeVectorsByIndex.OnNext(Index);
		} else {
			Cardfaces.SetActive(true);
		}

		nameLabel.text = player.Name;
		scoreLabel.text = player.Bankroll.ToString();
		RawImage rawImage = Avatar.GetComponent<RawImage>();
		StartCoroutine(DownloadAvatar(rawImage, player.Avatar));

		GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
		transform.SetParent(parent.transform, false);
		
		// 隐藏坐下按钮
		var image = parent.gameObject.GetComponent<Image>();
		image.enabled = false;

		registerRxEvent();
	}

	IEnumerator<WWW> DownloadAvatar(RawImage img, string url) {
		WWW www = new WWW(url);
		yield return www;
		img.texture = _.Circular(www.texture);
	}	

	private void turnTo(Dictionary<string, object> dict) {
		if (Uid == GameData.Shared.Uid) {
			showOP(dict);
		} else {
			StartCoroutine(myTurn());				
		}
	}

	private IEnumerator myTurn(float elaspe = 0) {
		countdown.SetActive(true);
		activated = true;

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

	float opacity = 0.7f;
	float animDuration = 0.4f;

	void foldCards(GameObject go, Action callback = null) {
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

	void fold() {
		moveOut();

		var canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();

		if (Uid == GameData.Shared.Uid) {
			var copy = Instantiate(MyCards, canvas.transform, true);
			
			MyCards.GetComponent<CanvasGroup>().alpha = opacity;
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

		transform.Find("Info").GetComponent<CanvasGroup>().alpha = opacity;
	}

	void showOP(Dictionary<string, object> data) {
		// 隐藏头像
		circle.SetActive(false);

		OPGo = (GameObject)Instantiate(Resources.Load("Prefab/OP"));	
		OPGo.transform.SetParent(transform, false);
		
		var op = OPGo.GetComponent<OP>();
		op.StartWithCmds(data);
	}
}
