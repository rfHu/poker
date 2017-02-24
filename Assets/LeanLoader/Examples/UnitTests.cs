using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitTests : MonoBehaviour {

	private Texture2D facebookProfileImage;

	// Use this for initialization
	void Start () {
		LeanTest.expected = 11;
		string jsonStr = "{'store name':'gingerbees','fruits':{'apple':'fuji','pear':{'color':'yellow'},'grapes':'cabernet'},'likes':'5',credit:300.06,check:true}";
		LeanJSON j = new LeanJSON( jsonStr );
		LeanTest.expect(j["store name"]=="gingerbees", "JSON CONVERSION" );
		LeanJSON j2 = j.Object("fruits");
		LeanTest.expect(j2["apple"]=="fuji", "JSON INNER CONVERSION" );
		LeanTest.expect(j.Object("fruits").Object("pear").String("color")=="yellow", "JSON LONG LINKED" );

		// Escaped characters
		jsonStr = @"{phrase:""Best Quote!"",name:""jerry's place""}";
		j = new LeanJSON( jsonStr );
		LeanTest.expect(j["phrase"].Length>0, "QUOTED ESCAPE" );
		LeanTest.expect( j["name"].IndexOf("'")>=0, "SINGLE QUOUTE");

		// Load a JSON file from a webserver
		Dictionary<string, object> postParams = new Dictionary<string, object>(){ { "adnumber", 3} }; 
		LeanLoader.load("http://dentedpixel.com/assets/advertising_json.php", new LLOptions().setOnLoad(onAdvertisingJSONLoaded).setPostParams(postParams) );

		// Load a profile image from Facebook https://graph.facebook.com/matt.lemieux.9/picture?type=large&redirect=false
		Dictionary<string, object> callbackParams = new Dictionary<string, object>(){ { "hello", "world"} };
		LeanLoader.load( "https://graph.facebook.com/DentedPixel/picture?type=large&redirect=false", new LLOptions().setOnLoad( personImageLoaded ).setUseCache(true).setCacheLife(1).setOnLoadParam( callbackParams ) );
		
		TextAsset jsonFile = (TextAsset)Resources.Load("example_json");
		LeanJSON j3 = new LeanJSON( jsonFile.text );
		LeanTest.expect(j3.Object("data")["url"].IndexOf("http")>=0, "JSON ODDLY FORMATTED" );

		// Debug.Log("before count:"+LeanLoader.count+" time:"+Time.time);

		StartCoroutine( timeBasedTesting() );
	}

	IEnumerator timeBasedTesting(){
		// Load a bunch of cached images with cache expiration of 1 second
		LeanLoader.load("http://ccutters.com/wp-content/uploads/2013/10/crazyBus.jpg", new LLOptions().setUseCache(true).setCacheLife(5).setOnLoad(normalImageLoaded));
		LeanLoader.load("http://ccutters.com/wp-content/uploads/2013/10/attic-antics.jpg", new LLOptions().setUseCache(true).setCacheLife(5).setOnLoad(normalImageLoaded));
		LeanLoader.load("http://ccutters.com/wp-content/uploads/2013/10/energy-dom.jpg", new LLOptions().setUseCache(true).setCacheLife(1).setOnLoad(normalImageLoaded));
		LeanLoader.load("http://carboncutters.herokuapp.com/img/MediumIcon175.jpg", new LLOptions().setUseCache(true).setCacheLife(1).setOnLoad(normalImageLoaded));
		
		yield return new WaitForSeconds(3);
		int beforeCount = LeanLoader.count;
		// Debug.Log("before count:"+LeanLoader.count+" time:"+Time.time);

		// 2 seconds later, load 5 more unique images
		LeanLoader.load("http://ccutters.com/wp-content/uploads/2013/10/energy-dom.jpg?unique=5", new LLOptions().setUseCache(true).setCacheLife(1));
		LeanLoader.load("http://ccutters.com/wp-content/uploads/2013/10/attic-antics.jpg?unique=6", new LLOptions().setUseCache(true).setCacheLife(1));
		
		yield return new WaitForSeconds(1); // hold up for 1 second has to make sure the background thread has started
		// Debug.Log("after count:"+LeanLoader.count+" time:"+Time.time);
		LeanTest.expect(LeanLoader.count < beforeCount, "CACHED IMAGES DELETED", "expected count:"+LeanLoader.count+" beforeCount:"+beforeCount);

		// Load image telling it to cache to the file i/o
		LeanLoader.load("http://dentedpixel.com/assets/Monkeyshines.png", new LLOptions().setOnLoad(normalImageLoaded).setUseFile(true).setUseCache(true).setCacheLife(60));
		yield return new WaitForSeconds(2);
		// 2 seconds later call that same image making sure it has loaded from the disk
		LeanLoader.load("http://dentedpixel.com/assets/Monkeyshines.png", new LLOptions().setOnLoad(didLoadFromFileCache).setUseFile(true).setUseCache(true).setCacheLife(60));
	}

	void didLoadFromFileCache( Texture2D tex ){
		LeanTest.expect(tex!=null, "FILE I/O CACHE");
	}

	void normalImageLoaded( Texture2D tex ){

	}

	void personImageLoaded( Texture2D tex, Dictionary<string,object> hash ){
		LeanTest.expect( tex!=null, "FACEBOOK PROFILE IMAGE" );
		facebookProfileImage = tex;
	}

	void onAdvertisingJSONLoaded( LeanJSON json ){
		// Debug.Log("json:"+json);
		LeanTest.expect(json.Array("ads").Length>=0 && json.Array("ads")[0]["link"].Length>=0, "JSON WEBSERVER LOAD" );
		LeanTest.expect(json.Object("ads").String("link").IndexOf("http")>=0, "WEBSERVER POSTING" );
	}


	void OnGUI(){
		if(facebookProfileImage)
			GUI.DrawTexture( new Rect(0.0f,0.0f,Screen.width*0.2f,Screen.width*0.2f), facebookProfileImage);
	}
	
}
