using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Extensions;
using UnityEngine.UI.ProceduralImage;

public class RecallPage : MonoBehaviour {
    public Text SBBB;
	public List<Card> Cards;
    public GameObject playerTag;
	public GameObject Rect;
	public GameObject LeftIndicator;
	public GameObject RightIndicator;
	public Text Current;
	public Text Total;
	public GameObject UserGo;
    public Toggle Collect;

	public Text InsuranceText;
    public GameObject PlayerList;

	private int totalNumber;
	private int currentNumber;
    private string favhand_id;
    private bool isCollected;
    private Color bClolor = new Color(5 / 255f, 150 / 255f, 213 / 255f);

	void Awake()
	{
        SBBB.text = GameData.Shared.SB + "/" + GameData.Shared.BB;

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
				{"f", "playback"},
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

		Current.text = currentNumber.ToString();
		Total.text =  string.Format("/ {0}", totalNumber);

		Rect.transform.Clear();

		var comCards = ret.Dict("community").IL("cards");
		
		var list = ret.List("list");
        for (int num = 0; num < list.Count; num++)
        {
            var dict = list[num] as Dictionary<string, object>;
            if (dict == null)
            {
                continue;
            }

            var user = Instantiate(UserGo).GetComponent<RecallUser>();
            user.gameObject.SetActive(true);
            user.Show(dict);

            if (user.PublicCardNum > 1)
            {
                for (int i = 0; i < user.PublicCardNum + 1 && i < comCards.Count; i++)
                {
                    var card = Instantiate(Cards[i].gameObject);
                    card.GetComponent<Card>().Show(comCards[i]);
                    card.transform.SetParent(user.transform, false);
                }
            }
            user.transform.SetParent(Rect.transform, false);

            if (num == list.Count - 1)
	        {
                 Instantiate(playerTag, user.transform, false);
            }
            else if (num == 0)
            {
                SetBTag(user, "小盲");
            }
            else if (num == 1)
            {
                SetBTag(user, "大盲");
            }
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
        PlayerList.GetComponent<RectTransform>().sizeDelta -= new Vector2(0, 64);
	}

    private void SetBTag(RecallUser user, string str)
    {

        GameObject pTag = Instantiate(playerTag, user.transform, false);
        pTag.GetComponent<ProceduralImage>().color = bClolor;
        var text = pTag.transform.FindChild("Text").GetComponent<Text>();
        text.text = str;
        text.color = Color.white;
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

