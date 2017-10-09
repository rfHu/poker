using MTTMsgPage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MTTMsgP3 : MonoBehaviour {

    public Transform Content;

    public GameObject ChildPerfab;

    private List<MTTP3ChildGo> childGos = new List<MTTP3ChildGo>();
    private int highLightLevel;
    private Color openCol = new Color(24 / 255, 1, 1);
    MTTType nowType;

    public void SetPage() 
    {
        transform.parent.parent.GetComponent<MTTMsg>().SetGoSize(true);
        SetHighLightCol(new Color(1, 1, 1, 0.6f), highLightLevel);

        if (nowType != GameData.MatchData.MTTType)
        {
            nowType = GameData.MatchData.MTTType;

            int[,] newArr = new int[,] { };
            if (GameData.MatchData.MTTType == MTTType.Normal)
                newArr = new int[,]{{20, 0},{30, 0},{50, 0},{100, 0},{150, 10},{200, 10},{250, 25},{300, 25},{400, 50},{600, 50},
                        {800, 75},{1000, 100},{1200, 125},{1600, 150}, {2000, 200}, {2500, 250}, {3000,300}, {4000,400}, {5000,500},{ 6000,600}, 
                        {7000,700}, {8000,800}, {10000,1000}, {12000,1200}, {16000,1600}, {20000,2000},{25000,2500}, {30000,3000}, {40000,4000}, 
                        {50000,5000}, {60000,6000}, {80000,8000}, {100000,10000}, {120000,12000}, {150000,15000}, {180000,18000}, {210000,21000}, 
                        {240000,24000}, {280000,28000}, {320000,32000}, {360000,36000}, {400000,40000}, {450000,45000}, {500000,50000}, 
                        {550000,55000}, {600000,60000}, {650000,65000}, {700000,70000}, {800000,80000}, {900000,90000},{1000000,100000}, 
                        {1200000,120000}, {1400000,140000}, {1600000,160000}, {1800000,180000}, {2000000,200000}};
            else
                newArr = new int[,] {{20, 0}, {40, 0}, {80, 0}, {160, 0}, {240, 0}, {320, 0}, {400, 40}, {640, 80}, {800, 120},
                        {1200, 200},{1600, 200}, {2400, 400}, {3200, 600}, {4000, 600}, {6400, 800}, {8000, 1200}, {12000, 2000}, 
                        {16000, 2000}, {24000, 4000}, {32000, 6000}, {40000, 6000}, {64000, 8000}, {80000, 12000}, {120000, 20000},
                        {160000, 20000}, {240000, 40000}, {320000, 60000}, {400000, 60000}, {640000, 80000},{800000, 120000} };

            for (int num = 0; num < 56; num++)
            {
                if (newArr.GetLength(0) > num)
                { // 复用的部分

                    MTTP3ChildGo user;

                    if (num < childGos.Count)
                    {
                        user = childGos[num];
                        user.gameObject.SetActive(true);
                    }
                    else
                    {
                        var go = GameObject.Instantiate(ChildPerfab, Content);
                        user = go.GetComponent<MTTP3ChildGo>();
                        childGos.Add(user);
                    }

                    user.SetText(num + 1, newArr[num, 0], newArr[num, 1]);
                }
                else if (childGos.Count > num)
                { // 超出的部分隐藏
                    childGos[num].gameObject.SetActive(false);
                }
            }

        }
        highLightLevel = GameData.Shared.BlindLv;
        SetHighLightCol(openCol, highLightLevel);
    }

    private void SetHighLightCol(Color color, int num)
    {
        if (num >= Content.childCount)
            return;

        Transform turnColor = Content.GetChild(num);
        turnColor.GetChild(1).GetComponentInChildren<Text>().color = color;
        turnColor.GetChild(2).GetComponentInChildren<Text>().color = color;
    }
}
