﻿using BestHTTP.SocketIO;
using System;
using System.Collections.Generic;
using BestHTTP.SocketIO.Transports;
using Extensions;
using UnityEngine;
using BestHTTP;

public sealed class Connect  {
	private SocketManager manager;
	private string url = "http://61.143.225.47:7001/socket.io/"; 

	private Dictionary<int, Action<Dictionary<string, object>>> actions = new Dictionary<int, Action<Dictionary<string, object>>>();

	private int seq = 0;

	private Connect() {
		// Charles Proxy
		if (Debug.isDebugBuild && !string.IsNullOrEmpty(GameData.Shared.Proxy)) {
			HTTPManager.Proxy = new HTTPProxy(new Uri(GameData.Shared.Proxy));
		}

		SocketOptions options = new SocketOptions();
		options.ConnectWith = TransportTypes.WebSocket;

		manager = new SocketManager(new Uri(url), options);
		manager.Socket.On("connect", onConnect);

		manager.Open();	
	}

	private void onConnect(Socket socket, Packet packet, params object[] args) {
		Emit(new Dictionary<string, object>{
			{"f", "login"},
			{"args", new Dictionary<string, object>{
				{"sid", GameData.Shared.Sid}
			}}	
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

	public void Close() {
		Emit(new Dictionary<string, object>{
			{"f", "exit"}
		}, (_) => {
			manager.Close();
			Commander.Shared.Exit();
		});	
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
	private bool hasSet = false;

	public void Setup() {
		if (hasSet) {
			return ;
		}

		hasSet = true;

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

			var rxdata = new RxData(json.Dict("args"), json.String("e"));

			if (e == "prompt") {
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
					RxSubjects.Deal.OnNext(rxdata);	
					break;
				case  "moveturn":
					RxSubjects.MoveTurn.OnNext(rxdata);
					break;
				case  "fold":
					RxSubjects.Fold.OnNext(rxdata);
					break;
				case  "check":
					RxSubjects.Check.OnNext(rxdata);
					break;
				case  "raise":
					RxSubjects.Raise.OnNext(rxdata);
					break;
				case  "call":
					RxSubjects.Call.OnNext(rxdata);
					break;
				case  "all_in":
					RxSubjects.Raise.OnNext(rxdata);
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
}

