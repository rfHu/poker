using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplainPage : MonoBehaviour {
	public GameObject Prefab;

	private bool hasInit = false;


	void OnEnable()
	{
		if (hasInit) {
			return ;
		}

		hasInit = true;
		Instantiate(Prefab, transform, false);	
	}
	
}
