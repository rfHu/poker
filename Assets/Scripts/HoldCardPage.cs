using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityEngine.UI;

public class HoldCardPage : MonoBehaviour {

    public Text Total;
    public Text Current;
    public Slider PageSlider;
    public GameObject HoldCardMsgPrefab;
    public GameObject Rect;

    private List<HoldCardMsg> users = new List<HoldCardMsg>();

    private int totalNumber = 0;
    private int currentNumber = 0;
    private string roomID;
    private bool requesting = false;

    void Awake()
    {
        // 切换房间了，把弹框关闭
        GameData.Shared.Room.Subscribe((rid) =>
        {
            if (!gameObject.activeInHierarchy || string.IsNullOrEmpty(roomID))
            {
                return;
            }

            if (rid != roomID)
            {
                PoolMan.Despawn(transform);
            }
        }).AddTo(this);
    }

    void OnSpawned()
    {
        GetComponent<DOPopup>().Show();

        if (GameData.Shared.Room.Value == roomID && currentNumber != totalNumber)
        {
            request(currentNumber);
        }
        else
        {
            currentNumber = 0;
            request(0);
        }

        roomID = GameData.Shared.Room.Value;
    }

    void OnDespawned()
    {
        requesting = false;
    }

    public void request(int num = 0)
    {
        if (requesting)
        {
            return;
        }

        var dict = new Dictionary<string, object>() {
			{"page", num},
		};

        Connect.Shared.Emit(
            new Dictionary<string, object>() {
				{"f", "holdcards"},
				{"args", dict}
			},
            (json) =>
            {
                reload(json);
                requesting = false;
            },
            () =>
            {
                requesting = false;
            }
        );
        requesting = true;
    }

    private void reload(Dictionary<string, object> ret)
    {
        totalNumber = ret.Int("total_page");
        currentNumber = ret.Int("currentPage");
        PageSlider.maxValue = totalNumber;
        PageSlider.minValue = Mathf.Min(totalNumber, 1);
        PageSlider.value = currentNumber;

        Total.text = string.Format("/ {0}", totalNumber);
        Current.text = currentNumber.ToString();

        var list = ret.List("data");
        for (int num = 0; num < 10; num++)
        {
            if (list.Count > num)
            { // 复用的部分
                var dt = list[num] as Dictionary<string, object>;

                HoldCardMsg user;

                if (num < users.Count)
                {
                    user = users[num];
                    user.gameObject.SetActive(true);
                }
                else
                {
                    var go = GameObject.Instantiate(HoldCardMsgPrefab, Rect.transform);
                    user = go.GetComponent<HoldCardMsg>();
                    users.Add(user);
                }

                user.Show(dt);
            }
            else if (users.Count > num)
            { // 超出的部分隐藏
                users[num].gameObject.SetActive(false);
            }
        }
    }

    public void Right()
    {
        if (currentNumber >= totalNumber)
        {
            return;
        }

        request(currentNumber + 1);
    }

    public void Left()
    {
        if (currentNumber <= 1)
        {
            return;
        }

        request(currentNumber - 1);
    }

    public void Right2End()
    {
        request(0);
    }

    public void Left2Start()
    {
        if (totalNumber < 1)
        {
            return;
        }
        request(1);
    }

    public void OnPointUpSlider()
    {
        request((int)PageSlider.value);
    }
}
