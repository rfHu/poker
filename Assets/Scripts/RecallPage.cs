using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Extensions;

public class RecallPage : MonoBehaviour {
	public Text Owner;
	// 红：f50057  绿：00c853
	public GameObject Cards;
	public GameObject Rect;
	public GameObject LeftIndicator;
	public GameObject RightIndicator;
	public Text Current;
	public Text Total;

	private int totalNumber;
	private int currentNumber;

	void Awake()
	{
		request();	
	}

	void request(int num = 0) {
		var dict = new Dictionary<string, object>() {
			{"handid", num}
		};

		Connect.Shared.Emit(
			new Dictionary<string, object>() {
				{"f", "handresult"},
				{"args", dict}
			},
			(json) => {
				reload(json);
			}
		);
	}

	void reload(Dictionary<string, object> data) {
		var ret = data.Dict("ret");

		totalNumber = ret.Int("total_hand");
		currentNumber = ret.Int("cur_hand");

		Owner.text = string.Format("{0}（{1}）", GameData.Shared.RoomName, GameData.Shared.StartTime.ToString("yyyy-MM-dd")); 
		Current.text = currentNumber.ToString();
		Total.text =  string.Format("/ {0}", totalNumber);

		Cards.transform.Clear();
		Rect.transform.Clear();

		var comCards = ret.Dict("community").IL("cards");

		// 公共牌
		foreach(int num in comCards) {
			var index = Card.CardIndex(num);
			var go = (GameObject)Instantiate(Resources.Load("Prefab/Card"));
			var card = go.GetComponent<Card>();
			card.SetSize(new Vector2(68, 98));
			card.Show(index);
			go.transform.SetParent(Cards.transform, false);
		}

		var list = ret.List("list");
		foreach(object entry in list) {
			var dict = entry as Dictionary<string, object>;
			if (dict == null) {
				continue ;
			}

			var userGo = (GameObject)Instantiate(Resources.Load("Prefab/Recall/RecallUser"));
			var user = userGo.GetComponent<RecallUser>();
			user.Show(dict);
			user.transform.SetParent(Rect.transform, false);
		}
	}

	public void Up() {
		if (currentNumber >= totalNumber) {
			return ;
		}

		currentNumber++;
		request(currentNumber);
	}

	public void Down() {
		if (currentNumber <= 0) {
			return ;
		}

		currentNumber--;
		request(currentNumber);
	}
}
