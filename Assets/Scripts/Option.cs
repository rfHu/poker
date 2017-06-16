using DarkTonic.MasterAudio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Option : MonoBehaviour {

    public Toggle[] Toggles;

    public Color NormalColor = MaterialUI.MaterialColor.cyanA200;
    public Color DisableColor = MaterialUI.MaterialColor.grey400;

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

        Toggles[0].onValueChanged.AddListener((isOn) => 
        {
            GameData.Shared.TalkSound = isOn;
        });

        Toggles[2].onValueChanged.AddListener((isOn) => 
        {
            GameData.Shared.ChatBubble = isOn;
        });

        Toggles[0].isOn = GameData.Shared.TalkSound;
        Toggles[2].isOn = GameData.Shared.ChatBubble;
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

    public void ToggleMute()
    {
        if (GameData.Shared.muted)
        {
            MasterAudio.UnmuteEverything();
        }
        else
        {
            MasterAudio.MuteEverything();
        }

        GameData.Shared.muted = !GameData.Shared.muted;
    }
}
