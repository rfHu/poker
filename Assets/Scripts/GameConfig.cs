using System.Collections.Generic;
using System;
using Extensions;
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
	public AutoDeposit Trust = new AutoDeposit(); 
	public ReactiveProperty<string> ShowCard = new ReactiveProperty<string>();

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
		RxSubjects.TakeSeat.AsObservable().Subscribe((e) => {
			var index = e.Data.Int("where");
			var playerInfo = e.Data.Dict("who");
			var player = new Player(playerInfo, index);

			GameData.Shared.Players[index] = player;
		});

		RxSubjects.Paused.AsObservable().Subscribe((e) => {
			Paused.Value = true; 	
		});

		RxSubjects.Started.AsObservable().Subscribe((e) => {
			GameStarted = true;
			LeftTime.Value = e.Data.Int("left_time");
			Paused.Value = false; 
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

			// 重连的用户，reload scene
			var loginStatus = e.Data.Int("is_enter_look");

			if (loginStatus == 1) {
				AuditList.Value = new List<object>();
#if UNITY_EDITOR
#else
			    Commander.Shared.VoiceIconToggle(true);
#endif
				SceneManager.LoadScene("PokerGame");
            } 	
		});

		RxSubjects.Deal.Subscribe((e) => {
			Pot.Value = e.Data.Int("pot");
			PrPot.Value = Pot.Value - e.Data.Int("pr_pot");

			GameStartState = true;

			var state = e.Data.Int("state");

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
					setPbCards(list, state);
				} else {
					var k = Convert.ToInt16(item.Key);
					if (!Players.ContainsKey(k)) {
						continue;
					}

					Players[k].Cards.Value = list;
				}
			}

			var pbList = data.IL("-1");
			float delay = 1f;
			
			if (pbList.Count >= 3) { // 等待亮牌动画
				delay += pbList.Count * Card.TurnCardDuration;	
			} 

			Observable.Timer(TimeSpan.FromSeconds(delay)).AsObservable().Subscribe((_) => {
				MaxFiveRank.Value = e.Data.Int("maxFiveRank");	
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
			var map = new Dictionary<string, ActionState>() {
				{"call", ActionState.Call},
				{"check", ActionState.Check},
				{"all_in", ActionState.Allin},
				{"raise", ActionState.Raise}
			};

			Pot.Value = userAction.pot;

			var index = userAction.seat;
			if (!Players.ContainsKey(index)) {
				return ;
			}
			
			var player = Players[index];
			player.PrChips.Value = userAction.pr_chips;
			player.Bankroll.Value = userAction.bankroll;

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

				// 每次修改时，先重置，防止不触发
				AuditCD.Value = 0;
				AuditCD.Value = sec;
			}
		);

		RxSubjects.Audit.Subscribe((e) => {
			var array = e.Data.List("ids");
			AuditList.Value = array;
		});

        RxSubjects.Modify.Subscribe((e) =>{

            var data = e.Data;
            var str = "";

            foreach (var item in e.Data)
	        {
                switch (item.Key) 
                {
                    case "bankroll_multiple":
                        BankrollMul = data.IL("bankroll_multiple");
                        str = "房主将记分牌带入倍数改为：" + GameData.Shared.BankrollMul[0] + "-" + GameData.Shared.BankrollMul[1];
                        PokerUI.Toast(str);
                        break;

                    case "time_limit": 
                        Duration += data.Long("time_limit");
                        LeftTime.Value += data.Long("time_limit");
                        str = "房主将牌局延长了" + data.Long("time_limit") / 3600f + "小时";
                        PokerUI.Toast(str);
                        break;

                    case "ante":
                        Ante.Value = data.Int("ante");
                        str = "房主将底注改为：" + Ante.Value; 
                        PokerUI.Toast(str);
                        break;

                    case "need_audit":
                        NeedAudit = data.Int("need_audit") == 1;
                        str = GameData.Shared.NeedAudit ? "房主开启了授权带入" : "房主关闭了授权带入";
                        PokerUI.Toast(str);
                        break;

                    case "straddle":
                        Straddle.Value = data.Int("straddle") != 0;
                        str = Straddle.Value ? "房主开启了Straddle" : "房主关闭了Straddle"; 
                        PokerUI.Toast(str);
						break;

                    case "turn_countdown":
						SettingThinkTime = data.Int("turn_countdown");
                        str = "房主将思考时间改为" + SettingThinkTime +"秒";
                        PokerUI.Toast(str);
                        break;
                    default:
                        break;
                }
            }
        });

        RxSubjects.KickOut.Subscribe((e) =>{
            string Uid = e.Data.String("uid");
            string name = FindPlayer(Uid).Name;
            string str = "玩家 " + name + " 被房主请出房间";
            PokerUI.Toast(str);
        });

        RxSubjects.StandUp.Subscribe((e) =>
        {
            string Uid = e.Data.String("uid");
            string name = FindPlayer(Uid).Name;
            string str = "玩家 " + name + " 被房主强制站起";
            PokerUI.Toast(str);
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

        RxSubjects.Insurance.Subscribe((e) =>
        {
            switch (e.Data.Int("type"))
            {
                case 1:
                    PokerUI.Toast("多名领先玩家，不支持购买保险"); break;
                case 2:
                    PokerUI.Toast("无反超风险，不用购买保险"); break;
                case 3:
                    string name = FindPlayer(e.Data.String("uid")).Name;
                    PokerUI.Toast(name + " 玩家正在购买保险");
                    break;
                case 10:
                    PokerUI.Toast("玩家购买的保险没有命中"); break;
                case 11:
                    PokerUI.Toast("玩家购买的保险命中了"); break;
                case 12:
                    PokerUI.Toast("系统自动购买的保险命中了"); break;
                default:
                    break;
            }
        });

        RxSubjects.ToInsurance.Subscribe((e) =>
        {
            var outsCard = e.Data.IL("outs");
            var pot = e.Data.Int("pot");
            var cost = e.Data.Int("cost");
            var scope = e.Data.IL("scope");
            var mustBuy = e.Data.Int("must_buy") == 2 ? true : false;
            var time = e.Data.Int("time");

            var InsurancePopup = (GameObject)GameObject.Instantiate(Resources.Load("Prefab/Insurance"));
            InsurancePopup.GetComponent<DOPopup>().Show();
            InsurancePopup.GetComponent<Insurance>().Init(outsCard, pot, cost, scope, mustBuy, time);
            
        });
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
	public ReactiveProperty<List<object>> AuditList = new ReactiveProperty<List<object>>();
	public bool GameStartState = false;
	public bool SeeCardState = false;

	// 已设置的思考时间（下一手生效）
	public int SettingThinkTime = 15;	

	// 当前的思考时间
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

	public int MySeat {
		get {
			if (string.IsNullOrEmpty(Uid)) {
				return -1;
			}

			return FindPlayerIndex(Uid);
		}
	}

	//public int Ante = 0;
	public int Coins = 0;
	public int SB = 0;
	public int BB = 0;
	//public bool Straddle = false;
	public string RoomName = "";

	// 游戏是否已经开始，跟暂停状态无关
	public bool GameStarted = false; 
	public float Rake = 0;
	public long Duration = 0;
	public bool NeedAudit = false;
	public bool IPLimit = false;
	public bool GPSLimit = false;
	public ReactiveProperty<long> LeftTime = new ReactiveProperty<long>(0);
    public ReactiveProperty<int> Ante = new ReactiveProperty<int>(0);
    public ReactiveProperty<bool> Straddle = new ReactiveProperty<bool>(false);

	public ReactiveProperty<int> Pot = new ReactiveProperty<int>();
	public ReactiveProperty<int> PrPot = new ReactiveProperty<int>();

	public ReactiveProperty<bool> Paused = new ReactiveProperty<bool>();
	public string GameCode = "";
	public ReactiveProperty<int> MaxFiveRank = new ReactiveProperty<int>();

	public ReactiveProperty<int> DealerSeat = new ReactiveProperty<int>();

	public DateTime StartTime;
	public bool InGame = false;  

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
		Ante.Value = options.Int("ante");
		PlayerCount = options.Int("max_seats");
		Rake = options.Float("rake_percent");
		Duration = options.Long("time_limit");
		NeedAudit = options.Int("need_audit") == 1;
		GPSLimit = options.Int("gps_limit") > 0;
		IPLimit = options.Int("ip_limit") == 1;
		GameCode = options.String("code");
		Straddle.Value = options.Int("straddle") != 0;
        SettingThinkTime = ThinkTime = options.Int("turn_countdown");
		var bb = options.Int("limit");
		BB = bb ;
		SB = bb / 2;
		DealerSeat.Value = json.Int("dealer_seat");
		RoomName = json.String("name");
		Pot.Value = json.Int("pot");
		PrPot.Value = Pot.Value - json.Int("pr_pot");
		InGame = json.Bool("is_ingame");
		MaxFiveRank.Value = json.Int("maxFiveRank");
		LeftTime.Value = json.Long("left_time");

		// ReactiveProperty 对同样的值不会触发onNext，所以这里强制执行一次
		var pause = json.Int("is_pause") != 0;
		if (pause == Paused.Value) {
			Paused.Value = !pause;
		}
		Paused.Value = pause;
		
		// 删除公共牌重新添加
		var cards = json.IL("shared_cards");
		PublicCards.Clear();
		foreach(int value in cards) {
			PublicCards.Add(value);
		}
		
		var startTs = json.Int("begin_time");
		StartTime = _.DateTimeFromTimeStamp(startTs);

		// 游戏是否已开始
		GameStarted = startTs != 0;

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
			AuditCD.Value = Players[mySeat].AuditCD;
			Coins = Players[mySeat].Coins;
		} else {
			AuditCD.Value = 0;
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