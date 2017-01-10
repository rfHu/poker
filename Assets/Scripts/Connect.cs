using BestHTTP.SocketIO;
using System;
using System.Collections.Generic;
using BestHTTP.SocketIO.Transports;
using BestHTTP.Cookies;
using Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
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

		GConf.userToken = "s%3AyqZMe2unwrbWgoK7JfZ8wvrAj_cbCBk5.yCzKvWbZQbYikN5DrozgGB2iRyRjgLsvveCfwwSm42c";

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

		GConf.uid = profile.String("uid");
		GConf.name = profile.String("name");
		GConf.avatar = profile.String("avatar");
		GConf.pin = token.String("pin");
	}

	public void Emit(Dictionary<string, object> json, Action<Dictionary<string, object>> callback = null) {
		seq++;
		json["seq"] = seq;
		json["pin"] = GConf.pin;
		json["uid"] = GConf.uid;
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

			// Ext.Log(json);

			if (json == null) {
				return ;
			}

			var e  = json.String("e");

			if (String.IsNullOrEmpty(e)) {
				return ;
			}

			// 监听look事件，收到才进入房间
			if (e == "look") {
				EnterGame(json);
			} else if (e == "prompt") {
				GInfo.HasSeat = json.Dict("args").Bool("unseat");
			}

			var data = new DelegateArgs(json);

			// 通过事件广播出去
			switch(e) {
                case "takeseat":
					Delegates.shared.OnTakeSeat(data);
					break;
				case "takecoin":
					Delegates.shared.OnTakeCoin(data);
					break;
				default:
					break;
			}
		});
	}

	bool enter = false;

	// 只允许进入一次
	void EnterGame(Dictionary<string, object> json) {
		if (enter) {
			return ;
		}

		enter = true;

		var args = json.Dict("args");
		var options = args.Dict("options");

		GConf.isOwner = options.String("ownerid") == GConf.uid;
		GConf.bankroll = options.ListInt("bankroll_multiple"); 
		GConf.ante = options.Int("ant");
		GConf.playerCount = options.Int("max_seats");
		GConf.rake = options.Float("rake_percent");
		GConf.duration = options.Int("time_limit");
		GConf.needAduit = options.Int("need_audit") == 1;
		GConf.GPSLimit = options.Int("gps_limit") == 1;
		GConf.IPLimit = options.Int("ip_limit") == 1;
		GConf.roomName = args.String("name");

		var bb = options.Int("limit");
		GConf.bb = bb ;
		GConf.sb = bb / 2;

		SceneManager.LoadScene("PokerGame");
	}
}

