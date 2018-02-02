using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamerDataList : MonoBehaviour {

    public GameObject ListChildPre;

    public Transform ToggleList;

    public CButton EnterBtn;

    List<string> aimUid;

    public void OnSpawned()
    {
        EnterBtn.interactable = false;
        aimUid = new List<string>();

        for (int i = 0; i < ToggleList.childCount; i++)
        {
            Destroy(ToggleList.GetChild(i).gameObject);
        }

        var list = GameData.Shared.Players;
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
        PokerUI.Alert("请确认是否支付 <color=#ffca28>100金币</color>", (() =>
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
