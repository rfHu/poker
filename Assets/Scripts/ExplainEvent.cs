using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class ExplainEvent : MonoBehaviour {

    public GameObject NormalList;

    public GameObject KingThreeList;

    public GameObject NormalText;

    void Awake() 
    {
        GameData.Shared.Type.Subscribe((type) => 
        {
            SetList();
        }).AddTo(this);

        SetList();
    }

    private void SetList()
    {
        bool isKingThree = GameData.Shared.Type.Value == GameType.KingThree;

        NormalList.SetActive(!isKingThree);
        KingThreeList.SetActive(isKingThree);
        NormalText.SetActive(!isKingThree);
    }
}
