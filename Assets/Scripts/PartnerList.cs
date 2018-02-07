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

    bool hasChacked;

    public void Init(string uid)
    {
        hasChacked = false;
        EnterBtn.interactable = false;
        aimUid = new List<string>();

        HTTP.Get("/post-fee", new Dictionary<string, object>() { }, (data) =>
        {
            var feedata = Json.Decode(data) as Dictionary<string, object>;
            checkFee = feedata.Int("checkFee");
            Text_CheckFee.text = checkFee.ToString();
        });

        ToggleList.Clear();

        var list = GameData.Shared.Players;

        float num = 738 + Mathf.Ceil(list.Count / 3f) * 269 - 40;
        GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x, num);


        foreach (var item in list)
        {
            var player = item.Value;
            if (player.Uid == GameData.Shared.Uid)
                continue;
            var go = Instantiate(ListChildPre, ToggleList);
            go.GetComponentInChildren<Text>().text = player.Name;
            go.GetComponentInChildren<Avatar>().SetImage(player.Avatar);
            go.GetComponent<Toggle>().onValueChanged.AddListener((bool isOn) =>
            {
                if (isOn)
                {
                    if (aimUid.Count == 2)
                    {
                        go.GetComponent<Toggle>().isOn = false;
                    }
                    else
                    {
                        aimUid.Add(player.Uid);
                        if (aimUid.Count == 2)
                        {
                            HTTP.Get("/has-checked", new Dictionary<string, object>(){
                {"a_uid", aimUid[0]},
                {"b_uid", aimUid[1]},
                     { "room_id", GameData.Shared.Room.Value},
            }, (data) => {
                var dataDic = Json.Decode(data) as Dictionary<string, object>;
                hasChacked = dataDic.Int("has") == 1;
                EnterBtn.transform.GetChild(1).gameObject.SetActive(!hasChacked);
                EnterBtn.interactable = true;
            });
                        }
                    }
                }
                else
                {
                    aimUid.Remove(player.Uid);
                    if (aimUid.Count != 2)
                        EnterBtn.interactable = false;
                }
            });

            if (player.Uid == uid)
            {
                go.GetComponent<Toggle>().isOn = true;
            }
        }
    }

    public void OnClickEnter()
    {
        if (hasChacked)
        {
            AskPartnersData();
        }
        else
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
                     AskPartnersData();
                 }
             }), null);
        }
    }

    private void AskPartnersData()
    {
        HTTP.Get("/check-partner", new Dictionary<string, object>(){
                {"a_uid", aimUid[0]},
                {"b_uid", aimUid[1]},
                     { "room_id", GameData.Shared.Room.Value},
                }, (data) =>
                {
                    var partnerData = PoolMan.Spawn("PartnerData");
                    partnerData.GetComponent<DOPopup>().Show();
                    var dataDic = Json.Decode(data) as Dictionary<string, object>;
                    partnerData.GetComponent<PartnerData>().Init(dataDic);
                });
    }
}
