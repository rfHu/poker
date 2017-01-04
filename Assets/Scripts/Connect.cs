using BestHTTP.SocketIO;
using System;
using UnityEngine;
using System.Collections.Generic;

public class Connect  {
	private SocketManager manager;
	private string url = "http://61.143.225.47:7001/socket.io/"; 

	private Dictionary<int, Action> actions;

	private int seq = 0;

	public Connect() {
		SocketOptions options = new SocketOptions();
		manager = new SocketManager(new Uri(url), options);
		manager.Socket.On("connect", OnConnect);
		manager.Open();	
	}

	void OnConnect(Socket socket, Packet packet, params object[] args) {
		Dictionary<string, object> parameters = new Dictionary<string, object>{
			{"uid", "u1226"},
			{"passwd", "110633"}
		};

		Emit(new Dictionary<string, object>{
			{"seq", 3}, 
			{"uid", 1},
			{"f", "login"},
			{"args", parameters}
		});
	}

	public void Emit(Dictionary<string, object> json, Action callback = null) {
		seq++;
		json["seq"] = seq;
		manager.Socket.Emit("rpc", json);
		actions.Add(seq, callback);
	}

	void Close() {
		manager.Close();
	}

	public static Connect shared;

	public static void Setup() {
		shared = new Connect();
		shared.manager.Socket.On("rpc_ret", (socket, packet, args) => {
			Debug.Log(args);
		});
	}
}