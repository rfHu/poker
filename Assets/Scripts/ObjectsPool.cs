using UnityEngine;
using PathologicalGames;

public class ObjectsPool : MonoBehaviour {
    private static bool hasInit = false;
    private static GameObject shared;

    public static void Init() {
        if (hasInit) {
            return ;
        }

        shared = new GameObject();
        shared.name = "PoolManager";
        UnityEngine.Object.DontDestroyOnLoad(shared);
        PoolManager.Pools.Create("Shared", shared);
        hasInit = true;
    }

    public static GameObject Shared {
        get {
            return shared;
        }
    }

    public void SetCamera(Camera camera) {
        // var rootCanvas = GetComponent<Canvas>();
        // rootCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        // rootCanvas.worldCamera = camera;
        // rootCanvas.sortingLayerName = "AboveParticle";
        // rootCanvas.sortingOrder = 2; 
    }
}