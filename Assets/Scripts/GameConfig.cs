using System.Collections.Generic;
using System;

// 游戏全局配置
public class GConf {
	public static int playerCount = 9;
	public static string userToken = ""; 
	public static string uid = "";
	public static string pin = "";
	public static string name = ""; 
	public static string avatar = "";
	public static string coins = "";
	public static string room = "1777";
	public static bool isOwner = false;
	public static int ante = 0;
	public static List<int> bankroll;
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
public class GInfo {
	public static bool hasSeat = false;
	public static bool canBuyCoins = false;
}
