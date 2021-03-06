﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SNGMsgPage : MonoBehaviour {

    public Text PlayerNum;

    public Text Time;

    public GameObject[] Cups;

    private Color normalCol = new Color(1,1,1,0.7f);

    public Transform BlindLvParents;

    private int highLight = 0;

    public void Init() 
    {
        PlayerNum.text = GameData.Shared.PlayerCount.Value.ToString();

        Time.text = GameData.MatchData.Time.ToString() + "分钟";

        SetCups();

        BlindLvParents.GetChild(highLight).GetChild(1).GetComponent<Text>().color = normalCol;
        highLight = GameData.Shared.BlindLv;
        BlindLvParents.GetChild(highLight).GetChild(1).GetComponent<Text>().color = MaterialUI.MaterialColor.cyanA200;     
    }

    private void SetCups()
    {
        int[] coinArr = new int[0];
        var seats = GameData.Shared.PlayerCount.Value;
        var factor = GameData.MatchData.SNGJoinFee * seats;

        switch (seats)
        {   
            case 2:
                coinArr = new int[1];
                coinArr[0] = factor;
                break;
            case 6:
                coinArr = new int[2];
                coinArr[0] = (int)(factor * 0.6f);
                coinArr[1] = (int)(factor * 0.4f);
                break;
            case 9:
                coinArr = new int[3];
                coinArr[0] = (int)(factor * 0.5f);
                coinArr[1] = (int)(factor * 0.3f);
                coinArr[2] = (int)(factor * 0.2f);
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
