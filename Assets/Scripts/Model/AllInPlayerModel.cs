using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AllInPlayerModel : MonoBehaviour {

        public Text Name;

        public List<Card> Cards;

        public Text Kind;

        string kind1 = "AllIn玩家";
        string kind2 = "正在购买保险";

        public void Init(string name, List<int> cards, bool isSelf)
        {
            Name.text = name;


            Cards[0].Show(cards[0]);
            Cards[1].Show(cards[1]);

            Kind.text = isSelf ? kind2 : kind1;
        }

}
