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
        var pool = PoolManager.Pools.Create("Shared", shared);
		pool.dontReparent = true;

        hasInit = true;
    }

    // public static void SetCamera(Camera camera) {
    //     var canvas = shared.GetComponent<Canvas>();
    //     canvas.renderMode = RenderMode.ScreenSpaceCamera;
    //     canvas.worldCamera = camera;
    //     canvas.sortingLayerName = "AboveParticle";
    //     canvas.sortingOrder = 2; 
    // }
}