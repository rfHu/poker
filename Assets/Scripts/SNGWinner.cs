using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SNGWinner : MonoBehaviour {

    public Text coinNum;

    public GameObject StayInRoom;

    private bool isThird;

    public void Init(int coin, bool isThird) 
    {
        this.isThird = isThird; 

        coinNum.text = "奖金 X " + coin.ToString();

        StayInRoom.SetActive(isThird);
    }

    public void ShareSNGResult() 
    {
        Commander.Shared.ShareSNGResult();
    }

    public void LeftRoom() 
    {
        GetComponent<DOPopup>().Close();
        if (!isThird)
        {
            External.Instance.ExitCb(() =>
            {
                _.Log("Unity: Game End");
                Commander.Shared.GameEnd(GameData.Shared.Room, "record_sng.html");
            });
        }
        else{
		    External.Instance.Exit();   
        }
    }
    public void Stay() 
    {
        GetComponent<DOPopup>().Close();
    }
}
