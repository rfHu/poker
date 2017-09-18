using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoviceBoot : MonoBehaviour {

    int clickTimes = 0;

	private ModalHelper modal;

	void Awake()
	{
		modal = ModalHelper.Create();	
		modal.Show(transform.parent, null, new Color(0, 0, 0, 102 / 255f));
		transform.SetAsLastSibling();
	}

	void OnDestroy()
	{
		modal.Despawn();		
	}

    public void OnClick() 
    {
        clickTimes++;
		var childCount = transform.childCount;

        if (clickTimes >= childCount)
        {
            Destroy(gameObject);
        }
        else
        {
     		transform.GetChild(clickTimes).gameObject.SetActive(true);
            transform.GetChild(clickTimes - 1).gameObject.SetActive(false);
        }
    }
}
