using BestHTTP.SocketIO;
using System;
using System.Collections.Generic;
using BestHTTP.SocketIO.Transports;
using BestHTTP.Cookies;
using Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using BestHTTP;

public class Connect  {
	private SocketManager manager;
	private string url = "http://61.143.225.47:7001/socket.io/"; 

	private Dictionary<int, Action<Dictionary<string, object>>> actions = new Dictionary<int, Action<Dictionary<string, object>>>();

	private int seq = 0;

	public Connect() {
		// Charles Proxy
		if (Debug.isDebugBuild) {
			HTTPManager.Proxy = new HTTPProxy(new Uri("http://localhost:8888"));
		}

		GConf.userToken = "s%3ATYD1Z2P08yQ7Uyu3o1KZZevf4v6K8sh1.S5l%2BfAqlAHThje7kJafU3qCKk3Yij%2B%2F5F1EHqifp540";

		SocketOptions options = new SocketOptions();
		options.ConnectWith = TransportTypes.WebSocket;

		manager = new SocketManager(new Uri(url), options);
		manager.setCookie = (request) => {
			var cookie = new Cookie("connect.sid", GConf.userToken);
			request.Cookies.Add(cookie);
		};
		manager.Socket.On("connect", OnConnect);

		manager.Open();	
	}

	void OnConnect(Socket socket, Packet packet, params object[] args) {
		Emit(new Dictionary<string, object>{
			{"f", "login"}
		}, (json) => {
			// 登陆成功，写用户数据
			SaveUserInfo(json);

			// 进入房间
			EnterRoom();
		});
	}

	void EnterRoom() {
		Emit(new Dictionary<string, object>{
			{"f", "entergame"},
			{"args", GConf.room}
		}, (json) => {
			var error = json.Int("err");

			if (error != 0) {				
				Debug.Log(error);
			}
		});
	}

	private void SaveUserInfo(Dictionary<string, object> json) {
		var ret = json.Dict("ret");
		var profile = ret.Dict("profile");
		var token = ret.Dict("token");

		GConf.Uid = profile.String("uid");
		GConf.name = profile.String("name");
		GConf.avatar = profile.String("avatar");
		GConf.pin = token.String("pin");
	}

	public void Emit(Dictionary<string, object> json, Action<Dictionary<string, object>> callback = null) {
		seq++;
		json["seq"] = seq;
		json["pin"] = GConf.pin;
		json["uid"] = GConf.Uid;
		manager.Socket.Emit("rpc", json);

		if (callback != null) {
			actions.Add(seq, callback);
		}
	}

	void Close() {
		manager.Close();
	}

	public static Connect shared;

	public static void Setup() {
		shared = new Connect();
		shared.setup();	
	}

	private void setup() {
		manager.Socket.On("rpc_ret", (socket, packet, args) => {
			if (args.Length == 0) {
				return ;
			}

			var json = args[0] as Dictionary<string, object>;
			
			if (json == null) {
				return ;
			}

			var ret = json.Dict("ret");
			if (ret.ContainsKey("cmds")) {
				GConf.MyCmd.SetCmd(ret.Dict("cmds"));
			}

			int seq = json.Int("seq");

			if (actions.ContainsKey(seq)) {
				actions[seq](json);
				actions.Remove(seq);
			} 
		});

		manager.Socket.On("push", (socket, packet, args) => {
			if (args.Length == 0) {
				return ;
			}

			var json = args[0] as Dictionary<string, object>;

			if (json == null) {
				return ;
			}

			var e  = json.String("e");

			if (String.IsNullOrEmpty(e)) {
				return ;
			}

			var evt = new DelegateArgs(json.Dict("args"));

			// 监听look事件，收到才进入房间
			if (e == "look") {
				refreshGameInfo(evt.Data);
			} else if (e == "prompt") {
				GConf.MyCmd.SetCmd(evt.Data);
			}

			// 通过事件广播出去
			switch(e) {
                case "takeseat":
					Delegates.shared.OnTakeSeat(evt);
					break;
				case "takecoin":
					Delegates.shared.OnTakeCoin(evt);
					break;
				case "unseat":
					Delegates.shared.OnUnSeat(evt);
					break;
				case "ready":
					Delegates.shared.OnReady(evt);
					break;
				case "gamestart":
					Delegates.shared.OnGameStart(evt);
					break;
				case "seecard":
					Delegates.shared.OnSeeCard(evt);
					break;
				case "look":
					Delegates.shared.OnLook(evt);
					break;
				case "deal":
					Delegates.shared.OnDeal(evt);
					break;
				case  "moveturn":
					Delegates.shared.OnMoveTurn(evt);
					break;
				case  "fold":
					Delegates.shared.OnFold(evt);
					break;
				case  "check":
					Delegates.shared.OnCheck(evt);
					break;
				case  "raise":
					Delegates.shared.OnRaise(evt);
					break;
				case  "call":
					Delegates.shared.OnCall(evt);
					break;
				case  "all_in":
					Delegates.shared.OnAllIn(evt);
					break;
				case "bye":
					Delegates.shared.OnExclusion(evt);
					break;
				case "gameover":
					Delegates.shared.OnGameOver(evt);
					break;
				default:
					break;
			}
		});
	}

	private bool enter = false;

	// 只允许进入一次
	void refreshGameInfo(Dictionary<string, object> json) {
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
		GConf.roomName = json.String("name");
		GConf.DealerSeat = json.Int("dealer_seat");
		
		var startTs = json.Int("begin_time");
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
			var index = Convert.ToInt32(entry.Key);
			var player = new Player(dict, index);
			GConf.Players.Add(index, player);
		}

		if (enter) {
			// @TODO: 刷新游戏界面	
		} else {
			SceneManager.LoadScene("PokerGame");
		}

		enter = true;
	}
}

