using BestHTTP.SocketIO;
using System;
using System.Collections.Generic;
using BestHTTP.SocketIO.Transports;
using BestHTTP.Cookies;
using Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using BestHTTP;

public sealed class Connect  {
	private SocketManager manager;
	private string url = "http://61.143.225.47:7001/socket.io/"; 

	private Dictionary<int, Action<Dictionary<string, object>>> actions = new Dictionary<int, Action<Dictionary<string, object>>>();

	private int seq = 0;

	private Connect() {
		// Charles Proxy
		if (Debug.isDebugBuild) {
			HTTPManager.Proxy = new HTTPProxy(new Uri("http://localhost:8888"));
		}

		GConf.userToken = "s%3AERf7PZFc3sWUYmYZLGBpFUlcF8CvX99r.wu5DJl6e1Q%2BHmZvVmbQp6WAVU%2BaqNLWO63IisE5S9%2B4";

		SocketOptions options = new SocketOptions();
		options.ConnectWith = TransportTypes.WebSocket;

		manager = new SocketManager(new Uri(url), options);
		manager.setCookie = (request) => {
			var cookie = new Cookie("connect.sid", GConf.userToken);
			request.Cookies.Add(cookie);
		};
		manager.Socket.On("connect", onConnect);
		manager.Open();	
	}

	private void onConnect(Socket socket, Packet packet, params object[] args) {
		Emit(new Dictionary<string, object>{
			{"f", "login"}
		}, (json) => {
			// 登陆成功，写用户数据
			saveUserInfo(json);

			// 进入房间
			enterRoom();
		});
	}

	private void enterRoom() {
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

	private void saveUserInfo(Dictionary<string, object> json) {
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

	private void close() {
		manager.Close();
	}

	public static Connect Shared {
		get {
			if (instance == null) {
				instance = new Connect();
			}

			return instance;
		}
	}

	private static Connect instance;
	private bool setuped = false;

	public void Setup() {
		if (setuped) {
			return ;
		}

		setuped = true;

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
				case "pausing": case "paused":
					Delegates.shared.OnPaused(evt);
					break;
				case "start":
					Delegates.shared.OnStart(evt);
					break;
				case "game_end":
					Delegates.shared.OnGameEnd(evt);
					break;
				case "un_audit":
					Delegates.shared.OnAudit(evt);
					break;
				default:
					break;
			}
		});
	}

	private bool entered = false;

	// 只允许进入一次
	private void refreshGameInfo(Dictionary<string, object> json) {
		GConf.ModifyByJson(json);	

		if (entered) {
			// Skip
		} else {
			SceneManager.LoadScene("PokerGame");
		}

		entered = true;
	}
}

