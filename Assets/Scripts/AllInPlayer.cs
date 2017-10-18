using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using System.Linq;

public class AllInPlayer : MonoBehaviour {

    public Text Name;

    public HorizontalLayoutGroup CardsParent;

    [SerializeField]
    private List<CardContainer> CardContainers; 

    public Text Kind;

    public Text WinRateText;

    public void Init(Dictionary<string, object> data, int maxPercent)
    {
        var player = GameData.Shared.FindPlayer(data.String("uid"));
        var outsNumber = data.Int("ct");

        Name.text = player.Name;

        //omaha特殊处理
        bool isOmaha = GameData.Shared.Type.Value == GameType.Omaha;
        CardContainers[2].gameObject.SetActive(isOmaha);
        CardContainers[3].gameObject.SetActive(isOmaha);
        CardsParent.spacing = isOmaha ? -46 : 8;

        for (int i = 0; i < player.Cards.Value.Count; i++)
        {
            CardContainers[i].CardInstance.Show(player.Cards.Value[i]);
        }

        Kind.text = outsNumber.ToString() + "张";
        WinRateText.text = data.Int("win_rate") + "%";
        string color =  data.Int("win_rate") == maxPercent? "#ff1744" : "#868d94";
        WinRateText.transform.parent.gameObject.SetActive(maxPercent != -1);
        WinRateText.transform.parent.GetComponent<ProceduralImage>().color = _.HexColor(color);
    }

}
