using Extensions;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using System.Linq;

[RequireComponent(typeof(DOPopup))]
public class OwnerPanel : MonoBehaviour {
	public Text PauseText;
    public Button CloseButton;
    public Slider MultipleSlider1;
    public Slider MultipleSlider2;
    public Slider ExtendTimeSlider;
    public Slider AnteSlider;
    public Toggle Need_auditToggle;
    public Toggle StraddleToggle;

    public GameObject AnteScriptNum;
    public Transform AnteSuperscript;
    public Text MSliderNum1;
    public Text MSliderNum2;
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

        MultipleSlider2.value = GameData.Shared.BankrollMul[0];
        MultipleSlider1.value = GameData.Shared.BankrollMul[1];

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
                    AnteSuperscript.GetComponent<RectTransform>().sizeDelta = new Vector2(993, 42);
                }
                else if (BBsub == 1)
                {
                    AnteSuperscript.GetComponent<RectTransform>().sizeDelta = new Vector2(882, 42);
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

    private List<int> getMulSortList() {
        var value1 = (int)MultipleSlider1.value;
        var value2 = (int)MultipleSlider2.value;

        var list = new List<int>{
            value1,
            value2
        };

        list.Sort();

        return list;
    }

    private void checkChangeSlider() {
        var index1 = MultipleSlider1.transform.GetSiblingIndex();
        var index2 = MultipleSlider1.transform.GetSiblingIndex();

        var value1 = MultipleSlider1.value;
        var value2 = MultipleSlider2.value;

        // 谁数值小，谁在后面
        if (index1 > index2 && value1 <= value2) {
            return ;
        }

        if (value1 > value2) {
            setMulDown(MultipleSlider2);
            setMulUp(MultipleSlider1);
        } else {
            setMulDown(MultipleSlider1);
            setMulUp(MultipleSlider2);
        }
    }

    private void setMulUp(Slider slider) {
        slider.transform.Find("Background").gameObject.SetActive(true);
        setColor(slider, MaterialUI.MaterialColor.cyanA200); // 青色
    }

    private void setMulDown(Slider slider) {
        slider.transform.SetAsLastSibling();
        slider.transform.Find("Background").gameObject.SetActive(false);
        setColor(slider, _.HexColor("#A7A9AE")); // 灰色
    }

    private void setColor(Slider slider, Color color) {
        slider.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = color;
    }

    public void MultipeSliderChanged() 
    {
        dict.Remove("bankroll_multiple");
        var bankroll_multiple = getMulSortList();

        MSliderNum1.text = MultipleSlider1.value.ToString();
        MSliderNum2.text = MultipleSlider2.value.ToString();

        checkChangeSlider();

        if (bankroll_multiple.SequenceEqual(GameData.Shared.BankrollMul))
        {
            isChanged[0] = false;            
        }
        else 
        {
            dict.Add("bankroll_multiple", bankroll_multiple);
            isChanged[0] = true;
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
    
    void SaveButtonInteractable() 
    {
        foreach (var item in isChanged)
        {
            if (item)
            {
                setSaveButton(true);
                return;
            }
        }

        setSaveButton(false);
    }

    private void setSaveButton(bool interactable) {
        SaveButton.interactable = interactable;

        var image = SaveButton.GetComponent<ProceduralImage>();
        var text = SaveButton.transform.Find("Text").GetComponent<Text>();
        Color color;

        if (interactable) {
            color = MaterialUI.MaterialColor.cyanA200;
        } else {
            color = MaterialUI.MaterialColor.grey400;
        }

         image.color = color;
         text.color = color;
    }

    public void SendRequest()
    {
        Connect.Shared.Emit(new Dictionary<string, object>() {
				{"f", "modify"},
				{"args", dict}
			});
    }
}
