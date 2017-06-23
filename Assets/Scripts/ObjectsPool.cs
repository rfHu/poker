using UnityEngine;

public class ObjectsPool : MonoBehaviour {
    private static bool exists = false;

    public static void Setup() {
        if (exists) {
            return ;
        }

        exists = true;

        GameObject go = (GameObject)Instantiate(Resources.Load("Prefab/PoolManager"));
        go.SetActive(true);
        UnityEngine.Object.DontDestroyOnLoad(go);
    }
}