using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using UniRx;
using Extensions;
using DarkTonic.MasterAudio;
using MaterialUI;

public class Controller : MonoBehaviour {
	public GameObject seat;

	public GameObject gameInfo;
	public GameObject gameInfoWrapper;
	public GameObject startButton;

	public List<GameObject> PublicCards;

	public List<GameObject> Seats;	

	// public GameObject Cutoff;

	List<Vector2> anchorPositions = new List<Vector2>();

	public GameObject OwnerButton; 

	void Awake () {
		setupSeats();
		registerRxEvents();

		// 数据驱动开发，在这里重新reload数据，触发事件
		GameData.Shared.Reload();
		showGameInfo();
		setupDealer();
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

	void changePositions(int index) {
		var count = GameData.Shared.PlayerCount;
		var left = anchorPositions.Skip(count - index).Take(index);
		var right = anchorPositions.Take(count - index);
		var newVectors = left.Concat(right).ToList(); 

		for (var i = 0; i < Seats.Count; i++) {
			Seats[i].GetComponent<Seat>().ChgPos(newVectors[i]);
		}
	}

	// 顺时针生成位置信息
	List<Vector2> getVectors(int total) {
		float width = G.UICvs.GetComponent<RectTransform>().rect.width;
		float height = G.UICvs.GetComponent<RectTransform>().rect.height;

		float top = height - 200;
		float bottom = 200; 
		float right = width / 2 - 70;
		float left = -width / 2 + 70; 

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

	void showGameInfo() {
		if (GameData.Shared.Owner && !GameData.Shared.GameStarted) {
			startButton.SetActive(true);
		}

		if (GameData.Shared.Owner) {
			OwnerButton.SetActive(true);
		}

		var roomName = GameData.Shared.RoomName;
		if (String.IsNullOrEmpty(roomName)) {
			roomName = "佚名";
		}

		addGameInfo(string.Format("[ {0} ]", roomName));

		var sb = GameData.Shared.SB;
		var bb = GameData.Shared.BB; 

		if (GameData.Shared.Straddle) {
			addGameInfo(string.Format("盲注: {0}/{1}/{2}", sb, bb, bb * 2));			
 		} else {
			addGameInfo(string.Format("盲注: {0}/{1}", sb, bb));
		}

		if (!string.IsNullOrEmpty(GameData.Shared.GameCode)) {
			addGameInfo(String.Format("邀请码: {0}", GameData.Shared.GameCode));
		}

		var ipLimit = GameData.Shared.IPLimit;
		var gpsLimit = GameData.Shared.GPSLimit;

		if (ipLimit && gpsLimit) {
			addGameInfo("IP 及 GPS 限制");
		} else if (gpsLimit) {
			addGameInfo("GPS 限制");
		} else if (ipLimit) {
			addGameInfo("IP 限制");
		}
	}

	private void setupDealer() {
		var dealer = (GameObject)Instantiate(Resources.Load("Prefab/Dealer"));
		dealer.GetComponent<Dealer>().Init(Seats);
	}

	private void addGameInfo(string text) {
		GameObject label = Instantiate(gameInfo);
		label.SetActive(true);
		label.GetComponent<Text>().text = text;
		label.transform.SetParent(gameInfoWrapper.transform, false);
	}

	private void setupSeats() {
		var numberOfPlayers = GameData.Shared.PlayerCount;

		anchorPositions = getVectors (numberOfPlayers);

		for (int i = 0; i < numberOfPlayers; i++) {
			GameObject cpseat = Instantiate (seat);
			cpseat.SetActive(true);
			
			var st = cpseat.GetComponent<Seat>();
			st.Init(i, anchorPositions[i]);		
			Seats.Add (cpseat);
		}
	}

	void registerRxEvents() {
		Action<Player> showPlayer = (obj) => {
			var parent = Seats[obj.Index].transform;
			var go = (GameObject)GameObject.Instantiate(Resources.Load("Prefab/Player"));
			go.GetComponent<PlayerObject>().ShowPlayer(obj, parent);
		};

		Action<int> enableSeat = (index) => {
			Seats[index].GetComponent<Image>().enabled = true;
		};

		RxSubjects.ChangeVectorsByIndex.AsObservable().DistinctUntilChanged().Subscribe(changePositions).AddTo(this);

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
			if (GameData.Shared.GameStartState) {
				var data = GameData.Shared.PublicCards;

				if (data.Count < 3) {
					return ;
				}

				SoundGroupVariation.SoundFinishedEventHandler cb = () => {
					MasterAudio.PlaySound("fapai");

					if (data.Count == 3) {
						showCards();
					} else {
						getCardFrom(e.Index).Show(e.Value, true);
					}
				};

				var sounds = MasterAudio.GetAllPlayingVariationsInBus("Wait");

				if (sounds.Count > 0) {
					sounds.Last().SoundFinished += cb;
				} else {
					cb();
				}
			} else {
				getCardFrom(e.Index).Show(e.Value, false);
			}
		}).AddTo(this);

		GameData.Shared.PublicCards.ObserveReset().Subscribe((_) => {
			resetAllCards();
		}).AddTo(this);

		RxSubjects.GameEnd.Subscribe((e) => {
			Commander.Shared.GameEnd();
		}).AddTo(this);

		RxSubjects.Ending.Subscribe((e) => {
			ToastManager.Show("房主提前结束牌局", 2f, _.HexColor("#212932"), new Color(1, 1, 1, 1), 22, null);	
		}).AddTo(this);
	}
	
	private Card getCardFrom(int index) {
		return PublicCards[index].GetComponent<Card>();
	}

	private void showCards() {
		var cards = GameData.Shared.PublicCards.ToList();

		if (cards.Count != 3) {
			return ;
		}

		for(var i = 0; i < cards.Count; i++) {
			var time = i * Card.TurnCardDuration;
			var local = i;

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
