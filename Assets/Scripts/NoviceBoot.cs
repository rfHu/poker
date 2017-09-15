using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoviceBoot : MonoBehaviour {

    public GameObject[] Steps;

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
        if (clickTimes > Steps.Length-1)
        {
            Destroy(gameObject);
        }
        else
        {
            Steps[clickTimes].SetActive(true);
            Steps[clickTimes - 1].SetActive(false);
        }
    }
}
