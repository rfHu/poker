using UnityEngine;

public class ObjectsPool : MonoBehaviour {

    public static void Setup() {
        GameObject go = (GameObject)Instantiate(Resources.Load("Prefab/PoolManager"));
        go.SetActive(true);
        UnityEngine.Object.DontDestroyOnLoad(go);
    }
}