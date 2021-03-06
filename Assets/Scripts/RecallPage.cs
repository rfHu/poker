﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using System.Collections;
using UniRx;

[RequireComponent(typeof(DOPopup))]
public class RecallPage : MonoBehaviour {
	public GameObject RecallUserPrefab;

	public GameObject Rect;
	public GameObject LeftIndicator;
	public GameObject RightIndicator;
	public Text Current;
	public Text Total;
    public Toggle Collect;

    public RectTransform PlayerList;
    public GameObject InsuranceGo;
    public GameObject Win27Go;

    public Slider HandSlider;
    public Toggle SelfToggle;

	private int totalNumber = 0;
	
	private int currentNumber {
		get {
			return self ? selfCurrent : totalCurrent;
		}

		set {
			if (self) {
				selfCurrent = value;
			} else {
				totalCurrent = value;
			}
		}
	}

	private List<RecallUser> users = new List<RecallUser>();

    private string favhand_id;
	private int handId;

    private bool requesting = false;
    private string roomID;

    private bool self = false;

	private int totalCurrent = 0;
	private int selfCurrent = 0;

    void Awake()
    {
        // 切换房间了，把弹框关闭
        GameData.Shared.Room.Subscribe((rid) => {
            if (!gameObject.activeInHierarchy || string.IsNullOrEmpty(roomID)) {
                return ;
            }

            if (rid != roomID) 
            {
                PoolMan.Despawn(transform);            
            }
        }).AddTo(this);

        HandSlider.onValueChanged.AddListener((value) => {
            Current.text = value.ToString();
        });

        SelfToggle.onValueChanged.AddListener((isOn) => {
            self = isOn;
            request(currentNumber);                
        });
    }

    void OnSpawned() {
        GetComponent<DOPopup>().Show();

        if (GameData.Shared.Room.Value == roomID && currentNumber != totalNumber) {
            request(currentNumber);
        } else {
			totalCurrent = 0;
			selfCurrent = 0;
            request(0);
        }

        roomID = GameData.Shared.Room.Value;
    }

    void OnDespawned() {
        requesting = false;
    }

	public void request(int num = 0) {
        if (requesting) {
            return ;
        }

        var dict = new Dictionary<string, object>() {
			{"handid", num},
            {"self", self? 1 : 0},
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
		HandSlider.maxValue = totalNumber;
        HandSlider.minValue = Mathf.Min(totalNumber, 1);
		HandSlider.value = currentNumber = ret.Int("cur_hand");
		handId = ret.Int("handid");

		Total.text = string.Format("/ {0}", totalNumber);
		Current.text = currentNumber.ToString();

        var insuValue = ret.Dict("insurance").Int("score");
        var win27Value = ret.Int("award_27");
        SetPublicValue(insuValue, InsuranceGo);
        SetPublicValue(win27Value, Win27Go);

		var comCards = ret.Dict("community").IL("cards");
		var list = ret.List("list");

        for (int num = 0; num < 9; num++)
        {   
            if (list.Count > num) { // 复用的部分
                var dt = list[num] as Dictionary<string, object>;
                var tag = findTag(list, num);

                RecallUser user;

                if (num < users.Count) {
                    user = users[num];
                    user.gameObject.SetActive(true);
                } else {
                    var go = GameObject.Instantiate(RecallUserPrefab, Rect.transform);
					user = go.GetComponent<RecallUser>();
					users.Add(user);
                }

                user.Show(dt);
                user.SetComCard(comCards, GameData.Shared.Type.Value);
                user.SetTag(tag);
            } else if (users.Count > num) { // 超出的部分隐藏
                users[num].gameObject.SetActive(false);
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

	public void Right() {
		if (currentNumber >= totalNumber) {
			return ;
		}

		request(currentNumber + 1);
	}

	public void Left() {
		if (currentNumber <= 1) {
			return ;
		}

		request(currentNumber - 1);
	}

	public void Right2End() {
		request(0);
	}

	public void Left2Start() {
		if (totalNumber < 1) {
			return ;
		}
		request(1);
	}

    public void CollectOrCancel() 
    {
       Collect.isOn = !Collect.isOn;

        if (Collect.isOn)
        {
			collect();
        }
        else {
			cancelCollect();	
        }
    }

	private void collect() {
		if (handId <= 0) {
			return ;
		}

		var dict = new Dictionary<string, object>() {
				{"roomid", roomID},
				{"handid", handId},
			};

		Connect.Shared.Emit(new Dictionary<string, object>() {
				{"f", "fav"},
				{"args", dict}
			},
			(json, err) =>
			{
				if (err == 0)
				{
					favhand_id = json.String("favhand_id");
				}
			}
		);	
	}

	private void cancelCollect() {
		if (string.IsNullOrEmpty(favhand_id)) {
			return ;
		}

		var dict = new Dictionary<string, object>() {
				{"favhand_id", favhand_id},
			};

		Connect.Shared.Emit(new Dictionary<string, object>() {
				{"f", "notfav"},
				{"args", dict}
			}
		);
	}

    public void ShareRecord() {
		if (handId <= 0) {
			return ;
		}

        Commander.Shared.ShareRecord(handId);
    }

    public void OnPointUpSlider() 
    {
        request((int)HandSlider.value);
    }
}