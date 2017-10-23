using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class ExplainPage2 : MonoBehaviour {
	public GameObject KingThreePrefab;
	public GameObject NormalPrefab;

	private GameObject kingThreeContent;
	private GameObject normalContent;

	private bool hasInit = false;

	private GameObject getContent(GameType type) {
		if (type == GameType.KingThree) {
			if (kingThreeContent == null) {
				kingThreeContent = Instantiate(KingThreePrefab, transform, false);
			}

			return kingThreeContent;
		} else {
			if (normalContent == null) {
				normalContent = Instantiate(NormalPrefab, transform, false);
			}

			return normalContent; 
		}
	}

	void OnEnable()
	{
		if (hasInit) {
			return ;
		}

		GameData.Shared.Type.Subscribe((type) => {
			if (kingThreeContent != null) {
				kingThreeContent.SetActive(false);
			}

			if (normalContent != null) {
				normalContent.SetActive(false);
			}

			getContent(type).SetActive(true);
		}).AddTo(this);		
	}	
}
