using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace frame8.ScrollRectItemsAdapter.Editor
{
    public static class ThankYou
	{
		const string KEY_VISITED_THANK_YOU_PAGE = "frame8.ScrollRectItemsAdapter.Editor.ThankYouVisited";

		[UnityEditor.Callbacks.DidReloadScripts]
		static void Init()
		{
			if (!EditorPrefs.HasKey(KEY_VISITED_THANK_YOU_PAGE))
			{
				EditorPrefs.SetBool(KEY_VISITED_THANK_YOU_PAGE, true);
				Application.OpenURL("http://thefallengames.com/unityassetstore/optimizedscrollviewadapter/thankyou");
			}
		}
    }
}
