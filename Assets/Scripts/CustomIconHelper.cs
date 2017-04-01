//  Copyright 2016 MaterialUI for Unity http://materialunity.com
//  Please see license file for terms and conditions of use, and more information.

using UnityEngine;
using System.Linq;
using MaterialUI;

	public static class CustomIconHelper
    {
		private static Font m_Font;
		private static VectorImageSet m_IconSet;
		
		static CustomIconHelper()
		{
			var fontName = "custom-icomoon";

			if (m_Font == null)
			{
				m_Font = VectorImageManager.GetIconFont(fontName);
			}
			
			if (m_IconSet == null)
			{
				m_IconSet = VectorImageManager.GetIconSet(fontName);
			}
		}

		public static ImageData GetIcon(string name)
		{
            Glyph glyph = m_IconSet.iconGlyphList.Where(x => x.name.ToLower().Equals(name.ToLower())).FirstOrDefault();
			if (glyph == null)
			{
				Debug.LogError("Could not find an icon with the name: " + name + " inside the MaterialDesign icon font");
				return null;
			}

			return new ImageData(new VectorImageData(glyph, m_Font));
		}
	}
	