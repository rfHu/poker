using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SNGMsgPage : MonoBehaviour {

    public Text Rank;

    public Text PlayerNum;

    public Text Time;

    public GameObject[] Cups;

    private int[][] data = { new int[] { 200, 3 }, new int[] { 500, 5 }, new int[] { 1000, 10 }, new int[] { 2000, 10 } };

    public void Init() 
    {
        Rank.text = GameData.Shared.SNGRank.Value == 0 ? "/" : GameData.Shared.SNGRank.ToString();
        PlayerNum.text = GameData.Shared.SeatsCount.ToString();
        Time.text = data[GameData.Shared.SNGType - 1][1].ToString();

        SetCups();
    }

    private void SetCups()
    {
        int[] coinArr = new int[0];
        switch (GameData.Shared.SeatsCount)
        {
            case 2:
                coinArr = new int[1];
                coinArr[0] = data[GameData.Shared.SNGType][0] * GameData.Shared.SeatsCount;
                break;
            case 6:
                coinArr = new int[2];
                coinArr[0] = (int)(data[GameData.Shared.SNGType][0] * GameData.Shared.SeatsCount * 0.6f);
                coinArr[1] = (int)(data[GameData.Shared.SNGType][0] * GameData.Shared.SeatsCount * 0.4f);
                break;
            case 9:
                coinArr = new int[2];
                coinArr[0] = (int)(data[GameData.Shared.SNGType][0] * GameData.Shared.SeatsCount * 0.5f);
                coinArr[1] = (int)(data[GameData.Shared.SNGType][0] * GameData.Shared.SeatsCount * 0.3f);
                coinArr[2] = (int)(data[GameData.Shared.SNGType][0] * GameData.Shared.SeatsCount * 0.2f);
                break;
            default:
                break;
        }
        for (int i = 0; i < Cups.Length; i++)
        {
            if (i < coinArr.Length)
            {
                Cups[i].SetActive(true);
                Cups[i].transform.GetChild(1).GetComponent<Text>().text = coinArr[i].ToString();
            }
            else
            {
                Cups[i].SetActive(false);
            }
        }
    }

}
