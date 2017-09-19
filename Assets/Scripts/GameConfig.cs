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
	Raise = 5,
	Straddle = 6
}

public enum GameType {
	Normal,
	SNG,
	MTT
}

public enum MatchRoomStat {
	Default = 0,
	WaitingStart = 5,
	Rest = 10,
	WaitingFinal = 15
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
	public static int MaxWinPercent = -1;

	sealed public class RestoreData {
		public int BuyTimeCost = 10;
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

	public ReactiveProperty<int> HeadValue = new ReactiveProperty<int>();
	public AutoDeposit Trust = new AutoDeposit(); 
	public ReactiveProperty<string> ShowCard = new ReactiveProperty<string>();

	public BehaviorSubject<RestoreData> Countdown = new BehaviorSubject<RestoreData>(new RestoreData());

	public Subject<ActionState> ActState = new Subject<ActionState>();

	public BehaviorSubject<List<int>> Cards = new BehaviorSubject<List<int>>(new List<int>());

	public bool SeeCardAnim = false;

	public BehaviorSubject<PlayerState> PlayerStat = new BehaviorSubject<PlayerState>(PlayerState.Normal);

	public BehaviorSubject<int> ReservedCD = new BehaviorSubject<int>(0); 

	public ReactiveProperty<String> LastAct = new ReactiveProperty<String>();

    public ReactiveProperty<int> Rank = new ReactiveProperty<int>();

    public BehaviorSubject<int> WinPercent = new BehaviorSubject<int>(-1);

    public int AddonCount = 0;
    public int RebuyCount = 0;

	public int LeftRebuy {
		get {
			return GameData.MatchData.Rebuy - RebuyCount;
		}
	}

	public int LeftAddon {
		get {
			return GameData.MatchData.Addon - AddonCount;
		}
	}

	public bool CanRebuy {
		get {
			return LeftRebuy > 0;
		}
	}

	public bool CanAddon{
		get {
			return LeftAddon > 0;
		}
	}

	public int readyState = -1;

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
        Rank.Value = GameData.Shared.Type.Value == GameType.MTT ? json.Int("rank") : json.Int("match_rank");
		readyState = json.Int("is_ready");
		HeadValue.Value = json.Int("head_value");

        if (GameData.Shared.IsMatch())
        {
            AddonCount = json.Int("add_on");
            RebuyCount = json.Int("rebuy_count");
        }

        if (json.ContainsKey("win_rate"))
        {
            WinPercent.OnNext(json.Int("win_rate"));
        }

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
			dt.BuyTimeCost = json.Int("show_moretime");

			Countdown.OnNext(dt);
		}

		var cards = json.IL("cards");
		Cards.OnNext(cards);

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
		RxSubjects.TakeSeat.Subscribe((e) => {
			var index = e.Data.Int("where");
			var playerInfo = e.Data.Dict("who");
			var player = new Player(playerInfo, index);

			GameData.Shared.Players[index] = player;
		});

		RxSubjects.Paused.AsObservable().Subscribe((e) => {
			Paused.OnNext(e.Data.Int("type")); 	
		});

		RxSubjects.Started.AsObservable().Subscribe((e) => {
			GameStarted.Value = true;
			LeftTime.Value = e.Data.Int("left_time");
			Paused.OnNext(0); 
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
			GameStarted.Value = true;

			var json = e.Data.Dict("room");
			PublicCardAnimState = true;

			byJson(json, true);
		});
	
		RxSubjects.Look.Subscribe((e) => {
			PublicCardAnimState = false;
			byJson(e.Data);
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
					player.Cards.OnNext(list);
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

				if (e.Data.ContainsKey("maxFive")) {
					HighlightIndex.Value = Card.ExtractHighlightCards(e.Data.IL("maxFive"), MaxFiveRank.Value);
				}

                if (e.Data.ContainsKey("win_rates"))
                {
                    var winRates = e.Data.Dict("win_rates");
					Player.MaxWinPercent = Convert.ToInt16(winRates.Values.Max());

                    foreach (var item in winRates)
                    {
                        var player = GetPlayer(int.Parse(item.Key));
                        int percent = Convert.ToInt16(item.Value);
                        player.WinPercent.OnNext(percent);
                    }
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
			var uid = e.Data.String("uid");

			Coins = coins;

			if (uid == GameData.Shared.Uid) {
				GameData.Shared.Bankroll.Value = bankroll;
			}			

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
				player.Cards.OnNext(cards);
			}
		});

		RxSubjects.GameOver.Subscribe((e) => {
			InGame = false;
            if (NeedInsurance.Value)
            {
                InsuranceState.Value = false;
            }

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
			ShowAudit.Value = array.Count > 0;
		});

		// 倒计时
		Observable.Interval(TimeSpan.FromSeconds(1)).AsObservable().Subscribe((_) => {
			// 游戏已暂停，不需要修改
			if (GameStarted.Value && Paused.Value > 0) {
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

        RxSubjects.RaiseBlind.Subscribe((e) => {
            BlindLv = e.Data.Int("blind_lv");
        });

        RxSubjects.CurrentRank.Subscribe((e) => {
            Rank.Value = e.Data.Int("rank");
        });

        RxSubjects.NoTalking.Subscribe((e) => {
            bool type = e.Data.Int("type") == 1;
            string uid = e.Data.String("uid");
            if (uid == GameData.Shared.Uid)
            {
                GameData.Shared._talkLimit.Value = type;
            }
        });

		TalkLimit = _talkLimit.CombineLatest(InsuranceState, (x, y) => x || y).ToReadOnlyReactiveProperty();

        RxSubjects.HalfBreak.Subscribe((e) => {
            MatchData.MatchRoomStatus.OnNext(MatchRoomStat.Rest);
            LeftTime.Value = e.Data.Int("ct");
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

	public bool IsMatchState = false;

	public int DealState = -1;
	public ReactiveProperty<bool> ShowAudit  = new ReactiveProperty<bool>(false);
	public bool PublicCardAnimState = false;

	// 已设置的思考时间（下一手生效）
	public int SettingThinkTime = 15;	

	// 当前的思考时间
	public int ThinkTime = 15;

	public ReactiveProperty<PlayerState> SelfState = new ReactiveProperty<PlayerState>();  

	public bool Owner = false;
    public string OwnerName;
	public List<int> BankrollMul;
	public BehaviorSubject <int> PlayerCount = new BehaviorSubject<int>(0);
	public string Sid; 
	public string Uid = "";
	public string Pin = "";
	public string Name = ""; 
	public string Avatar = "";
	public ReactiveProperty<string> Room = new ReactiveProperty<string>();

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
	public ReactiveProperty<string> RoomName = new ReactiveProperty<string>();
	public ReactiveProperty<int> TableNumber = new ReactiveProperty<int>(-1) ; // MTT牌桌号 
    
    public ReactiveProperty<GameType> Type = new ReactiveProperty<GameType>();
    public int BlindLv;
	public ReactiveProperty<int> Rank = new ReactiveProperty<int>();

	public bool IsMatch() {
		return Type.Value != GameType.Normal;
	}

	public class MatchData {
		public static int Type;
        public static int LimitLv; // 终止报名，但还可以增购级别；从1开始计数，与BlindLV比应该-1

		public static bool IsPaused {
			get {
				var v = MatchRoomStatus.Value;
				return GameData.Shared.Type.Value == GameType.MTT && (v == MatchRoomStat.Rest || v == MatchRoomStat.WaitingStart);
			}
		}

		public static bool CanRebuyLv() {
			return GameData.Shared.BlindLv < LimitLv - 1; 
		}

		public static bool CanAddonLv() {
			return GameData.Shared.BlindLv == LimitLv - 1;
		}

		public static bool CanBuyLv() {
			return CanRebuyLv() || CanAddonLv();
		}

        public static int Addon;
        public static int Rebuy;
		public static bool IsHunter;

		public static BehaviorSubject<MatchRoomStat> MatchRoomStatus = new BehaviorSubject<MatchRoomStat>(MatchRoomStat.Default);

		public static string MatchString {
			get {
				var map = new Dictionary<int, string>(){
					{1, "快速赛"},
					{2, "标准赛"},
					{3, "长时赛"},
					{4, "深筹赛"}
				};

				return map[Type];
			}
		}

        public static int[] Data
        {
            get {
                int[][] data = { 
                                  new int[] { 200, 2000, 3 },
                                  new int[] { 500, 4000, 5 }, 
                                  new int[] { 1000, 4000, 10 },
                                  new int[] { 2000, 8000, 10 }
                              };

                    return data[Type - 1]; 
            }
        }

        public static int SNGJoinFee
        {
            get {
                return Data[0];
            }
        }

        public static int SNGServerFee 
        {
            get {
                return Data[0] / 10;
            }
        }

        public static int BankrollNum
        {
            get {
                return Data[1];
            }
        }

        public static int Time 
        {
            get {
                return Data[2];
            }
        }

        public static int JoinFee;
        public static int RebuyFee;



	}

	// 游戏是否已经开始，跟暂停状态无关
	public ReactiveProperty<bool> GameStarted = new ReactiveProperty<bool>(false); 
	public float Rake = 0;
	public int Duration = 0;
	public bool NeedAudit = false;
	public ReactiveProperty<bool> IPLimit = new ReactiveProperty<bool>(false);
	public ReactiveProperty<bool> GPSLimit = new ReactiveProperty<bool>(false);
    public ReactiveProperty<bool> NeedInsurance = new ReactiveProperty<bool>(false);
    public bool Award27 = false;
    public bool BuryCard = false;
	public DateTime CreateTime; 
	public ReactiveProperty<int> LeftTime = new ReactiveProperty<int>(0);
    public ReactiveProperty<int> Ante = new ReactiveProperty<int>(0);
    public ReactiveProperty<bool> Straddle = new ReactiveProperty<bool>(false);

	public ReactiveProperty<bool> OffScore = new ReactiveProperty<bool>(false);
	public ReactiveProperty<int> Bankroll = new ReactiveProperty<int>(0);

	public ReactiveProperty<int> Pot = new ReactiveProperty<int>();
	public ReactiveProperty<List<Dictionary<string, object>>> Pots = new ReactiveProperty<List<Dictionary<string, object>>>(); 

	public BehaviorSubject<int> Paused = new BehaviorSubject<int>(0);
	public ReactiveProperty<string> GameCode = new ReactiveProperty<string>();
	public ReactiveProperty<int> MaxFiveRank = new ReactiveProperty<int>();
	public ReactiveProperty<List<int>> HighlightIndex = new ReactiveProperty<List<int>>();

	public ReactiveProperty<int> DealerSeat = new ReactiveProperty<int>();

	public DateTime StartTime;
	public bool InGame = false;  

	public BehaviorSubject<int> AuditCD = new BehaviorSubject<int>(0);

	// 内部使用变量，禁言状态
	private ReactiveProperty<bool> _talkLimit = new ReactiveProperty<bool>(false);

	// 外部使用变量，综合保险与禁言
    public ReadOnlyReactiveProperty<bool> TalkLimit;

	// 保险状态
    public ReactiveProperty<bool> InsuranceState = new ReactiveProperty<bool>(false);
	
	// private Dictionary<string, object> jsonData;

	private GameType string2GameType(string type) {
		if (type == "sng") {
			return GameType.SNG;
		} else if (type == "mtt") {
			return GameType.MTT;
		}

		return GameType.Normal;	
	}

	private void byJson(Dictionary<string, object> json, bool gameStart = false) {
		var options = json.Dict("options");
		var gamers = json.Dict("gamers");

		Coins = json.Int("coins");
		Bankroll.Value = json.Int("bankroll");	
		BB = options.Int("limit") ;
        OwnerName = json.Dict("owner").String("name");
		Owner = options.String("ownerid") == GameData.Shared.Uid;
		BankrollMul = options.IL("bankroll_multiple"); 
		Ante.Value = options.Int("ante");
		PlayerCount.OnNext(options.Int("max_seats"));
		Rake = options.Float("rake_percent");
		Duration = options.Int("time_limit");
		NeedAudit = options.Int("need_audit") == 1;
		GPSLimit.Value = options.Int("gps_limit") > 0;
		IPLimit.Value = options.Int("ip_limit") == 1;
        Award27 = options.Int("award_27") == 1;
        BuryCard = options.Int("bury_card") == 1;
		GameCode.Value = options.String("code");
		Straddle.Value = options.Int("straddle") == 1;
        SettingThinkTime = ThinkTime = options.Int("turn_countdown");
		OffScore.Value = options.Int("off_score") == 1;
		
        NeedInsurance.Value = options.Int("need_insurance") == 1;
		DealerSeat.Value = json.Int("dealer_seat");
		Pot.Value = json.Int("pot");
		Pots.Value = json.DL("pots");

        Type.Value = string2GameType(json.String("type"));

        if (IsMatch())
        {
            MatchData.Type = options.Int("sub_type");
            MatchData.LimitLv = options.Int("limit_level");
            MatchData.Rebuy = options.Int("rebuy_count");
            MatchData.Addon = options.Int("add_on");
			MatchData.IsHunter = options.Int("reward_ratio") > 0;
            BlindLv = json.Int("blind_lv");
            MatchData.JoinFee = options.Int("join_fee");
            MatchData.RebuyFee = options.Int("rebuy_fee");
		    MatchData.MatchRoomStatus.OnNext((MatchRoomStat)json.Int("match_room_status"));
			LeftTime.Value = MatchData.MatchRoomStatus.Value == MatchRoomStat.Rest ? json.Int("half_break_countdown") : json.Int("blind_countdown");
        } else {
			LeftTime.Value = json.Int("left_time");
		}

		RoomName.Value = GameData.Shared.Type.Value == GameType.MTT ? json.String("match_name") : json.String("name");
		if (GameData.Shared.Type.Value == GameType.MTT) {
			TableNumber.Value = json.Int("name"); // MTT的牌桌名称就是牌桌号
		} else {
			TableNumber.Value = -1;
		}
		
		InGame = json.Bool("is_ingame");

		MaxFiveRank.Value = json.Int("maxFiveRank");

        _talkLimit.Value = json.Int("talk_limit") == 1;
        InsuranceState.Value = false; // 这个重置应该在pause设置之前
        ShowAudit.Value = json.List("un_audit").Count > 0;
		CreateTime = _.DateTimeFromTimeStamp(json.Int("create_time"));

		var startTs = json.Int("begin_time");
		StartTime = _.DateTimeFromTimeStamp(startTs);
		// 游戏是否已开始
		GameStarted.Value = startTs != 0;
		Paused.OnNext(json.Int("is_pause"));
		
		// 删除公共牌重新添加
		var cards = json.IL("shared_cards");
		PublicCards.Clear();
		foreach(int value in cards) {
			PublicCards.Add(value);
		}

		// 先设置公共牌，再高亮
		HighlightIndex.Value = Card.ExtractHighlightCards(json.IL("maxFive"), MaxFiveRank.Value);

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

			// 大小盲
			if (gameStart && player.PrChips.Value != 0) {
				player.ChipsChange = true;
				
				// Straddle 
				if (player.PrChips.Value == 2 * GameData.Shared.BB) {
					player.ActState.OnNext(ActionState.Straddle);
				}
			}

			Players[index] = player;
		}

		var mySeat = MySeat;
		if (mySeat != -1) {
			var player = Players[mySeat];
			AuditCD.OnNext(player.AuditCD);
			GameData.Shared.Rank.Value = player.Rank.Value;
		} else {
			AuditCD.OnNext(0);
			GameData.Shared.Rank.Value = 0;
		}
	}

	public static GameData Shared = new GameData();

	public string MatchID;

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
