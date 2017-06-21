using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using System.Collections;
using UniRx;

public class RecallPage : MonoBehaviour {
    public Text SBBB;
	public GameObject Rect;
	public GameObject LeftIndicator;
	public GameObject RightIndicator;
	public Text Current;
	public Text Total;
	public GameObject UserGo;
    public Toggle Collect;
    public RecallUser[] Users;

    public RectTransform PlayerList;
    public GameObject InsuranceGo;
	public Text InsuranceText;

	private int totalNumber;
	private int currentNumber;
    private string favhand_id;
    private bool isCollected;

	void Awake()
	{
        SBBB.text = GameData.Shared.SB + "/" + GameData.Shared.BB;
        Collect.onValueChanged.AddListener(delegate(bool isOn) { 
            CollectOrCancel(); 
        });
	}

    private bool requesting = false;

    public void Show() {
        gameObject.SetActive(true);
        GetComponent<DOPopup>().Show(destroyOnClose: false);
        request();
    }

	public void request(int num = 0) {
        if (requesting) {
            return ;
        } 

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

				MainThreadDispatcher.StartUpdateMicroCoroutine(reload(json));
                requesting = false;
			},
            () => {
                requesting = false;
            }
		);

        requesting = true;
	}

	private IEnumerator reload(Dictionary<string, object> data) {
        foreach(var user in Users) {
            user.gameObject.SetActive(false);
        }

		var ret = data.Dict("ret");

		totalNumber = ret.Int("total_hand");
		currentNumber = ret.Int("cur_hand");

		Current.text = currentNumber.ToString();
		Total.text =  string.Format("/ {0}", totalNumber);

        var insuValue = ret.Dict("insurance").Int("score");

        if (insuValue != 0)
        {
            InsuranceGo.SetActive(true);
            InsuranceText.text = _.Number2Text(insuValue);
            InsuranceText.color = _.GetTextColor(insuValue);
            InsuranceGo.SetActive(false);
        } else {
            InsuranceGo.SetActive(false);
        }

        yield return null;

		var comCards = ret.Dict("community").IL("cards");

		var list = ret.List("list");
        for (int num = 0; num < list.Count; num++)
        {
            var dict = list[num] as Dictionary<string, object>;
            if (dict == null)
            {
                continue;
            }

            var user = Users[num];
            user.gameObject.SetActive(true);
            user.Show(dict);

            user.SetComCard(comCards);

            user.transform.SetParent(Rect.transform, false);

            if (num == 0)
            {
                user.SetTag(RecallUser.UserTag.SmallBlind);
            }
            else if (num == 1)
            {
                user.SetTag(RecallUser.UserTag.BigBlind);
            } else if (num == list.Count - 1)
            {
                user.SetTag(RecallUser.UserTag.Dealer); 
            }

            yield return null;
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
	}

	public void Up() {
		if (currentNumber >= totalNumber) {
			return ;
		}

		request(currentNumber + 1);
	}

	public void Down() {
		if (currentNumber <= 1) {
			return ;
		}

		request(currentNumber - 1);
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
                if (json.Int("err") == 0 && f == "fav") {
                    favhand_id = json.Dict("ret").String("favhand_id");
                } 
            }
        );
    }

    public void ShareRecord() {
        Commander.Shared.ShareRecord(currentNumber);
    }
}

