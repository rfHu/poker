using BestHTTP.SocketIO;
using System;
using System.Collections.Generic;
using BestHTTP.SocketIO.Transports;
using Extensions;
using UnityEngine;
using BestHTTP;
using UniRx;

public sealed class Connect  {
	public static string Proxy;
	public static string Domain = "https://socket.dev.poker.top";

	private SocketManager manager;

	private Dictionary<int, Action<Dictionary<string, object>>> successCallbacks = new Dictionary<int, Action<Dictionary<string, object>>>();

	private int seq = 0;

	private Connect() {
		// Charles Proxy
		if (!string.IsNullOrEmpty(Connect.Proxy)) {
			HTTPManager.Proxy = new HTTPProxy(new Uri(Connect.Proxy));
		}

		SocketOptions options = new SocketOptions();
		options.ConnectWith = TransportTypes.WebSocket;

		_.Log("Unity: Socket URL=" + Connect.Domain);

		manager = new SocketManager(new Uri(Connect.Domain + "/socket.io/"), options);
		manager.Socket.On("connect", onConnect);
		manager.Socket.On("reconnect", onConnect);

		manager.Socket.On("connecting", onDisconnect);
		manager.Socket.On("disconnect", onDisconnect);
		manager.Socket.On("reconnecting", onDisconnect);
		manager.Socket.On("reconnect_attempt", onDisconnect);

		manager.Socket.On("reconnect_failed", onReconnectFail);
		manager.Socket.On("error", onError);

		manager.Open();	
	}

	private void onConnect(Socket socket, Packet packet, params object[] args) {
		_.Log("Unity: 连接建立成功，执行login逻辑……");
		
		Emit(new Dictionary<string, object>{
			{"f", "login"},
			{"args", new Dictionary<string, object>{
				{"sid", GameData.Shared.Sid}
			}}	
		}, (json) => {
			var err = json.Int("err");
			if (err != 0) {
				PokerUI.ConflictAlert();

				return ;
			}

			_.Log("Unity: 登陆成功，准备进入房间……");

			// 登陆成功，写用户数据
			saveUserInfo(json);

			// 进入房间
			enterGame();
		});
	}

	private void onDisconnect(Socket socket, Packet packet, params object[] args) {
		
	}

	private void onReconnectFail(Socket socket, Packet packet, params object[] args) {
		_.Log("Reconnect Fail");
	}	

	private void onError(Socket socket, Packet packet, params object[] args) {
		_.Log("Connect Error");
	}

	private void enterGame() {
		Emit(new Dictionary<string, object>{
			{"f", "entergame"},
			{"args", new Dictionary<string, object> {
				{"roomid", GameData.Shared.Room},
				{"ver", Application.version}
			}}
		}, (json) => {
			var error = json.Int("err");

			if (error == 400) {				
				PokerUI.DisAlert("房间不存在！");
			}

			_.Log("Unity: 进入房间逻辑执行完毕");
		}, () => {
			PokerUI.DisAlert("连接服务器超时");
		});
	}

	private void saveUserInfo(Dictionary<string, object> json) {
		var ret = json.Dict("ret");
		var profile = ret.Dict("profile").ToObject<ProfileModel>();
		var token = ret.Dict("token");

		GameData.Shared.Uid = profile.uid;
		GameData.Shared.Name = profile.name;
		GameData.Shared.Avatar = profile.avatar;
		GameData.Shared.Pin = token.String("pin");
	}

	public void Emit(String rpc, Dictionary<string, object> args = null, Action<Dictionary<string, object>> success = null, Action error = null, int timeout = 5) {
		var dict = new Dictionary<string, object>{
			{"f", rpc},
			{"args", args}
		};

		Emit(dict, success, error, timeout);
	}

	public void Emit(Dictionary<string, object> json, Action<Dictionary<string, object>> success = null, Action error = null, int timeout = 5) {
		if (manager.State != BestHTTP.SocketIO.SocketManager.States.Open) {
			return ;
		}

		if (string.IsNullOrEmpty(GameData.Shared.Sid)) {
			return ;
		}

		seq++;
		json["seq"] = seq;
		json["pin"] = GameData.Shared.Pin;
		json["uid"] = GameData.Shared.Uid;
		manager.Socket.Emit("rpc", json);

		if (success != null) {
			IDisposable dispose = null;

			// 设置超时
			if (error != null) {
				dispose = Observable.Timer(TimeSpan.FromSeconds(timeout)).AsObservable().Subscribe((_) => {
					successCallbacks.Remove(seq);
					error();
				});
			}

			successCallbacks.Add(seq, (data) => {
				if (dispose != null) {
					dispose.Dispose();
				}
				
				var err = data.Int("err");

				if (err == 403) {
					PokerUI.ConflictAlert();	
					return ;
				}

				successCallbacks.Remove(seq);
				success(data);
			});
		}
	}

	public void Close(Action callback = null) {
		var acted = false;

		Action act = () => {
			if (acted) {
				return ;
			}

			acted = true;

			if (callback != null) {
				callback();
			}

			close();
		};

		// 2s后，不管服务器是否退出成功，客户端都强制退出
		Observable.Timer(TimeSpan.FromSeconds(2)).Subscribe((_) => {
			act();
		});
		
		if (manager.State == SocketManager.States.Open) {
			Emit(new Dictionary<string, object>{
				{"f", "exit"}
			}, (_) => {
				act();
			});
		} else {
			act();
		}
	}

	public void CloseImmediate() {
		close();
	}

	public static Connect Shared {
		get {
			return instance;
		}
	}

	public bool IsClosed() {
		return manager.State == SocketManager.States.Closed;		
	}

	private static Connect instance;

	static public void Setup() {
		if (string.IsNullOrEmpty(GameData.Shared.Sid) || string.IsNullOrEmpty(GameData.Shared.Room)) {
			return ;
		}

		_.Log("Unity: SID、RoomID设置成功，准备建立连接");

		// 强制断开连接
		if (instance != null) {
			_.Log("Unity: 尝试建立新连接，强制断开");
			instance.close();
		}

		// 开始游戏（退出时会被暂停）
		Time.timeScale = 1;

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

			var argsDict = json.Dict("args");
			var e  = json.String("e");

			if (argsDict.ContainsKey("cmds")) {
				var cmds = argsDict.Dict("cmds");
				GameData.MyCmd.SetCmd(cmds);
			}

			if (String.IsNullOrEmpty(e)) {
				return ;
			}

			var rxdata = new RxData(argsDict, e);

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
					RxSubjects.Bye.OnNext(rxdata);
					break;
				case "gameover":
					RxSubjects.GameOver.OnNext(rxdata);
					break;
				case "pausing": 
					RxSubjects.Pausing.OnNext(rxdata);
					break;
				case "paused":
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
                case "modify":
                    RxSubjects.Modify.OnNext(rxdata);
                    break;
                case "emoticon":
                    RxSubjects.Emoticon.OnNext(rxdata);
                    break;
                case "standup":
                    RxSubjects.StandUp.OnNext(rxdata);
                    break;
                case "kickout":
                    RxSubjects.KickOut.OnNext(rxdata);
                    break;
				case "showcard":
					RxSubjects.ShowCard.OnNext(rxdata);
					break;
                case "insurance":
                    RxSubjects.Insurance.OnNext(rxdata);
                    break;
                case "to_insurance":
                    RxSubjects.ToInsurance.OnNext(rxdata);
                    break;
                case "moretime":
                    RxSubjects.Moretime.OnNext(rxdata);
                    break;
				case "someone_seecard":
					RxSubjects.SomeOneSeeCard.OnNext(rxdata);
					break;
                case "expression":
                    RxSubjects.Expression.OnNext(rxdata);
                    break;
				case "gamer_state":
					RxSubjects.GamerState.OnNext(rxdata);
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
		instance = null;
	}
}

