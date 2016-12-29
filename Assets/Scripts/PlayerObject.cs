using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameFlow;

public class PlayerObject : MonoBehaviour {
	public Player player;
	
	Text nameLabel;
	Text scoreLabel;
	GameObject countdown;

	public int thinkingSeconds = 15;

	void Awake() {
		nameLabel = transform.Find("Name").GetComponent<Text>();
		scoreLabel = transform.Find("Button").Find("Coins").GetComponent<Text>();
		countdown = transform.Find("Circle").Find("InnerCircle").gameObject;

		// 倒计时隐藏
		countdown.GetComponent<Image>().enabled = false;
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
	
	void Update () {
		
	}
}
