using System.Collections.Generic;
using System;

// 游戏全局配置
public class GConf {
	public static int playerCount;
	public static string userToken = ""; 
	public static string uid = "";
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
}

// 游戏变动数据
// public class GInfo {
// 	private static bool hasSeat = false;
// 	public static bool HasSeat {
// 		get {
// 			return hasSeat;
// 		}

// 		set {
// 			hasSeat = value;
// 		}
// 	}

// 	private static bool canBuyCoins = false;
// 	public static bool CanBuyCoins {
// 		get {
// 			return canBuyCoins;
// 		}

// 		set {
// 			canBuyCoins = value;
// 		}
// 	}
// }
