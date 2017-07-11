using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class SNGWinner : MonoBehaviour {

    public Text coinNum;

    public GameObject StayInRoom;

    public GameObject[] RankNumGo;

    public Sprite[] NumberImgs;

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

    public void Init(int rank, int score, bool gameEnd) 
    {
        this.gameEnd = gameEnd;
        if (score != 0)
        {
            coinNum.text = score.ToString();
        }
        else 
        {
            coinNum.text = "没奖不可怕，调整心态再接再厉吧！";
        }

        StayInRoom.SetActive(!gameEnd);

        rank = setRankNum(rank);
    }

    private int setRankNum(int rank)
    {
        List<int> rankNum = new List<int>();
        while (rank != 0)
        {
            rankNum.Add(rank % 10);
            rank /= 10;
        }
        rankNum.Reverse();

        for (int i = 0; i < RankNumGo.Length; i++)
        {
            if (i < rankNum.Count)
            {
                RankNumGo[i].SetActive(true);
                RankNumGo[i].GetComponent<Image>().sprite = NumberImgs[rankNum[i]];
            }
            else
            {
                RankNumGo[i].SetActive(false);
            }
        }
        return rank;
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
