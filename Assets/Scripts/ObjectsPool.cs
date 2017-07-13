using UnityEngine;

public class ObjectsPool : MonoBehaviour {
    private static bool exists = false;

    private static Canvas canvas;

    public static void Setup() {
        if (exists) {
            // canvas.worldCamera = cam;
            return ;
        }

        exists = true;

        GameObject go = (GameObject)Instantiate(Resources.Load("Prefab/PoolManager"));
        go.SetActive(true);
        UnityEngine.Object.DontDestroyOnLoad(go);

        // canvas = go.GetComponent<Canvas>();
        // canvas.worldCamera = cam;
    }
}