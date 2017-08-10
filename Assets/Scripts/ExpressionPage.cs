using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpressionPage : MonoBehaviour {

    public List<GameObject> ExpressionPic;

    void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            child.AddComponent<Button>();

            child.GetComponent<Button>().onClick.AddListener(() =>
            {
                var dict = new Dictionary<string, object>() {
			        {"expression", child.name}
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
