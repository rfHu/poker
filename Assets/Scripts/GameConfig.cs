using System.Collections.Generic;
using System;
using Extensions;
using UniRx;
using System.Linq;
using UnityEngine.SceneManagement;

public enum ActionState {
	None = 0,
	Fold = 1,
	Check = 2,
	Allin = 3,
	Call = 4,
	Raise = 5
}

sealed public class Player {
	sealed public class RestoreData {
		public int seconds = 0;
		public Dictionary<string, object> data; 
	}

	public string Name = "";
	public string Avatar = "";
	public string Uid = "";
	public ReactiveProperty<int> Bankroll = new ReactiveProperty<int>();
	public int Index = -1;
	public ReactiveProperty<int> PrChips = new ReactiveProperty<int>();
	public bool InGame = false;

	public BehaviorSubject<RestoreData> Countdown = new BehaviorSubject<RestoreData>(new RestoreData());

	public Subject<ActionState> ActState = new Subject<ActionState>();

	public ReactiveProperty<List<int>> Cards = new ReactiveProperty<List<int>>();
	
	public Player(Dictionary<string, object> json, int index) {
		Name = json.String("name");
		Avatar = json.String("avatar");
		Uid = json.String("uid");
		
		// 用户记分牌
		Bankroll.Value = json.Int("bankroll");

		// 用户该轮上的筹码
		PrChips.Value = json.Int("pr_chips");

		Index = index;
		InGame = json.Bool("is_ingame");	

		var cd = json.Int("turn_countdown");
		if (cd > 0) {
			var dt = new RestoreData();
			dt.seconds = cd;
			dt.data = new Dictionary<string, object>{
				{"cmds", json.Dict("cmds")}
			};

			Countdown.OnNext(dt);
		}

		var cards = json.IL("cards");
		Cards.Value = cards;
	}

	public Player() {}

	public ReactiveProperty<bool> Destroyed = new ReactiveProperty<bool>(false);

	public ReactiveProperty<GameoverJson> Winner = new ReactiveProperty<GameoverJson>();

	public void Destroy() {
		Destroyed.Value = true;
	}
}

public class GameoverJson {
	public List<int> cards { get; set; }
	public int prize { get; set; }
	public int chips {get; set;}
	public string uid { get; set; }
	public int seat { get; set; }

	public int Gain() {
		return prize - chips;
	}

	public GameoverJson(Dictionary<string, object> dict) {
		prize = dict.Int("prize");
		chips = dict.Int("chips");
		uid = dict.String("uid");
		seat = dict.Int("seat");
		cards = dict.IL("cards");
	}
}

sealed public class GameData {
	private GameData() {
		RxSubjects.TakeSeat.AsObservable().Subscribe((e) => {
			var index = e.Data.Int("where");
			var playerInfo = e.Data.Dict("who");
			var player = new Player(playerInfo, index);

			GameData.Shared.Players[index] = player;
		});

		RxSubjects.Paused.AsObservable().Subscribe((e) => {
			Paused = true;
		});

		RxSubjects.Started.AsObservable().Subscribe((e) => {
			Paused = false;
		});

		RxSubjects.UnSeat.AsObservable().Subscribe((e) => {
			var index = e.Data.Int("where");
			GameData.Shared.Players.Remove(index);
		});

		RxSubjects.GameStart.AsObservable().Subscribe((e) => {
			var json = e.Data.Dict("room");
			GameStartState = true;
			byJson(json);
		});

		RxSubjects.Deal.Subscribe((e) => {
			Pot.Value = e.Data.Int("pot");
			PrPot.Value = Pot.Value - e.Data.Int("pr_pot");

			// 发下一张牌的时候，重置所有prchips
			foreach(Player player in Players.Values) {
				player.PrChips.Value = 0;
			}
		});

		var sceneLoaded = false; 
		RxSubjects.Look.Subscribe((e) => {
			GameStartState = false;
			byJson(e.Data);

			// 只允许进入一次
			if (sceneLoaded) {
				// Skip
			} else {
				SceneManager.LoadScene("PokerGame");
			}

			sceneLoaded = true;
		});

		RxSubjects.Deal.Subscribe((e) => {
			var data = e.Data.Dict("deals");

			foreach(KeyValuePair<string, object>item in data) {
				var list = item.Value.GetIL();

				if (list == null || list.Count <= 0) {
					continue;
				}

				if (item.Key == "-1") {
					foreach(int pb in list) {
						PublicCards.Add(pb);
					}
				} else {
					var k = Convert.ToInt16(item.Key);
					if (!Players.ContainsKey(k)) {
						continue;
					}

					Players[k].Cards.Value = list;
				}
			}
		});
		
		RxSubjects.Ready.Subscribe((e) => {
			var index = e.Data.Int("where");
			var bankroll = e.Data.Int("bankroll");

			if (Players.ContainsKey(index)) {
				var player = Players[index];
				player.Bankroll.Value = bankroll;	
			}
		});

		RxSubjects.Fold.Subscribe((e) => {
			var index = e.Data.Int("seat");

			if (Players.ContainsKey(index)) {
				Players[index].ActState.OnNext(ActionState.Fold);
			}
		});

		Action<RxData> act = (e) => {
			var mop = e.Data.ToObject<Mop>();
			var map = new Dictionary<string, ActionState>() {
				{"call", ActionState.Call},
				{"check", ActionState.Check},
				{"all_in", ActionState.Allin},
				{"raise", ActionState.Raise}
			};

			Pot.Value = mop.pot;

			var index = mop.seat;
			if (!Players.ContainsKey(index)) {
				return ;
			}
			
			var player = Players[index];
			player.PrChips.Value = mop.pr_chips;
			player.Bankroll.Value = mop.bankroll;

			player.ActState.OnNext(map[e.E]);
		};

		RxSubjects.Call.Subscribe(act);
		RxSubjects.AllIn.Subscribe(act);
		RxSubjects.Check.Subscribe(act);
		RxSubjects.Raise.Subscribe(act);

		RxSubjects.SeeCard.Subscribe((e) => {
			var cards = e.Data.IL("cards");
			var index = e.Data.Int("seat");
			if (Players.ContainsKey(index)) {
				Players[index].Cards.Value = cards;
			}
		});

		RxSubjects.GameOver.Subscribe((e) => {
			var data = e.Data.Dict("scorelist");

			foreach(KeyValuePair<string, object> item in data) {
				var dict = (Dictionary<string, object>)item.Value;
				var json = new GameoverJson(dict); 
				var index = Convert.ToInt32(item.Key);

				if (Players.ContainsKey(index))  {
					Players[index].Winner.Value = json;
				}
			}

			var room = e.Data.Dict("room");
			Pot.Value = room.Int("pot");
			PrPot.Value = Pot.Value - room.Int("pr_pot");
		});
	}

	public Player FindMyPlayer() {
		foreach (var player in Players) {
			if (player.Value.Uid == Uid) {
				return player.Value;
			}
		}

		return null;
	}

	public bool GameStartState = false;

	public int ThinkTime = 15;
	public bool Owner = false;	
	public List<int> BankrollMul;
	public int PlayerCount;
	public string UserToken = ""; 
	public string Uid = "";
	public string Pin = "";
	public string Name = ""; 
	public string Avatar = "";
	public string Room = "587450f87133077573039b92";
	public int MySeat = -1;
	public int Ante = 0;
	public int Coins = 0;
	public int SB = 0;
	public int BB = 0;
	public bool Straddle = false;
	public string RoomName = "";

	// 游戏是否已经开始，跟暂停状态无关
	public bool GameStarted = false; 
	public float Rake = 0;
	public int Duration = 0;
	public bool NeedAduit = false;
	public bool IPLimit = false;
	public bool GPSLimit = false;

	public ReactiveProperty<int> Pot = new ReactiveProperty<int>();
	public ReactiveProperty<int> PrPot = new ReactiveProperty<int>();

	public bool Paused = false;
	public string GameCode = "";

	public ReactiveProperty<int> DealerSeat = new ReactiveProperty<int>();

	public DateTime StartTime;
	public bool InGame = false;  

	private Dictionary<string, object> jsonData;

	public void Reload() {
		if (jsonData != null) {
			byJson(jsonData);
		}
	}

	private void byJson(Dictionary<string, object> json) {
		jsonData = json;

		var options = json.Dict("options");
		var gamers = json.Dict("gamers");

		Owner = options.String("ownerid") == GameData.Shared.Uid;
		BankrollMul = options.IL("bankroll_multiple"); 
		Ante = options.Int("ant");
		PlayerCount = options.Int("max_seats");
		Rake = options.Float("rake_percent");
		Duration = options.Int("time_limit");
		NeedAduit = options.Int("need_audit") == 1;
		GPSLimit = options.Int("gps_limit") == 1;
		IPLimit = options.Int("ip_limit") == 1;
		GameCode = options.String("code");
		RoomName = json.String("name");
		DealerSeat.Value = json.Int("dealer_seat");
		Straddle = json.Int("straddle") != 0;

		Pot.Value = json.Int("pot");
		PrPot.Value = Pot.Value - json.Int("pr_pot");
		Paused = json.Int("is_pause") != 0;
		InGame = json.Bool("is_ingame");

		// 删除公共牌重新添加
		var cards = json.IL("shared_cards");
		PublicCards.Clear();
		foreach(int value in cards) {
			PublicCards.Add(value);
		}
		
		var startTs = json.Int("begin_time");
		StartTime = _.DateTimeFromTimeStamp(startTs);

		if (startTs != 0)
        {
			GameStarted = true;
        }

		var bb = options.Int("limit");
		BB = bb ;
		SB = bb / 2;

		// 逐个删除，才能触发Remove事件
		foreach(var key in Players.Keys.ToList()) {
			Players.Remove(key);
		}		

		foreach(KeyValuePair<string, object> entry in gamers) {
			var dict = entry.Value as Dictionary<string, object>;

			if (dict == null) {
				continue;
			}

			var index = Convert.ToInt32(entry.Key);
			var player = new Player(dict, index);
			Players[index] = player;
		}
	}

	public static GameData Shared = new GameData();

	public ReactiveDictionary<int, Player> Players = new ReactiveDictionary<int, Player>(); 

	public Player GetPlayer(int index) {
		if (Players.ContainsKey(index)) {
			return Players[index];
		}

		return new Player();
	}

	public ReactiveCollection<int> PublicCards = new ReactiveCollection<int>();

	public class MyCmd {
		public static bool Takecoin = false;
		public static bool Unseat = false;

		public static void SetCmd(Dictionary<string, object> data) {
			// 目前只处理带入记分牌与站起事件
			foreach(KeyValuePair<string, object> entry in data) {
				if (entry.Key == "takecoin") {
					MyCmd.Takecoin = Convert.ToBoolean(entry.Value);
			   	} else if (entry.Key == "unseat") {
					MyCmd.Unseat = Convert.ToBoolean(entry.Value);	   
				}
			}
		}
	}
}