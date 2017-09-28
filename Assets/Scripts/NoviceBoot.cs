using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class NoviceBoot : MonoBehaviour {

    int clickTimes = 0;

	private ModalHelper modal;

	void Awake()
	{
		modal = ModalHelper.Create();	
		modal.Show(G.DialogCvs.transform, null, new Color(0, 0, 0, 153 / 255f));

		RxSubjects.GameReset.Subscribe((_) => {
			Destroy(gameObject);
		}).AddTo(this);
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
