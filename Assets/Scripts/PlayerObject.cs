using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerObject : MonoBehaviour {
	public GameObject playerPrefab;
	public Canvas canvas;
	Player player;

	PlayerObject(Player player, Vector2 vector) {
		this.player = player;
		ShowPlayer(vector);
	}

	GameObject ShowPlayer(Vector2 vector) {
		Player player = new Player();	
		player.avatar = "https://ss1.bdstatic.com/70cFvXSh_Q1YnxGkpoWK1HF6hhy/it/u=3081053742,1983158129&fm=116&gp=0.jpg";
		player.name = "singno";
		player.score = 50000;

		GameObject playerObject = Instantiate(playerPrefab);
		playerObject.transform.Find("Name").GetComponent<Text>().text = player.name ;
		playerObject.transform.Find("Button").Find("Coins").GetComponent<Text>().text = player.score.ToString();
		RawImage rawImage = playerObject.transform.Find("Circle").Find("Avatar").GetComponent<RawImage>();
		playerObject.transform.SetParent(canvas.transform,  false);
		playerObject.GetComponent<RectTransform>().localPosition = vector;

		StartCoroutine(DownloadAvatar(rawImage, player.avatar));

		return playerObject;
	}

	IEnumerator<WWW> DownloadAvatar(RawImage img, string url) {
		WWW www = new WWW(url);
		yield return www;
		img.texture = Ext.Circular(www.texture);
	}	
	
	void Update () {
		
	}
}
