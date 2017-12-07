using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UniRx;
using System;
using MaterialUI;
using System.Linq;

[RequireComponent(typeof(DOPopup))]
public class Supplement : MonoBehaviour {

	public Text Blind;
	public Text Score;
	public Text Coins;
	public Text Pay;
	public Slider slider;

    public GameObject ClubTitle;
    public Transform ClubListGo;
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
        var clubs = GameData.Shared.AllowClubs;
        ClubTitle.SetActive(clubs.Count != 0);
        ClubListGo.gameObject.SetActive(clubs.Count != 0);
        if (clubs.Count == 0)
            return;

        ClubListGo.GetComponent<RectTransform>().sizeDelta = new Vector2(704, (int)Math.Ceiling(clubs.Count / 2.0) * 83);

		ClubListGo.transform.Clear();

        for (int i = 0; i < clubs.Count; i++)
        {

            var club = clubs[i] as Dictionary<string, object>;
            var clubID = club.String("_id");
            var clubName = club.String("name");
            Transform clubToggle;

            clubToggle = Instantiate(ClubToggle, ClubListGo.transform).transform;

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
            if (err == 1201)
            {
                _.PayFor(() =>
                {
                    RxSubjects.TakeCoin.OnNext(new RxData());
                });
            }
            else if (err == 1203)
            {
                PokerUI.Toast("联盟额度不足");
            }
            else if (err == 0 && GameData.Shared.AllowClubs.Count > 1)
            {
				var clubs = GameData.Shared.AllowClubs;

				Func<object, bool> filter = (x) => {
					var d = x as Dictionary<string, object>;
					return d.String("_id") == aimClubID;
				}; 

                GameData.Shared.AllowClubs = clubs.Where((dict) => filter(dict)).ToList();
            }

            GetComponent<DOPopup>().Close();
		});
	}
}
