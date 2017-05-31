using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using UniRx;
using Extensions;
using DarkTonic.MasterAudio;
using SimpleJSON;
using DG.Tweening;

public class Controller : MonoBehaviour {
	public GameObject seat;

	public GameObject gameInfo;
	public GameObject gameInfoWrapper;
	public GameObject startButton;

	public List<GameObject> PublicCards;

	public List<GameObject> Seats;	

	public GameObject PauseGame;

	public GameObject BBGo;
	public GameObject InviteCodeGo;
	public GameObject TimeLeftGo;

    public GameObject SeeLeftCard;
    public GameObject BuyTurnTime;

	public SpkTextGo SpkText;

	public GameObject SeeCardTable;
	public GameObject SeeCardTips;

	// public GameObject Cutoff;

	List<Vector2> anchorPositions = new List<Vector2>();

	public GameObject OwnerButton;

    public GameObject ExpressionButton;

	private bool hasShowEnding = false;

	public static Controller Instance; 

	void Awake () {
		if (Instance != null) {
			Destroy(Instance);
		}

		Instance = this;

		load();
    }

	public void load() {
		registerRxEvents();

		setupDealer();
		setMuteState();
		#if UNITY_EDITOR
		#else
				Commander.Shared.VoiceIconToggle(true);
		#endif
	}

	public void OnStartClick() {
		Connect.Shared.Emit(new Dictionary<string, object>(){
			{"f", "start"}
		}, (data) => {
			var err = data.Int("err");
			if (err == 0) {
				startButton.SetActive(false);
			}
		});
	}

	public void OnClickBuyTime() {
		var data = new Dictionary<string, object>(){
			{"type",112}
		};

        Connect.Shared.Emit(new Dictionary<string, object>() { 
			{"f", "moretime"},
			{"args", data}
        },(redata) => {
			var display = redata.Dict("ret").Int("display");
			if (display == 0)
			{
				BuyTurnTime.SetActive(false);
			}
        });
	}

	public void OnClickSeeCard() 
    {
		SeeLeftCard.SetActive(false);

        Connect.Shared.Emit(new Dictionary<string, object>() {
				{"f", "seecard"},
                {"args", null}
        }, (data) => {
            var err = data.Int("err");

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
		if (String.IsNullOrEmpty(roomName)) {
			roomName = "佚名";
		}

		addGameInfo(string.Format("[ {0} ]", roomName));

		var ipLimit = GameData.Shared.IPLimit;
		var gpsLimit = GameData.Shared.GPSLimit;

		if (ipLimit && gpsLimit) {
			addGameInfo("IP 及 GPS 限制");
		} else if (gpsLimit) {
			addGameInfo("GPS 限制");
		} else if (ipLimit) {
			addGameInfo("IP 限制");
		}

        if (GameData.Shared.NeedInsurance)
        {
            addGameInfo("保险模式");
        }
	}

    private void setBBGoText()
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
		var dealer = (GameObject)Instantiate(Resources.Load("Prefab/Dealer"));
		dealer.GetComponent<Dealer>().Init(Seats);
	}

	private void setMuteState() {
		if (GameData.Shared.muted) {
			MasterAudio.MuteEverything();
		}
	}

	private void addGameInfo(string text) {
		GameObject label = Instantiate(gameInfo);
		label.SetActive(true);
		label.GetComponent<Text>().text = text;
		label.transform.SetParent(gameInfoWrapper.transform, false);
	}

	private void setupSeats(int numberOfPlayers) {
		anchorPositions = getVectors (numberOfPlayers);

		for (int i = 0; i < numberOfPlayers; i++) {
			GameObject cpseat = Instantiate (seat);
			cpseat.SetActive(true);
			
			var st = cpseat.GetComponent<Seat>();
			st.Init(i, anchorPositions[i]);		
			Seats.Add (cpseat);
		}
	}

	private void setNotStarted() {
		if (GameData.Shared.Owner) {
			startButton.SetActive(true);
            PauseGame.SetActive(false);
		} else {
			PauseGame.SetActive(true);
			PauseGame.transform.Find("Text").GetComponent<Text>().text = "等待房主开始游戏";
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
		getCardFrom(pair.Key).Show(pair.Value, true, () => {
			queueIsActive = false;
			startQueue();
		});
	}

	void registerRxEvents() {
		// 玩家数是不能改变的，所以在这里认为数据准备好了
		GameData.Shared.PlayerCount.Where((value) => value > 0).Subscribe((value) => {
			setupSeats(value);
		}).AddTo(this);

		var infoShow = false;
		GameData.Shared.GameInfoReady.Where((ready) => ready && !infoShow).Subscribe((_) => {
			showGameInfo();
			infoShow = true;
		}).AddTo(this);

		GameData.Shared.LeftTime.Subscribe((value) => {
			if (!GameData.Shared.GameStarted) {
				setText(TimeLeftGo, "暂未开始");
                return;
			}

			if (value > 5 * 60) {
				hasShowEnding = false;
			} else {
				if (!hasShowEnding) {
					PokerUI.Toast("牌局将在5分钟内结束");
				}

				hasShowEnding = true;
			}

			setText(TimeLeftGo, secToStr(value));
		}).AddTo(this);

        GameData.Shared.Ante.Where((value) => value >= 0).Subscribe((value) => {
            setBBGoText();
        }).AddTo(this);

        GameData.Shared.Straddle.Subscribe((value) => {
            setBBGoText();
        }).AddTo(this);

		GameData.Shared.Paused.Subscribe((pause) => {
			if (!GameData.Shared.GameStarted) {
				setNotStarted();
				return ;
			}

			PauseGame.transform.Find("Text").GetComponent<Text>().text = "房主已暂停游戏";

			if (pause && !GameData.Shared.InGame) {
				PauseGame.SetActive(true);
			} else {
				PauseGame.SetActive(false);
			}
		}).AddTo(this);

		Action<Player> showPlayer = (obj) => {
			var parent = Seats[obj.Index].transform;
			var go = (GameObject)GameObject.Instantiate(Resources.Load("Prefab/Player"));
			go.GetComponent<PlayerObject>().ShowPlayer(obj, parent);
		};

		Action<int> enableSeat = (index) => {
			Seats[index].GetComponent<Image>().enabled = true;
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

		GameData.Shared.PublicCards.ObserveAdd().Subscribe((e) => {
			if (GameData.Shared.PublicCardAnimState) {
				MasterAudio.PlaySound("fapai");
				cardAnimQueue.Enqueue(new KeyValuePair<int, int>(e.Index, e.Value));
				startQueue();
			} else {
				getCardFrom(e.Index).Show(e.Value, false);
			}
		}).AddTo(this);

		GameData.Shared.PublicCards.ObserveReset().Subscribe((_) => {
			resetAllCards();
		}).AddTo(this);

		RxSubjects.GameEnd.Subscribe((e) => {
			// 关闭连接
			Connect.Shared.CloseImmediate();

			// 获取roomID，调用ExitCb后无法获取
			var roomID = GameData.Shared.Room;

			// 清理
			External.Instance.ExitCb(() => {
				_.Log("Unity: Game End");
				Commander.Shared.GameEnd(roomID);
			});	
		}).AddTo(this);

		RxSubjects.Ending.Subscribe((e) => {
			PokerUI.Toast("房主提前结束牌局");	
		}).AddTo(this);

        RxSubjects.Emoticon.Subscribe((e) =>
        {
            int fromSeatIndex = e.Data.Int("seat");
            int toSeatIndex = e.Data.Int("toseat");
            int pid = e.Data.Int("pid");
            bool isToMe = false;

            var fromSeat = Seats[0]; 
            var toSeat = Seats[0];

            foreach (var seat in Seats)
            {
                if (seat.GetComponent<Seat>().Index == fromSeatIndex)
                    fromSeat = seat;
                
                if (seat.GetComponent<Seat>().Index == toSeatIndex)
                {
                    toSeat = seat;

                    if (toSeatIndex == GameData.Shared.FindPlayerIndex(GameData.Shared.Uid))
                        isToMe = true;
                }
            }

            var em = (GameObject)GameObject.Instantiate(Resources.Load("Prefab/Emoticon"));
            em.GetComponent<Emoticon>().Init(fromSeat, toSeat, pid, isToMe);
        }).AddTo(this);

        RxSubjects.Expression.Subscribe((e) => {
            var expressionName = e.Data.String("expression");
            var uid = e.Data.String("uid");

            var expression = (GameObject)GameObject.Instantiate(Resources.Load("Prefab/Expression"));

            if (uid == GameData.Shared.Uid)
            {
                var parent = ExpressionButton.transform;
                SingleExpression(expression, parent);

				// 隐藏按钮
				findExpCvg().alpha = 0;
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

                var player = aimSeat.transform.FindChild("Player(Clone)");
                SingleExpression(expression, player);
            }

            expression.transform.FindChild("Face").GetComponent<Animator>().SetTrigger(expressionName);
           
			Observable.Timer(TimeSpan.FromSeconds(3)).Subscribe((_) => {
				Destroy(expression);
				if (uid == GameData.Shared.Uid) {
					findExpCvg().alpha = 1; // 显示按钮
				}
			}).AddTo(this);
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


        RxSubjects.GameOver.Subscribe((e) =>{
			if (GameData.Shared.PublicCards.Count < 5) {
            	SeeLeftCard.SetActive(true);
			}
        }).AddTo(this);

        RxSubjects.GameStart.Subscribe((e) => {
            SeeLeftCard.SetActive(false);
        }).AddTo(this);

		RxSubjects.Look.Subscribe((e) => {
			SeeLeftCard.SetActive(false);
        }).AddTo(this);

        RxSubjects.TurnToMyAction.Subscribe((action) => {
            if (action){
                BuyTurnTime.SetActive(true);
            }
            else 
            {
                BuyTurnTime.SetActive(false);
            }
        }).AddTo(this);

		RxSubjects.Pass.Subscribe((_) => {
			if (GameData.Shared.InGame) {
				PokerUI.Toast("记分牌带入成功，将在下局游戏生效");
			} else {
				PokerUI.Toast("记分牌带入成功");
			}
		}).AddTo(this);

		RxSubjects.SomeOneSeeCard.Subscribe((e) => {
			var name = e.Data.String("name");

			var go = (GameObject)GameObject.Instantiate(SeeCardTips);
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

		RxSubjects.Pausing.Subscribe((_) => {
			var text = "房主已暂停游戏";

			if (GameData.Shared.InGame) {
				text += "（下一手生效）";
			}

			PokerUI.Toast(text);
		}).AddTo(this);

		RxSubjects.Modify.Subscribe((e) =>{

            var data = e.Data;
            var str = "";

            foreach (var item in e.Data)
	        {
                switch (item.Key) 
                {
                    case "bankroll_multiple":
                        GameData.Shared.BankrollMul = data.IL("bankroll_multiple");
                        str = "房主将记分牌带入倍数改为：" + GameData.Shared.BankrollMul[0] + "-" + GameData.Shared.BankrollMul[1];
                        PokerUI.Toast(str);
                        break;

                    case "time_limit": 
                        GameData.Shared.Duration += data.Long("time_limit");
                        GameData.Shared.LeftTime.Value += data.Long("time_limit");
                        str = "房主将牌局延长了" + data.Long("time_limit") / 3600f + "小时";
                        PokerUI.Toast(str);
                        break;

                    case "ante":
                        GameData.Shared.Ante.Value = data.Int("ante");
                        str = "房主将底注改为：" + GameData.Shared.Ante.Value; 
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
                    default:
                        break;
                }
            }
        }).AddTo(this);

		 RxSubjects.KickOut.Subscribe((e) =>{
            string Uid = e.Data.String("uid");
            string name = GameData.Shared.FindPlayer(Uid).Name;
            string str = name + "被房主请出房间";
            PokerUI.Toast(str);
        }).AddTo(this);

        RxSubjects.StandUp.Subscribe((e) =>
        {
            string Uid = e.Data.String("uid");
            string name = GameData.Shared.FindPlayer(Uid).Name;
            string str = name + "被房主强制站起";
            PokerUI.Toast(str);
        }).AddTo(this);

		 RxSubjects.ToInsurance.Subscribe((e) =>
        {
            var InsurancePopup = (GameObject)GameObject.Instantiate(Resources.Load("Prefab/Insurance"));
            InsurancePopup.GetComponent<DOPopup>().Show(modal: false);
            InsurancePopup.GetComponent<Insurance>().Init(e.Data);
        }).AddTo(this);

         RxSubjects.Seating.Subscribe((action) => 
         {
             ExpressionButton.SetActive(action);
         }).AddTo(this);
	}

	private CanvasGroup findExpCvg() {
		return ExpressionButton.transform.Find("Btn").GetComponent<CanvasGroup>();
	}

    private static void SingleExpression(GameObject expression, Transform parent)
    {
        if (parent.FindChild("Expression(Clone)") != null)
            Destroy(parent.FindChild("Expression(Clone)").gameObject);

        expression.transform.SetParent(parent, false);
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
