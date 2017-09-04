using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class AllInPlayer : MonoBehaviour {

    public Text Name;

	private Card card1 {
		get {
			return CardContainers[0].CardInstance;
		}
	}

	private Card card2 {
		get {
			return CardContainers[1].CardInstance;
		}
	}
    [SerializeField]
    private List<CardContainer> CardContainers; 

    public Text Kind;

    public Text WinRateText;

    public void Init(Dictionary<string, object> data)
    {
        var player = GameData.Shared.FindPlayer(data.String("uid"));
        var outsNumber = data.Int("ct");

        Name.text = player.Name;


        card1.Show(player.Cards.Value[0]);
        card2.Show(player.Cards.Value[1]);

        Kind.text = outsNumber.ToString() + "张";
        WinRateText.text = data.Int("win_rate") + "%";
    }

}
