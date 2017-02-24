using UnityEngine;
using System.Collections;

public class ProgressBarCS : MonoBehaviour {

	private LeanLoading[] imgLoading;

	void Start () {
		imgLoading = new LeanLoading[3];
		imgLoading[0] = LeanLoader.load("http://dentedpixel.com/assets/Monkeyshines.png", new LLOptions().setOnLoad(onCubeImageLoaded));
		imgLoading[1] = LeanLoader.load("http://www.mayang.com/textures/Fabric/images/Fine%20Fabric%20Textures/blue_silk_2020005.JPG", new LLOptions().setOnLoad(onCubeImageLoaded));
		imgLoading[2] = LeanLoader.load("http://www.mayang.com/textures/Fabric/images/Fine%20Fabric%20Textures/furry_material_2020094.JPG", new LLOptions().setOnLoad(onCubeImageLoaded));
	}

	private int cubeIter;

	private void onCubeImageLoaded( Texture2D tex ){
		GameObject cube = GameObject.Find("Cube"+cubeIter);
		cube.GetComponent<Renderer>().material.mainTexture = tex; // Texture used as a Material Texture

		cubeIter++;
	}

	void OnGUI(){
		GUI.Label(new Rect(0.0f,Screen.height*0.0f,Screen.width,Screen.height*0.1f), " Image1 Progress:"+ imgLoading[0].progress*100.0 +"%");
		GUI.Label(new Rect(0.0f,Screen.height*0.05f,Screen.width,Screen.height*0.1f), " Image2 Progress:"+ imgLoading[1].progress*100.0 +"%");
		GUI.Label(new Rect(0.0f,Screen.height*0.1f,Screen.width,Screen.height*0.1f), " Image3 Progress:"+ imgLoading[2].progress*100.0 +"%");
	}

	private Material mat;

	void OnPostRender(){
		if( !mat )
	        mat = new Material(Shader.Find("GUI/Text Shader") );

		float progress = System.Convert.ToSingle(imgLoading[0].progress+imgLoading[1].progress+imgLoading[2].progress)/3.0f;

		GL.PushMatrix();
		mat.SetPass(0);
		GL.LoadOrtho();
		GL.Begin(GL.QUADS);

		GL.Color(new Color(0f,0f,0f,0.5f));
		GL.Vertex3(0.4f+0.005f,0.45f-0.01f,0f);
		GL.Vertex3(0.6f+0.005f,0.45f-0.01f,0f);
		GL.Vertex3(0.6f+0.005f,0.55f-0.01f,0f);
		GL.Vertex3(0.4f+0.005f,0.55f-0.01f,0f);

		GL.Color(Color.white);
		GL.Vertex3(0.4f,0.45f,0f);
		GL.Vertex3(0.6f,0.45f,0f);
		GL.Vertex3(0.6f,0.55f,0f);
		GL.Vertex3(0.4f,0.55f,0f);

		GL.Color(Color.cyan);
		GL.Vertex3(0.4f+0.007f,0.45f+0.01f,0f);
		GL.Vertex3(0.4f+0.007f,0.55f-0.01f,0f);
		GL.Color(Color.blue);
		GL.Vertex3(0.4f+0.2f*progress-0.007f,0.55f-0.01f,0f);
		GL.Vertex3(0.4f+0.2f*progress-0.007f,0.45f+0.01f,0f);

		GL.End();
		GL.PopMatrix();
	}

}
