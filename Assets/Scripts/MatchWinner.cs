using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.IO;

public class MatchWinner : MonoBehaviour {

    public GameObject coinImg;

    public Text coinNum;

    public GameObject StayInRoom;

    public GameObject SharePicBtn;

    public GameObject[] RankNumGo;

    public Sprite[] NumberImgs;

    private static Transform instance;

    public static bool IsSpawned {
        get {
            return PoolMan.IsReady() && PoolMan.IsSpawned(instance);
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

    public void Init(Dictionary<string, object> json) 
    {
        var rank = json.Int("rank");
        var score = json.Int("score");
        var isEnd = json.Int("is_end") == 1;
        var win = json.Int("win");

        this.gameEnd = isEnd;

        coinImg.SetActive(score != 0);
        SharePicBtn.SetActive(score != 0 || win > 0);

        var awardText = json.Dict("award").String("award");

        if (score > 0) {
            coinNum.text = score.ToString();
        } else if (!string.IsNullOrEmpty(awardText)) {
            coinNum.text = string.Format("恭喜您获得：\n<b><size=60>{0}</size></b>", awardText);
        } else {
            coinNum.text = "调整好状态再来一局吧！";
        }

        StayInRoom.SetActive(!gameEnd);
        setRankNum(rank);
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
#if UNITY_IOS
		Commander.Shared.ShareSNGResult();
#elif UNITY_ANDROID
        CaptureByUnity();
#else
				return null;
#endif

    }

    public void LeftRoom() 
    {
        GetComponent<DOPopup>().Close();
        if (gameEnd)
        {
            // 获取ID，调用ExitCb后无法获取
            var id = "";
            var toPage = "";

            switch (GameData.Shared.Type) 
            {
                case GameType.MTT: id = GameData.Shared.MatchID; toPage = "record_mtt.html"; break;
                case GameType.SNG: id = GameData.Shared.Room.Value; toPage = "record_sng.html"; break;
                default:
                    break;
            }

            External.Instance.ExitCb(() =>
            {
                Commander.Shared.GameEnd(id, toPage);
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

    private void CaptureByUnity()
    {
        //图片名
        string filename = "pokerScreen.png";

        Commander.Shared.ShareSNGResult(Application.persistentDataPath + "/" + filename);

        Application.CaptureScreenshot(filename, 0);

    }  
}
