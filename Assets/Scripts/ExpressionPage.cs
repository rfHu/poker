﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpressionPage : MonoBehaviour {

    public GameObject[] Buttons;

    void Awake()
    {
        foreach (var item in Buttons)
	    {
            item.AddComponent<Button>();

            item.GetComponent<Button>().onClick.AddListener(() =>
            {
                var dict = new Dictionary<string, object>() {
			        {"expression", item.name}
		        };

                Connect.Shared.Emit(
                    new Dictionary<string, object>() {
				        {"f", "expression"},
				        {"args", dict}
			    });

                transform.GetComponent<DOPopup>().Close();
            });
        }
    }
}
