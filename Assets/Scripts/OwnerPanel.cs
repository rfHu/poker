using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using System.Linq;

[RequireComponent(typeof(DOPopup))]
public class OwnerPanel : MonoBehaviour {
    public Button CloseButton;
    public Slider MultipleSlider1;
    public Slider MultipleSlider2;
    public Slider ExtendTimeSlider;
    public Slider AnteSlider;
    public Slider Turn_countdownSlider;
    public Toggle Need_auditToggle;
    public Toggle StraddleToggle;
    public Toggle OffScoreToggle;

    public GameObject AnteScriptNum;
    public Transform AnteSuperscript;
    public Text MSliderNum1;
    public Text MSliderNum2;
    public Text ETSliderNum;
    public Text ASliderNum;
    public Text T_cNum;
    public Text MyCoins;
    public Text CostCoins;

    public CButton SaveButton;
    
    private List<int> AnteSuperScriptNums;
    private List<int> bankroll_multiple;

    private Dictionary<string, object> dict = new Dictionary<string, object>();
    private bool[] isChanged = new bool[7]{ false, false, false, false, false ,false, false};
    private List<int> turn_countdownNum =new List<int>{ 10, 12, 15, 20 };



	public void Init()
	{
        AnteSliderInit();

        MultipleSlider2.value = GameData.Shared.BankrollMul[0];
        MultipleSlider1.value = GameData.Shared.BankrollMul[1];

        Need_auditToggle.isOn = GameData.Shared.NeedAudit;
        StraddleToggle.isOn = GameData.Shared.Straddle.Value;
        OffScoreToggle.isOn = GameData.Shared.OffScore.Value;
        MyCoins.text = _.Num2CnDigit(GameData.Shared.Coins);

        Turn_countdownSlider.value = turn_countdownNum.IndexOf(GameData.Shared.SettingThinkTime);

        ExtendTimeSlider.value = 0;
        ETSliderNum.text = "0h";

        for (int i = 0; i < isChanged.Length; i++)
        {
            isChanged[i] = false;
        }
	}

    void Awake() {
        
    }

    private void AnteSliderInit()
    {
        //清除原上标
        for (int num = 0; num < AnteSuperscript.childCount; num++)
        {
            Destroy(AnteSuperscript.GetChild(num).gameObject);
        }
        
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
        int i = BBsub - 2;
        if (i < 0)
            i = 0;

        for (; i <= BBsub; i++)
            AnteSuperScriptNums.Add(anteNums[i]);

        AnteSuperScriptNums.Add(GameData.Shared.BB);

        for (int j = 0; j < AnteSuperScriptNums.Count; j++)
        {
            GameObject text = Instantiate(AnteScriptNum);
            text.GetComponent<Text>().text = ("" + AnteSuperScriptNums[j]).PadRight(3,' ');
            text.transform.SetParent(AnteSuperscript,false);
            text.SetActive(true);

            if (AnteSuperScriptNums[j] == GameData.Shared.Ante.Value)
            {
                ASliderNum.text = AnteSuperScriptNums[j].ToString();
                AnteSlider.value = j;
            }
        }
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
        var realValue = buyTimeCost;
        ETSliderNum.text = (buyTimeCost / 100).ToString() + "h";
        CostCoins.text = realValue.ToString();

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

    private float buyTimeCost {
        get {
           var value = ExtendTimeSlider.value;
           var cost = 0f;

           if (value <= 1) {
                cost = (value / 2) * 100;
           } else {
               cost = (value - 1) * 100;
           }

           return cost;
        }
    }

    public void AnteSliderChanged() 
    {
        ASliderNum.text = "" + AnteSuperScriptNums[(int)AnteSlider.value];

        dict.Remove("ante");

        if (AnteSuperScriptNums[(int)AnteSlider.value] != GameData.Shared.Ante.Value)
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

        if (StraddleToggle.isOn != GameData.Shared.Straddle.Value)
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

    public void OffScoreToggleChanged() {
         dict.Remove("off_score");

        if (OffScoreToggle.isOn != GameData.Shared.OffScore.Value)
        {
            dict.Add("off_score", OffScoreToggle.isOn ? 1 : 0);
            isChanged[6] = true;
        }
        else 
        {
            isChanged[6] = false;
        }
        SaveButtonInteractable();
    }

    public void Turn_countdownSliderChanged()
    {
        T_cNum.text = turn_countdownNum[(int)Turn_countdownSlider.value] + "s";

        dict.Remove("turn_countdown");

        if (GameData.Shared.SettingThinkTime != turn_countdownNum[(int)Turn_countdownSlider.value])
        {
            int turnTime = turn_countdownNum[(int)Turn_countdownSlider.value];
            dict.Add("turn_countdown", turnTime);
            isChanged[5] = true;
        }
        else 
        {
            isChanged[5] = false;
        }
        SaveButtonInteractable();

    }

    
    void SaveButtonInteractable() 
    {
        if (buyTimeCost > GameData.Shared.Coins) {
            SaveButton.interactable = false;
            return ;
        } 

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
