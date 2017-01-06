using BestHTTP.SocketIO;
using System;
using System.Collections.Generic;
using BestHTTP.SocketIO.Transports;
using BestHTTP.Cookies;
using Extensions;
using UnityEngine;

public class Connect  {
	private SocketManager manager;
	private string url = "http://61.143.225.47:7001/socket.io/"; 

	private Dictionary<int, Action<Dictionary<string, object>>> actions = new Dictionary<int, Action<Dictionary<string, object>>>();

	private int seq = 0;

	public Connect() {
		GameConfig.userToken = "s%3AyqZMe2unwrbWgoK7JfZ8wvrAj_cbCBk5.yCzKvWbZQbYikN5DrozgGB2iRyRjgLsvveCfwwSm42c";

		SocketOptions options = new SocketOptions();
		options.ConnectWith = TransportTypes.WebSocket;

		manager = new SocketManager(new Uri(url), options);
		manager.setCookie = (request) => {
			var cookie = new Cookie("connect.sid", GameConfig.userToken);
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
		});
	}

	private void SaveUserInfo(Dictionary<string, object> json) {

	}

	public void Emit(Dictionary<string, object> json, Action<Dictionary<string, object>> callback = null) {
		seq++;
		json["seq"] = seq;
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
		shared._Setup();	
	}

	private void _Setup() {
		manager.Socket.On("rpc_ret", (socket, packet, args) => {
			if (args.Length == 0) {
				return ;
			}

			var json = args[0] as Dictionary<string, object>;
			
			if (json == null) {
				return ;
			}

			int seq = json.IntValue("seq");

			if (actions.ContainsKey(seq)) {
				actions[seq](json);
				actions.Remove(seq);
			} 
		});

		manager.Socket.On("rpc", (socket, packet, args) => {
			if (args.Length == 0) {
				return ;
			}

			var json = args[0] as Dictionary<string, object>;

			if (json == null) {
				return ;
			}

			
		});
	}
}

