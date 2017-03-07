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
	public int AuditCD = 0; 
	public int Coins = 0;

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
		AuditCD = json.Int("unaudit_countdown");
		Coins = json.Int("coins");

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

	public ReactiveProperty<GameOverJson> OverData = new ReactiveProperty<GameOverJson>();

	public void Destroy() {
		Destroyed.Value = true;
	}
}

public class GameOverJson {
	public List<int> cards { get; set; }
	public int prize { get; set; }
	public int chips {get; set;}
	public string uid { get; set; }
	public int seat { get; set; }

	public int Gain() {
		return prize - chips;
	}

	public GameOverJson(Dictionary<string, object> dict) {
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
	
		RxSubjects.Look.Subscribe((e) => {
			GameStartState = false;
			byJson(e.Data);

			if (SceneManager.GetActiveScene().name == "PokerGame") {
				// Skip
			} else {
				SceneManager.LoadScene("PokerGame");
			}
		});

		RxSubjects.Deal.Subscribe((e) => {
			Pot.Value = e.Data.Int("pot");
			PrPot.Value = Pot.Value - e.Data.Int("pr_pot");

			GameStartState = true;

			// 发下一张牌的时候，重置所有prchips
			foreach(Player player in Players.Values) {
				player.PrChips.Value = 0;
			}

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

			MaxFiveRank.Value = e.Data.Int("maxFiveRank");
		});

		Action<RxData> updateCoins = (e) => {
			var index = e.Data.Int("where");
			var bankroll = e.Data.Int("bankroll");

			if (Players.ContainsKey(index)) {
				var player = Players[index];
				player.Bankroll.Value = bankroll;	
			}
		};
		
		RxSubjects.Ready.Subscribe(updateCoins);
		RxSubjects.TakeMore.Subscribe(updateCoins);
		
		RxSubjects.Pass.Subscribe((e) => {
			var index = e.Data.Int("where");
			var inGame = e.Data.Bool("is_ingame");
			var bankroll = e.Data.Int("bankroll");
			var coins = e.Data.Int("coins");

			Coins = coins;

			if (index < 0 || inGame || bankroll <= 0) {
				return ;
			}

			if (Players.ContainsKey(index)) {
				Players[index].Bankroll.Value = bankroll;
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
			SeeCardState = true;

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
				var json = new GameOverJson(dict); 
				var index = Convert.ToInt32(item.Key);

				if (Players.ContainsKey(index))  {
					Players[index].OverData.Value = json;
				}
			}

			var room = e.Data.Dict("room");
			Pot.Value = room.Int("pot");
			PrPot.Value = Pot.Value - room.Int("pr_pot");
		});

		RxSubjects.UnAuditCD.AsObservable().Where((e) => {
			return e.Data.String("uid") == GameData.Shared.Uid;
		}).Subscribe(
			(e) => {
				var sec = e.Data.Int("sec");
				if (sec <= 0) {
					return ;
				}

				AuditCD.Value = sec;
			}
		);

		RxSubjects.Audit.Subscribe((e) => {
			var array = e.Data.List("ids");
			AuditList.Value = array;
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

	public string Proxy;

	public ReactiveProperty<List<object>> AuditList = new ReactiveProperty<List<object>>();
	public bool GameStartState = false;
	public bool SeeCardState = false;
	
	public int ThinkTime = 15;
	public bool Owner = false;	
	public List<int> BankrollMul;
	public int PlayerCount;
	public string Sid; 
	public string Uid = "";
	public string Pin = "";
	public string Name = ""; 
	public string Avatar = "";
	public string Room;
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
	public long Duration = 0;
	public bool NeedAudit = false;
	public bool IPLimit = false;
	public bool GPSLimit = false;

	public ReactiveProperty<int> Pot = new ReactiveProperty<int>();
	public ReactiveProperty<int> PrPot = new ReactiveProperty<int>();

	public bool Paused = false;
	public string GameCode = "";
	public ReactiveProperty<int> MaxFiveRank = new ReactiveProperty<int>();

	public ReactiveProperty<int> DealerSeat = new ReactiveProperty<int>();

	public DateTime StartTime;
	public bool InGame = false;  

	public ReactiveProperty<bool> Muted = new ReactiveProperty<bool>(false);

	public ReactiveProperty<int> AuditCD = new ReactiveProperty<int>(); 

	private Dictionary<string, object> jsonData;

	public void Reload() {
		if (jsonData != null) {
			byJson(jsonData);
		}
	}

	private void byJson(Dictionary<string, object> json) {
		jsonData = json;

		// 除了gamestart状态外，其他状态都在这里重置
		SeeCardState = false;

		var options = json.Dict("options");
		var gamers = json.Dict("gamers");

		Owner = options.String("ownerid") == GameData.Shared.Uid;
		BankrollMul = options.IL("bankroll_multiple"); 
		Ante = options.Int("ant");
		PlayerCount = options.Int("max_seats");
		Rake = options.Float("rake_percent");
		Duration = options.Long("time_limit");
		NeedAudit = options.Int("need_audit") == 1;
		GPSLimit = options.Int("gps_limit") > 0;
		IPLimit = options.Int("ip_limit") == 1;
		GameCode = options.String("code");
		RoomName = json.String("name");
		DealerSeat.Value = json.Int("dealer_seat");
		Straddle = json.Int("straddle") != 0;

		Pot.Value = json.Int("pot");
		PrPot.Value = Pot.Value - json.Int("pr_pot");
		Paused = json.Int("is_pause") != 0;
		InGame = json.Bool("is_ingame");
		MaxFiveRank.Value = json.Int("maxFiveRank");
		MySeat = json.Int("my_seat");

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

		if (MySeat != -1) {
			AuditCD.Value = Players[MySeat].AuditCD;
			Coins = Players[MySeat].Coins;
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

	public Player GetMyPlayer() {
		foreach(var player in Players) {
			if (player.Value.Uid == Uid) {
				return player.Value;
			}
		}

		return null;
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