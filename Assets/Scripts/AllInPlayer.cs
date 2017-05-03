using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AllInPlayer : MonoBehaviour {

        public Text Name;

        public List<Card> Cards;

        public Text Kind;

        string kind1 = "AllIn玩家";
        string kind2 = "购买保险中";

        public void Init(string name, List<int> cards, string uid)
        {
            Name.text = name;


            Cards[0].Show(cards[0]);
            Cards[1].Show(cards[1]);

            Kind.text = uid == GameData.Shared.Uid? kind2 : kind1;
        }

}
