using UnityEngine;

class CardContainer: MonoBehaviour {
	public Card CardInstance {
		get {
			return card;
		}
	}

	private Card card;

	void Awake()
	{
		card = Card.LoadCard(transform);		
	}
}