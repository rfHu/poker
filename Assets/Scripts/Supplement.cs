using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UniRx;
using System;
using MaterialUI;

[RequireComponent(typeof(DOPopup))]
public class Supplement : MonoBehaviour {

	public Text Blind;
	public Text Score;
	public Text Coins;
	public Text Pay;
	public Slider slider;

    public GameObject ClubTitle;
    public Transform ClubGoList;
    public GameObject ClubToggle;

    private string aimClubID;

	void Awake() {
		RxSubjects.UnSeat.Where((e) => {
			var uid = e.Data.String("uid");
			return GameData.Shared.Uid == uid;
		}).Subscribe((e) => {
			GetComponent<DOPopup>().Close();
		}).AddTo(this);

		slider.onValueChanged.AddListener(OnChange);
	}

    void OnSpawned()
    {
        aimClubID = "";
        RequestAllowClub();
        
        Coins.text = _.Num2CnDigit(GameData.Shared.Coins);
        Blind.text = string.Format("{0}/{1}", GameData.Shared.BB / 2, GameData.Shared.BB);

        var mul = GameData.Shared.BankrollMul;
        var bb100 = 100 * GameData.Shared.BB;
        var min = mul[0] * bb100;
        var max = mul[1] * bb100;
        var bankroll = GameData.Shared.Bankroll.Value;

        if (bankroll >= max)
        {
            return;
        }

        int smin;
        int smax;

        if (bankroll < min)
        {
            smin = Mathf.CeilToInt((min - bankroll) / (float)bb100) * bb100;
        }
        else
        {
            smin = bb100;
        }

        smax = Mathf.CeilToInt((max - bankroll) / (float)bb100) * bb100;

        slider.maxValue = slider.minValue = slider.value = 0;
        slider.maxValue = smax;
        slider.minValue = smin;

        OnChange(smin);
    }

    private void RequestAllowClub()
    {
        ClubTitle.SetActive(false);
        ClubGoList.gameObject.SetActive(false);
        if (GameData.Shared.ClubID == "")
            return;

        Connect.Shared.Emit(new Dictionary<string, object>(){
            {"f", "allowclubs"},
            }, (e) => {
                var clubs = e.List("clubs");
                ClubTitle.SetActive(clubs.Count != 0);
                ClubGoList.gameObject.SetActive(clubs.Count != 0);
                if (clubs.Count == 0)
                    return;

            int maxNum = Mathf.Max(clubs.Count, ClubGoList.childCount);

                for (int i = 0; i < maxNum ; i++)
                {
                    if (i < clubs.Count)
                    {
                        var club = clubs[i] as Dictionary<string, object>;
                        var clubID = club.String("_id");
                        var clubName = club.String("name");
                        Transform clubToggle;


                        if (i < ClubGoList.childCount)
                        {
                            clubToggle = ClubGoList.GetChild(i);
                            clubToggle.gameObject.SetActive(true);
                            clubToggle.GetComponent<Toggle>().isOn = false;
                            clubToggle.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                        }
                        else
                        {
                            clubToggle = Instantiate(ClubToggle, ClubGoList.transform).transform;
                        }

                        clubToggle.GetComponentInChildren<Text>().text = clubName;
                        clubToggle.GetComponent<Toggle>().onValueChanged.AddListener((isOn) =>
                        {
                            if (!isOn)
                                return;
                            aimClubID = clubID;
                        });


                        if (clubs.Count == 1 || GameData.Shared.ClubID == clubID)
                        {
                            clubToggle.GetComponent<Toggle>().isOn = true;
                        }
                    }
                    else
                    {
                        if (ClubGoList.childCount > i)
                        {
                            ClubGoList.GetChild(i).gameObject.SetActive(false);
                        }
                    }
                }
            });
    }
    

    public void OnChange(float value) {
		int step = GameData.Shared.BB * 100; 
		int newValue = value.StepValue(step);

		// 解决赋值循环导致崩溃		
		if (newValue > slider.maxValue) {
			return ;
		}

		slider.value = newValue;
		Score.text = newValue.ToString();
		Pay.text = (newValue * GameData.Shared.Rake).ToString();
	}

	public void TakeCoin() {
		float value = slider.value;

        Dictionary<string, object> args = new Dictionary<string, object>();
        args.Add("multiple", value / (100 * GameData.Shared.BB));
        args.Add("ver", Application.version);
        if (aimClubID != "")
        {
            args.Add("club_id", aimClubID);
        }

        Connect.Shared.Emit(new Dictionary<string, object>(){
			{"f", "takecoin"},
			{"args", args}
		}, (json, err) => {
			if (err == 1201) {
				_.PayFor(() => {
					RxSubjects.TakeCoin.OnNext(new RxData());
				});	
			}
            else if (err == 1203)
            {
                PokerUI.Toast("联盟额度不足");
            }

            GetComponent<DOPopup>().Close();
		});
	}
}
