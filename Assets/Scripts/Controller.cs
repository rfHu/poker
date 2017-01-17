using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Extensions;
using UIWidgets;

public class Controller : MonoBehaviour {
	public GameObject seat;
	public Canvas canvas;

	public GameObject gameInfo;
	public GameObject gameInfoWrapper;
	public GameObject startButton;

	public List<Vector2> positions = new List<Vector2>(); 

	public List<GameObject> PublicCards;

	public GameObject Pot;

	public List<GameObject> Seats;	

	void Start () {
		List<Button> buttons = new List<Button>();
		int numberOfPlayers = GConf.playerCount;

		for (int i = 0; i < numberOfPlayers; i++) {
			GameObject copySeat = Instantiate (seat);
			copySeat.transform.SetParent (canvas.transform, false);
			buttons.Add (copySeat.GetComponent<Button>());
			Seats.Add (copySeat);
		}

		positions = GetVectors (numberOfPlayers);
		int iter = 0;

		foreach(Button button in buttons) {
			button.GetComponent<RectTransform> ().anchoredPosition = positions[iter] ;
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

		float top = height - 200;
		float bottom = 250; 
		float right = width / 2 - 100;
		float left = -width / 2 + 100; 

		float hh = top - bottom;
		float ww = width - 200;

		float h3 = hh / 3;
		float h4 = hh / 4;
		float w3 = ww / 2 - ww / 3;

		Vector2 vector = new Vector2 (0, bottom);

		if (total == 2) {
			return new List<Vector2>{
				vector,
				new Vector2(0, top)
			};
		}

		if (total == 6) {
			return new List<Vector2> {
				vector, 
				new Vector2(right, bottom + h3),
				new Vector2(right, bottom + 2 * h3),
				new Vector2(0, top),
				new Vector2(left, bottom + 2 * h3),
				new Vector2(left, bottom + h3)
			};
		}

		if (total == 7) {
			return new List<Vector2> {
				vector, 
				new Vector2(right, bottom + h3),
				new Vector2(right, bottom + 2 * h3),
				new Vector2(w3, top),
				new Vector2(-w3, top),
				new Vector2(left, bottom + 2 * h3),
				new Vector2(left, bottom + h3)
			};
		}

		if (total == 8) {
			return new List<Vector2> {
				vector, 
				new Vector2(right, bottom + h4),
				new Vector2(right, bottom + 2 * h4),
				new Vector2(right, bottom + 3 * h4),
				new Vector2(0, top),
				new Vector2(left, bottom + 3 * h4),
				new Vector2(left, bottom + 2 * h4),
				new Vector2(left, bottom + h4)
			};
		}

		if (total == 9) {
			return new List<Vector2> {
				vector, 
				new Vector2(right, bottom + h4),
				new Vector2(right, bottom + 2 * h4),
				new Vector2(right, bottom + 3 * h4),
				new Vector2(w3, top),
				new Vector2(-w3, top),
				new Vector2(left, bottom + 3 * h4),
				new Vector2(left, bottom + 2 * h4),
				new Vector2(left, bottom + h4)
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
		Delegates.shared.MoveTurn += new EventHandler<DelegateArgs>(onMoveTurn);
		Delegates.shared.GameOver += new EventHandler<DelegateArgs>(onGameOver);

		// 游戏操作相关
		Delegates.shared.Check += new EventHandler<DelegateArgs>(onCheck);
		Delegates.shared.Fold += new EventHandler<DelegateArgs>(onFold);
		Delegates.shared.AllIn += new EventHandler<DelegateArgs>(onAllIn);
		Delegates.shared.Raise += new EventHandler<DelegateArgs>(onRaise);
		Delegates.shared.Call += new EventHandler<DelegateArgs>(onCall);
	}

	void removeListeners() {
		Delegates.shared.TakeSeat -= new EventHandler<DelegateArgs>(onTakeSeat);
		Delegates.shared.TakeSeat -= new EventHandler<DelegateArgs>(onUnSeat);
		Delegates.shared.Ready -= new EventHandler<DelegateArgs>(onReady);
		Delegates.shared.GameStart -= new EventHandler<DelegateArgs>(onGameStart);
		Delegates.shared.SeeCard -= new EventHandler<DelegateArgs>(onSeeCard);
		Delegates.shared.Deal -= new EventHandler<DelegateArgs>(onDeal);
		Delegates.shared.MoveTurn -= new EventHandler<DelegateArgs>(onMoveTurn);
		Delegates.shared.GameOver += new EventHandler<DelegateArgs>(onGameOver);

		// 游戏操作相关
		Delegates.shared.Check -= new EventHandler<DelegateArgs>(onCheck);
		Delegates.shared.Fold -= new EventHandler<DelegateArgs>(onFold);
		Delegates.shared.AllIn -= new EventHandler<DelegateArgs>(onAllIn);
		Delegates.shared.Raise -= new EventHandler<DelegateArgs>(onRaise);
		Delegates.shared.Call -= new EventHandler<DelegateArgs>(onCall);
	}

	void OnDestroy()
	{
		removeListeners();
	}

	void  onTakeSeat(object sender, DelegateArgs e) {
		var args = e.Data;
		var index = args.Int("where");
		var playerInfo = args.Dict("who");
		var player = new Player(playerInfo, index);

		// 存在用户的情况下，认为数据出错，强制执行删除
		if (playerObjects.ContainsKey(index)) {
			RemovePlayer(index);
		}

		GConf.Players.Add(index, player);
		showPlayer(player);	
	}

	void onReady(object sender, DelegateArgs e) {
		var args = e.Data;
		var index = args.Int("where");
		var bankroll = args.Int("bankroll");

		playerObjects[index].SetScore(bankroll);
	}

	void onUnSeat(object sender, DelegateArgs e) {
		var args = e.Data;

		if (!args.ContainsKey("where")) {
			return ;
		}

		var index = args.Int("where");
		RemovePlayer(index);
	}

	private int prevMoveTurnIndex = -1;

	void onMoveTurn(object sender, DelegateArgs e) {
		PlayerObject prevObj;
		playerObjects.TryGetValue(prevMoveTurnIndex, out prevObj);

		if (prevObj != null) {
			prevObj.MoveOut();
		}

		var index = e.Data.Int("seat");
		prevMoveTurnIndex = index;

		playerObjects[index].TurnTo(e.Data);
	}

	Dictionary<int, PlayerObject> playerObjects = new Dictionary<int, PlayerObject>();

	void showPlayer(Player data) {
		GameObject go = (GameObject)Instantiate(Resources.Load("Prefab/Player"));
		PlayerObject playerObject = go.GetComponent<PlayerObject>();
	 	playerObject.Index = data.Index;
		playerObject.Uid = data.Uid;

		playerObject.ShowPlayer(data);
		playerObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
		playerObject.transform.SetParent(Seats[data.Index].transform, false);
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

	Card findLastCard() {
		foreach(GameObject obj in PublicCards) {
			var card = obj.GetComponent<Card>();
			if (card.IsBack) {
				return card;
			}
		}

		return null;
	}

	void onDeal(object sender, DelegateArgs e) {
		var deals = e.Data.Dict("deals").IL("-1");
		
		if (deals.Count <= 0) {
			return ;
		}

		foreach(int item in deals) {
			var idx = Controller.CardIndex(item);
			var card = findLastCard();

			if (card != null) {
				card.Show(idx);
			}
		}
	}

	void updatePot(int pot, int prev) {
		Pot.GetComponent<Pots>().PrevPot.text = prev.ToString();
		Pot.GetComponent<Pots>().DC.text =  "底池:" + pot.ToString();
	}

	void resetAllCards() {
		foreach(GameObject obj in PublicCards) {
			obj.GetComponent<Card>().Turnback();
			obj.SetActive(false);
		}		
	}

	void onGameStart(object sender, DelegateArgs e) {
		resetAllCards();
		Pot.SetActive(true);

		var args = e.Data.Dict("room");
		var pot = args.Int("pot");
		updatePot(pot, pot - args.Int("pr_pot"));	

		var gamers = args.Dict("gamers");
		foreach(KeyValuePair<string, object> entry in gamers) {
			var idx = Convert.ToInt32(entry.Key);
			var dict = entry.Value as Dictionary<string, object>;

			if (dict == null) {
				continue;
			}

			var prchips = dict.Int("pr_chips");

			if (prchips != 0) {
				playerObjects[idx].transform.Find("Chips").gameObject.SetActive(true);
				playerObjects[idx].Chips.text = prchips.ToString();
			}
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
		 var cards = e.Data.IL("cards");
		 
		 int[] cvs = new int[]{
			 Controller.CardIndex(cards[0]),
			 Controller.CardIndex(cards[1])
		 };

		 var playerObject = playerObjects[index];
		 var first = playerObject.MyCards.transform.Find("First");
		 var second = playerObject.MyCards.transform.Find("Second");

		 playerObject.MyCards.SetActive(true);

		 // 隐藏自己的名称
		 playerObject.HideName();
		 first.GetComponent<Card>().Show(cvs[0]);
		 second.GetComponent<Card>().Show(cvs[1]);
	}

	public static int CardIndex(int number) {
		var pairs = Controller.CardValues(number);
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

	public static int[] CardValues(int number) {
		var a = number >> 4;
		var b = number & 0x0f;

		// 第一个花色、第二个数值
		return new int[]{a, b};
	}

	void setChips(DelegateArgs e) {
		var mop = e.Data.ToObject<Mop>();

		if (!playerObjects.ContainsKey(mop.seat)) {
			return ;
		}

		playerObjects[mop.seat].Chips.text = mop.pr_chips.ToString();
	}

	void onCheck(object sender, DelegateArgs e) {
		// Do nothing
	}

	void onRaise(object sender, DelegateArgs e) {
		setChips(e);
	}

	void onCall(object sender, DelegateArgs e) {
		setChips(e);		
	}
	
	void onAllIn(object sender, DelegateArgs e) {
		setChips(e);
	}
	
	void onFold(object sender, DelegateArgs e) {
		var mop = e.Data.ToObject<Mop>();
		playerObjects[mop.seat].transform.Find("Info").GetComponent<CanvasGroup>().alpha = 0.7f;	
	}

	void onGameOver(object sender, DelegateArgs e) {
		Debug.Log("GameOver");	
	}
}
