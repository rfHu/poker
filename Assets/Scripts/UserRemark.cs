using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Collections.Generic;

public class UserRemark: MonoBehaviour {
    public InputField Input;
    public Text InputCount;
    public string Uid;

    void Awake()
    {
        Input.OnValueChangedAsObservable().Subscribe((text) => {
            var txt = text.CnCut(40);
            Input.text = txt; 
            setTextCount(txt);
        }).AddTo(this);
    }

    public void Show(string uid, string remark) {
        this.Uid = uid;
        Input.text = remark;

        GetComponent<DOPopup>().Show();
    }

    private void setTextCount(string text) {
        InputCount.text = string.Format( "（{0} / 40）", text.CnCount());
    }

    public void Hide() {
        GetComponent<DOPopup>().Close();
    }

    public void OnReturn() {
        PoolMan.Spawn("User").GetComponent<UserDetail>().Init(Uid);
    }

    private bool requesting = false;

    public void OnSave() {
        if (requesting) {
            return ;
        } 

        requesting = true;

        HTTP.Post("/remark", new Dictionary<string, object> {
            {"user_id", Uid},
            {"remark", Input.text}
        }, (_) => {
            requesting = false;
            PoolMan.Spawn("User").GetComponent<UserDetail>().Init(Uid);
        });
    }
}