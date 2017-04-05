using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Emoticon : MonoBehaviour {

    public GameObject emoticonEntity;
    private Animator _animator;
    private Image _image;

    int pid;
    GameObject toSeat;

    void Awake() {
        _animator = emoticonEntity.GetComponent<Animator>();
        _image = emoticonEntity.GetComponent<Image>();
    }

    public void Init(GameObject fromSeat, GameObject toSeat, int pid) 
    {
        this.pid = pid;
        this.toSeat = toSeat;

        transform.SetParent(G.UICvs.transform, false);
        transform.position = fromSeat.GetComponent<RectTransform>().position;

        if (toSeat.GetComponent<Seat>().GetPos() == SeatPosition.Right)
        {
            transform.localScale = new Vector3(-1, 1, 0);
        }

        Tween tween = transform.DOMove(toSeat.transform.position, 0.6f).SetEase(Ease.OutQuad);
        tween.Play();
        _animator.SetTrigger(pid.ToString());

        tween.OnComplete(PlayEmoticon);
    }

    void PlayEmoticon() 
    {
        StartCoroutine(DestoryObj());
    }

        IEnumerator DestoryObj()  
    {  
        for(float timer = 2.8f; timer >= 0; timer -= Time.deltaTime)  
            yield return 0;

        GameObject.Destroy(gameObject);
    } 
}
