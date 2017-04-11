﻿using Extensions;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(DOPopup))]
public class OwnerPanel : MonoBehaviour {
	public Text PauseText;
    public Button CloseButton;
    public Slider LargeMultipeSlider;
    public Slider SmallMultipeSlider;
    public Slider ExtendTimeSlider;
    public Slider AnteSlider;
    public Toggle Need_auditToggle;
    public Toggle StraddleToggle;

    public GameObject AnteScriptNum;
    public Transform AnteSuperscript;
    public Text LMSliderNum;
    public Text SMSliderNum;
    public Text ETSliderNum;
    public Text ASliderNum;

	private string pauseStr = "暂停牌局";
	private string continueStr = "继续牌局";

    
    private List<int> AnteSuperScriptNums;
    private List<int> bankroll_multiple;
    private int time_limit;
    private int ante;

    private bool isChanged;

    public void SetIsChanged()
    {
        isChanged = true;
    }

	void Awake()
	{
		GameData.Shared.Paused.Subscribe((pause) => {
			if (pause && GameData.Shared.GameStarted) {
				PauseText.text = continueStr;
			} else {
				PauseText.text = pauseStr;
			}
		}).AddTo(this);


        bankroll_multiple = GameData.Shared.BankrollMul;
        SmallMultipeSlider.value = bankroll_multiple[0];
        LargeMultipeSlider.value = bankroll_multiple[1];

        ExtendTimeSlider.value = 0;
        time_limit = 0;

        AnteSliderInit();

        ETSliderNum.text = "0h";

        Need_auditToggle.isOn = GameData.Shared.NeedAudit;
        StraddleToggle.isOn = GameData.Shared.Straddle;

        isChanged = false;
	}

    private void AnteSliderInit()
    {
        
        int[] anteNums = {1,2,5,10,25,50,100};
        int[] BBNums = { 2, 4, 10, 20, 50, 100, 200 };

        int BBsub = 0;
        for (; BBsub < BBNums.Length; BBsub++)
        {
            if (BBNums[BBsub] == GameData.Shared.BB) 
            {
                if (BBsub == 0)
                {
                    AnteSlider.maxValue = 2;
                    AnteSuperscript.GetComponent<RectTransform>().sizeDelta = new Vector2(653, 28);
                }
                else if (BBsub == 1)
                {
                    AnteSuperscript.GetComponent<RectTransform>().sizeDelta = new Vector2(588, 28);
                    AnteSlider.maxValue = 3;
                }
                else
                    AnteSlider.maxValue = 4;
                break;
            }
        }
        AnteSuperScriptNums = new List<int>();
        AnteSuperScriptNums.Add(0);
        int i = BBsub - 3;
        if (i < 0)
            i = 0;

        for (; i <= BBsub; i++)
            AnteSuperScriptNums.Add(anteNums[i]);

        AnteSuperScriptNums.Add(GameData.Shared.BB);

        for (int j = 0; j < AnteSuperScriptNums.Count; j++)
        {
            GameObject text = Instantiate(AnteScriptNum);
            text.GetComponent<Text>().text = "" + AnteSuperScriptNums[j];
            text.transform.SetParent(AnteSuperscript,false);
            text.SetActive(true);

            if (AnteSuperScriptNums[j] == GameData.Shared.Ante)
            {
                ASliderNum.text = AnteSuperScriptNums[j].ToString();
                AnteSlider.value = j;
            }
        }
    }

	public void Stop() {
		GetComponent<DOPopup>().Close();

		// 二次确定
		PokerUI.Alert("确定提前结束牌局", () => {
			Connect.Shared.Emit(new Dictionary<string, object>() {
				{"f", "pause"},
				{"args", "3"}
			});
		}, null);
	}

	public void Pause() {
		string f;

		if (GameData.Shared.Paused.Value && GameData.Shared.GameStarted) {
			f = "start";
		} else {
			f = "pause";
		}

		Connect.Shared.Emit(new Dictionary<string, object>() {
			{"f", f},
			{"args", "0"}
		}, (data) => {
			var err = data.Int("err");
			
			if (err == 0) {
				if (f == "start") {
					PauseText.text = pauseStr;
				} else {
					PauseText.text = continueStr;
				}
			} else {
				var msg = data.String("ret");
				GetComponent<DOPopup>().Close();
				PokerUI.Alert(msg);	
			}
		});		
	}

    public void MultipeSliderChanged() 
    {
        if ( SmallMultipeSlider.value >= LargeMultipeSlider.value)
        {
            LargeMultipeSlider.value = SmallMultipeSlider.value;
        }

        LMSliderNum.text = LargeMultipeSlider.value.ToString();
        SMSliderNum.text = SmallMultipeSlider.value.ToString();
    }

    public void AnteSliderChanged() 
    {
        ASliderNum.text = "" + AnteSuperScriptNums[(int)AnteSlider.value];
    }

    public void ETSliderChanged() 
    {
        if (ExtendTimeSlider.value == 0)
            ETSliderNum.text = "0h";
        else if (ExtendTimeSlider.value == 1)
            ETSliderNum.text = "0.5h";
        else
            ETSliderNum.text = (ExtendTimeSlider.value - 1).ToString() + "h";
    }

    // void OnDestroy() 
    // {
    //     if (isChanged)
    //     {
    //         SendRequest();
    //     }
    // }

    private void SendRequest()
    {
        var dict = new Dictionary<string, object>();

        bool isChange = false;

        //只添加改变的字段
        if (SmallMultipeSlider.value != bankroll_multiple[0]||LargeMultipeSlider.value != bankroll_multiple[1])
        {
            bankroll_multiple[0] = (int)SmallMultipeSlider.value;
            bankroll_multiple[1] = (int)LargeMultipeSlider.value;
            dict.Add("bankroll_multiple", bankroll_multiple);
            isChange = true;
        }

        if (ExtendTimeSlider.value !=0)
        {
            if (ExtendTimeSlider.value == 1)
                time_limit = 1800;
            else
                time_limit = (int)(ExtendTimeSlider.value - 1) * 3600;
            dict.Add("time_limit", time_limit);
            isChange = true;
        }

        if (AnteSuperScriptNums[(int)AnteSlider.value] != GameData.Shared.Ante)
        {
            ante = AnteSuperScriptNums[(int)AnteSlider.value];
            dict.Add("ante", ante);
            isChange = true;
        }

        if (Need_auditToggle.isOn != GameData.Shared.NeedAudit)
        {
            dict.Add("need_audit", Need_auditToggle.isOn ? 1 : 0);
            isChange = true;
        }

        if (StraddleToggle.isOn != GameData.Shared.Straddle)
        {
            dict.Add("straddle", StraddleToggle.isOn ? 1 : 0);
            isChange = true;
        }

        if (!isChange)
        {
            return;
        }

        Connect.Shared.Emit(new Dictionary<string, object>() {
				{"f", "modify"},
				{"args", dict}
			});
    }
}
