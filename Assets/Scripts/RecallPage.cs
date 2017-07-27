using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using System.Collections;
using UniRx;

[RequireComponent(typeof(DOPopup))]
public class RecallPage : MonoBehaviour {
    public Text SBBB;
	public GameObject Rect;
	public GameObject LeftIndicator;
	public GameObject RightIndicator;
	public Text Current;
	public Text Total;
    public Toggle Collect;

    public RectTransform PlayerList;
    public GameObject InsuranceGo;
    public GameObject Win27Go;

	private int totalNumber;
	private int currentNumber;
    private string favhand_id;

    private List<RecallUser> Users = new List<RecallUser>();

    private bool requesting = false;

    void OnSpawned() {
        SBBB.text = GameData.Shared.SB + "/" + GameData.Shared.BB;
        GetComponent<DOPopup>().Show();
        request();
    }

    void OnDespawned() {
        requesting = false;
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
				reload(json);
                requesting = false;
			},
            () => {
                requesting = false;
            }
		);

        requesting = true;
	}

	private void reload(Dictionary<string, object> ret) {
		totalNumber = ret.Int("total_hand");
		currentNumber = ret.Int("cur_hand");

		Current.text = currentNumber.ToString();
		Total.text =  string.Format("/ {0}", totalNumber);

        var insuValue = ret.Dict("insurance").Int("score");
        var win27Value = ret.Int("award_27");

        SetPublicValue(insuValue, InsuranceGo);
        SetPublicValue(win27Value, Win27Go);

		var comCards = ret.Dict("community").IL("cards");

		var list = ret.List("list");
        for (int num = 0; num < 9; num++)
        {   
            if (list.Count > num) {
                var dt = list[num] as Dictionary<string, object>;
                var tag = findTag(list, num);

                RecallUser user;

                if (num < Users.Count) {
                    user = Users[num];
                    user.transform.SetParent(Rect.transform, false);
                    user.gameObject.SetActive(true);
                } else {
                    user = PoolMan.Spawn("RecallUser", Rect.transform).GetComponent<RecallUser>();
                    user.transform.SetParent(Rect.transform, false);
                    Users.Add(user);
                }

                user.Show(dt);
                user.SetComCard(comCards);
                user.SetTag(tag);
            } else if (Users.Count > num) {
                Users[num].gameObject.SetActive(false);
            }
        }

        if (!string.IsNullOrEmpty(ret.String("favhand_id")))
        {
            Collect.isOn = true;
            this.favhand_id = ret.String("favhand_id");
        }
        else 
        {
            Collect.isOn = false;
        }		
	}

    private void SetPublicValue(int value, GameObject go)
    {
        if (value != 0)
        {
            go.SetActive(true);
            var valueText = go.transform.Find("Text").GetComponent<Text>();
            valueText.text = _.Number2Text(value);
            valueText.color = _.GetTextColor(value);
        }
        else
        {
            go.SetActive(false);
        }
    }

    private RecallUser.UserTag findTag(List<object> list, int index) {
        if (index == 0)
        {
            return RecallUser.UserTag.SmallBlind;
        }
        else if (index == 1)
        {
            return RecallUser.UserTag.BigBlind;
        } else if (index == list.Count - 1)
        {
            return RecallUser.UserTag.Dealer; 
        }

        return RecallUser.UserTag.None;
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
       Collect.isOn = !Collect.isOn;

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
            (json, err) =>
            {
                if (err == 0 && f == "fav") {
                    favhand_id = json.String("favhand_id");
                } 
            }
        );
    }

    public void ShareRecord() {
        Commander.Shared.ShareRecord(currentNumber);
    }
}

