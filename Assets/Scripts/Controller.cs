using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using UniRx;
using DarkTonic.MasterAudio;
using SimpleJSON;
using DG.Tweening;

public class Controller : MonoBehaviour {
	public GameObject LoadingModal;

	public GameObject startButton;

	public List<GameObject> PublicCards;
	[SerializeField]private Text[] gameInfoTexts;

	public List<GameObject> Seats;	

	public GameObject PauseGame;

	public GameObject BBGo;
	public GameObject InviteCodeGo;
	public GameObject TimeLeftGo;
	public GameObject Logo;
	public GameObject SNGGo;
	[SerializeField]private GameObject MTTGo; 

	public GameObject SNGBtn;

    public GameObject SeeLeftCard;

	public SpkTextGo SpkText;

	public GameObject SeeCardTable;
	public GameObject SeeCardTips;

    public GameObject SNGMsgButton;

	// public GameObject Cutoff;

	List<Vector2> anchorPositions = new List<Vector2>();

	public GameObject OwnerButton;

    public GameObject ExpressionButton;

    public GameObject ChatButton;

    public GameObject TalkButton;

	private bool hasShowEnding = false;

	public static Controller Instance; 

	public static Vector2 LogoVector;

	public Camera FXCam;

	void Awake () {
		ObjectsPool.Shared.SetCamera(FXCam);
		MaterialUI.DialogManager.SetParentCanvas(G.MaterialCvs);

		if (Instance != null) {
			PoolMan.DespawnAll();
			Destroy(Instance);
		}

		Instance = this;
		LogoVector = Logo.transform.position;

		load();
    }

    void Start() 
    {
        if (GameData.Shared.IsMatch())
        {
            OwnerButton.SetActive(false);
        } else {
			OwnerButton.SetActive(true);
		}
    }

	void OnDestroy()
	{
		// 在这里重置相关数据
		GameData.Shared.PlayerCount.Value = 0;
	}

	public void load() {
		var infoShow = false;
		GameData.Shared.GameInfoReady.Where((ready) => ready && !infoShow).Subscribe((_) => {
			infoShow = true;

			showGameInfo();
            SNGSetting();
			gameReload();
			registerEvents();
			setupDealer();
			setOptions();
		}).AddTo(this);
		
		#if UNITY_EDITOR
		#else
				Commander.Shared.VoiceIconToggle(true);
		#endif
	}

	public void OnStartClick() {
		Connect.Shared.Emit(new Dictionary<string, object>(){
			{"f", "start"},
            {"for_match", 1},
		}, (data, err) => {
			if (err == 0) {
				startButton.SetActive(false);
			}
		});
	}

	public void OnClickSeeCard() 
    {
		SeeLeftCard.SetActive(false);

        Connect.Shared.Emit(new Dictionary<string, object>() {
				{"f", "seecard"},
                {"for_match", 1},
        }, (data, err) => {
            if (err != 0)
            {
                PokerUI.Toast(data.String("ret"));
            }
        });
    }

	void changePositions(int index, bool anim = true) {
		var count = GameData.Shared.PlayerCount.Value;
		var left = anchorPositions.Skip(count - index).Take(index);
		var right = anchorPositions.Take(count - index);
		var newVectors = left.Concat(right).ToList(); 

		for (var i = 0; i < Seats.Count; i++) {
			Seats[i].GetComponent<Seat>().ChgPos(newVectors[i], anim);
		}
	}

	public static int TopMargin = 290;

	// 顺时针生成位置信息
	List<Vector2> getVectors(int total) {
		float width = G.UICvs.GetComponent<RectTransform>().rect.width;
		float height = G.UICvs.GetComponent<RectTransform>().rect.height;

		float top = height - TopMargin;
		float bottom = TopMargin + 20; 
		float right = width / 2 - 100;
		float left = -width / 2 + 100; 

		float hh = top - bottom;
		float ww = width - 200;

		// float h3 = hh / 3;
		float h4 = hh / 4;
		float w3 = ww / 2 - ww / 3;

		Vector2 vector = new Vector2 (0, bottom);

		if (total == 2) {
			return new List<Vector2>{
				vector,
				new Vector2(0, top),
			};
		}

		if (total == 6) {
			return new List<Vector2> {
				vector, 
				new Vector2(left, bottom + h4),
				new Vector2(left, bottom + 3 * h4),
				new Vector2(0, top),
				new Vector2(right, bottom + 3 * h4),
				new Vector2(right, bottom + h4),
			};
		}

		if (total == 7) {
			return new List<Vector2> {
				vector, 
				new Vector2(left, bottom + h4),
				new Vector2(left, bottom + 3 * h4),
				new Vector2(-w3, top),
				new Vector2(w3, top),
				new Vector2(right, bottom + 3 * h4),
				new Vector2(right, bottom + h4),
			};
		}

		if (total == 8) {
			return new List<Vector2> {
				vector, 
				new Vector2(left, bottom + h4),
				new Vector2(left, bottom + 2 * h4),
				new Vector2(left, bottom + 3 * h4),
				new Vector2(0, top),
				new Vector2(right, bottom + 3 * h4),
				new Vector2(right, bottom + 2 * h4),
				new Vector2(right, bottom + h4),
			};
		}

		if (total == 9) {
			return new List<Vector2> {
				vector, 
				new Vector2(left, bottom + h4),
				new Vector2(left, bottom + 2 * h4),
				new Vector2(left, bottom + 3 * h4),
				new Vector2(-w3, top),
				new Vector2(w3, top),
				new Vector2(right, bottom + 3 * h4),
				new Vector2(right, bottom + 2 * h4),
				new Vector2(right, bottom + h4),
			};
		}

		throw new Exception("不支持游戏人数");
	}

	private void setText(GameObject go, String text) {
		go.transform.Find("Value").GetComponent<Text>().text = text;
	}

	void showGameInfo() {
		if (!String.IsNullOrEmpty(GameData.Shared.GameCode)) {
			InviteCodeGo.SetActive(true);
			setText(InviteCodeGo, String.Format("{0}", GameData.Shared.GameCode));
		} else {
			InviteCodeGo.SetActive(false);
		}

		var roomName = GameData.Shared.RoomName;
		gameInfoTexts[0].text = string.Format("[ {0} ]", roomName);

		if (GameData.Shared.Type == GameType.MTT) {
			var num = GameData.Shared.TableNumber; 
			if (num != 0) {
				gameInfoTexts[1].text = "牌桌" + num;
			} else {
				gameInfoTexts[1].text = "决赛桌";
			}
		}

		var ipLimit = GameData.Shared.IPLimit;
		var gpsLimit = GameData.Shared.GPSLimit;

		if (ipLimit && gpsLimit) {
			gameInfoTexts[1].text = "IP 及 GPS 限制";
		} else if (gpsLimit) {
			gameInfoTexts[1].text = "GPS 限制";
		} else if (ipLimit) {
			gameInfoTexts[1].text = "IP 限制";
		}

		if (GameData.Shared.NeedInsurance)
		{
			gameInfoTexts[2].text = "保险模式";
		}
		
	}

    private void setBBText()
    {

        var sb = GameData.Shared.SB;
        var bb = GameData.Shared.BB;
        var straStr = "";
        var anteStr = "";

        if (GameData.Shared.Straddle.Value)
        {
            straStr = string.Format("/{0}", bb * 2);
        }

        if (GameData.Shared.Ante.Value > 0)
        {
            anteStr = string.Format(" ({0})", GameData.Shared.Ante.Value);
        }

        setText(BBGo, string.Format("{0}/{1}{2}{3}", sb, bb, straStr, anteStr));
    }

	private void setupDealer() {
		var dealer = PoolMan.Spawn("Dealer");
		dealer.GetComponent<Dealer>().Init(Seats);
	}

	private void setOptions() {
		if (GameSetting.muted) {
			MasterAudio.MuteEverything();
		}
        Commander.Shared.OptionToggle(!GameSetting.talkSoundClose, 2);
        Commander.Shared.OptionToggle(!GameSetting.chatBubbleClose, 1);
	}

	private void setupSeats(int numberOfPlayers) {
		// 删除已有座位
		var seats = FindObjectsOfType<Seat>();
		foreach(var seat in seats) {
			PoolMan.Despawn(seat.transform);
		}
		Seats.Clear();
		
		anchorPositions = getVectors (numberOfPlayers);

		for (int i = 0; i < numberOfPlayers; i++) {
			GameObject cpseat = PoolMan.Spawn("Seat").gameObject;
			cpseat.SetActive(true);
			
			var st = cpseat.GetComponent<Seat>();
			st.Init(i, anchorPositions[i]);		
			Seats.Add (cpseat);
		}
	}

	private void setNotStarted() {
		if (GameData.Shared.Owner && !GameData.Shared.IsMatch()) {
			startButton.SetActive(true);
            PauseGame.SetActive(false);
		} else {
			startButton.SetActive(false);
			PauseGame.SetActive(true);

			var text = GameData.Shared.IsMatch() ? "比赛报名中" : "等待房主开始游戏";
			PauseGame.transform.Find("Text").GetComponent<Text>().text = text;
		}
	}

	private string secToStr(long seconds) {
		var hs = 3600;
		var ms = 60;

		var h = Mathf.FloorToInt(seconds / hs);		
		var m = Mathf.FloorToInt(seconds % hs / ms);
		var s = (seconds % ms);

		return string.Format("{0}:{1}:{2}", fix(h), fix(m), fix(s));	
	}

	private string fix<T>(T num) {
		var str = num.ToString();
		if (str.Length < 2) {
			return "0" + str;
		}
		return str;
	}

	private bool queueIsActive = false;
	private Queue<KeyValuePair<int, int>> cardAnimQueue = new Queue<KeyValuePair<int, int>>();
	private void startQueue()  {
		if (cardAnimQueue.Count <= 0 || queueIsActive) {
			return ;
		}

		queueIsActive = true;

		var pair = cardAnimQueue.Dequeue();
		var card = getCardFrom(pair.Key);

		card.Show(pair.Value, true, () => {
			queueIsActive = false;
			startQueue();
		});

		if (!GameData.Shared.InGame) {
			card.Darken();
		}
	}

	private void gameReload() {
		SeeLeftCard.SetActive(false);
		cardAnimQueue.Clear();
		queueIsActive = false;

		if (GameData.Shared.IsMatch()) {
			SNGBtn.SetActive(true);
		} else {
			SNGBtn.SetActive(false);
		}

		setBBText();
	}

	private void registerEvents() {
		GameData.Shared.LeftTime.Subscribe((value) => {
			if (!GameData.Shared.GameStarted) {
				setText(TimeLeftGo, "未开始");
				return;
			}

			setText(TimeLeftGo, secToStr(value));

			if (GameData.Shared.IsMatch()) {
				return ;
			}

			if (value > 5 * 60) {
				hasShowEnding = false;
			} else {
				if (!hasShowEnding) {
					PokerUI.Toast("牌局将在5分钟内结束");
				}

				hasShowEnding = true;
			}

		}).AddTo(this);

		RxSubjects.UnSeat.AsObservable().Subscribe((e) => {
			var uid = e.Data.String("uid");
			if (uid == GameData.Shared.Uid && e.Data.Int("type") == 2) {
				PokerUI.Alert("您已连续3次超时，先站起来休息下吧~");
			}
		}).AddTo(this);

		 GameData.Shared.Ante.Subscribe((value) => {
            setBBText();
        }).AddTo(this);

        GameData.Shared.Straddle.Subscribe((value) => {
            setBBText();
        }).AddTo(this);

		GameData.Shared.PlayerCount.Where((value) => value > 0).Subscribe((value) => {
			setupSeats(value);
		}).AddTo(this);

		RxSubjects.Connecting.Subscribe((stat) => {
			LoadingModal.transform.SetAsLastSibling();
			LoadingModal.SetActive(stat); 
		}).AddTo(this);

		RxSubjects.Seating.Subscribe((action) => 
        {
			foreach(var go in Seats) {
				var seat = go.GetComponent<Seat>();
				if (action) {
					seat.Hide();
				} else if (seat.Index >= 0) {
					seat.Show();
				}
			}
            ExpressionButton.SetActive(action);
        }).AddTo(this);
       
	   	subsPublicCards();
		subsPlayer();
		subsRoomSetting();

        RxSubjects.Emoticon.Subscribe((e) =>
        {
            if (GameSetting.emoticonClose)
                return;

            int fromSeatIndex = e.Data.Int("seat");
            int toSeatIndex = e.Data.Int("toseat");
            int pid = e.Data.Int("pid");
            bool isToMe = false;

            var fromSeatPos = new Vector2(0, 1920); 
            var toSeat = Seats[0];

            foreach (var seat in Seats)
            {
                if (seat.GetComponent<Seat>().Index == fromSeatIndex)
                    fromSeatPos = seat.GetComponent<RectTransform>().anchoredPosition;
                
                if (seat.GetComponent<Seat>().Index == toSeatIndex)
                {
                    toSeat = seat;

                    if (toSeatIndex == GameData.Shared.FindPlayerIndex(GameData.Shared.Uid))
                        isToMe = true;
                }
            }

            var em = PoolMan.Spawn("Emo" + pid);
            em.GetComponent<Emoticon>().Init(fromSeatPos, toSeat, isToMe);
        }).AddTo(this);

		Dictionary<string, Transform> expressCache = new Dictionary<string, Transform>(){};
        RxSubjects.Expression.Subscribe((e) => {
            var expressionName = e.Data.String("expression");
            var uid = e.Data.String("uid");
            var expression = PoolMan.Spawn("Expression");
			Transform parent;

            if (uid == GameData.Shared.Uid)
            {
                parent = ExpressionButton.transform;
                //有可能出现点击表情时，玩家已经站起的现象
                if (!parent.gameObject.activeInHierarchy)
                    return;
                
            }
            else
            {
                var seatIndex = e.Data.Int("seat");
                var aimSeat = Seats[0];

                foreach (var seat in Seats)
                {
                    if (seat.GetComponent<Seat>().Index == seatIndex)
                    {
                        aimSeat = seat;
                        break;
                    }
                }

                parent = aimSeat.GetComponentInChildren<PokerPlayer.PlayerOppo>().transform;
            }

			// 删除上一个
			if (expressCache.ContainsKey(uid)) {
				var exp = expressCache[uid];
				PoolMan.Despawn(exp);
				expressCache.Remove(uid);
			}

            expression.GetComponent<Expression>().SetTrigger(expressionName, parent);
			expressCache.Add(uid, expression);
        }).AddTo(this);

        RxSubjects.MatchRank.Subscribe((json) => {
            var rank = json.Data.Int("rank");
            var score = json.Data.Int("score");
            var isEnd = json.Data.Int("is_end") == 1;

            var SNGWinner = PoolMan.Spawn("SNGWinner");
            SNGWinner.GetComponent<DOPopup>().ShowModal(new Color(0, 0, 0, 0.7f), closeOnClick: false);
            SNGWinner.GetComponent<SNGWinner>().Init(rank , score, isEnd);
        }).AddTo(this);

		RxSubjects.ShowAudio.Where(isGuest).Subscribe((json) => {
			var N = JSON.Parse(json);
			var name = N["name"].Value;
			SpkText.ShowMessage(String.Format("{0}发送了一段语音", name));
		}).AddTo(this);

		RxSubjects.SendChat.Where(isGuest).Subscribe((json) => {
			var N = JSON.Parse(json);
			var name = N["name"].Value;
			var text = N["text"].Value;

			SpkText.ShowMessage(String.Format("{0}: {1}", name, text));
		}).AddTo(this);

        RxSubjects.Award27.Subscribe((e) => {
            var win27Emo = PoolMan.Spawn("Win27Emo");
            win27Emo.transform.SetParent(G.UICvs.transform, false);
            win27Emo.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }).AddTo(this);

        RxSubjects.GameOver.Subscribe((e) =>{
			if (GameData.Shared.IsMatch()) {
				return ;
			}

            if (GameData.Shared.PublicCards.Count < 5)
            {
            	SeeLeftCard.SetActive(true);
			}
        }).AddTo(this);

        RxSubjects.GameStart.Subscribe((e) => {
			gameReload();
        }).AddTo(this);

		RxSubjects.Look.Subscribe((e) => {
			gameReload();
        }).AddTo(this);

		RxSubjects.Pass.Subscribe((_) => {
			if (GameData.Shared.InGame) {
				PokerUI.Toast("记分牌带入成功（下一手生效）");
			} else {
				var text = GameData.Shared.IsMatch() ?  "报名成功" : "记分牌带入成功";
				PokerUI.Toast(text);
			}
		}).AddTo(this);

		RxSubjects.SomeOneSeeCard.Subscribe((e) => {
			var name = e.Data.String("name");

			var go = (GameObject)GameObject.Instantiate(SeeCardTips, G.UICvs.transform);
			var cvg = go.GetComponent<CanvasGroup>(); 
			var text = go.transform.Find("Text").GetComponent<Text>();

			text.text = String.Format("<color=#4FC3F7FF>{0}</color>看了剩余公共牌", name);
			
			go.SetActive(true);
			cvg.alpha = 0;
			go.transform.SetParent(SeeCardTable.transform);
			
			cvg.DOFade(1, 0.2f).OnComplete(() => {
				Observable.Timer(TimeSpan.FromSeconds(2.5)).Subscribe((tmp) => {
					cvg.DOFade(0, 0.2f).OnComplete(() => {
						Destroy(go);
					});
				}).AddTo(this);
			});	
		}).AddTo(this);

		RxSubjects.ToInsurance.Subscribe((e) =>
        {
            var InsurancePopup = PoolMan.Spawn("Insurance");
            InsurancePopup.GetComponent<DOPopup>().Show();
            InsurancePopup.GetComponent<Insurance>().Init(e.Data, true);
        }).AddTo(this);

        RxSubjects.ShowInsurance.Subscribe((e) =>
        {
            var InsurancePopup = PoolMan.Spawn("Insurance");
            InsurancePopup.GetComponent<DOPopup>().Show();
            InsurancePopup.GetComponent<Insurance>().Init(e.Data, false);
        }).AddTo(this);

		RxSubjects.RaiseBlind.Subscribe((e) => {
			var bb = e.Data.Int("big_blind");
			var cd = e.Data.Int("blind_countdown");

			GameData.Shared.BB = bb;
			GameData.Shared.LeftTime.Value = cd;

			PokerUI.Toast(string.Format("下一手盲注将升至{0}/{1}", bb / 2, bb));			
		}).AddTo(this);

		RxSubjects.OffScore.Subscribe((e) => {
			var type = e.Data.Int("type");
			if (type == 0) {
				PokerUI.Toast("您已提前下分，将在本局结束后结算");
				return ;
			} 

			if (type != 1) {
				return ;
			}

			var name = e.Data.String("name");
			var avatar = e.Data.String("avatar");

			var dt = e.Data.Dict("data");
			var takecoin = dt.Int("takecoin");
			var profit = dt.Int("bankroll") - takecoin;

			PoolMan.Spawn("OffScore").GetComponent<OffScore>().Show(avatar, takecoin, profit);

			// 已下分，bankroll为0
			GameData.Shared.Bankroll.Value = 0;
		}).AddTo(this);

        RxSubjects.ToAddOn.Subscribe((e) => {
            var go = PoolMan.Spawn("RebuyOrAddon");
            go.GetComponent<DOPopup>().Show();
            go.GetComponent<RebuyOrAddon>().Init(false, true);
        }).AddTo(this);

        RxSubjects.ToRebuy.Subscribe((e) =>
        {
            var go = PoolMan.Spawn("RebuyOrAddon");
            go.GetComponent<DOPopup>().Show();
            go.GetComponent<RebuyOrAddon>().Init(true,true);
        }).AddTo(this);

		RxSubjects.GameEnd.Subscribe((e) => {
			// 关闭连接
			Connect.Shared.CloseImmediate();

			if (SNGWinner.IsSpawned) {
				return ;
			}

			// 获取roomID，调用ExitCb后无法获取
			var roomID = GameData.Shared.Room;
            var matchID = GameData.Shared.MatchID;
            var ID = "";
            var page = "";
			// 清理
			External.Instance.ExitCb(() => {
                switch (GameData.Shared.Type)
                {
                    case GameType.Normal:
                        ID = roomID;
                        page = "record.html";
                        break;
                    case GameType.SNG:
                        ID = roomID;
                        page = "record_sng.html";
                        break;
                    case GameType.MTT:
                        ID = matchID;
                        page = "record_mtt.html";
                        break;
                    default:
                        break;
                }

				Commander.Shared.GameEnd(ID, page);
			});	
		}).AddTo(this);

		RxSubjects.MTTMatch.Subscribe((e) => {
			var type = e.Data.Int("type");

			if (type == 3) {
				GameData.Shared.Room = e.Data.String("data");
				Connect.Shared.EnterGame();
			}
		}).AddTo(this);
	}

	private void subsPlayer() {
		Action<Player> showPlayer = (obj) => {
			var parent = Seats[obj.Index].transform;

			if (obj.Uid == GameData.Shared.Uid) {
				var go = PoolMan.Spawn("PlayerSelf");
				go.GetComponent<PokerPlayer.PlayerSelf>().Init(obj, parent);
			} else {
				var go = PoolMan.Spawn("PlayerOppo");
				go.GetComponent<PokerPlayer.PlayerOppo>().Init(obj, parent);
			}

			parent.GetComponent<Seat>().Hide();
		};

		Action<int> enableSeat = (index) => {
			Seats[index].GetComponent<Seat>().Show();
		};

		RxSubjects.ChangeVectorsByIndex.AsObservable().DistinctUntilChanged().Subscribe((index) => {
			changePositions(index);
		}).AddTo(this);

		GameData.Shared.Players.ObserveReplace().Subscribe((data) => {
			data.OldValue.Destroy();
			enableSeat(data.OldValue.Index);
			showPlayer(data.NewValue);	
		}).AddTo(this);

		GameData.Shared.Players.ObserveAdd().Subscribe((data) => {
			showPlayer(data.Value);
		}).AddTo(this);

		GameData.Shared.Players.ObserveRemove().Subscribe((data) => {
			enableSeat(data.Value.Index);
			data.Value.Destroy();
		}).AddTo(this);

		GameData.Shared.Players.ObserveReset().Subscribe((data) => {
            // Skip
		}).AddTo(this);

		// 第一次手动初始化
		foreach(var player in GameData.Shared.Players.ToList()) {
			showPlayer(player.Value);
		}	
	}

	private void subsRoomSetting() {
		RxSubjects.Ending.Subscribe((e) => {
			PokerUI.Toast("房主提前结束牌局");	
		}).AddTo(this);

		GameData.Shared.Paused.Subscribe((pause) => {
			if (!GameData.Shared.GameStarted) {
				setNotStarted();
				return ;
			}

			// 服务器升级
			if (pause == 5) {
				PokerUI.ToastThenExit("服务器升级中…");
				return ;
			} 

			PauseGame.transform.Find("Text").GetComponent<Text>().text = "房主已暂停游戏";

			if (pause > 0 && !GameData.Shared.InGame) {
				PauseGame.SetActive(true);
			} else {
				PauseGame.SetActive(false);
			}
		}).AddTo(this);

		RxSubjects.Pausing.Subscribe((e) => {
			var type = e.Data.Int("type");
			var text = "";

			if (type == 5) {
				text = "服务器即将升级，牌局将强制暂停";
			} else {
				text = "房主已暂停游戏";

				if (GameData.Shared.InGame) {
					text += "（下一手生效）";
				}
			}

			PokerUI.Toast(text);
		}).AddTo(this);

		RxSubjects.Modify.Subscribe((e) =>{
            var data = e.Data;

            foreach (var item in e.Data)
	        {
				var str = "";
                switch (item.Key) 
                {
                    case "bankroll_multiple":
                        GameData.Shared.BankrollMul = data.IL("bankroll_multiple");
                        str = "房主将记分牌倍数改为：" + GameData.Shared.BankrollMul[0] + "-" + GameData.Shared.BankrollMul[1];
                        PokerUI.Toast(str);
                    	break;

                    case "time_limit": 
                        GameData.Shared.Duration += data.Long("time_limit");
                        GameData.Shared.LeftTime.Value += data.Long("time_limit");

						var time = data.Long("time_limit") / 3600f;
						var digit = "小时";

						if (time < 1) {
							time = time * 60;
							digit = "分钟";
						}

                        str = "房主将牌局延长了" + time.ToString()  + digit;
                        PokerUI.Toast(str);
                        break;

                    case "ante":
                        GameData.Shared.Ante.Value = data.Int("ante");
                        str = "房主将前注改为：" + GameData.Shared.Ante.Value; 
                        PokerUI.Toast(str);
                        break;

                    case "need_audit":
                        GameData.Shared.NeedAudit = data.Int("need_audit") == 1;
                        str = GameData.Shared.NeedAudit ? "房主开启了授权带入" : "房主关闭了授权带入";
                        PokerUI.Toast(str);
                        break;

                    case "straddle":
                        GameData.Shared.Straddle.Value = data.Int("straddle") != 0;
                        str = GameData.Shared.Straddle.Value ? "房主开启了Straddle" : "房主关闭了Straddle"; 
                        PokerUI.Toast(str);
						break;

                    case "turn_countdown":
						GameData.Shared.SettingThinkTime = data.Int("turn_countdown");
                        str = "房主将思考时间改为" + GameData.Shared.SettingThinkTime +"秒";
                        PokerUI.Toast(str);
                        break;

					case "off_score":
						var off = data.Int("off_score") == 1;
						GameData.Shared.OffScore.Value = off;
					 	str = off ? "房主开启了提前下分" : "房主关闭了提前下分";
						PokerUI.Toast(str);
						break;	
                    default:
                        break;
                }
            }
        }).AddTo(this);

		 RxSubjects.KickOut.Subscribe((e) =>{
            string str = e.Data.String("name") + "被房主请出房间";
            PokerUI.Toast(str);
        }).AddTo(this);

        RxSubjects.StandUp.Subscribe((e) =>
        {
            int seat = e.Data.Int("where");
            if (seat >-1)
            {
                string str = e.Data.String("name") + "被房主强制站起";
                PokerUI.Toast(str);
            }
        }).AddTo(this);

		GameData.Shared.TalkLimit.Subscribe((limit) => 
        {
            TalkLimit(limit);
        }).AddTo(this);

        RxSubjects.NoTalking.Subscribe((e) => 
        {
            string Uid = e.Data.String("uid");
            bool type = e.Data.Int("type") == 1;
            string str = e.Data.String("name");

            str += type ? "被房主禁言" : "被解除禁言";

            if (Uid == GameData.Shared.Uid)
            {
                TalkLimit(type);
            }

            PokerUI.Toast(str);

        }).AddTo(this);
	}

	private void subsPublicCards() {
		GameData.Shared.PublicCards.ObserveAdd().Subscribe((e) => {
			if (GameData.Shared.PublicCardAnimState) {
				G.PlaySound("fapai");
				cardAnimQueue.Enqueue(new KeyValuePair<int, int>(e.Index, e.Value));
				startQueue();
			} else {
				getCardFrom(e.Index).Show(e.Value, false);
			}
		}).AddTo(this);

		GameData.Shared.PublicCards.ObserveReset().Subscribe((_) => {
			resetAllCards();
		}).AddTo(this);

		// 第一次手动初始化
		var list = GameData.Shared.PublicCards.ToList();

		for (var i = 0; i < list.Count; i++) {
			getCardFrom(i).Show(list[i], false);	
		}
	}

    private void SNGSetting()
    {
		SNGGo.SetActive(false);
		MTTGo.SetActive(false);

		if (GameData.Shared.Type == GameType.MTT) {
			MTTGo.SetActive(true);
		} else if (GameData.Shared.Type == GameType.SNG) {
			SNGGo.SetActive(true);	
		}
    }

    private void TalkLimit(bool limit)
    {
        ChatButton.SetActive(!limit);
        TalkButton.SetActive(!limit);
#if UNITY_EDITOR
#else
				Commander.Shared.VoiceIconToggle(!limit);
#endif
    }

	private bool isGuest(string json) {
		var N = JSON.Parse(json);
		var uid = N["uid"].Value;

		return GameData.Shared.FindPlayerIndex(uid) == -1;
	}

	private Card getCardFrom(int index) {
		return PublicCards[index].GetComponent<Card>();
	}

	private void showAllCards() {
		var cards = GameData.Shared.PublicCards.ToList();

		if (cards.Count < 3) {
			return ;
		}

		for(var i = 0; i < cards.Count; i++) {
			var time = i * Card.TurnCardDuration;
			var local = i;

			if (local >= PublicCards.Count) {
				return ;
			}

			if (time == 0) {
				getCardFrom(local).Show(cards[local], true);
			} else {
				Observable.Timer(TimeSpan.FromSeconds(time)).AsObservable().Subscribe((_) => {
					getCardFrom(local).Show(cards[local], true);
				}).AddTo(this);			
			}
		}
	}

	void resetAllCards() {
		foreach(GameObject obj in PublicCards) {
			var card = obj.GetComponent<Card>();
			card.Turnback();
			card.Hide();
		}		
	}
}
