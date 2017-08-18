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

        public void Init(string name, List<int> cards, string uid, int outsNumber)
        {
            Name.text = name;


            card1.Show(cards[0]);
            card2.Show(cards[1]);

            Kind.text = outsNumber.ToString() + "张";
        }

}
