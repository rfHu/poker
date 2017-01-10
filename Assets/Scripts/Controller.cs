using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Controller : MonoBehaviour {
	public GameObject seat;
	public Canvas canvas;

	public GameObject gameInfo;
	public GameObject gameInfoWrapper;
	public GameObject playerPrefab;
	public GameObject startButton;

	public Dictionary<int, PlayerObject> Players = new Dictionary<int, PlayerObject>();

	void Start () {
		List<Button> buttons = new List<Button>();
		int numberOfPlayers = GConf.playerCount;

		for (int i = 0; i < numberOfPlayers; i++) {
			GameObject copySeat = Instantiate (seat);
			copySeat.transform.SetParent (canvas.transform, false);
			buttons.Add (copySeat.GetComponent<Button>());
		}

		Vector2[] vectors = GetVectors (numberOfPlayers);
		int iter = 0;

		foreach(Button button in buttons) {
			button.GetComponent<RectTransform> ().localPosition = vectors[iter] ;

			int identifer = iter;
			Vector2 vector = vectors[identifer];

			button.onClick.AddListener(() => {
				GameObject playerObject = Instantiate(playerPrefab);
				PlayerObject player = playerObject.GetComponent<PlayerObject>();
				player.Index = identifer;

				// player.ShowPlayer();
				// playerObject.transform.SetParent(canvas.transform,  false);
				// playerObject.GetComponent<RectTransform>().localPosition = vector;

				Players.Add(player.Index, player);

				// 坐下
				Connect.shared.Emit(
					new Dictionary<string, object>(){
						{"f", "takeseat"},
						{"args", player.Index.ToString()}
					}
				);		
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
		if (GConf.isOwner) {
			startButton.SetActive(true);
		}

		AddGameInfo(string.Format("[ {0} ]", GConf.roomName));

		if (GConf.isStraddle) {
			AddGameInfo(string.Format("盲注：{0}/{1}/{2}", GConf.sb, GConf.bb, GConf.bb * 2));			
 		} else {
			AddGameInfo(string.Format("盲注：{0}/{1}", GConf.sb, GConf.bb));
		}

        if (GConf.IPLimit && GConf.GPSLimit) {
			AddGameInfo("IP、GPS限制");
		} else if (GConf.GPSLimit) {
			AddGameInfo("GPS限制");
		} else if (GConf.IPLimit) {
			AddGameInfo("IP限制");
		}
	}

	void AddGameInfo(string text) {
		GameObject label = Instantiate(gameInfo);
		label.GetComponent<Text>().text = text;
		label.transform.SetParent(gameInfoWrapper.transform, false);
	}
}
