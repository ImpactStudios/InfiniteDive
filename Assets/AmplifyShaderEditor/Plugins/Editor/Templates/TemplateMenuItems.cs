// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>
using UnityEditor;

namespace AmplifyShaderEditor
{
	public class TemplateMenuItems
	{
		[MenuItem( "Assets/Create/Amplify Shader/Universal/PBR", false, 85 )]
		public static void ApplyTemplateUniversalPBR()
		{
			AmplifyShaderEditorWindow.CreateConfirmationTemplateShader( "94348b07e5e8bab40bd6c8a1e3df54cd" );
		}
		[MenuItem( "Assets/Create/Amplify Shader/Universal/Unlit", false, 85 )]
		public static void ApplyTemplateUniversalUnlit()
		{
			AmplifyShaderEditorWindow.CreateConfirmationTemplateShader( "2992e84f91cbeb14eab234972e07ea9d" );
		}
		[MenuItem( "Assets/Create/Amplify Shader/Universal/2D Custom Lit", false, 85 )]
		public static void ApplyTemplateUniversal2DCustomLit()
		{
			AmplifyShaderEditorWindow.CreateConfirmationTemplateShader( "ece0159bad6633944bf6b818f4dd296c" );
		}
		[MenuItem( "Assets/Create/Amplify Shader/Universal/2D Lit", false, 85 )]
		public static void ApplyTemplateUniversal2DLit()
		{
			AmplifyShaderEditorWindow.CreateConfirmationTemplateShader( "199187dac283dbe4a8cb1ea611d70c58" );
		}
		[MenuItem( "Assets/Create/Amplify Shader/Universal/2D Unlit", false, 85 )]
		public static void ApplyTemplateUniversal2DUnlit()
		{
			AmplifyShaderEditorWindow.CreateConfirmationTemplateShader( "cf964e524c8e69742b1d21fbe2ebcc4a" );
		}
		[MenuItem( "Assets/Create/Amplify Shader/Universal/Decals", false, 85 )]
		public static void ApplyTemplateUniversalDecals()
		{
			AmplifyShaderEditorWindow.CreateConfirmationTemplateShader( "c2a467ab6d5391a4ea692226d82ffefd" );
		}
	}
}
