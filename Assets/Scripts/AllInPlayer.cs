using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AllInPlayer : MonoBehaviour {

        public Text Name;

        public List<Card> Cards;

        public Text Kind;

        public void Init(string name, List<int> cards, string uid, int outsNumber)
        {
            Name.text = name;


            Cards[0].Show(cards[0]);
            Cards[1].Show(cards[1]);

            Kind.text = outsNumber.ToString() + "张";
        }

}
