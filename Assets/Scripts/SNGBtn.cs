using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class SNGBtn: MonoBehaviour {
    public void OnClick() {
        var SNGMsgPage = PoolMan.Spawn("SNGMsgPage");
        SNGMsgPage.GetComponent<DOPopup>().Show();
        SNGMsgPage.GetComponent<SNGMsgPage>().Init();
    }

    private CompositeDisposable disposables = new CompositeDisposable();

    public Text Msg;

    void OnEnable()
    {
        GameData.SNGData.Rank.Subscribe((rank) => {
            if (rank <= 0) {
                Msg.text = "赛事信息";
            } else {
                Msg.text = string.Format("第<color=#ffd028>{0}</color>名", rank);
            }
        }).AddTo(disposables);
    }

    void OnDisable()
    {
        disposables.Clear();
    }
}