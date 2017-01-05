using BestHTTP.SocketIO;
using System;
using System.Collections.Generic;
using BestHTTP.SocketIO.Transports;
using BestHTTP.Cookies;
using UnityEngine;

public class Connect  {
	private SocketManager manager;
	private string url = "http://61.143.225.47:7001/socket.io/"; 

	private Dictionary<int, Action> actions;

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
		});
	}

	public void Emit(Dictionary<string, object> json, Action callback = null) {
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
		shared.manager.Socket.On("rpc_ret", (socket, packet, args) => {
			if (args.Length == 0) {
				return ;
			}
			
			var json = args[0] as Dictionary<string, object>;

			if (json != null) {
				Ext.Log(json);
			}
		});
	}
}