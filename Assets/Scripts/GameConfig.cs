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
	public static DateTime startTime = DateTime.Now;
	public static float rake = 0;
	public static int duration = 0;
	public static bool needAduit = false;
	public static bool IPLimit = false;
	public static bool GPSLimit = false;

	public static Dictionary<int, Player> Players = new Dictionary<int, Player>();

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
}

public class Player {
	public string Name = "";
	public string Avatar = "";
	public string Uid = "";
	public int Bankroll = 0;
	public int Index;

	public Player(Dictionary<string, object> json, int index) {
		Name = json.String("name");
		Avatar = json.String("avatar");
		Uid = json.String("uid");
		Bankroll = json.Int("bankroll");
		Index = index;
	}
}