using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MaterialUI;
using UnityEngine.UI;
using Unity.Linq;
using UnityEngine.UI.ProceduralImage;

public class OPColor : MonoBehaviour {
	[SerializeField]private Text text;

	public bool ColorEnabled {
		set {
			var color = value ? MaterialColor.cyanA200 : MaterialColor.grey600;
			var gos = gameObject.ChildrenAndSelf();

			gos.OfComponent<VectorImage>().ForEach((image) => {
				image.color = color;
			});	

			gos.OfComponent<ProceduralImage>().ForEach((image) => {
				image.color = color;
			});

			if(text != null) {
				text.color = color;
			}
		}
	} 	
}
