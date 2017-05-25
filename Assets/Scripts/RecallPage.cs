using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Extensions;

public class RecallPage : MonoBehaviour {
	public Text Owner;
	public GameObject Cards;
	public GameObject Rect;
	public GameObject LeftIndicator;
	public GameObject RightIndicator;
	public Text Current;
	public Text Total;
	public GameObject UserGo;
    public Toggle Collect;

	public Text InsuranceText;

	private int totalNumber;
	private int currentNumber;
    private string favhand_id;
    private bool isCollected;

	void Awake()
	{
		request();

        Collect.onValueChanged.AddListener(delegate(bool isOn) { 
            CollectOrCancel(); 
        });
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
				if (this == null) {
					return ;
				}

				reload(json);
			}
		);
	}

	void reload(Dictionary<string, object> data) {
		var insuranceGo = InsuranceText.transform.parent.gameObject;
		insuranceGo.SetActive(false);
		
		var ret = data.Dict("ret");

		totalNumber = ret.Int("total_hand");
		currentNumber = ret.Int("cur_hand");

		Owner.text = "牌局回顾";
		// Owner.text = string.Format("{0}（{1}）", GameData.Shared.RoomName, GameData.Shared.StartTime.ToString("yyyy-MM-dd")); 
		Current.text = currentNumber.ToString();
		Total.text =  string.Format("/ {0}", totalNumber);

		Cards.transform.Clear();
		Rect.transform.Clear();

		var comCards = ret.Dict("community").IL("cards");
		
		// 公共牌
		for (int i = 0; i < 5; i++) {
			var go = (GameObject)Instantiate(Resources.Load("Prefab/Card"));
			var card = go.GetComponent<Card>();
			card.SetSize(new Vector2(87, 125));

			if (i < comCards.Count) {
				card.Show(comCards[i]);
			}
 			
			go.transform.SetParent(Cards.transform, false);

		}
		
		var list = ret.List("list");
		foreach(object entry in list) {
			var dict = entry as Dictionary<string, object>;
			if (dict == null) {
				continue ;
			}

			var user = Instantiate(UserGo).GetComponent<RecallUser>();
			user.gameObject.SetActive(true);
			user.Show(dict);
			user.transform.SetParent(Rect.transform, false);
		}

        if (!string.IsNullOrEmpty(ret.String("favhand_id")))
        {
            isCollected = true;
            Collect.isOn = true;
            this.favhand_id = ret.String("favhand_id");
        }
        else 
        {
            isCollected = false;
            Collect.isOn = false;
        }

		var insuValue = ret.Dict("insurance").Int("score");

		if (insuValue == 0) {
			return ;
		}
		
		insuranceGo.SetActive(true);
		InsuranceText.text = _.Number2Text(insuValue);	
		InsuranceText.color = _.GetTextColor(insuValue);
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

    public void CollectOrCancel() 
    {
        if (isCollected == Collect.isOn)
            return;
        else
            isCollected = Collect.isOn;
       

        var dict = new Dictionary<string, object>() { };
        string f = "";

        if (Collect.isOn)
        {
            dict = new Dictionary<string, object>() {
                {"roomid", GameData.Shared.Room},
			    {"handid", currentNumber},
		    };

            f = "fav";
        }
        else {
            dict = new Dictionary<string, object>() {
                {"favhand_id", favhand_id},
		    };

            f = "notfav";
        }

        Connect.Shared.Emit(new Dictionary<string, object>() {
				{"f", f},
				{"args", dict}
			},
            (json) =>
            {
                if (json.Int("err") == 0) {
                    if (f == "fav")
                    {
                        var ret = json.Dict("ret");
                        favhand_id = ret.String("favhand_id");
                    }
                } 
            }
        );
    }

    public void ShareRecord() {
        Commander.Shared.ShareRecord(currentNumber);
    }
}

