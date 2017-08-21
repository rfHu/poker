using System;
using UnityEngine;

class CardContainer: MonoBehaviour {
	public Card CardInstance {
		get {
			return card;
		}
	}

    public string Index;

    public bool isDark;

	private Card card;

	void Awake()
	{
		card = Card.LoadCard(transform);		
		card.gameObject.SetActive(false);

        if (!String.IsNullOrEmpty(Index))
        {
            card.ShowInExplain(int.Parse(Index));
            if (isDark)
            {
                card.Darken();
            }
        }
	}
}