using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Reflection;

namespace RWDTools
{
	public class GradientToolEditorUtility 
	{
		static MethodInfo linearGradientMethodField;
		static MethodInfo LinearGradientMethodField
		{
			get {
				if (linearGradientMethodField == null)
				{
					linearGradientMethodField = typeof(EditorGUILayout).GetMethod("GradientField", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(string), typeof(Gradient), typeof(GUILayoutOption[]) }, null);
				}
				return linearGradientMethodField; }
			set {
				linearGradientMethodField = value;
			}
		}

		public static Gradient DrawField(string label, Gradient gradient, params GUILayoutOption[] options)
		{
			if (gradient == null)
				gradient = new Gradient();

			gradient = (Gradient) LinearGradientMethodField.Invoke(null, new object[] { label, gradient, options });

			return gradient;
		}

		public static bool Compare(LinearGradient gradientA, LinearGradient gradientB)
		{
            if (gradientA.Gradient == null || gradientB.Gradient == null)
                return true;

            if (gradientA.Gradient.colorKeys == null || gradientB.Gradient.colorKeys == null)
                return true;

			if (gradientA.Gradient.colorKeys.Length != gradientB.Gradient.colorKeys.Length)
				return true;

			if (gradientA.Gradient.alphaKeys.Length != gradientB.Gradient.alphaKeys.Length)
				return true;

			for(int i = 0; i < gradientA.Gradient.colorKeys.Length; i++)
			{
				if (gradientA.Gradient.colorKeys[i].color != gradientB.Gradient.colorKeys[i].color)
					return true;

				if (gradientA.Gradient.colorKeys[i].time != gradientB.Gradient.colorKeys[i].time)
					return true;
			}
			for (int i = 0; i < gradientA.Gradient.alphaKeys.Length; i++)
			{
				if (gradientA.Gradient.alphaKeys[i].alpha != gradientB.Gradient.alphaKeys[i].alpha)
					return true;

				if (gradientA.Gradient.alphaKeys[i].time != gradientB.Gradient.alphaKeys[i].time)
					return true;
			}
            if (gradientA.Angle != gradientB.Angle)
                return true;

            return false;
		}

		public static bool Compare(QuadGradient gradientA, QuadGradient gradientB)
		{
			if (gradientA.TopLeft != gradientB.TopLeft)
				return true;

			if (gradientA.TopRight != gradientB.TopRight)
				return true;

			if (gradientA.BottomLeft != gradientB.BottomLeft)
				return true;

			if (gradientA.BottomRight != gradientB.BottomRight)
				return true;

			return false;
		}

        public static bool Compare(RadialGradient gradientA, RadialGradient gradientB)
        {
            if (gradientA.Anchor != gradientB.Anchor)
                return true;

            if (gradientA.Inner != gradientB.Inner)
                return true;

            if (gradientA.Outer != gradientB.Outer)
                return true;

            if (gradientA.Scale != gradientB.Scale)
                return true;

            if (gradientA.UseHSV != gradientB.UseHSV)
                return true;

            return false;
        }
    }
}
