using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Emoticon : MonoBehaviour {

    private Animator _animator;
    private Image _image;
    int pid;
    GameObject toSeat;
    public Sprite[] images;

    void Awake() {
        _animator = gameObject.GetComponent<Animator>();
        _image = gameObject.GetComponent<Image>();
    }

    public void Init(GameObject fromSeat, GameObject toSeat, int pid) 
    {
        _.Log("1");
        this.pid = pid;
        this.toSeat = toSeat;

        transform.SetParent(G.UICvs.transform, false);
        transform.position = fromSeat.GetComponent<RectTransform>().position;
        _image.sprite = images[pid - 1];

        Tween tween = transform.DOMove(toSeat.transform.position, 0.6f).SetEase(Ease.Linear);
        tween.Play();
        tween.OnComplete(PlayEmoticon);
    }

    void PlayEmoticon() 
    {
        _animator.enabled = true;
        transform.SetParent(toSeat.transform, false);
        _animator.SetTrigger(pid.ToString());
        StartCoroutine(DestoryObj());
    }

        IEnumerator DestoryObj()  
    {  
        for(float timer = 2.5f; timer >= 0; timer -= Time.deltaTime)  
            yield return 0;

        GameObject.Destroy(gameObject);
    } 
}
