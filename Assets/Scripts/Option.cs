using DarkTonic.MasterAudio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Option : MonoBehaviour {

    public Toggle[] Toggles;
    //0.聊天 1.游戏 2，文字 3.动态

    Color NormalColor = MaterialUI.MaterialColor.cyanA200;
    Color DisableColor = MaterialUI.MaterialColor.grey400;

    void Awake() 
    {
        foreach (var toggle in Toggles)
        {
            toggle.onValueChanged.AddListener((isOn) => 
            {
                Text text = toggle.transform.parent.FindChild("Text (1)").GetComponent<Text>();
                OnToggleChange(text, isOn);
            });
        }

        Toggles[0].isOn = !GameData.Shared.talkSoundClose;
        Toggles[1].isOn = !GameData.Shared.muted;
        Toggles[2].isOn = !GameData.Shared.chatBubbleClose;
        Toggles[3].isOn = !GameData.Shared.emoticonClose;

        Toggles[0].onValueChanged.AddListener((isOn) => 
        {
            Commander.Shared.OptionToggle(isOn,2);
            GameData.Shared.talkSoundClose = !isOn;
        });

        Toggles[1].onValueChanged.AddListener((isOn) => 
        {
            if (GameData.Shared.muted)
            {
                MasterAudio.UnmuteEverything();
            }
            else
            {
                MasterAudio.MuteEverything();
            }

            GameData.Shared.muted = !isOn;
        });

        Toggles[2].onValueChanged.AddListener((isOn) => 
        {
            Commander.Shared.OptionToggle(isOn, 1);
            GameData.Shared.chatBubbleClose = !isOn;
        });

        Toggles[3].onValueChanged.AddListener((isOn) => 
        {
            GameData.Shared.emoticonClose = !isOn;
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
