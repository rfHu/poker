using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.UI.ProceduralImage;
using Extensions;

public class PlayerObject : MonoBehaviour {
	public int Index;
	
	Text nameLabel;
	Text scoreLabel;
	GameObject countdown;

	public bool activated = false;
	public float thinkTime = 15;
	public string Uid = "";

	public GameObject Cardfaces;
	public GameObject MyCards;
	public Text Chips;

	void Awake() {
		nameLabel = transform.Find("Name").GetComponent<Text>();
		scoreLabel = transform.Find("Coins").Find("Text").GetComponent<Text>();
		countdown = transform.Find("Circle").Find("Countdown").gameObject;

		// 倒计时隐藏
		countdown.SetActive(false);
	}

	public void SetScore(int score) {
		scoreLabel.text = score.ToString();
	}

	public void ShowPlayer(Player player) {
		nameLabel.text = player.Name;
		scoreLabel.text = player.Bankroll.ToString();
		RawImage rawImage = transform.Find("Circle").Find("Avatar").GetComponent<RawImage>();
		StartCoroutine(DownloadAvatar(rawImage, player.Avatar));
	}

	IEnumerator<WWW> DownloadAvatar(RawImage img, string url) {
		WWW www = new WWW(url);
		yield return www;
		img.texture = Ext.Circular(www.texture);
	}	


	public void TurnTo(float elaspe = 0) {
		if (Uid == GConf.Uid) {
			showOP();
		} else {
			StartCoroutine(MyTurn(elaspe));				
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

	void showOP() {
		var obj = (GameObject)Instantiate(Resources.Load("Prefab/OP"));	
		var op = obj.GetComponent<OP>();

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
