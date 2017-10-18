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

    void OnSpawned() 
    {
        SetList(GameData.Shared.Type.Value);
    }

    private void SetList(GameType type)
    {
        Toggles[0].SetActive(!GameData.Shared.IsMatch() && type != GameType.SixPlus);
        Toggles[2].SetActive(type == GameType.Omaha);

        bool isKingThree = type == GameType.KingThree;
        NormalList.SetActive(!isKingThree);
        KingThreeList.SetActive(isKingThree);
        NormalText.SetActive(!isKingThree);

        GetComponent<ToggleGroup>().SetAllTogglesOff();
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
