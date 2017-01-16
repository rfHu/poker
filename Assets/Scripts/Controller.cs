﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Extensions;

public class Controller : MonoBehaviour {
	public GameObject seat;
	public Canvas canvas;

	public GameObject gameInfo;
	public GameObject gameInfoWrapper;
	public GameObject startButton;

	public List<Vector2> positions = new List<Vector2>(); 

	public GameObject PublicCards;

	void Start () {
		List<Button> buttons = new List<Button>();
		int numberOfPlayers = GConf.playerCount;

		for (int i = 0; i < numberOfPlayers; i++) {
			GameObject copySeat = Instantiate (seat);
			copySeat.transform.SetParent (canvas.transform, false);
			buttons.Add (copySeat.GetComponent<Button>());
		}

		positions = GetVectors (numberOfPlayers);
		int iter = 0;

		foreach(Button button in buttons) {
			button.GetComponent<RectTransform> ().localPosition = positions[iter] ;
			int identifer = iter;

			button.onClick.AddListener(() => {
				// 已有座位
				if (GConf.MyCmd.Unseat) {
					return ;
				}

				// 坐下
				Connect.shared.Emit(
					new Dictionary<string, object>(){
						{"f", "takeseat"},
						{"args", identifer}
					}
				);		
			});

			iter++;
		}

		ShowGameInfo();
		addListeners();
	}

	// 逆时针生成位置信息
	List<Vector2> GetVectors(int total) {
		float width = canvas.GetComponent<RectTransform>().rect.width;
		float height = canvas.GetComponent<RectTransform>().rect.height;

		float top = height / 2 - 150;
		float bottom = -height / 2 + 180;
		float right = width / 2 - 100;
		float left = -width / 2 + 100; 

		Vector2 number1 = new Vector2 (0, bottom);

		if (total == 2) {
			return new List<Vector2>{
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
			return new List<Vector2> {
				number1, 
				new Vector2(right, 0 - h3),
				new Vector2(right, 0 + h3),
				new Vector2(0, top),
				new Vector2(left, 0 + h3),
				new Vector2(left, 0 - h3)
			};
		}

		if (total == 7) {
			return new List<Vector2> {
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
			return new List<Vector2> {
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
			return new List<Vector2> {
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

		throw new Exception("不支持游戏人数");
	}

	void ShowGameInfo() {
		if (GConf.isOwner && !GConf.GameStarted) {
			startButton.SetActive(true);
		}

		AddGameInfo(string.Format("{0}", GConf.roomName));

		if (GConf.isStraddle) {
			AddGameInfo(string.Format("盲注:{0}/{1}/{2}", GConf.sb, GConf.bb, GConf.bb * 2));			
 		} else {
			AddGameInfo(string.Format("盲注:{0}/{1}", GConf.sb, GConf.bb));
		}

        if (GConf.IPLimit && GConf.GPSLimit) {
			AddGameInfo("IP、GPS限制");
		} else if (GConf.GPSLimit) {
			AddGameInfo("GPS限制");
		} else if (GConf.IPLimit) {
			AddGameInfo("IP限制");
		}

		showPlayers();
	}

	void showPlayers() {
		foreach(KeyValuePair<int, Player> entry in GConf.Players) {
			showPlayer(entry.Value);
		}
	}

	void AddGameInfo(string text) {
		GameObject label = Instantiate(gameInfo);
		label.GetComponent<Text>().text = text;
		label.transform.SetParent(gameInfoWrapper.transform, false);
	}

	void addListeners() {
		Delegates.shared.TakeSeat += new EventHandler<DelegateArgs>(onTakeSeat);
		Delegates.shared.UnSeat += new EventHandler<DelegateArgs>(onUnSeat);
		Delegates.shared.Ready += new EventHandler<DelegateArgs>(onReady);
		Delegates.shared.GameStart += new EventHandler<DelegateArgs>(onGameStart);
		Delegates.shared.SeeCard += new EventHandler<DelegateArgs>(onSeeCard);
		Delegates.shared.Deal += new EventHandler<DelegateArgs>(onDeal);
	}

	void removeListeners() {
		Delegates.shared.TakeSeat -= new EventHandler<DelegateArgs>(onTakeSeat);
		Delegates.shared.TakeSeat -= new EventHandler<DelegateArgs>(onUnSeat);
		Delegates.shared.Ready -= new EventHandler<DelegateArgs>(onReady);
		Delegates.shared.GameStart -= new EventHandler<DelegateArgs>(onGameStart);
		Delegates.shared.SeeCard -= new EventHandler<DelegateArgs>(onSeeCard);
		Delegates.shared.Deal -= new EventHandler<DelegateArgs>(onDeal);
	}

	void OnDestroy()
	{
		removeListeners();
	}

	void  onTakeSeat(object sender, DelegateArgs e) {
		var args = e.Data.Dict("args");
		var index = args.Int("where");
		var playerInfo = args.Dict("who");
		var player = new Player(playerInfo, index);
		GConf.Players.Add(index, player);
		showPlayer(player);	
	}

	void onReady(object sender, DelegateArgs e) {
		var args = e.Data.Dict("args");
		var index = args.Int("where");
		var bankroll = args.Int("bankroll");

		playerObjects[index].SetScore(bankroll);
	}

	void onUnSeat(object sender, DelegateArgs e) {
		var args = e.Data.Dict("args");

		if (!args.ContainsKey("where")) {
			return ;
		}

		var index = args.Int("where");
		RemovePlayer(index);
	}

	Dictionary<int, PlayerObject> playerObjects = new Dictionary<int, PlayerObject>();

	void showPlayer(Player data) {
		GameObject go = (GameObject)Instantiate(Resources.Load("Prefab/Player"));
		PlayerObject playerObject = go.GetComponent<PlayerObject>();
	 	playerObject.Index = data.Index;
		playerObject.Uid = data.Uid;

		playerObject.ShowPlayer(data);
        playerObject.transform.SetParent(canvas.transform, false);
        playerObject.GetComponent<RectTransform>().localPosition = positions[data.Index];

		playerObjects.Add(playerObject.Index, playerObject);
	}

	public void RemovePlayer(int index) {
		PlayerObject player;
		playerObjects.TryGetValue(index, out player);

		if (player == null) {
			return ;
		}

		playerObjects.Remove(index);
		Destroy(player.gameObject);
		GConf.Players.Remove(index);
	}

	int FindMyIndex() {
		foreach(KeyValuePair<int, PlayerObject> entry in playerObjects) {
			if (entry.Value.Uid == GConf.Uid) {
				return entry.Key;
			}
		}

		return -1;
	}

	void onDeal(object sender, DelegateArgs e) {
		var deals = e.Data.Dict("args").Dict("deals").IL("-1");
		
		if (deals.Count <= 0) {
			return ;
		}

		foreach(int item in deals) {
			var idx = cardIndex(item);
			var card = (GameObject)Instantiate(Resources.Load("Prefab/Card"));
			card.GetComponent<Card>().Show(idx);
			card.transform.SetParent(PublicCards.transform, false);
		}
	}

	void addPublicCard(int index = -1) {
		var go = (GameObject)Instantiate(Resources.Load("Prefab/Card"));
		var card = go.GetComponent<Card>();

		if (index == -1) {
			// skip
		} else {
			card.Show(index);
		}
        
		go.GetComponent<RectTransform>().sizeDelta = new Vector2(70, 0);
        go.transform.SetParent(PublicCards.transform, false);
	}

	void onGameStart(object sender, DelegateArgs e) {
		PublicCards.transform.Clear();

		// 发三张公共牌
		for (int i = 0; i < 3; i++) {
			addPublicCard();	
		}

		var uid = e.Data.String("uid");
		var index = FindMyIndex();

		foreach(KeyValuePair<int, PlayerObject> entry in playerObjects) {
			// 我自己
			if (entry.Key == index) {
				// skip
			} else {
				var gameObj = entry.Value.Cardfaces;
				gameObj.SetActive(true);
			}
		}
	}

	void onSeeCard(object sender, DelegateArgs e) {
		 var index = FindMyIndex();
		 var cards = e.Data.Dict("args").IL("cards");
		 
		 int[] cvs = new int[]{
			 cardIndex(cards[0]),
			 cardIndex(cards[1])
		 };

		 var playerObject = playerObjects[index];
		 var first = playerObject.MyCards.transform.Find("First");
		 var second = playerObject.MyCards.transform.Find("Second");

		 playerObject.MyCards.SetActive(true);

		 // 隐藏自己的名称
		 playerObject.transform.Find("Name").gameObject.SetActive(false);

		 first.GetComponent<Card>().Show(cvs[0]);
		 second.GetComponent<Card>().Show(cvs[1]);
	}

	int cardIndex(int number) {
		var pairs = cardValues(number);
		int index;

		// 服务器数值为2~14
		if (pairs[1] ==  14) {
			index = 0;
		} else {
			index = pairs[1] - 1;
		}

		index = index + (4 - pairs[0]) * 13;

		return index;
	}

	int[] cardValues(int number) {
		var a = number >> 4;
		var b = number & 0x0f;

		// 第一个花色、第二个数值
		return new int[]{a, b};
	}
}
