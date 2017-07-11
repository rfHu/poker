using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class SNGWinner : MonoBehaviour {

    public Text coinNum;

    public GameObject StayInRoom;

    private static Transform instance;

    public static bool IsSpawned {
        get {
            return PoolMan.IsSpawned(instance);
        }
    }

    private bool gameEnd = false;

    void OnDespawned() {
        gameEnd = false;
    }

    void Awake() {
        RxSubjects.GameEnd.Subscribe((_) => {
            gameEnd = true;
        }).AddTo(this);

        instance = transform;
    }

    public void Init(int coin, bool gameEnd) 
    {
        this.gameEnd = gameEnd; 
        coinNum.text = coin.ToString();
        StayInRoom.SetActive(!gameEnd);
    }

    public void ShareSNGResult() 
    {
        Commander.Shared.ShareSNGResult();
    }

    public void LeftRoom() 
    {
        GetComponent<DOPopup>().Close();
        if (gameEnd)
        {
            External.Instance.ExitCb(() =>
            {
                Commander.Shared.GameEnd(GameData.Shared.Room, "record_sng.html");
            });
        }
        else{
		    External.Instance.Exit();   
        }
    }
    public void Stay() 
    {
        if (gameEnd) {
            LeftRoom();
        } else {
            GetComponent<DOPopup>().Close();
        }
    }
}
