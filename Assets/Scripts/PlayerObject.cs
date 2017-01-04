using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.UI.ProceduralImage;

public class PlayerObject : MonoBehaviour {
	public Player player;
	
	Text nameLabel;
	Text scoreLabel;
	GameObject countdown;

	public bool activated = false;
	public float thinkTime = 15;

	void Awake() {
		nameLabel = transform.Find("Name").GetComponent<Text>();
		scoreLabel = transform.Find("Coins").Find("Text").GetComponent<Text>();
		countdown = transform.Find("Circle").Find("Countdown").gameObject;

		// 倒计时隐藏
		countdown.SetActive(false);
	}

	public void ShowPlayer() {
		Player player = new Player();	
		player.avatar = "https://ss1.bdstatic.com/70cFvXSh_Q1YnxGkpoWK1HF6hhy/it/u=3081053742,1983158129&fm=116&gp=0.jpg";
		player.name = "singno";
		player.score = 50000;

		nameLabel.text = player.name;
		scoreLabel.text = player.score.ToString();
		RawImage rawImage = transform.Find("Circle").Find("Avatar").GetComponent<RawImage>();
		StartCoroutine(DownloadAvatar(rawImage, player.avatar));
	}

	IEnumerator<WWW> DownloadAvatar(RawImage img, string url) {
		WWW www = new WWW(url);
		yield return www;
		img.texture = Ext.Circular(www.texture);
	}	

	public IEnumerator setActivated(float elaspe) {
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
}
