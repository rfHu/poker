#pragma strict
import System.Xml;

private var imgLoading:LeanLoading[];

function Start () {
	imgLoading = new LeanLoading[3];
	imgLoading[0] = LeanLoader.load("http://dentedpixel.com/assets/Monkeyshines.png", LLOptions().setOnLoad(onCubeImageLoaded));
	imgLoading[1] = LeanLoader.load("http://www.mayang.com/textures/Fabric/images/Fine%20Fabric%20Textures/blue_silk_2020005.JPG", LLOptions().setOnLoad(onCubeImageLoaded));
	imgLoading[2] = LeanLoader.load("http://www.mayang.com/textures/Fabric/images/Fine%20Fabric%20Textures/furry_material_2020094.JPG", LLOptions().setOnLoad(onCubeImageLoaded));
}

private var cubeIter:int;

private function onCubeImageLoaded( tex:Texture2D ){
	var cube:GameObject = GameObject.Find("Cube"+cubeIter);
	cube.GetComponent.<Renderer>().material.mainTexture = tex; // Texture used as a Material Texture

	cubeIter++;
}

function OnGUI(){
	GUI.Label(new Rect(0.0f,Screen.height*0.0f,Screen.width,Screen.height*0.1f), " Image1 Progress:"+ imgLoading[0].progress*100.0 +"%");
	GUI.Label(new Rect(0.0f,Screen.height*0.05f,Screen.width,Screen.height*0.1f), " Image2 Progress:"+ imgLoading[1].progress*100.0 +"%");
	GUI.Label(new Rect(0.0f,Screen.height*0.1f,Screen.width,Screen.height*0.1f), " Image3 Progress:"+ imgLoading[2].progress*100.0 +"%");
}

private var mat : Material;

function OnPostRender(){
	if( !mat )
        mat = new Material(Shader.Find("GUI/Text Shader") );

    	var progress:float = (imgLoading[0].progress+imgLoading[1].progress+imgLoading[2].progress)/3.0;

    	GL.PushMatrix();
		mat.SetPass(0);
		GL.LoadOrtho();
		GL.Begin(GL.QUADS);

		GL.Color(Color(0,0,0,0.5));
		GL.Vertex3(0.4+0.005,0.45-0.01,0);
		GL.Vertex3(0.6+0.005,0.45-0.01,0);
		GL.Vertex3(0.6+0.005,0.55-0.01,0);
		GL.Vertex3(0.4+0.005,0.55-0.01,0);

		GL.Color(Color.white);
		GL.Vertex3(0.4,0.45,0);
		GL.Vertex3(0.6,0.45,0);
		GL.Vertex3(0.6,0.55,0);
		GL.Vertex3(0.4,0.55,0);

		GL.Color(Color.cyan);
		GL.Vertex3(0.4+0.007,0.45+0.01,0);
		GL.Vertex3(0.4+0.007,0.55-0.01,0);
		GL.Color(Color.blue);
		GL.Vertex3(0.4+0.2*progress-0.007,0.55-0.01,0);
		GL.Vertex3(0.4+0.2*progress-0.007,0.45+0.01,0);

		

		GL.End();
		GL.PopMatrix();
}
