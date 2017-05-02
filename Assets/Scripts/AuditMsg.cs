using UnityEngine;
using System;

public class AuditMsg : MonoBehaviour {
	public Action Click;

	public void OnClick() {
		if (Click == null) {
			return ;
		}

		Click();	
	}
}
