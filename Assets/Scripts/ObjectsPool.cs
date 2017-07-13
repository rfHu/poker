using UnityEngine;

public class ObjectsPool : MonoBehaviour {
    public static ObjectsPool Shared {
        get {
            if (shared == null) {
                shared = initPool();
            }

            return shared;
        }
    }

    private static ObjectsPool shared;

    private static ObjectsPool initPool() {
        GameObject go = (GameObject)Instantiate(Resources.Load("Prefab/PoolManager"));
        go.SetActive(true);
        UnityEngine.Object.DontDestroyOnLoad(go);

        return go.GetComponent<ObjectsPool>(); 
    }


    public void SetCamera(Camera camera) {
        // var rootCanvas = GetComponent<Canvas>();
        // rootCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        // rootCanvas.worldCamera = camera;
        // rootCanvas.sortingLayerName = "AboveParticle";
        // rootCanvas.sortingOrder = 2; 
    }
}