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
                Text text = toggle.transform.parent.FindChild("text(1)").GetComponent<Text>();
                OnToggleChange(text, isOn);
            });
        }
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
