using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SNGWinner : MonoBehaviour {

    public Text coinNum;

    public GameObject StayInRoom;

    public void Init(int coin, bool isFirst) 
    {
        coinNum.text = "奖金 X " + coin.ToString();

        StayInRoom.SetActive(!isFirst);
    }

    public void ShareSNGResult() 
    {
        Commander.Shared.ShareSNGResult();
    }

    public void LeftRoom() 
    {
        GetComponent<DOPopup>().Close();
		External.Instance.Exit();
    }
    public void Stay() 
    {
        GetComponent<DOPopup>().Close();
    }
}
