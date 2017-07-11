using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UnityEngine.UI.ProceduralImage;

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
            var image = GetComponent<ProceduralImage>();
            var rect = GetComponent<RectTransform>();
            
            if (rank <= 0) {
                Msg.gameObject.SetActive(false);
                image.enabled = false; 
                rect.anchoredPosition = new Vector2(150, 450);
            } else {
                Msg.gameObject.SetActive(true);
                Msg.text = string.Format("第<color=#ffd028><size=45>{0}</size></color>名", rank);
                image.enabled = true;
                rect.anchoredPosition = new Vector2(0, 450);
            }
        }).AddTo(disposables);
    }

    void OnDisable()
    {
        disposables.Clear();
    }
}