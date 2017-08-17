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


    Color NormalColor = MaterialUI.MaterialColor.cyanA200;
    Color DisableColor = MaterialUI.MaterialColor.grey400;

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
                    GameSetting.cardColor = int.Parse(item.name);
                }
            });
        }


        foreach (var item in BgCol)
        {
            item.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    GameSetting.bgColor = int.Parse(item.name);
                }
            });
        }

        Toggles[0].isOn = !GameSetting.talkSoundClose;
        Toggles[1].isOn = !GameSetting.muted;
        Toggles[2].isOn = !GameSetting.chatBubbleClose;
        Toggles[3].isOn = !GameSetting.emoticonClose;

        Toggles[0].onValueChanged.AddListener((isOn) => 
        {
            Commander.Shared.OptionToggle(isOn,2);
            GameSetting.talkSoundClose = !isOn;
        });

        Toggles[1].onValueChanged.AddListener((isOn) => 
        {
            if (GameSetting.muted)
            {
                MasterAudio.UnmuteEverything();
            }
            else
            {
                MasterAudio.MuteEverything();
            }

            GameSetting.muted = !isOn;
        });

        Toggles[2].onValueChanged.AddListener((isOn) => 
        {
            Commander.Shared.OptionToggle(isOn, 1);
            GameSetting.chatBubbleClose = !isOn;
        });

        Toggles[3].onValueChanged.AddListener((isOn) => 
        {
            GameSetting.emoticonClose = !isOn;
        });

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
}
