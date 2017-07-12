using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.IO;

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
#if UNITY_IOS
		Commander.Shared.ShareSNGResult();
#elif UNITY_ANDROID
        SharePicforAndroid();
#else
				return null;
#endif
        
    }

    public void LeftRoom() 
    {
        GetComponent<DOPopup>().Close();
        if (gameEnd)
        {
            // 获取roomID，调用ExitCb后无法获取
            var room = GameData.Shared.Room;

            External.Instance.ExitCb(() =>
            {
                Commander.Shared.GameEnd(room, "record_sng.html");
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

    public void SharePicforAndroid()
    {
        //获取系统时间并命名相片名  
        System.DateTime now = System.DateTime.Now;
        string times = now.ToString();
        times = times.Trim();
        times = times.Replace("/", "-");
        times = times.Replace(" ", "-");
        times = times.Replace(":", "-");
        string filename = "poker" + times + ".png";

        //截取屏幕  
        Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.ETC_RGB4, false);
        texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        texture.Apply();
        //转为字节数组  
        byte[] bytes = texture.EncodeToPNG();

        string destination = "/sdcard/texas.poker.top/sharePhoto";
        //判断目录是否存在，不存在则会创建目录  
        if (!Directory.Exists(destination))
        {
            Directory.CreateDirectory(destination);
        }
        string Path_save = destination + "/" + filename;
        //存图片  
        System.IO.File.WriteAllBytes(Path_save, bytes);

        Commander.Shared.ShareSNGResult(Path_save);
    }
}
