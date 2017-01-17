using System.Collections;
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

	void Awake()
	{
		request();	
	}

	void request(int num = 0) {
		var dict = new Dictionary<string, object>() {
			{"handid", num}
		};

		Connect.shared.Emit(
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

		Owner.text = string.Format("{0}（{1}）", GConf.roomName, GConf.StartTime.ToString("yyyy-MM-dd")); 
		Current.text = ret.Int("cur_hand").ToString();
		Total.text =  string.Format("/ {0}", ret.Int("total_hand").ToString());

		Cards.transform.Clear();
		Rect.transform.Clear();

		var comCards = ret.Dict("community").IL("cards");

		// 公共牌
		foreach(int num in comCards) {
			var index = Controller.CardIndex(num);
			var go = (GameObject)Instantiate(Resources.Load("Prefab/Card"));
			go.GetComponent<Card>().Show(index);
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
}
