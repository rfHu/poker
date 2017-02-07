using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using DG.Tweening;
using UniRx;
using Extensions;

public class Controller : MonoBehaviour {
	public GameObject seat;
	public Canvas canvas;

	public GameObject gameInfo;
	public GameObject gameInfoWrapper;
	public GameObject startButton;

	public List<GameObject> PublicCards;

	public GameObject Pot;

	public List<GameObject> Seats;	

	GameObject dealer;

	List<Vector2> anchorPositions = new List<Vector2>();

	public GameObject OwnerButton; 

	void Start () {
		registerRxEvents();

		// 数据驱动开发，在这里重新reload数据，触发事件
		GameData.Shared.Reload();
		showGameInfo();
	}

	void changePositions(int index) {
		if (index == 0) {
			return ;
		}

		var count = GameData.Shared.PlayerCount.Value;
		var left = anchorPositions.Skip(count - index).Take(index);
		var right = anchorPositions.Take(count - index);
		var newVectors = left.Concat(right).ToList(); 

		for (var i = 0; i < Seats.Count; i++) {
			Seats[i].GetComponent<RectTransform>().DOAnchorPos(newVectors[i], 0.15f);
		}
	}

	// 逆时针生成位置信息
	List<Vector2> getVectors(int total) {
		float width = canvas.GetComponent<RectTransform>().rect.width;
		float height = canvas.GetComponent<RectTransform>().rect.height;

		float top = height - 200;
		float bottom = 200; 
		float right = width / 2 - 100;
		float left = -width / 2 + 100; 

		float hh = top - bottom;
		float ww = width - 200;

		float h3 = hh / 3;
		float h4 = hh / 4;
		float w3 = ww / 2 - ww / 3;

		Vector2 vector = new Vector2 (0, bottom);

		if (total == 2) {
			return new List<Vector2>{
				vector,
				new Vector2(0, top)
			};
		}

		if (total == 6) {
			return new List<Vector2> {
				vector, 
				new Vector2(right, bottom + h3),
				new Vector2(right, bottom + 2 * h3),
				new Vector2(0, top),
				new Vector2(left, bottom + 2 * h3),
				new Vector2(left, bottom + h3)
			};
		}

		if (total == 7) {
			return new List<Vector2> {
				vector, 
				new Vector2(right, bottom + h3),
				new Vector2(right, bottom + 2 * h3),
				new Vector2(w3, top),
				new Vector2(-w3, top),
				new Vector2(left, bottom + 2 * h3),
				new Vector2(left, bottom + h3)
			};
		}

		if (total == 8) {
			return new List<Vector2> {
				vector, 
				new Vector2(right, bottom + h4),
				new Vector2(right, bottom + 2 * h4),
				new Vector2(right, bottom + 3 * h4),
				new Vector2(0, top),
				new Vector2(left, bottom + 3 * h4),
				new Vector2(left, bottom + 2 * h4),
				new Vector2(left, bottom + h4)
			};
		}

		if (total == 9) {
			return new List<Vector2> {
				vector, 
				new Vector2(right, bottom + h4),
				new Vector2(right, bottom + 2 * h4),
				new Vector2(right, bottom + 3 * h4),
				new Vector2(w3, top),
				new Vector2(-w3, top),
				new Vector2(left, bottom + 3 * h4),
				new Vector2(left, bottom + 2 * h4),
				new Vector2(left, bottom + h4)
			};
		}

		throw new Exception("不支持游戏人数");
	}

	void showGameInfo() {
		if (GameData.Shared.Owner && !GameData.Shared.GameStarted) {
			startButton.SetActive(true);
		}

		var roomName = GameData.Shared.RoomName;
		if (String.IsNullOrEmpty(roomName)) {
			roomName = "佚名";
		}

		addGameInfo(string.Format("{0}", roomName));

		var sb = GameData.Shared.SB;
		var bb = GameData.Shared.BB; 

		if (GameData.Shared.Straddle) {
			addGameInfo(string.Format("盲注:{0}/{1}/{2}", sb, bb, bb * 2));			
 		} else {
			addGameInfo(string.Format("盲注:{0}/{1}", sb, bb));
		}

		if (!string.IsNullOrEmpty(GameData.Shared.GameCode)) {
			addGameInfo(String.Format("邀请码:{0}", GameData.Shared.GameCode));
		}

		var ipLimit = GameData.Shared.IPLimit;
		var gpsLimit = GameData.Shared.GPSLimit;

		if (ipLimit && gpsLimit) {
			addGameInfo("IP、GPS限制");
		} else if (gpsLimit) {
			addGameInfo("GPS限制");
		} else if (ipLimit) {
			addGameInfo("IP限制");
		}
	}

	private void addGameInfo(string text) {
		GameObject label = Instantiate(gameInfo);
		label.GetComponent<Text>().text = text;
		label.transform.SetParent(gameInfoWrapper.transform, false);
	}

	private Dictionary<int, PlayerObject> players = new Dictionary<int, PlayerObject>();

	private void destroyPlayer(int index) {
		if (!players.ContainsKey(index)) {
			return ;
		}

		var item = players[index];
		players.Remove(index);
		Destroy(item);
	}

	void registerRxEvents() {
		Action<Player> showPlayer = (obj) => {
			var parent = Seats[obj.Index].transform;
			var go = (GameObject)GameObject.Instantiate(Resources.Load("Prefab/Player"));
			var po = go.GetComponent<PlayerObject>();
			
			players[obj.Index] = po;
			po.ShowPlayer(obj, parent);
		};

		Action<int> enableSeat = (index) => {
			Seats[index].GetComponent<Image>().enabled = true;
		};

		var shouldSub = true;
		GameData.Shared.PlayerCount.AsObservable().TakeWhile((_) => shouldSub).Subscribe((numberOfPlayers) => {
			anchorPositions = getVectors (numberOfPlayers);

			for (int i = 0; i < numberOfPlayers; i++) {
				GameObject cpseat = Instantiate (seat);
				
				var st = cpseat.GetComponent<Seat>();
				st.Index = i;
				cpseat.transform.SetParent (canvas.transform, false);
				cpseat.GetComponent<RectTransform>().anchoredPosition = anchorPositions[i];
				Seats.Add (cpseat);
			}

			shouldSub = false;
		}).AddTo(this);

		RxSubjects.ChangeVectorsByIndex.AsObservable().DistinctUntilChanged().Subscribe((index) => {
			changePositions(index);
		}).AddTo(this);

		GameData.Shared.Players.ObserveReplace().Subscribe((data) => {
			destroyPlayer(data.OldValue.Index);
			enableSeat(data.OldValue.Index);
			showPlayer(data.NewValue);	
		}).AddTo(this);

		GameData.Shared.Players.ObserveAdd().Subscribe((data) => {
			showPlayer(data.Value);
		}).AddTo(this);

		GameData.Shared.Players.ObserveRemove().Subscribe((data) => {
			enableSeat(data.Value.Index);
			destroyPlayer(data.Value.Index);
		}).AddTo(this);

		GameData.Shared.Players.ObserveReset().Subscribe((data) => {
            // Skip
		}).AddTo(this);

		GameData.Shared.PublicCards.ObserveAdd().Subscribe((e) => {
			var index = Card.CardIndex(e.Value);	
			PublicCards[e.Index].GetComponent<Card>().Show(index);
		}).AddTo(this);

		GameData.Shared.PublicCards.ObserveReset().Subscribe((_) => {
			resetAllCards();
		}).AddTo(this);

		GameData.Shared.DealerSeat.AsObservable().Where((value) => value > 0).Subscribe((value) => {
			if (dealer == null) {
				dealer = (GameObject)Instantiate(Resources.Load("Prefab/Dealer"));
				dealer.transform.SetParent(canvas.transform, false);
			}

			Seats[value].GetComponent<Seat>().SetDealer(dealer);
		}).AddTo(this);

		RxSubjects.SeeCard.Subscribe((e) => {
			var cards = e.Data.IL("cards");
			var index = e.Data.Int("seat");
			players[index].SeeCard(cards);	
		}).AddTo(this);

		RxSubjects.Fold.Subscribe((e) => {
			var index = e.Data.Int("seat");
			players[index].Fold();
		}).AddTo(this);

		Action<RxData> act = (e) => {
			var mop = e.Data.ToObject<Mop>();
			players[mop.seat].Act(mop);
		};

		RxSubjects.Call.Subscribe(act).AddTo(this);
		RxSubjects.AllIn.Subscribe(act).AddTo(this);
		RxSubjects.Check.Subscribe(act).AddTo(this);
		RxSubjects.Raise.Subscribe(act).AddTo(this);
	}

	void resetAllCards() {
		foreach(GameObject obj in PublicCards) {
			var card = obj.GetComponent<Card>();
			card.Turnback();
			card.Hide();
		}		
	}
}
