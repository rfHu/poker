using BestHTTP.JSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartnerList : MonoBehaviour {

    public GameObject ListChildPre;

    public Transform ToggleList;

    public CButton EnterBtn;

    public Text Text_CheckFee;

    List<string> aimUid;

    int checkFee;

    public void OnSpawned()
    {
        EnterBtn.interactable = false;
        aimUid = new List<string>();

        HTTP.Get("/post-fee", new Dictionary<string, object>() { }, (data) =>
        {
            var feedata = Json.Decode(data) as Dictionary<string, object>;
            checkFee = feedata.Int("checkFee");
            Text_CheckFee.text = checkFee.ToString();
        });

        for (int i = 0; i < ToggleList.childCount; i++)
        {
            Destroy(ToggleList.GetChild(i).gameObject);
        }

        var list = GameData.Shared.Players;

        float num = 698 + Mathf.Ceil(list.Count / 3f) * 269 - 40;
        GetComponent<RectTransform>().sizeDelta =new Vector2(GetComponent<RectTransform>().sizeDelta.x, num);


        foreach (var item in list)
        {
            var player = item.Value;
            var go = Instantiate(ListChildPre, ToggleList);
            go.GetComponentInChildren<Text>().text = player.Name;
            go.GetComponentInChildren<Avatar>().SetImage(player.Avatar);
            go.GetComponent<Toggle>().onValueChanged.AddListener((bool isOn) => {
                if (isOn)
                {
                    if (aimUid.Count == 2)
                    {
                        go.GetComponent<Toggle>().isOn = false;
                    }
                    else
                    {
                        aimUid.Add(player.Uid);
                    }
                }
                else
                {
                    aimUid.Remove(player.Uid);
                }

                EnterBtn.interactable = aimUid.Count == 2 ? true : false;

            });
        }

    }


    public void OnClickEnter()
    {
        PokerUI.Alert("请确认是否支付 <color=#ffca28>" + checkFee + "金币</color>", (() =>
         {
             if (GameData.Shared.Coins < 100)
             {
                 _.PayFor(() =>
                 {
                     RxSubjects.TakeCoin.OnNext(new RxData());
                 });
             }
             else
             {
                 Connect.Shared.Emit(new Dictionary<string, object>() {
            {"f", "partnerinfo"},
            {"args",  new Dictionary<string, object>(){
                {"a_uid", aimUid[0]},
                {"b_uid", aimUid[1]}

            }
                     }
                 },(data) => {
                     var PartnerData = PoolMan.Spawn("PartnerData");
                     PartnerData.GetComponent<PartnerData>().Init(data);
                 });
             }
         }), null);
    }
}
