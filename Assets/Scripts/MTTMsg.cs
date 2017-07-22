using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MTTMsg : MonoBehaviour {

    public Toggle[] Toggles;

    public Text Test;

    private Color SelectCol = new Color(33 / 255, 41 / 255, 50 / 255);
    private Color UnSelectCol = new Color(1, 1, 1, 0.6f);

    void Awake()
    {
        foreach (var item in Toggles)
        {
            item.onValueChanged.AddListener((isOn) => 
            {
                if (isOn)
                {
                    item.transform.Find("Label").GetComponent<Text>().color = SelectCol;
                }
                else 
                {
                    item.transform.Find("Label").GetComponent<Text>().color = UnSelectCol;
                }
            });
        }

        Test.text = "6666666";
    }
}
