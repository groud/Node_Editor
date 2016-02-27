using UnityEngine;
using System.Collections;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework 
{
	public static class NodeEditorGUI 
	{
		// static GUI settings, textures and styles
		public static int knobSize = 16;

		//public static Color NE_LightColor = new Color (0.4f, 0.4f, 0.4f);
		//public static Color NE_TextColor = new Color (0.7f, 0.7f, 0.7f);

		public static Texture2D Background;
		public static Texture2D GUIBoxSelection;
		/*public static Texture2D AALineTex;
		public static Texture2D GUIBox;
		public static Texture2D GUIButton;*/


		public static GUISkin nodeSkin;
		public static GUISkin editorSkin;
		public static GUISkin defaultSkin;
		
		public static void Init() 
		{
			Background = ResourceManager.LoadTexture("Textures/background.png");
			GUIBoxSelection = ResourceManager.LoadTexture("Textures/BoxSelection.png");
			// Skin & Styles
			nodeSkin = (GUISkin) Resources.Load("GUISkins/editor");
			editorSkin = (GUISkin) Resources.Load("GUISkins/editor");
		}

		public static void StartNodeGUI () 
		{
			defaultSkin = GUI.skin;
			if (nodeSkin == null)
				Init();
			GUI.skin = nodeSkin;
		}

		public static void EndNodeGUI () 
		{
			GUI.skin = defaultSkin;
		}

		public static void StartEditorGUI () 
		{
			defaultSkin = GUI.skin;
			if (nodeSkin == null)
				Init();
			GUI.skin = nodeSkin;
		}

		public static void EndEditorGUI () 
		{
			GUI.skin = defaultSkin;
		}

		#region Connection Drawing

		/// <summary>
		/// Draws a node connection from start to end, horizontally
		/// </summary>
		public static void DrawConnection (Vector2 startPos, Vector2 endPos, Color col) 
		{
			Vector2 startVector = startPos.x <= endPos.x? Vector2.right : Vector2.left;
			DrawConnection (startPos, startVector, endPos, -startVector, col);
		}
		/// <summary>
		/// Draws a node connection from start to end with specified vectors
		/// </summary>
		public static void DrawConnection (Vector2 startPos, Vector2 startDir, Vector2 endPos, Vector2 endDir, Color col) 
		{
			#if NODE_EDITOR_LINE_CONNECTION
			DrawConnection (startPos, startDir, endPos, endDir, ConnectionDrawMethod.StraightLine, col);
			#else
			DrawConnection (startPos, startDir, endPos, endDir, ConnectionDrawMethod.Bezier, col);

			#endif
		}
		/// <summary>
		/// Draws a node connection from start to end with specified vectors
		/// </summary>
		public static void DrawConnection (Vector2 startPos, Vector2 startDir, Vector2 endPos, Vector2 endDir, ConnectionDrawMethod drawMethod, Color col) 
		{
			if (drawMethod == ConnectionDrawMethod.Bezier) 
			{
				float dirFactor = 80;//Mathf.Pow ((startPos-endPos).magnitude, 0.3f) * 20;
				//Debug.Log ("DirFactor is " + dirFactor + "with a bezier lenght of " + (startPos-endPos).magnitude);
				RTEditorGUI.DrawBezier (startPos, endPos, startPos + startDir * dirFactor, endPos + endDir * dirFactor, col * Color.gray, null, 3);
			}
			else if (drawMethod == ConnectionDrawMethod.StraightLine)
				RTEditorGUI.DrawLine (startPos, endPos, col * Color.gray, null, 3);
		}

		/// <summary>
		/// Gets the second connection vector that matches best, accounting for positions
		/// </summary>
		internal static Vector2 GetSecondConnectionVector (Vector2 startPos, Vector2 endPos, Vector2 firstVector) 
		{
			if (firstVector.x != 0 && firstVector.y == 0)
				return startPos.x <= endPos.x? -firstVector : firstVector;
			else if (firstVector.y != 0 && firstVector.x == 0)
				return startPos.y <= endPos.y? -firstVector : firstVector;
			else
				return -firstVector;
		}

		#endregion
	}
}