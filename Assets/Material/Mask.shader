Shader "Custom/Mask" {
	Properties 
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_Mask("Mask Texture",2D)="white"{}

		// required for UI.Mask
         _StencilComp ("Stencil Comparison", Float) = 8
         _Stencil ("Stencil ID", Float) = 0
         _StencilOp ("Stencil Operation", Float) = 0
         _StencilWriteMask ("Stencil Write Mask", Float) = 255
         _StencilReadMask ("Stencil Read Mask", Float) = 255
         _ColorMask ("Color Mask", Float) = 15
	}
	SubShader 
	{
		Tags{"Queue"="Transparent"}
		Lighting On
		Zwrite off
		Blend SrcAlpha OneMinusSrcAlpha

		Stencil
         {
             Ref [_Stencil]
             Comp [_StencilComp]
             Pass [_StencilOp] 
             ReadMask [_StencilReadMask]
             WriteMask [_StencilWriteMask]
         }

		pass
		{
			SetTexture[_Mask]{combine texture}
			SetTexture[_MainTex]{combine texture,previous}
		}
	}
}