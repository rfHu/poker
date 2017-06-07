﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;


public class GamerList : MonoBehaviour {

    public GameObject ListObj;

    public GameObject NoneText;

    public Transform Content;

	// Use this for initialization
	void Awake () {
            var data = new Dictionary<string, object>(){
			    {"type", 2}
		    };

            Connect.Shared.Emit(new Dictionary<string, object>(){
        	    {"f", "gamerlist"},
                {"args",data},
        }, (json) => 
        {
			var ret = json.Dict("ret");
            var list = ret.List("list");

            if (list.Count > 0)
            {
                NoneText.SetActive(false);
            }

            foreach (var item in list)
            {
                var dict = item as Dictionary<string, object>;
                var go = Instantiate(ListObj, Content);
                go.SetActive(true);
                go.GetComponent<GamerListObj>().Init(dict);
            }
        });
	}

}