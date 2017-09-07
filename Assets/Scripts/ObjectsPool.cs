using UnityEngine;
using PathologicalGames;

public class ObjectsPool : MonoBehaviour {
    private static bool hasInit = false;

    public static void Init(GameObject prefab, Camera camera) {
        if (hasInit) {
            return ;
        }

        var shared = (GameObject)Instantiate(prefab);
        shared.name = "PoolManager";
        UnityEngine.Object.DontDestroyOnLoad(shared);
        PoolManager.Pools.Create("Shared", shared);
		SetCamera(shared, camera);

        hasInit = true;
    }

    public static void SetCamera(GameObject gameObject, Camera camera) {
        var rootCanvas = gameObject.GetComponent<Canvas>();
        rootCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        rootCanvas.worldCamera = camera;
        rootCanvas.sortingLayerName = "AboveParticle";
        rootCanvas.sortingOrder = 2; 
    }
}