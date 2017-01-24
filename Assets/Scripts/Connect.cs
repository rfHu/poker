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

		GameData.Shared.UserToken = "s%3AERf7PZFc3sWUYmYZLGBpFUlcF8CvX99r.wu5DJl6e1Q%2BHmZvVmbQp6WAVU%2BaqNLWO63IisE5S9%2B4";

		SocketOptions options = new SocketOptions();
		options.ConnectWith = TransportTypes.WebSocket;

		manager = new SocketManager(new Uri(url), options);
		manager.setCookie = (request) => {
			var cookie = new Cookie("connect.sid", GameData.Shared.UserToken);
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
			{"args", GameData.Shared.Room}
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

		GameData.Shared.Uid = profile.String("uid");
		GameData.Shared.Name = profile.String("name");
		GameData.Shared.Avatar = profile.String("avatar");
		GameData.Shared.Pin = token.String("pin");
	}

	public void Emit(Dictionary<string, object> json, Action<Dictionary<string, object>> callback = null) {
		seq++;
		json["seq"] = seq;
		json["pin"] = GameData.Shared.Pin;
		json["uid"] = GameData.Shared.Uid;
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
				GameData.MyCmd.SetCmd(ret.Dict("cmds"));
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
			var rxdata = new RxData(json.Dict("args"));

			// 监听look事件，收到才进入房间
			if (e == "look") {
				refreshGameInfo(evt.Data);
			} else if (e == "prompt") {
				GameData.MyCmd.SetCmd(rxdata.Data);
			}

			// 通过事件广播出去
			switch(e) {
                case "takeseat":
					RxSubjects.TakeSeat.OnNext(rxdata);
					break;
				case "takecoin":
					RxSubjects.TakeCoin.OnNext(rxdata);
					break;
				case "unseat":
					RxSubjects.UnSeat.OnNext(rxdata);
					break;
				case "ready":
					RxSubjects.Ready.OnNext(rxdata);	
					break;
				case "gamestart":
					RxSubjects.GameStart.OnNext(rxdata);
					break;
				case "seecard":
					RxSubjects.SeeCard.OnNext(rxdata);
					break;
				case "look":
					RxSubjects.Look.OnNext(rxdata);
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
					RxSubjects.Exclusion.OnNext(rxdata);
					break;
				case "gameover":
					RxSubjects.GameOver.OnNext(rxdata);
					break;
				case "pausing": case "paused":
					RxSubjects.Paused.OnNext(rxdata);
					break;
				case "start":
					RxSubjects.Paused.OnNext(rxdata);
					break;
				case "game_end":
					RxSubjects.GameEnd.OnNext(rxdata);
					break;
				case "un_audit":
					RxSubjects.Audit.OnNext(rxdata);
					break;
				default:
					break;
			}
		});
	}

	private bool entered = false;

	// 只允许进入一次
	private void refreshGameInfo(Dictionary<string, object> json) {
		GameData.Shared.InitByJson(json);

		if (entered) {
			// Skip
		} else {
			SceneManager.LoadScene("PokerGame");
		}

		entered = true;
	}
}

