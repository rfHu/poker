using DarkTonic.MasterAudio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Option : MonoBehaviour {

    public Toggle[] Toggles;
    //0.聊天 1.游戏 2，文字 3.动态
    public Toggle[] PokerCol;
    public Toggle[] BgCol;

    public Image ShowGBImg;

    Color NormalColor = MaterialUI.MaterialColor.cyanA200;
    Color DisableColor = MaterialUI.MaterialColor.grey400;

    int PokerColType = GameSetting.cardColor;
    int BGColType = GameSetting.bgColor;

    public Sprite[] bgSprites;

    void Awake() 
    {
        foreach (var toggle in Toggles)
        {
            toggle.onValueChanged.AddListener((isOn) => 
            {
                Text text = toggle.transform.parent.Find("Text (1)").GetComponent<Text>();
                OnToggleChange(text, isOn);
            });
        }

        foreach (var item in PokerCol)
        {
            item.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    PokerColType = int.Parse(item.name);
                }
            });
        }

        foreach (var item in BgCol)
        {
            item.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    var num = int.Parse(item.name);
                    BGColType = num;
                    ShowGBImg.sprite = bgSprites[num];
                }
            });
        }
        PokerCol[PokerColType].isOn = true;
        BgCol[BGColType].isOn = true;

        Toggles[0].isOn = !GameSetting.talkSoundClose;
        Toggles[1].isOn = !GameSetting.muted;
        Toggles[2].isOn = !GameSetting.chatBubbleClose;
        Toggles[3].isOn = !GameSetting.emoticonClose;
    }

    public void OnToggleChange(Text text, bool isOn) 
    {
        if (isOn)
        {
            text.text = "开";
            text.color = NormalColor;
        }
        else 
        {
            text.text = "关";
            text.color = DisableColor;
        }
    }

    public void Enter() 
    {
        if (GameSetting.talkSoundClose == Toggles[0].isOn)
        {
            Commander.Shared.OptionToggle(Toggles[0].isOn, 2);
            GameSetting.talkSoundClose = !Toggles[0].isOn;
        }

        if (GameSetting.muted == Toggles[1].isOn)
        {
            if (GameSetting.muted)
            {
                MasterAudio.UnmuteEverything();
            }
            else
            {
                MasterAudio.MuteEverything();
            }

            GameSetting.muted = !Toggles[1].isOn;
        }

        if (GameSetting.chatBubbleClose == Toggles[2].isOn)
        {
            Commander.Shared.OptionToggle(Toggles[2].isOn, 1);
            GameSetting.chatBubbleClose = !Toggles[2].isOn;
        }

        GameSetting.emoticonClose = !Toggles[3].isOn;

        if (GameSetting.cardColor != PokerColType)
        {
            GameSetting.cardColor = PokerColType;
            RxSubjects.CardStyleChange.OnNext(0);
        }

        if (GameSetting.bgColor != BGColType)
        {
            GameSetting.bgColor = BGColType;
            RxSubjects.BGChange.OnNext(0);
        }

        Exit();
    }

    public void Exit() 
    {
        GetComponent<DOPopup>().Close();
    }
}
