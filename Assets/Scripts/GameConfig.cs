using System.Collections.Generic;
using System;
using UniRx;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;

public enum ActionState {
	None = 0,
	Fold = 1,
	Check = 2,
	Allin = 3,
	Call = 4,
	Raise = 5
}

public static class ActionStateExt {
	public static ActionState ToActionEnum(this String str) {
		var map = new Dictionary<string, ActionState>() {
			{"call", ActionState.Call},
			{"check", ActionState.Check},
			{"all_in", ActionState.Allin},
			{"raise", ActionState.Raise},
			{"fold", ActionState.Fold}
		};

		if (map.ContainsKey(str)) {
			return map[str];			
		}

		return ActionState.None;
	}
}

// 1、带入中，2、审核中，3、游戏中，4、留座中，5、托管中
public enum PlayerState {
	Waiting = 1,
	Auditing,
	Normal,
	Reserve,
	Hanging
}

public class AutoDeposit {
	public ReactiveProperty<string> SelectedFlag = new ReactiveProperty<string>();
	public ReactiveProperty<int> CallNumber = new ReactiveProperty<int>();

	public ReactiveProperty<bool> ShouldShow = new ReactiveProperty<bool>();

	public void SetDeposit(string flag, int num) {
		if (flag.Length < 2) {
			flag = "0" + flag;
		}

		CallNumber.Value = num;
		ShouldShow.Value = true;
	}

	public void Hide() {
		SelectedFlag.Value = null;
		CallNumber.Value = -100;
		ShouldShow.Value = false;
	}

	public string FlagString() {
		var flag = SelectedFlag.Value;

		if (flag == null) {
			return "00";
		}

		return flag;
	}
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
	public bool ChipsChange = false;
	public ReactiveProperty<bool> Allin = new ReactiveProperty<bool>(); 

	public bool ActStateTrigger = true;

	public AutoDeposit Trust = new AutoDeposit(); 
	public ReactiveProperty<string> ShowCard = new ReactiveProperty<string>();

	public BehaviorSubject<RestoreData> Countdown = new BehaviorSubject<RestoreData>(new RestoreData());

	public Subject<ActionState> ActState = new Subject<ActionState>();

	public ReactiveProperty<List<int>> Cards = new ReactiveProperty<List<int>>();

	public bool SeeCardAnim = false;

	public BehaviorSubject<PlayerState> PlayerStat = new BehaviorSubject<PlayerState>(PlayerState.Normal);

	public BehaviorSubject<int> ReservedCD = new BehaviorSubject<int>(0); 

	public ReactiveProperty<String> LastAct = new ReactiveProperty<String>();

	public void SetState(int state, int cd = 0) {
		var st = (PlayerState)state;
		PlayerStat.OnNext(st);

		if (Uid == GameData.Shared.Uid) {
			GameData.Shared.SelfState.Value = st;
		}

		ReservedCD.OnNext(cd);
	}
	
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
		Allin.Value = json.Bool("is_allin");
		LastAct.Value = json.String("last_act");

		var showValue = Convert.ToString(json.Int("showcard"), 2);

		if (showValue.Length < 2) {
			showValue = "0" + showValue;
		} 

		ShowCard.Value = showValue;

		var trust = json.Dict("trust");
		SetTrust(trust);

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

		var state = json.Int("gamer_state");
		var ucd = json.Int("unseat_countdown");
		SetState(state, ucd);
	}

	public Player() {}

	public void SetTrust(Dictionary<string, object> trust) {
		if (trust.ContainsKey("chooseid") && trust.ContainsKey("call")) {
			var flag = Convert.ToString(trust.Int("chooseid"), 2);
			var num = trust.Int("call");

			Trust.SetDeposit(flag, num);
		} else {
			Trust.ShouldShow.Value = false;
		}
	}

	public bool IsValid() {
		return !string.IsNullOrEmpty(Uid);
	}

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
	public int maxFiveRank = 0;

	public int Gain() {
		return prize - chips;
	}

	public GameOverJson(Dictionary<string, object> dict) {
		prize = dict.Int("prize");
		chips = dict.Int("chips");
		uid = dict.String("uid");
		seat = dict.Int("seat");
		cards = dict.IL("cards");
		maxFiveRank = dict.Int("maxFiveRank");	
	}
}

sealed public class GameData {
	private GameData() {
		SceneManager.sceneLoaded += (s, mode) => {
			if (s.name == "PokerGame") {
				byJson(jsonData);
			}
		};

		RxSubjects.TakeSeat.AsObservable().Subscribe((e) => {
			var index = e.Data.Int("where");
			var playerInfo = e.Data.Dict("who");
			var player = new Player(playerInfo, index);

			GameData.Shared.Players[index] = player;
		});

		RxSubjects.Paused.AsObservable().Subscribe((e) => {
			Paused.OnNext(true); 	
		});

		RxSubjects.Started.AsObservable().Subscribe((e) => {
			GameStarted = true;
			LeftTime.Value = e.Data.Int("left_time");
			Paused.OnNext(false); 
		});

		RxSubjects.UnSeat.AsObservable().Subscribe((e) => {
			var index = e.Data.Int("where");
			GameData.Shared.Players.Remove(index);

			var uid = e.Data.String("uid");
			// 清空数据
			if (uid == Uid) {
				GameData.Shared.MaxFiveRank.Value = 0;
			}
		});

		RxSubjects.GameStart.AsObservable().Subscribe((e) => {
			var json = e.Data.Dict("room");
			PublicCardAnimState = true;

			// 保存最新游戏数据
			jsonData = json;

			byJson(json);
		});
	
		RxSubjects.Look.Subscribe((e) => {
			PublicCardAnimState = false;
			var loginStatus = e.Data.Int("is_enter_look");

			// 保存最新游戏数据
			jsonData = e.Data;

			var scene = SceneManager.GetActiveScene();

			if (scene.name == "PokerGame") {
				byJson(e.Data);
			} else {
                Players.Clear();
				SceneManager.LoadScene("PokerGame");
			}
		});

		RxSubjects.Deal.Subscribe((e) => {
			Pot.Value = e.Data.Int("pot");

			if (e.Data.ContainsKey("pots")) {
				Pots.Value = e.Data.DL("pots");
			}

			PublicCardAnimState = true;

			var state = e.Data.Int("state");

			// 发下一张牌的时候，重置所有prchips
			foreach(Player player in Players.Values) {
				player.PrChips.Value = 0;
				player.ChipsChange = false;
			}

			var data = e.Data.Dict("deals");
			foreach(KeyValuePair<string, object>item in data) {
				var list = item.Value.GetIL();

				if (list == null || list.Count <= 0) {
					continue;
				}

				if (item.Key == "-1") {
					setPbCards(list, state);
				} else {
					var k = Convert.ToInt16(item.Key);
					if (!Players.ContainsKey(k)) {
						continue;
					}

					var player = Players[k];
					player.SeeCardAnim = true;
					player.Cards.Value = list;
				}
			}

			var pbList = data.IL("-1");
			var delay = 0.5f;
			
			if (pbList.Count >= 3) { // 等待亮牌动画
				delay += pbList.Count * Card.TurnCardDuration;	
			} 

			Observable.Timer(TimeSpan.FromSeconds(delay)).AsObservable().Subscribe((_) => {
                if (e.Data.ContainsKey("maxFiveRank"))
                {
				    MaxFiveRank.Value = e.Data.Int("maxFiveRank");	           
                }
			});
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
			var userAction = e.Data.ToObject<UserActionModel>();
			Pot.Value = userAction.pot;

			var index = userAction.seat;
			if (!Players.ContainsKey(index)) {
				return ;
			}
			
			var player = Players[index];
			player.ChipsChange = true;
			player.PrChips.Value = userAction.pr_chips;
			player.Bankroll.Value = userAction.bankroll;

			player.ActState.OnNext(e.E.ToActionEnum());
		};

		RxSubjects.Call.Subscribe(act);
		RxSubjects.AllIn.Subscribe(act);
		RxSubjects.Check.Subscribe(act);
		RxSubjects.Raise.Subscribe(act);

		RxSubjects.SeeCard.Subscribe((e) => {
			var cards = e.Data.IL("cards");
			var index = e.Data.Int("seat");
			if (Players.ContainsKey(index)) {
				var player = Players[index];
				player.SeeCardAnim = true;
				player.Cards.Value = cards;
			}
		});

		RxSubjects.GameOver.Subscribe((e) => {
			InGame = false;

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
			Pots.Value = room.DL("pots");
		});

		RxSubjects.UnAuditCD.AsObservable().Where((e) => {
			return e.Data.String("uid") == GameData.Shared.Uid;
		}).Subscribe(
			(e) => {
				var sec = e.Data.Int("sec");
				if (sec <= 0) {
					return ;
				}

				AuditCD.OnNext(sec);
			}
		);

		RxSubjects.Audit.Subscribe((e) => {
			var array = e.Data.List("ids");
			AuditList.OnNext(array);
		});

		// 倒计时
		Observable.Interval(TimeSpan.FromSeconds(1)).AsObservable().Subscribe((_) => {
			// 游戏已暂停，不需要修改
			if (GameStarted && Paused.Value) {
				return ;
			}

			var value = Math.Max(0, LeftTime.Value - 1);
			LeftTime.Value = value;
		});

		RxSubjects.GamerState.Subscribe((e) => {
			var uid = e.Data.String("uid");
			var state = e.Data.Int("state");
			var ucd = e.Data.Int("unseat_countdown");
			setState(uid, state, ucd);	
		});
	}

	private void setState(string uid, int state, int cd) {
		// 1、带入中，2、审核中，3、游戏中，4、留座中，5、托管中，0、已离座
		if (state == 0) {
			return ;
		}

		foreach (var player in Players) {
			if (player.Value.Uid == uid) {
				player.Value.SetState(state, cd);
			}
		}
	}
   
	private void setPbCards(List<int> list, int state) {
		if (list.Count <= 0) {
			return ;
		}

		DealState = state;
		
		if (state == 5 && PublicCards.Count >= 4) { // 转牌
			return ;	
		} else if (state == 6 && PublicCards.Count >= 5) { // 河牌
			return ;
		}

		if (state == 4) { // 翻牌
			PublicCards.Clear();
		}  

		foreach(int pb in list) {
			PublicCards.Add(pb);
		}
	}

	public int DealState = -1;
	public BehaviorSubject<List<object>> AuditList = new BehaviorSubject<List<object>>(new List<object>());
	public bool PublicCardAnimState = false;

	// 已设置的思考时间（下一手生效）
	public int SettingThinkTime = 15;	

	// 当前的思考时间
	public int ThinkTime = 15;

	public ReactiveProperty<PlayerState> SelfState = new ReactiveProperty<PlayerState>();  

	public bool Owner = false;
    public string OwnerName;
	public List<int> BankrollMul;
	public ReactiveProperty<int> PlayerCount = new ReactiveProperty<int>();
	public Subject<bool> GameInfoReady = new Subject<bool>();
	public string Sid; 
	public string Uid = "";
	public string Pin = "";
	public string Name = ""; 
	public string Avatar = "";
	public string Room;

	public int MySeat {
		get {
			if (string.IsNullOrEmpty(Uid)) {
				return -1;
			}

			return FindPlayerIndex(Uid);
		}
	}

	public int Coins = 0;
	public int SB {
		get {
			return BB / 2;
		}
	}
	public int BB = 0;
	public string RoomName = "";

	// 游戏是否已经开始，跟暂停状态无关
	public bool GameStarted = false; 
	public float Rake = 0;
	public long Duration = 0;
	public bool NeedAudit = false;
	public bool IPLimit = false;
	public bool GPSLimit = false;
    public bool NeedInsurance = false;
	public ReactiveProperty<long> LeftTime = new ReactiveProperty<long>(0);
    public ReactiveProperty<int> Ante = new ReactiveProperty<int>(-1);
    public ReactiveProperty<bool> Straddle = new ReactiveProperty<bool>(false);

	public ReactiveProperty<int> Pot = new ReactiveProperty<int>();
	public ReactiveProperty<List<Dictionary<string, object>>> Pots = new ReactiveProperty<List<Dictionary<string, object>>>(); 

	public BehaviorSubject<bool> Paused = new BehaviorSubject<bool>(false);
	public string GameCode = "";
	public ReactiveProperty<int> MaxFiveRank = new ReactiveProperty<int>();

	public ReactiveProperty<int> DealerSeat = new ReactiveProperty<int>();

	public DateTime StartTime;
	public bool InGame = false;  

	public BehaviorSubject<int> AuditCD = new BehaviorSubject<int>(0);

    public ReactiveProperty<bool> TalkLimit = new ReactiveProperty<bool>(false);

	private Dictionary<string, object> jsonData;

	private void byJson(Dictionary<string, object> json) {
		var options = json.Dict("options");
		var gamers = json.Dict("gamers");

		BB = options.Int("limit") ;
        OwnerName = json.Dict("owner").String("name");
		Owner = options.String("ownerid") == GameData.Shared.Uid;
		BankrollMul = options.IL("bankroll_multiple"); 
		Ante.Value = options.Int("ante");
		PlayerCount.Value = options.Int("max_seats");
		Rake = options.Float("rake_percent");
		Duration = options.Long("time_limit");
		NeedAudit = options.Int("need_audit") == 1;
		GPSLimit = options.Int("gps_limit") > 0;
		IPLimit = options.Int("ip_limit") == 1;
		GameCode = options.String("code");
		Straddle.Value = options.Int("straddle") != 0;
        SettingThinkTime = ThinkTime = options.Int("turn_countdown");
		
        NeedInsurance = options.Int("need_insurance") == 1;
		DealerSeat.Value = json.Int("dealer_seat");
		RoomName = json.String("name");
		Pot.Value = json.Int("pot");
		Pots.Value = json.DL("pots");
		
		InGame = json.Bool("is_ingame");
		MaxFiveRank.Value = json.Int("maxFiveRank");
		LeftTime.Value = json.Long("left_time");

        TalkLimit.Value = json.Int("talk_limit") == 1;

        AuditList.OnNext(json.List("un_audit"));

		var startTs = json.Int("begin_time");
		StartTime = _.DateTimeFromTimeStamp(startTs);
		// 游戏是否已开始
		GameStarted = startTs != 0;

		var pause = json.Int("is_pause") != 0;
		Paused.OnNext(pause);
		
		// 删除公共牌重新添加
		var cards = json.IL("shared_cards");
		PublicCards.Clear();
		foreach(int value in cards) {
			PublicCards.Add(value);
		}

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

		var mySeat = MySeat;

		if (mySeat != -1 && Players.ContainsKey(mySeat)) {
			AuditCD.OnNext(Players[mySeat].AuditCD);
			Coins = Players[mySeat].Coins;
		} else {
			AuditCD.OnNext(0);
		}

		GameInfoReady.OnNext(true);
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
		return FindPlayer(GameData.Shared.Uid);	
	}

	public Player FindPlayer(string uid) {
		foreach (var player in Players) {
			if (player.Value.Uid == uid) {
				return player.Value;
			}
		}

		return new Player();
	}

	public int FindPlayerIndex(string uid) {
		foreach(var player in Players) {
			if (player.Value.Uid == uid) {
				return player.Value.Index;
			}
		}

		return -1;
	}
	
	public ReactiveCollection<int> PublicCards = new ReactiveCollection<int>();

	// 静音设置
	private static string muteTag = "persist.txt?tag=mute";
	public bool muted {
		get {
			if (ES2.Exists(muteTag)) {
				return ES2.Load<bool>(muteTag);
			}

			return false;
		}

		set {
			ES2.Save(value, muteTag);
		}
	}

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