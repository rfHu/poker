using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour {
	public int numberOfPlayers = 2;
	public Button seat;
	public Canvas canvas;

	public GameObject gameInfo;
	public GameObject gameInfoParent;
	public GameObject playerPrefab;

	public Dictionary<string, object> gameInfoDictionary;

	void Start () {
		List<Button> buttons = new List<Button>();

		for (int i = 0; i < numberOfPlayers; i++) {
			Button copySeat = Instantiate (seat);
			copySeat.transform.SetParent (canvas.transform, false);
			buttons.Add (copySeat);
		}

		Vector2[] vectors = GetVectors (numberOfPlayers);
		int iter = 0;

		foreach(Button button in buttons) {
			button.GetComponent<RectTransform> ().localPosition = vectors[iter] ;

			int identifer = iter;
			Vector2 vector = vectors[identifer];
			button.onClick.AddListener(() => {
				ShowPlayer(vector);	
			});

			iter++;
		}

		ShowGameInfo();
	}

	// 逆时针生成位置信息
	Vector2[] GetVectors(int total) {
		float width = canvas.GetComponent<RectTransform>().rect.width;
		float height = canvas.GetComponent<RectTransform>().rect.height;

		float top = height / 2 - 150;
		float bottom = -height / 2 + 180;
		float right = width / 2 - 100;
		float left = -width / 2 + 100; 

		Vector2 number1 = new Vector2 (0, bottom);

		if (total == 2) {
			return new Vector2 []{
				number1,
				new Vector2(0, top)
			};
		}

		float hh = Mathf.Abs (top) + Mathf.Abs (bottom);
		float ww = Mathf.Abs (left) + Mathf.Abs (right);

		float h3 = hh / 2 - hh / 3;
		float h4 = hh / 4;
		float w3 = ww / 2 - ww / 3;

		if (total == 6) {
			return new Vector2[] {
				number1, 
				new Vector2(right, 0 - h3),
				new Vector2(right, 0 + h3),
				new Vector2(0, top),
				new Vector2(left, 0 + h3),
				new Vector2(left, 0 - h3)
			};
		}

		if (total == 7) {
			return new Vector2[] {
				number1, 
				new Vector2(right, 0 - h3),
				new Vector2(right, 0 + h3),
				new Vector2(w3, top),
				new Vector2(-w3, top),
				new Vector2(left, 0 + h3),
				new Vector2(left, 0 - h3)
			};
		}

		if (total == 8) {
			return new Vector2[] {
				number1, 
				new Vector2(right, 0 - h4),
				new Vector2(right, 0),
				new Vector2(right, 0 + h4),
				new Vector2(0, top),
				new Vector2(left, 0 + h4),
				new Vector2(left, 0),
				new Vector2(left, 0 - h4)
			};
		}

		if (total == 9) {
			return new Vector2[] {
				number1, 
				new Vector2(right, 0 - h4),
				new Vector2(right, 0),
				new Vector2(right, 0 + h4),
				new Vector2(w3, top),
				new Vector2(-w3, top),
				new Vector2(left, 0 + h4),
				new Vector2(left, 0),
				new Vector2(left, 0 - h4)
			};
		}

		return new Vector2[]{};
	}

	void ShowGameInfo() {
		gameInfoDictionary = new Dictionary<string, object>{
			{"sb", 10},
			{"bb", 20},
			{"owner", "singno"},
			{"IP", true},
			{"GPS", true}
		};

		
		AddGameInfo("[德州高手]");
		AddGameInfo("盲注:10/20/40");
		// AddGameInfo("IP、GPS限制");
	}

	void AddGameInfo(string text) {
		GameObject label = Instantiate(gameInfo);
		label.GetComponent<Text>().text = text;
		label.transform.SetParent(gameInfoParent.transform, false);
	}

	void  ShowPlayer(Vector2 vector) {
		Player player = new Player();	
		player.avatar = "https://ss1.bdstatic.com/70cFvXSh_Q1YnxGkpoWK1HF6hhy/it/u=3081053742,1983158129&fm=116&gp=0.jpg";
		player.name = "singno";
		player.score = 50000;

		GameObject playerObject = Instantiate(playerPrefab);
		playerObject.transform.Find("Name").GetComponent<Text>().text = player.name ;
		playerObject.transform.Find("Button").Find("Coins").GetComponent<Text>().text = player.score.ToString();
		Transform avbg = playerObject.transform.Find("AvatarBg");
		RawImage rawImage = avbg.Find("Avatar").GetComponent<RawImage>();
		playerObject.transform.SetParent(canvas.transform,  false);
		playerObject.GetComponent<RectTransform>().localPosition = vector;

		// Ext.Circular(avbg.GetComponent<RawImage>().texture);
		StartCoroutine(DownloadAvatar(rawImage, player.avatar));
	}

	IEnumerator<WWW> DownloadAvatar(RawImage img, string url) {
		WWW www = new WWW(url);
		yield return www;
		img.texture = Ext.Circular(www.texture);
	}		
}
