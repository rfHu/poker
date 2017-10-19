using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class ExplainEvent : MonoBehaviour {

    public GameObject NormalList;

    public GameObject KingThreeList;

    public GameObject NormalText;

    public GameObject[] Toggles;

    ToggleGroup Group;

    void Awake() 
    {
        foreach (var item in Toggles)
        {
            item.GetComponent<Toggle>().onValueChanged.AddListener((isOn) =>
            {
                Text text = item.transform.GetComponentInChildren<Text>();
                text.color = isOn ? _.HexColor("#18FFFFFF") : new Color(1, 1, 1, 0.6f);
            });
        }
    }

    void OnSpawned() 
    {
        SetList(GameData.Shared.Type.Value);
    }

    private void SetList(GameType type)
    {
        GetComponent<ToggleGroup>().SetAllTogglesOff();

        Toggles[0].SetActive(!GameData.Shared.IsMatch() && type != GameType.SixPlus);
        Toggles[2].SetActive(type == GameType.Omaha);

        bool isKingThree = type == GameType.KingThree;
        NormalList.SetActive(!isKingThree);
        KingThreeList.SetActive(isKingThree);
        NormalText.SetActive(!isKingThree);

        foreach (var item in Toggles)
        {
            if (item.activeInHierarchy)
            {
                item.GetComponent<Toggle>().isOn = true;
                break;
            }
        }
    }
}
