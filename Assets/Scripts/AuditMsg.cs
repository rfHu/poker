﻿using UnityEngine;
using System;
using UnityEngine.UI;

public class AuditMsg : MonoBehaviour {
	public Action Click;

    void OnSpawned() 
    {
        Text text = transform.Find("Text").GetComponent<Text>();
        if (GameData.Shared.IsMatch())
            text.text = "报名审核";
        else 
            text.text = "带入审核";
    }

	public void OnClick() {
		if (Click == null) {
			return ;
		}

		Click();	
	}
}
