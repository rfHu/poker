using BestHTTP.SocketIO;
using System;
using System.Collections.Generic;
using BestHTTP.SocketIO.Transports;
using Extensions;
using UnityEngine;
using BestHTTP;
using UniRx;
using MaterialUI;

public sealed class Connect  {
	private SocketManager manager;
	private string url = "https://socket.poker.top/socket.io/"; 

	private Dictionary<int, Action<Dictionary<string, object>>> successCallbacks = new Dictionary<int, Action<Dictionary<string, object>>>();

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
			enterGame();
		});
	}

	private void enterGame() {
		Emit(new Dictionary<string, object>{
			{"f", "entergame"},
			{"args", GameData.Shared.Room}
		}, (json) => {
			var error = json.Int("err");

			if (error == 400) {				
				PokerUI.DisAlert("房间不存在！");
			}
		}, () => {
			PokerUI.DisAlert("连接房间超时");
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

	public void Emit(Dictionary<string, object> json, Action<Dictionary<string, object>> success = null, Action error = null, int timeout = 5) {
		seq++;
		json["seq"] = seq;
		json["pin"] = GameData.Shared.Pin;
		json["uid"] = GameData.Shared.Uid;
		manager.Socket.Emit("rpc", json);

		if (success != null) {
			IDisposable dispose = null;

			// 设置8秒超时
			if (error != null) {
				dispose = Observable.Timer(TimeSpan.FromSeconds(timeout)).AsObservable().Subscribe((_) => {
					successCallbacks.Remove(seq);
					error();
				});
			}

			successCallbacks.Add(seq, (data) => {
				var err = data.Int("err");

				if (err == 403) {
					PokerUI.ConflictAlert();	
					return ;
				}

				successCallbacks.Remove(seq);
				
				if (dispose != null) {
					dispose.Dispose();
				}

				success(data);
			});
		}
	}

	public void Close(Action callback = null) {
		Action act = () => {
			manager.Socket.Off();
			manager.Close();

			if (callback != null) {
				callback();
			}

			instance = null;
		};

		Emit(new Dictionary<string, object>{
			{"f", "exit"}
		}, (_) => {
			act();
		}, act, 2);	
	}

	public void CloseImmediate() {
		close();
	}

	public static Connect Shared {
		get {
			return instance;
		}
	}

	private static Connect instance;

	static public void Setup() {
		// 强制断开连接
		if (instance != null) {
			instance.close();
		}

		instance = new Connect();

		instance.manager.Socket.On("rpc_ret", (socket, packet, args) => {
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

			if (instance.successCallbacks.ContainsKey(seq)) {
				instance.successCallbacks[seq](json);
			} 
		});

		instance.manager.Socket.On("push", (socket, packet, args) => {
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
				case "takemore":
					RxSubjects.TakeMore.OnNext(rxdata);
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
					RxSubjects.Started.OnNext(rxdata);
					break;
				case "game_end":
					RxSubjects.GameEnd.OnNext(rxdata);
					break;
				case "un_audit":
					RxSubjects.Audit.OnNext(rxdata);
					break;
				case "unaudit_countdown":
					RxSubjects.UnAuditCD.OnNext(rxdata);
					break;
				case "pass":
					RxSubjects.Pass.OnNext(rxdata);
					break;
				case "un_pass":
					RxSubjects.UnPass.OnNext(rxdata);
					break;
				case "ending":
					RxSubjects.Ending.OnNext(rxdata);
					break;
				default:
					break;
			}
		});
	}

	// 强制关闭
	private void close() {
		manager.Close();
		manager.Socket.Off();
	}
}

