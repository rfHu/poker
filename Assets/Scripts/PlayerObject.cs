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
	Transform circle;
	private float animDuration = 0.2f;

	GameObject chipsGo; 

	void Awake() {
		var info = transform.Find("Info");

		nameLabel = info.Find("Name").GetComponent<Text>();
		scoreLabel = info.Find("Coins").Find("Text").GetComponent<Text>();

		circle = info.Find("Circle");
		countdown = circle.Find("Countdown").gameObject;

		// 倒计时隐藏
		countdown.SetActive(false);

		Delegates.shared.Deal += new EventHandler<DelegateArgs>(onDeal);

		addRx();
	}

	private void addRx() {
		RxSubjects.Ready.Subscribe((e) => {
			var data = e.Data;

			if (data.Int("where") != Index) {
				return ;
			}

			var bankroll = data.Int("bankroll");
			scoreLabel.text = bankroll.ToString();
		}).AddTo(this);
	}

	void hideName() {
		nameLabel.gameObject.SetActive(false);
	}

	public void AddScore(int score) {
		var has = Convert.ToInt32(scoreLabel.text);
		scoreLabel.text = (has + score).ToString();
	}

	void OnDestroy()
	{
		Delegates.shared.Deal -= new EventHandler<DelegateArgs>(onDeal);
	}

	void onDeal(object sender, DelegateArgs e) {
		if (chipsGo != null) {
			chipsGo.GetComponent<ChipsGo>().HideChips();
		}
	}

	void moveOut() {
		activated = false;

		if (OPGo != null) {
			Destroy(OPGo);
			OPGo = null;
			circle.gameObject.SetActive(true); // 显示头像
		}

		Avatar.GetComponent<CircleMask>().Disable();
	}

	public void ShowPlayer(Player player, Transform parent) {
		Index = player.Index;
		Uid = player.Uid;

		if (Uid == GameData.Shared.Uid) {
			hideName();
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
	}

	IEnumerator<WWW> DownloadAvatar(RawImage img, string url) {
		WWW www = new WWW(url);
		yield return www;
		img.texture = _.Circular(www.texture);
	}	

	public void MoveOut() {
		moveOut();		
	}

	public void TurnTo(Dictionary<string, object> dict) {
		if (Uid == GameData.Shared.Uid) {
			showOP(dict);
		} else {
			StartCoroutine(MyTurn());				
		}
	}

	public IEnumerator MyTurn(float elaspe = 0) {
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

	void setChipText(int prchips) {
		if (chipsGo == null) {
			return ;
		}

		chipsGo.GetComponent<ChipsGo>().SetChips(prchips);
	}

	GameObject createChip(int prchips, Action<GameObject> callback = null) {
		var chips = (GameObject)Instantiate(Resources.Load("Prefab/UpChip"));
		chips.transform.SetParent(transform, false);

		chips.GetComponent<RectTransform>()
		.DOAnchorPos(new Vector2(80, 0), animDuraion)
		.OnComplete(() => {
			setChipText(prchips);

			if (callback != null) {
				callback(chips);
			}
		});

		return chips;
	}

	public void SetPrChips(int prchips) {
		if (prchips == 0) {
			return ;	
		}

		if (chipsGo == null) {
			chipsGo = createChip(prchips);			
		} else if (!chipsGo.GetComponent<ChipsGo>().Same(prchips)) {
			createChip(prchips, (go) => {
				Destroy(go);
			});
		}
	}

	float opacity = 0.7f;
	float animDuraion = 0.4f;

	void foldCards(GameObject go, Action callback = null) {
		var rectTrans = go.GetComponent<RectTransform>();
		rectTrans.DOAnchorPos(new Vector2(0, 0), animDuraion);
		rectTrans.DOScale(new Vector2(0.5f, 0.5f), animDuraion);

		var image = go.GetComponent<Image>();
		Tween tween; 

		if (image != null) {
			tween = image.DOFade(0, animDuraion);
		} else {
			var canvasGrp = go.GetComponent<CanvasGroup>();
			tween = canvasGrp.DOFade(0, animDuraion);
		}

		tween.OnComplete(() => {
			Destroy(go);
			if (callback != null) {
				callback();
			}
		});
	}

	public void Fold() {
		moveOut();

		var canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();

		if (Uid == GameData.Shared.Uid) {
			var copy = Instantiate(MyCards, canvas.transform, true);
			
			MyCards.GetComponent<CanvasGroup>().alpha = opacity;
			MyCards.gameObject.SetActive(false);

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
		circle.gameObject.SetActive(false);

		OPGo = (GameObject)Instantiate(Resources.Load("Prefab/OP"));	
		OPGo.transform.SetParent(transform, false);
		
		var op = OPGo.GetComponent<OP>();
		op.StartWithCmds(data);
	}
}
