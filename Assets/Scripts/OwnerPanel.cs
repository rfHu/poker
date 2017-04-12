using Extensions;
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

    public Button SaveButton;
	private string pauseStr = "暂停牌局";
	private string continueStr = "继续牌局";

    
    private List<int> AnteSuperScriptNums;
    private List<int> bankroll_multiple;

    private Dictionary<string, object> dict = new Dictionary<string, object>();
    private bool[] isChanged = new bool[5]{ false, false, false, false, false };

	void Awake()
	{
		GameData.Shared.Paused.Subscribe((pause) => {
			if (pause && GameData.Shared.GameStarted) {
				PauseText.text = continueStr;
			} else {
				PauseText.text = pauseStr;
			}
		}).AddTo(this);

        SmallMultipeSlider.value = GameData.Shared.BankrollMul[0];
        LargeMultipeSlider.value = GameData.Shared.BankrollMul[1];

        ExtendTimeSlider.value = 0;

        AnteSliderInit();

        ETSliderNum.text = "0h";

        Need_auditToggle.isOn = GameData.Shared.NeedAudit;
        StraddleToggle.isOn = GameData.Shared.Straddle;      
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

        dict.Remove("bankroll_multiple");

        LMSliderNum.text = LargeMultipeSlider.value.ToString();
        SMSliderNum.text = SmallMultipeSlider.value.ToString();

        if (SmallMultipeSlider.value != GameData.Shared.BankrollMul[0] || LargeMultipeSlider.value != GameData.Shared.BankrollMul[1])
        {
            List<int> bankroll_multiple = new List<int>();
            bankroll_multiple.Add((int)SmallMultipeSlider.value);
            bankroll_multiple.Add((int)LargeMultipeSlider.value);
            dict.Add("bankroll_multiple", bankroll_multiple);
            isChanged[0] = true;
        }
        else 
        {
            isChanged[0] = false;
        }
        SaveButtonInteractable();
    }

    public void ETSliderChanged() 
    {
        if (ExtendTimeSlider.value == 0)
            ETSliderNum.text = "0h";
        else if (ExtendTimeSlider.value == 1)
            ETSliderNum.text = "0.5h";
        else
            ETSliderNum.text = (ExtendTimeSlider.value - 1).ToString() + "h";

        dict.Remove("time_limit");

        if (ExtendTimeSlider.value != 0)
        {
            int time_limit;
            if (ExtendTimeSlider.value == 1)
                time_limit = 1800;
            else
                time_limit = (int)(ExtendTimeSlider.value - 1) * 3600;
            dict.Add("time_limit", time_limit);
            isChanged[1] = true;
        }
        else 
        {
            isChanged[1] = false;
        }
        SaveButtonInteractable();
    }

    public void AnteSliderChanged() 
    {
        ASliderNum.text = "" + AnteSuperScriptNums[(int)AnteSlider.value];

        dict.Remove("ante");

        if (AnteSuperScriptNums[(int)AnteSlider.value] != GameData.Shared.Ante)
        {
            int ante = AnteSuperScriptNums[(int)AnteSlider.value];
            dict.Add("ante", ante);
            isChanged[2] = true;
        }
        else 
        {
            isChanged[2] = false;
        }
        SaveButtonInteractable();
    }

    public void Need_auditToggleChanged()
    {
        dict.Remove("need_audit");

        if (Need_auditToggle.isOn != GameData.Shared.NeedAudit)
        {
            dict.Add("need_audit", Need_auditToggle.isOn ? 1 : 0);
            isChanged[3] = true;
        }
        else 
        {
            isChanged[3] = false;
        }
        SaveButtonInteractable();
    }

    public void StraddleToggleChanged()
    {
        dict.Remove("straddle");

        if (StraddleToggle.isOn != GameData.Shared.Straddle)
        {
            dict.Add("straddle", StraddleToggle.isOn ? 1 : 0);
            isChanged[4] = true;
        }
        else 
        {
            isChanged[4] = false;
        }
        SaveButtonInteractable();
    }

    // void OnDestroy() 
    // {
    //     if (isChanged)
    //     {
    //         SendRequest();
    //     }
    // }

    void SaveButtonInteractable() 
    {
        foreach (var item in isChanged)
        {
            if (item)
            {
                SaveButton.interactable = true;
                return;
            }
        }

        SaveButton.interactable = false;
    }

    public void SendRequest()
    {
        Connect.Shared.Emit(new Dictionary<string, object>() {
				{"f", "modify"},
				{"args", dict}
			});
    }
}
