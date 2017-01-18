using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.UI.ProceduralImage;
using Extensions;
using DG.Tweening;

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
	public Text Chips;

	GameObject OPGo;
	private float animDuration = 0.2f;

	void Awake() {
		var info = transform.Find("Info");

		nameLabel = info.Find("Name").GetComponent<Text>();
		scoreLabel = info.Find("Coins").Find("Text").GetComponent<Text>();
		countdown = info.Find("Circle").Find("Countdown").gameObject;

		// 倒计时隐藏
		countdown.SetActive(false);
	}

	public void HideName() {
		nameLabel.gameObject.SetActive(false);
	}

	public void SetScore(int score) {
		scoreLabel.text = score.ToString();
	}

	public void MoveOut() {
		activated = false;
		if (OPGo != null) {
			Destroy(OPGo);
		}
	}

	public void ShowPlayer(Player player) {
		nameLabel.text = player.Name;
		scoreLabel.text = player.Bankroll.ToString();
		RawImage rawImage = Avatar.GetComponent<RawImage>();
		StartCoroutine(DownloadAvatar(rawImage, player.Avatar));
	}

	IEnumerator<WWW> DownloadAvatar(RawImage img, string url) {
		WWW www = new WWW(url);
		yield return www;
		img.texture = _.Circular(www.texture);
	}	


	public void TurnTo(Dictionary<string, object> dict) {
		if (Uid == GConf.Uid) {
			showOP();
		} else {
			StartCoroutine(MyTurn());				
		}
	}

	public IEnumerator MyTurn(float elaspe = 0) {
		countdown.SetActive(true);
		activated = true;

		float time = thinkTime - elaspe;
		while (time > 0 && activated) {
			time = time - Time.deltaTime;
			countdown.GetComponent<ProceduralImage>().fillAmount = Mathf.Min(1, time / thinkTime);
			yield return new WaitForFixedUpdate();
		}

		activated = false;
		countdown.SetActive(false);
	}

	public void SetPrChips(int prchips) {
		if (prchips != 0) {
			transform.Find("Chips").gameObject.SetActive(true);
			Chips.text = prchips.ToString();
		}
	}

	public void SetDealer(GameObject dealer) {
		dealer.transform.SetParent(transform, false);
		dealer.GetComponent<RectTransform>().DOAnchorPos( new Vector2(-90, 0), animDuration);
	}

	void showOP() {
		OPGo = (GameObject)Instantiate(Resources.Load("Prefab/OP"));	
		OPGo.transform.SetParent(transform, false);
		
		var op = OPGo.GetComponent<OP>();

		op.CallAct = () => {
			op.Call();
		};

		op.R1Act = () => {
			
		};

		op.R2Act = () => {

		};

		op.R3Act = () => {

		};
	}
}
