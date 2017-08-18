using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class AllInPlayer : MonoBehaviour {

        public Text Name;

        public List<Transform> Cards
        {
            get
            {
                return CardContainers.Select(o => o.CardInstance.transform).ToList();
            }
        }
        [SerializeField]
        private List<CardContainer> CardContainers; 

        public Text Kind;

        public void Init(string name, List<int> cards, string uid, int outsNumber)
        {
            Name.text = name;


            Cards[0].GetComponent<Card>().Show(cards[0]);
            Cards[1].GetComponent<Card>().Show(cards[1]);

            Kind.text = outsNumber.ToString() + "张";
        }

}
