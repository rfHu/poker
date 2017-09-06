using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace RWDTools
{
	public class GradientToolEditorWindow : EditorWindow
	{
        enum SupportedGradientTypes
        {
            Linear,
            Quad,
            Radial
        }

        enum SupportedFileTypes
		{
			PNG,
			JPEG
		}

		static SupportedFileTypes saveType = (SupportedFileTypes) 0;
		static SupportedFileTypes SaveType {
			get {
				return saveType; }
			set {
				saveType = value;
				EditorPrefs.SetInt("GT_SaveType", (int) saveType);
			}
		}

		static SupportedGradientTypes gradientType = (SupportedGradientTypes) 0;
		static SupportedGradientTypes GradientType
		{
			get
			{
				return gradientType;
			}
			set
			{
				if (gradientType != value)
				{
					gradientType = value;
					if (AutoUpdatePreview)
					{
						UpdatePreview();
					}
					EditorPrefs.SetInt("GT_GradientType", (int) gradientType);
				}
			}
		}

        static FilterMode filterMode = FilterMode.Trilinear;
        static FilterMode FilterMode
        {
            get { return filterMode; }
            set
            {
                if (filterMode != value)
                {
                    filterMode = value;
                    EditorPrefs.SetInt("GT_FilterMode", (int)filterMode);
                    if (AutoUpdatePreview)
                    {
                        UpdatePreview();
                    }
                }
            }
        }

        static bool previewExpanded = true;
		static bool PreviewExpanded
		{
			get { return previewExpanded; }
			set { previewExpanded = value; EditorPrefs.SetBool("GT_PreviewExpanded", previewExpanded); }
		}

		static bool autoUpdatePreview = true;
		static bool AutoUpdatePreview
		{
			get { return autoUpdatePreview; }
			set { if (autoUpdatePreview != value) { autoUpdatePreview = value; EditorPrefs.SetBool("GT_AutoUpdatePreview", autoUpdatePreview); if (autoUpdatePreview) { UpdatePreview(); } } }
		}

        static bool useHSV = false;

        static bool showPO2Warning = false;

        static LinearGradient prevLinearGradient;
		static LinearGradient linearGradient = LinearGradient.Default; 
		static LinearGradient LinearGradient
		{
			get { return linearGradient; }
			set
            {
                if (GradientToolEditorUtility.Compare(prevLinearGradient, value))
                {
                    linearGradient = value;
                    prevLinearGradient = CopyLinearGradient(value);
                    SaveLinearGrad();

                    if (AutoUpdatePreview)
                        UpdatePreview();
                }
            }
		}

        static QuadGradient prevQuadGradient;
		static QuadGradient quadGradient = QuadGradient.Default;
		static QuadGradient QuadGradient
		{
			get { return quadGradient; }
			set {
				if (GradientToolEditorUtility.Compare(prevQuadGradient,value))
				{
					quadGradient = value;
					prevQuadGradient = value;
					SaveQuadGrad();

					if (AutoUpdatePreview)
					    UpdatePreview();
				}
			}
		}

        static RadialGradient prevRadialGradient;
        static RadialGradient radialGradient = RadialGradient.Default;
        static RadialGradient RadialGradient
        {
            get { return radialGradient; }
            set
            {
                if (GradientToolEditorUtility.Compare(prevRadialGradient, value))
                {
                    radialGradient = value;
                    prevRadialGradient = value;
                    SaveRadialGradient();

                    if (AutoUpdatePreview)
                        UpdatePreview();
                }
            }
        }

        static Texture2D preview;
		static Texture2D Preview {
			get { return preview; }
			set { preview = value; }
		}

		static Vector2 scrollPosition;
		static Vector2 ScrollPosition
		{
			get { return scrollPosition; }
			set { scrollPosition = value; }
		}

		static Vector2 gradientSize;
		public static Vector2 GradientSize
		{
			get
			{
				gradientSize.x = Mathf.Clamp(gradientSize.x, 16, 2048);
				gradientSize.y = Mathf.Clamp(gradientSize.y, 16, 2048);
				return gradientSize;
			}

			set
			{
				if (gradientSize != value)
				{

					gradientSize.x = Mathf.Clamp(gradientSize.x, 16, 2048);
					gradientSize.y = Mathf.Clamp(gradientSize.y, 16, 2048);

					gradientSize = new Vector2((int)value.x, (int)value.y);

					EditorPrefs.SetInt("GT_SizeX", (int)gradientSize.x);
					EditorPrefs.SetInt("GT_SizeY", (int)gradientSize.y);

					if (!((int)gradientSize.x).isPowerOfTwo() || !((int)gradientSize.y).isPowerOfTwo())
					{
						showPO2Warning = true; 
					}
					else
					{
						showPO2Warning = false; 
					}

					if (AutoUpdatePreview)
					{
						UpdatePreview();
					}
				}
			}
		}

        [MenuItem("Window/Gradient Tool")]
        static void OpenPopup()
        {
            GradientToolWindow = (GradientToolEditorWindow)(EditorWindow.GetWindow(typeof(GradientToolEditorWindow)));

            Vector2 minSize = new Vector2(300, 200);
            Vector2 maxSize = new Vector2(300, 600);

            GradientToolWindow.minSize = minSize;
            GradientToolWindow.maxSize = maxSize;
#if UNITY_5_3_OR_NEWER
            GradientToolWindow.titleContent = new GUIContent("Gradient Tool");
#else
			GradientToolWindow.title = "Gradient Tool";
#endif

            GradientToolWindow.ShowPopup();
        }

        static GradientToolEditorWindow GradientToolWindow;

        void OnEnable()
        {
            LoadEditorPrefs();
            UpdatePreview();
        }

        void LoadEditorPrefs()
        {

            saveType = (SupportedFileTypes)EditorPrefs.GetInt("GT_SaveType");
            previewExpanded = EditorPrefs.GetBool("GT_PreviewExpanded");
            autoUpdatePreview = EditorPrefs.GetBool("GT_AutoUpdatePreview");
            filterMode = (FilterMode)EditorPrefs.GetInt("GT_FilterMode");
            GradientType = (SupportedGradientTypes)EditorPrefs.GetInt("GT_GradientType");
            gradientSize = new Vector2(EditorPrefs.GetInt("GT_SizeX"), EditorPrefs.GetInt("GT_SizeY"));

            if (EditorPrefs.GetInt("GT_SizeX") == 0)
            {
                gradientSize = new Vector2(64, 64);
            }

            LoadLinearGrad();
            LoadQuadGrad();
            LoadRadialGrad();
        }

		static Texture2D GetActiveGradientTexture()
		{
            switch (GradientType)
			{
				case SupportedGradientTypes.Linear:
					return GradientToolUtility.GenerateLinearGradientTexture(LinearGradient, (int)GradientSize.x, (int)GradientSize.y, FilterMode);
				case SupportedGradientTypes.Quad:
					return GradientToolUtility.GenerateQuadGradientTexture(QuadGradient, (int)GradientSize.x, (int)GradientSize.y, FilterMode);
                case SupportedGradientTypes.Radial:
                    return GradientToolUtility.GenerateRadialGradientTexture(RadialGradient, (int)GradientSize.x, (int)GradientSize.y, FilterMode);
			}

			return null;
		}

		static byte[] EncodeGradient(SupportedFileTypes fileType)
		{
            switch (fileType)
			{
				case SupportedFileTypes.PNG:
					return GetActiveGradientTexture().EncodeToPNG();
				case SupportedFileTypes.JPEG:
					return GetActiveGradientTexture().EncodeToJPG(11);
				default:
					return GetActiveGradientTexture().EncodeToPNG();
			}
		}

        static LinearGradient CopyLinearGradient(LinearGradient linearGradient)
        {
            Gradient gradient = new Gradient();
            gradient.colorKeys = linearGradient.Gradient.colorKeys;
            gradient.alphaKeys = linearGradient.Gradient.alphaKeys;
            LinearGradient returnGradient = new LinearGradient(gradient, linearGradient.Angle);
            return returnGradient;
        }

        #region Runtime

        static void CreateRuntimeGradientAsset(string path)
        {
            SupportedGradientTypes activeType = (SupportedGradientTypes)GradientType;

            switch (activeType)
            {
                case SupportedGradientTypes.Linear:
                    GTLinearGradient newLinearGradient = ScriptableObject.CreateInstance<GTLinearGradient>();

                    newLinearGradient.LinearGradient.Gradient = LinearGradient.Gradient;
                    newLinearGradient.LinearGradient.Angle = LinearGradient.Angle;

                    newLinearGradient.Size = GradientSize;
                    newLinearGradient.FilterMode = FilterMode;

                    CreateAndSaveAsset(newLinearGradient, path);
                    break;
                case SupportedGradientTypes.Quad:
                    GTQuadGradient newQuadGradient = ScriptableObject.CreateInstance<GTQuadGradient>();

                    newQuadGradient.QuadGradient = QuadGradient;

                    newQuadGradient.Size = GradientSize;
                    newQuadGradient.FilterMode = FilterMode;

                    CreateAndSaveAsset(newQuadGradient, path);
                    break;
                case SupportedGradientTypes.Radial:
                    GTRadialGradient newRadialGradient = ScriptableObject.CreateInstance<GTRadialGradient>();

                    newRadialGradient.RadialGradient = RadialGradient;

                    newRadialGradient.Size = GradientSize;
                    newRadialGradient.FilterMode = FilterMode;

                    CreateAndSaveAsset(newRadialGradient, path);
                    break;
            }
        }

        static void CreateAndSaveAsset(UnityEngine.Object asset, string path)
        {

            AssetDatabase.CreateAsset(asset, "Assets/" + GetRelativePath(path, Application.dataPath));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new Uri(filespec);

            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        #endregion

        #region GUI

        void LinearGradientSettings()
        {
            LinearGradient newLinearGradient = LinearGradient;

            GUILayout.BeginHorizontal(GUILayout.Height(22));
            GUILayout.FlexibleSpace();
            GUILayout.Label("Angle", RWDStyles.MiddleLeftLabel);

            GUILayout.Label(RWDStyles.Disc);
            Rect discRect = GUILayoutUtility.GetLastRect();

            DrawLine(discRect.center, discRect.center + ((newLinearGradient.Angle - 90).DegToVector() * 13), Color.gray, 1, true);

            if (Event.current.type == EventType.mouseDrag && (Vector2.Distance(Event.current.mousePosition, discRect.center) < 20))
            {
                newLinearGradient.Angle = ReCalculateAngle(discRect, Event.current.mousePosition);
                GUI.FocusControl(null);
                Event.current.Use();
            }

            newLinearGradient.Angle = EditorGUILayout.FloatField(newLinearGradient.Angle, RWDStyles.SmallFloatField, GUILayout.Width(80));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            Gradient tempGrad = GradientToolEditorUtility.DrawField("", newLinearGradient.Gradient);
            newLinearGradient.Gradient.colorKeys = tempGrad.colorKeys;
            newLinearGradient.Gradient.alphaKeys = tempGrad.alphaKeys;
            
            LinearGradient = newLinearGradient;
        }

        void QuadGradientSettings()
        {
            GUILayout.BeginHorizontal(GUILayout.Height(22));
            GUILayout.FlexibleSpace();

            QuadGradient newQuadGradient = QuadGradient;

            GUILayout.BeginVertical();
            newQuadGradient.TopLeft = EditorGUILayout.ColorField(newQuadGradient.TopLeft);
            newQuadGradient.BottomLeft = EditorGUILayout.ColorField(newQuadGradient.BottomLeft);
            GUILayout.EndVertical();

            GUILayout.Label(RWDStyles.QuadGrad);

            GUILayout.BeginVertical();
            newQuadGradient.TopRight = EditorGUILayout.ColorField(QuadGradient.TopRight);
            newQuadGradient.BottomRight = EditorGUILayout.ColorField(QuadGradient.BottomRight);

            GUILayout.EndVertical();

            QuadGradient = newQuadGradient;

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        void RadialGradientSettings()
        {
            RadialGradient newRadialGradient = RadialGradient;
            GUILayout.BeginHorizontal();
            newRadialGradient.Inner = EditorGUILayout.ColorField("Inner Color", newRadialGradient.Inner);
            newRadialGradient.Outer = EditorGUILayout.ColorField("Outer Color", newRadialGradient.Outer);
            GUILayout.EndHorizontal();

            newRadialGradient.Scale = EditorGUILayout.FloatField("Scale", newRadialGradient.Scale);

            newRadialGradient.Anchor = EditorGUILayout.Vector2Field("Anchor Offset", newRadialGradient.Anchor);
            useHSV = EditorGUILayout.ToggleLeft("Use HSV", useHSV);
            newRadialGradient.UseHSV = useHSV;

            RadialGradient = newRadialGradient;
        }

		void DrawGradientSettings(SupportedGradientTypes gradientType)
		{
			switch (gradientType)
			{
				case SupportedGradientTypes.Linear:
                    LinearGradientSettings();
                break;
				case SupportedGradientTypes.Quad:
                    QuadGradientSettings();
		        break;
                case SupportedGradientTypes.Radial:
                    RadialGradientSettings();
                break;
			}
		}

		void OnGUI()
		{
			GUILayout.Label("Gradient Tool", RWDStyles.Heading);

			ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, GUIStyle.none);

			GUILayout.BeginVertical(RWDStyles.Section);

			GradientType = (SupportedGradientTypes) GUILayout.Toolbar((int) GradientType, Enum.GetNames(typeof(SupportedGradientTypes)));
			EditorGUILayout.Space();
			DrawGradientSettings(GradientType);
			EditorGUILayout.Space();
			GUILayout.EndVertical();
		
			GUILayout.BeginVertical(RWDStyles.Section);

			GUIContent CreateTitle = new GUIContent("Creation", "Creation \n\nUse this section to create new gradient based images, which you can save to your project and use in your project.");
			GUILayout.Label(CreateTitle, RWDStyles.SubHeading);

            GUILayout.BeginHorizontal(GUIStyle.none);
			GUIContent SizeTitle = new GUIContent("Size", "Size \n\nThe size, in pixels, of the output gradient.");
			GUILayout.Label(SizeTitle, RWDStyles.SubHeading, GUILayout.Width(40));
			GradientSize = EditorGUILayout.Vector2Field("", new Vector2((int)GradientSize.x, (int)GradientSize.y), GUILayout.Height(12));
			GUILayout.EndHorizontal();

			SaveType = ((SupportedFileTypes) EditorGUILayout.EnumPopup("Output Type",  SaveType));
			FilterMode = (FilterMode) EditorGUILayout.EnumPopup("Filter Mode",FilterMode);

			EditorGUILayout.Space();
			if (GUILayout.Button("Save Gradient Texture", RWDStyles.Button))
			{
				string savePath = EditorUtility.SaveFilePanel("Save Active Gradient", Application.dataPath, ((SupportedGradientTypes) GradientType) + " Gradient", (((SupportedFileTypes)SaveType)).ToString().ToLower());
				if (!string.IsNullOrEmpty(savePath))
				{
					File.WriteAllBytes(savePath, EncodeGradient(SaveType));
					AssetDatabase.Refresh();
				}
			}

			if (GUILayout.Button("Save Runtime Gradient Asset", RWDStyles.Button))
			{
				string savePath = EditorUtility.SaveFilePanel("Save Runtime Gradient Asset", Application.dataPath, ((SupportedGradientTypes)GradientType) + " Gradient", "asset");
				if (!string.IsNullOrEmpty(savePath))
				{
					EditorApplication.delayCall += () => CreateRuntimeGradientAsset(savePath);
				}
			}

			GUILayout.EndVertical();

			GUILayout.BeginVertical(RWDStyles.Section);

			GUILayout.BeginHorizontal(GUIStyle.none);
			PreviewExpanded = GUILayout.Toggle(PreviewExpanded, "", RWDStyles.Foldout, GUILayout.Width(12));

			GUIContent PreviewTitle = new GUIContent("Preview", "Preview \n\nView your gradient image before you save.");
			GUILayout.Label(PreviewTitle, RWDStyles.SubHeading);
			GUILayout.FlexibleSpace();
			if (PreviewExpanded)
			{
				GUILayout.Label("Auto-Update", RWDStyles.SmallBody);
				AutoUpdatePreview = GUILayout.Toggle(AutoUpdatePreview, "");
			}
			GUILayout.EndHorizontal();

			if (PreviewExpanded)
			{
				if (!AutoUpdatePreview)
				{
					if (GUILayout.Button("Update", RWDStyles.Button))
					{
						UpdatePreview();
					}
				}
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (Preview != null)
				{
                    float aspect = (float)Preview.width / (float)Preview.height;
                    float x = Mathf.Min(256, Mathf.Min(aspect * Mathf.Min(256, preview.height), preview.width));
                    float y = Mathf.Min(256, Mathf.Min((1f/aspect) * Mathf.Min(256, preview.width), preview.height));

                    GUILayout.Label("", RWDStyles.MidBox, GUILayout.Width(x), GUILayout.Height(y));
					Rect previewRect = GUILayoutUtility.GetLastRect();


					Rect alphaRect = new Rect(previewRect.x + 5, previewRect.y + 5, previewRect.width - 10, previewRect.height - 10);

					GUI.DrawTextureWithTexCoords(alphaRect, RWDStyles.AlphaPattern,new Rect(0,0,alphaRect.width/16,alphaRect.height/16));

					GUI.DrawTexture(alphaRect, Preview);

					if (Preview.width > 256 || Preview.height > 256)
					{
						GUI.Box(new Rect(previewRect.x + previewRect.width - 105, previewRect.y + previewRect.height - 35, 100, 30),Preview.width + "px by " + Preview.height +"px", RWDStyles.InfoBox);
					}
                    
                }
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

                if (showPO2Warning)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    GUILayout.Label("Non Power-of-2 or 0 values detected.", RWDStyles.SmallBody);

                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
            }


			GUILayout.EndVertical();

			GUILayout.EndScrollView();

			if (Event.current.type == EventType.mouseDown)
			{
				Event.current.Use();
				GUI.FocusControl(null);
			}

			Repaint();
		}

        #endregion

        #region Utility

        static float ReCalculateAngle(Rect rect, Vector2 position)
        {
            Vector2 midPosition = new Vector2(rect.x + rect.width / 2, rect.y + rect.height / 2);
            Vector2 direction = (position - midPosition).normalized;

            return (float)(new Vector2(direction.x, -direction.y)).ToAngleDeg();
        }

        static void UpdatePreview()
        {
            Preview = GetActiveGradientTexture();
        }

        #endregion

        #region Save / Load

        static void SaveLinearGrad()
        {
            EditorPrefs.SetFloat("GT_LinearGrad_Angle", LinearGradient.Angle);

            EditorPrefs.SetInt("GT_LinearGrad_ColorKey_Count", LinearGradient.Gradient.colorKeys.Length);
            EditorPrefs.SetInt("GT_LinearGrad_AlphaKey_Count", LinearGradient.Gradient.alphaKeys.Length);

            for (int i = 0; i < LinearGradient.Gradient.colorKeys.Length; i++)
            {
                EditorPrefs.SetFloat("GT_LinearGrad_ColorKey_" + i + "_Time", LinearGradient.Gradient.colorKeys[i].time);
                SaveColor("GT_LinearGrad_ColorKey_" + i + "_Color", LinearGradient.Gradient.colorKeys[i].color);
            }
            for (int i = 0; i < LinearGradient.Gradient.alphaKeys.Length; i++)
            {
                EditorPrefs.SetFloat("GT_LinearGrad_AlphaKey_" + i + "_Time", LinearGradient.Gradient.alphaKeys[i].time);
                EditorPrefs.SetFloat("GT_LinearGrad_AlphaKey_" + i + "_Alpha", LinearGradient.Gradient.alphaKeys[i].alpha);
            }
        }

        static void LoadLinearGrad()
        {
            LinearGradient linearGrad = new LinearGradient();  

            linearGrad.Angle = EditorPrefs.GetFloat("GT_LinearGrad_Angle");

            int colorKeyCount = EditorPrefs.GetInt("GT_LinearGrad_ColorKey_Count");
            int alphaKeyCount = EditorPrefs.GetInt("GT_LinearGrad_AlphaKey_Count");

            linearGrad.Gradient = new Gradient();

            GradientColorKey[] colorKeys = new GradientColorKey[colorKeyCount];
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[alphaKeyCount]; 

            for (int i = 0; i < colorKeyCount; i++) 
            {
                colorKeys[i].time = EditorPrefs.GetFloat("GT_LinearGrad_ColorKey_" + i + "_Time");
                colorKeys[i].color = LoadColor("GT_LinearGrad_ColorKey_" + i + "_Color");
            }
            for (int i = 0; i < alphaKeyCount; i++)
            {
                alphaKeys[i].time = EditorPrefs.GetFloat("GT_LinearGrad_AlphaKey_" + i + "_Time");
                alphaKeys[i].alpha = EditorPrefs.GetFloat("GT_LinearGrad_AlphaKey_" + i + "_Alpha");
            }

            linearGrad.Gradient.SetKeys(colorKeys, alphaKeys); 

            LinearGradient = linearGrad;
        }

        static void LoadQuadGrad()
        {
            QuadGradient quadGrad = new QuadGradient();

            quadGrad.TopLeft = LoadColor("GT_QuadGrad_TopLeft");
            quadGrad.TopRight = LoadColor("GT_QuadGrad_TopRight");
            quadGrad.BottomLeft = LoadColor("GT_QuadGrad_BottomLeft");
            quadGrad.BottomRight = LoadColor("GT_QuadGrad_BottomRight");

            QuadGradient = quadGrad;
        }

        static void SaveQuadGrad()
        {
            SaveColor("GT_QuadGrad_TopLeft", QuadGradient.TopLeft);
            SaveColor("GT_QuadGrad_TopRight", QuadGradient.TopRight);
            SaveColor("GT_QuadGrad_BottomLeft", QuadGradient.BottomLeft);
            SaveColor("GT_QuadGrad_BottomRight", QuadGradient.BottomRight);
        }

        static void LoadRadialGrad()
        {
            RadialGradient radialGrad = new RadialGradient();

            radialGrad.Inner = LoadColor("GT_RadialGrad_Offset_Inner");
            radialGrad.Outer = LoadColor("GT_RadialGrad_Offset_Outer");
            radialGrad.Anchor = new Vector2(EditorPrefs.GetFloat("GT_RadialGrad_Offset_x"), EditorPrefs.GetFloat("GT_RadialGrad_Offset_y"));
            radialGrad.Scale = EditorPrefs.GetFloat("GT_RadialGrad_Scale");

            RadialGradient = radialGrad;
        }

        static void SaveRadialGradient()
        {
            EditorPrefs.SetFloat("GT_RadialGrad_Offset_x", RadialGradient.Anchor.x);
            EditorPrefs.SetFloat("GT_RadialGrad_Offset_y", RadialGradient.Anchor.y);

            SaveColor("GT_RadialGrad_Offset_Inner", RadialGradient.Inner);
            SaveColor("GT_RadialGrad_Offset_Outer", RadialGradient.Outer);

            EditorPrefs.SetFloat("GT_RadialGrad_Scale", RadialGradient.Scale);
        }

        static void SaveColor(string slot, Color col)
        {
            EditorPrefs.SetFloat(slot + "_r", col.r);
            EditorPrefs.SetFloat(slot + "_g", col.g);
            EditorPrefs.SetFloat(slot + "_b", col.b);
            EditorPrefs.SetFloat(slot + "_a", col.a);
        }

        static Color LoadColor(string slot)
        {
            Color returnCol = Color.clear;
            returnCol.r = EditorPrefs.GetFloat(slot + "_r");
            returnCol.g = EditorPrefs.GetFloat(slot + "_g");
            returnCol.b = EditorPrefs.GetFloat(slot + "_b");
            returnCol.a = EditorPrefs.GetFloat(slot + "_a");
            return returnCol;
        }

        #endregion

        #region Drawing

        private static Texture2D aaLineTex = null;
		private static Texture2D lineTex = null;
		private static Material blitMaterial = null;
		private static Material blendMaterial = null;
		private static Rect lineRect = new Rect(0, 0, 1, 1);

		public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width, bool antiAlias)
		{
			if (!lineTex)
			{
				Initialize();
			}

			float dx = pointB.x - pointA.x;
			float dy = pointB.y - pointA.y;
			float len = Mathf.Sqrt(dx * dx + dy * dy);

			if (len < 0.001f)
			{
				return;
			}

			Texture2D tex;
			Material mat;
			if (antiAlias)
			{
				width = width * 3.0f;
				tex = aaLineTex;
				mat = blendMaterial;
			}
			else
			{
				tex = lineTex;
				mat = blitMaterial;
			}

			float wdx = width * dy / len;
			float wdy = width * dx / len;

			Matrix4x4 matrix = Matrix4x4.identity;
			matrix.m00 = dx;
			matrix.m01 = -wdx;
			matrix.m03 = pointA.x + 0.5f * wdx;
			matrix.m10 = dy;
			matrix.m11 = wdy;
			matrix.m13 = pointA.y - 0.5f * wdy;

			GL.PushMatrix();
			GL.MultMatrix(matrix);
			Graphics.DrawTexture(lineRect, tex, lineRect, 0, 0, 0, 0, color, mat);
			GL.PopMatrix();
		}

		private static void Initialize()
		{
			if (lineTex == null)
			{
				lineTex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
				lineTex.SetPixel(0, 1, Color.white);
				lineTex.Apply();
			}
			if (aaLineTex == null)
			{
				// TODO: better anti-aliasing of wide lines with a larger texture? or use Graphics.DrawTexture with border settings
				aaLineTex = new Texture2D(1, 3, TextureFormat.ARGB32, false);
				aaLineTex.SetPixel(0, 0, new Color(1, 1, 1, 0));
				aaLineTex.SetPixel(0, 1, Color.white);
				aaLineTex.SetPixel(0, 2, new Color(1, 1, 1, 0));
				aaLineTex.Apply();
			}

			// GUI.blitMaterial and GUI.blendMaterial are used internally by GUI.DrawTexture,
			// depending on the alphaBlend parameter. Use reflection to "borrow" these references.
			blitMaterial = (Material)typeof(GUI).GetMethod("get_blitMaterial", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
			blendMaterial = (Material)typeof(GUI).GetMethod("get_blendMaterial", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
		}

        #endregion

    }
}
