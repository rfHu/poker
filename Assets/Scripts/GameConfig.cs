﻿using System.Collections.Generic;
using System;
using Extensions;
using UniRx;
using UnityEngine;

// 游戏全局配置
sealed public class GConf {
	public static List<int> bankroll;
	public static int coins = 0;

	// 游戏是否已经开始，跟暂停状态无关
	public static bool GameStarted = false; 
	public static float rake = 0;
	public static int duration = 0;
	public static bool needAduit = false;

	public static bool TakeCoinSuccess = false;

	public static int Pot = 0;
	public static int PrPot = 0;

	public static bool Paused = false;

	public static int DealerSeat = -1; 

	public static void ModifyByJson(Dictionary<string, object> json) {
		var options = json.Dict("options");
		var gamers = json.Dict("gamers");

		GConf.bankroll = options.IL("bankroll_multiple"); 
		GConf.rake = options.Float("rake_percent");
		GConf.duration = options.Int("time_limit");
		GConf.needAduit = options.Int("need_audit") == 1;
		GConf.DealerSeat = json.Int("dealer_seat");

		Pot = json.Int("pot");
		PrPot = json.Int("pr_pot");
		Paused = json.Int("is_pause") != 0;
	}
}

sealed public class Player {
	public string Name = "";
	public string Avatar = "";
	public string Uid = "";
	public int Bankroll = 0;
	public int Index;
	public int PrChips = 0;
	private GameObject Go;

	public void DestroyGo() {
		GameObject.Destroy(Go);
	}

	public void Show(Transform parent) {
		Go = (GameObject)GameObject.Instantiate(Resources.Load("Prefab/Player"));
		Go.GetComponent<PlayerObject>().ShowPlayer(this, parent);
	}

	public Player(Dictionary<string, object> json, int index) {
		Name = json.String("name");
		Avatar = json.String("avatar");
		Uid = json.String("uid");
		
		// 用户记分牌
		Bankroll = json.Int("bankroll");

		// 用户该轮上的筹码
		PrChips = json.Int("pr_chips");

		Index = index;
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
	}

	public bool Owner = false;	
	public List<int> Bankroll;
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

	public bool TakeCoinSuccess = false;

	public int Pot = 0;
	public int PrPot = 0;

	public bool Paused = false;
	public string GameCode = "";

	public ReactiveProperty<int> DealerSeat = new ReactiveProperty<int>();

	public DateTime StartTime;

	public void InitByJson(Dictionary<string, object> json) {
		var options = json.Dict("options");
		var gamers = json.Dict("gamers");

		Owner = options.String("ownerid") == GameData.Shared.Uid;
		Bankroll = options.IL("bankroll_multiple"); 
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

		Pot = json.Int("pot");
		PrPot = json.Int("pr_pot");
		Paused = json.Int("is_pause") != 0;
		
		var startTs = json.Int("begin_time");
		StartTime = _.DateTimeFromTimeStamp(startTs);

		if (startTs != 0)
        {
			GameStarted = true;
        }

		var bb = options.Int("limit");
		BB = bb ;
		SB = bb / 2;

		// 先清除、再添加
		Players.Clear();

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