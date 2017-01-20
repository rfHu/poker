using System.Collections.Generic;
using System;
using Extensions;

// 游戏全局配置
public class GConf {
	public static int playerCount;
	public static string userToken = ""; 
	public static string Uid = "";
	public static string pin = "";
	public static string name = ""; 
	public static string avatar = "";
	public static string room = "587450f87133077573039b92";
	public static bool isOwner = false;
	public static int ante = 0;
	public static List<int> bankroll;
	public static int coins = 0;
	public static int sb = 0;
	public static int bb = 0;
	public static bool isStraddle = false;
	public static string roomName = "";

	// 游戏是否已经开始，跟暂停状态无关
	public static bool GameStarted = false; 
	public static float rake = 0;
	public static int duration = 0;
	public static bool needAduit = false;
	public static bool IPLimit = false;
	public static bool GPSLimit = false;

	public static Dictionary<int, Player> Players = new Dictionary<int, Player>();

	public static bool TakeCoinSuccess = false;

	public static int Pot = 0;
	public static int PrPot = 0;

	public static bool Paused = false;
	public static string GameCode = "";

	public class MyCmd {
		public static bool Takecoin = false;
		public static bool Unseat = false;

		public static void SetCmd(Dictionary<string, object> data) {
			foreach(KeyValuePair<string, object> entry in data) {
				if (entry.Key == "takecoin") {
					MyCmd.Takecoin = Convert.ToBoolean(entry.Value);
			   	} else if (entry.Key == "unseat") {
					MyCmd.Unseat = Convert.ToBoolean(entry.Value);	   
				}
			}
		}
	}

	public static int DealerSeat = -1; 
	public static DateTime StartTime = new DateTime();

	public static void ModifyByJson(Dictionary<string, object> json) {
		var options = json.Dict("options");
		var gamers = json.Dict("gamers");

		GConf.isOwner = options.String("ownerid") == GConf.Uid;
		GConf.bankroll = options.IL("bankroll_multiple"); 
		GConf.ante = options.Int("ant");
		GConf.playerCount = options.Int("max_seats");
		GConf.rake = options.Float("rake_percent");
		GConf.duration = options.Int("time_limit");
		GConf.needAduit = options.Int("need_audit") == 1;
		GConf.GPSLimit = options.Int("gps_limit") == 1;
		GConf.IPLimit = options.Int("ip_limit") == 1;
		GConf.GameCode = options.String("code");
		GConf.roomName = json.String("name");
		GConf.DealerSeat = json.Int("dealer_seat");

		Pot = json.Int("pot");
		PrPot = json.Int("pr_pot");
		Paused = json.Int("is_pause") != 0;
		
		var startTs = json.Int("begin_time");
		GConf.StartTime = _.DateTimeFromTimeStamp(startTs);

		if (startTs != 0)
        {
			GConf.GameStarted = true;
        }

		var bb = options.Int("limit");
		GConf.bb = bb ;
		GConf.sb = bb / 2;

		// 先清除、再添加
		GConf.Players.Clear();

		foreach(KeyValuePair<string, object> entry in gamers) {
			var dict = entry.Value as Dictionary<string, object>;

			if (dict == null) {
				continue;
			}

			var index = Convert.ToInt32(entry.Key);
			var player = new Player(dict, index);
			GConf.Players.Add(index, player);
		}
	}
}

public class Player {
	public string Name = "";
	public string Avatar = "";
	public string Uid = "";
	public int Bankroll = 0;
	public int Index;
	public int PrChips = 0;

	public Player(Dictionary<string, object> json, int index) {
		Name = json.String("name");
		Avatar = json.String("avatar");
		Uid = json.String("uid");
		Bankroll = json.Int("bankroll");
		PrChips = json.Int("pr_chips");

		Index = index;
	}
}