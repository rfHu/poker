using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.IO;

public class MatchWinner : MonoBehaviour {

    public Text AwardText;

    public GameObject TitleMsg;

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
        var award = json.Dict("award").String("award");

        this.gameEnd = isEnd;

        var isDefeat = score == 0 && win == 0;
        Image _image = GetComponent<Image>();
        Animator _animator = GetComponent<Animator>();

        //标题
        if (isDefeat)
        {
            _animator.enabled = false;
            _image.sprite = Resources.Load<Sprite>("Sprite/DefeatPage");
            Resources.UnloadUnusedAssets();
            TitleMsg.SetActive(false);
        }
        else 
        {
            _animator.enabled = true;
            _image.sprite = Resources.Load<Sprite>("Sprite/WinPage");
            Resources.UnloadUnusedAssets();
            TitleMsg.SetActive(true);
            bool isMTT = GameData.Shared.Type == GameType.MTT;
            TitleMsg.transform.GetChild(0).gameObject.SetActive(isMTT);
            TitleMsg.transform.GetChild(1).localScale = isMTT ? new Vector2(0.5f, 0.5f) : Vector2.one;
        }

        //文字显示
        if (score > 0) {
            AwardText.text = string.Format("恭喜您获得：\n<size=60>{0}</size>", score);
            AwardText.color = _.HexColor("#ffd54fff");
        } else if (!string.IsNullOrEmpty(award)) {
            AwardText.text = string.Format("恭喜您获得：\n<size=60>{0}</size>", award);
            AwardText.color = _.HexColor("#ffd54fff");
        } else {
            AwardText.text = "您被淘汰了\n<size=60>再接再厉，加油！</size>";
            AwardText.color = new Color(1, 1, 1, 0.6f);
        }

        SharePicBtn.SetActive(!isDefeat);
        StayInRoom.SetActive(!gameEnd);
        SetRankNum(rank);
    }

    private int SetRankNum(int rank)
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

        ScreenCapture.CaptureScreenshot(filename, 0);

    }  
}
