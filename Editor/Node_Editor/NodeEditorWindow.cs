using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

using NodeEditorFramework;
using NodeEditorFramework.core;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework
{
	public class NodeEditorWindow : EditorWindow 
	{
		// Singleton design pattern
		private static NodeEditorWindow instance;

		#region Menu entries
		/// <summary>
		/// Spawns an editor window
		/// </summary>
		[MenuItem ("Window/Node Editor")]
		public static void CreateEditor ()
		{
			instance = GetWindow<NodeEditorWindow> ();
			//Loads the icon
			iconTexture = ResourceManager.LoadTexture (EditorGUIUtility.isProSkin? "Textures/Icon_Dark.png" : "Textures/Icon_Light.png");
			//Set window title
			instance.titleContent = new GUIContent ("Node Editor", iconTexture);
		}
			
		/// <summary>
		/// Create a new canvas
		/// </summary>
		[MenuItem ("Node Editor/Create new canvas")]
		public static void newCanvas () {
			if (instance == null)
				NodeEditorWindow.CreateEditor ();
			instance.NewNodeCanvas ();
		}

		/// <summary>
		/// Loads a canvas.
		/// </summary>
		[MenuItem ("Node Editor/Load canvas")]
		public static void LoadCanvas () {
			if (instance == null)
				NodeEditorWindow.CreateEditor ();
			string path = EditorUtility.OpenFilePanel ("Load Node Canvas", NodeEditor.editorPath + "Resources/Saves/", "asset");
			if (!path.Contains (Application.dataPath)) {
				if (!string.IsNullOrEmpty (path))
					instance.ShowNotification (new GUIContent ("You should select an asset inside your project folder!"));
			} else {
				path = path.Replace (Application.dataPath, "Assets");
				instance.LoadNodeCanvas (path);
			}
		}

		/*[MenuItem ("Node Editor/Save canvas")]
		public static void SaveCanvas () {
			_editor.NewNodeCanvas ();
		}*/
		
		/// <summary>
		/// Handles opening canvas when double-clicking asset
		/// </summary>
		[UnityEditor.Callbacks.OnOpenAsset(1)]
		public static bool AutoOpenCanvas (int instanceID, int line) 
		{
			if (Selection.activeObject != null && Selection.activeObject.GetType () == typeof(NodeCanvas))
			{
				if (instance == null)
					NodeEditorWindow.CreateEditor ();
				string NodeCanvasPath = AssetDatabase.GetAssetPath (instanceID);
				instance.LoadNodeCanvas (NodeCanvasPath);
				return true;
			}
			return false;
		}

		#endregion


		#region General
		// Opened Canvas
		public NodeEditor nodeEditor;

		// GUI
		private Texture iconTexture;
		private Rect sideWindowRect = new Rect (0, 0, 250, 0);
		private enum SnapSide{
			None,
			Left,
			Right
		}
		private SnapSide snap;
		//public Rect canvasWindowRect { get { return new Rect (0, 0, position.width, position.height); } }


		public void OnDestroy () 
		{
			//NodeEditor.ClientRepaints -= instance.Repaint;
			//SaveCache ();

	#if UNITY_EDITOR
			// Remove callbacks
//			EditorLoadingControl.beforeEnteringPlayMode -= SaveCache;
//			EditorLoadingControl.lateEnteredPlayMode -= LoadCache;
			//EditorLoadingControl.beforeLeavingPlayMode -= SaveCache;
//			EditorLoadingControl.justLeftPlayMode -= LoadCache;
//			EditorLoadingControl.justOpenedNewScene -= LoadCache;

//			NodeEditorCallbacks.OnAddNode -= SaveNewNode;
//			NodeEditorCallbacks.OnAddTransition -= SaveNewTransition;

			// TODO: BeforeOpenedScene to save Cache, aswell as assembly reloads... 
	#endif
		}

		// Following section is all about caching the last editor session

		private void OnEnable () 
		{
			tempSessionPath = Path.GetDirectoryName (AssetDatabase.GetAssetPath (MonoScript.FromScriptableObject (this)));
	/*		LoadCache ();

	#if UNITY_EDITOR
			// This makes sure the Node Editor is reinitiated after the Playmode changed
			//EditorLoadingControl.beforeEnteringPlayMode -= SaveCache;
			//EditorLoadingControl.beforeEnteringPlayMode += SaveCache;
			EditorLoadingControl.lateEnteredPlayMode -= LoadCache;
			EditorLoadingControl.lateEnteredPlayMode += LoadCache;

			//EditorLoadingControl.beforeLeavingPlayMode -= SaveCache;
			//EditorLoadingControl.beforeLeavingPlayMode += SaveCache;
			EditorLoadingControl.justLeftPlayMode -= LoadCache;
			EditorLoadingControl.justLeftPlayMode += LoadCache;

			EditorLoadingControl.justOpenedNewScene -= LoadCache;
			EditorLoadingControl.justOpenedNewScene += LoadCache;

			NodeEditorCallbacks.OnAddNode -= SaveNewNode;
			NodeEditorCallbacks.OnAddNode += SaveNewNode;
	//		NodeEditorCallbacks.OnAddTransition -= SaveNewTransition;
	//		NodeEditorCallbacks.OnAddTransition += SaveNewTransition;

			// TODO: BeforeOpenedScene to save Cache, aswell as assembly reloads... 
	#endif*/
		}

		#endregion






		#region GUI

		private void OnGUI () 
		{
			// Specify the Canvas rect in the EditorState

			nodeEditor.canvasRect = canvasWindowRect;
			// If you want to use GetRect:
//			Rect canvasRect = GUILayoutUtility.GetRect (600, 600);
//			if (Event.current.type != EventType.Layout)
//				mainEditorState.canvasRect = canvasRect;

			// Perform drawing with error-handling
			try	{
				NodeEditor.DrawCanvas (mainNodeCanvas, mainEditorState);
			} catch (Exception e) { 
				// on exceptions in drawing flush the canvas to avoid locking the ui.
				NewNodeCanvas ();
				NodeEditor.ReInit (true);
				Debug.LogError ("Unloaded Canvas due to exception when drawing!");
				Debug.LogException (e);
			}

			// Draw Side Window
			NodeEditorGUI.StartNodeGUI ();

			BeginWindows();
			sideWindowRect = GUILayout.Window (1, sideWindowRect, DrawSideWindow, new GUIContent ("Node Editor (" + mainNodeCanvas.name + ")", "Opened Canvas path: " + openedCanvasPath));
			//Snapping to sides
			if (sideWindowRect.x + sideWindowRect.width >= position.width) {
				snap = SnapSide.Right;
			} else if (sideWindowRect.x <= 0) {
				snap = SnapSide.Left;
			} else {
				snap = SnapSide.None;
			}
			switch (snap) {
				case SnapSide.Right:
					sideWindowRect = new Rect (position.width - sideWindowRect.width, 0, sideWindowRect.width, position.height);
					break;
				case SnapSide.Left:
					sideWindowRect = new Rect (0, 0, sideWindowRect.width, position.height);
					break;
				case SnapSide.None:
					sideWindowRect.height = 0;
					break;
			}
			EndWindows();
			NodeEditorGUI.EndNodeGUI ();
		}

		private void DrawSideWindow (int unusedID) {		
			/*if (GUILayout.Button (new GUIContent ("Save Canvas", "Saves the Canvas to a Canvas Save File in the Assets Folder")))
			{
				string path = EditorUtility.SaveFilePanelInProject ("Save Node Canvas", "Node Canvas", "asset", "", NodeEditor.editorPath + "Resources/Saves/");
				if (!string.IsNullOrEmpty (path))
					SaveNodeCanvas (path);
			}

			if (GUILayout.Button (new GUIContent ("Load Canvas", "Loads the Canvas from a Canvas Save File in the Assets Folder"))) 
			{
				string path = EditorUtility.OpenFilePanel ("Load Node Canvas", NodeEditor.editorPath + "Resources/Saves/", "asset");
				if (!path.Contains (Application.dataPath)) 
				{
					if (!string.IsNullOrEmpty (path))
						ShowNotification (new GUIContent ("You should select an asset inside your project folder!"));
				}
				else
				{
					path = path.Replace (Application.dataPath, "Assets");
					LoadNodeCanvas (path);
				}
			}

			if (GUILayout.Button (new GUIContent ("New Canvas", "Loads an empty Canvas")))
				NewNodeCanvas ();
			*/
			if (GUILayout.Button (new GUIContent ("Recalculate All", "Initiates complete recalculate. Usually does not need to be triggered manually.")))
				NodeEditor.RecalculateAll (this.mainNodeCanvas);

			if (GUILayout.Button ("Force Re-Init"))
				NodeEditor.ReInit (true);

			if (NodeEditor.isTransitioning (mainNodeCanvas) && GUILayout.Button ("Stop Transitioning"))
				NodeEditor.StopTransitioning (mainNodeCanvas);

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label(new GUIContent ("Handle size", "The size of the Node Input/Output handles."),GUILayout.Width(80));
			NodeEditorGUI.knobSize = EditorGUILayout.IntSlider (NodeEditorGUI.knobSize, 12, 20);
			EditorGUILayout.EndHorizontal ();


			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label(new GUIContent("Zoom", "Use the mousewheel, seriously"),GUILayout.Width(80));
			mainEditorState.zoom = EditorGUILayout.Slider(mainEditorState.zoom, 0.6f, 2.0f);
			EditorGUILayout.EndHorizontal ();
		
				


            if (mainEditorState.selectedNode != null)
                if (Event.current.type != EventType.Ignore)
                    mainEditorState.selectedNode.DrawNodePropertyEditor();
			
			GUI.DragWindow ();
        }

		#endregion

		#region Cache

		private void SaveNewNode (Node node) 
		{
			if (!mainNodeCanvas.nodes.Contains (node))
				throw new UnityException ("Cache system: Writing new Node to save file failed as Node is not part of the Cache!");
			string path = tempSessionPath + "/LastSession.asset";
			if (AssetDatabase.GetAssetPath (mainNodeCanvas) != path)
				throw new UnityException ("Cache system error: Current Canvas is not saved as the temporary cache!");
			NodeEditorSaveManager.AddSubAsset (node, path);
			for (int knobCnt = 0; knobCnt < node.knobs.Count; knobCnt++)
				NodeEditorSaveManager.AddSubAsset (node.knobs [knobCnt], path);
			/*for (int transCnt = 0; transCnt < node.transitions.Count; transCnt++)
			{
				if (node.transitions[transCnt].startNode == node)
					NodeEditorSaveManager.AddSubAsset (node.transitions [transCnt], path);
			}*/

			AssetDatabase.SaveAssets ();
			AssetDatabase.Refresh ();
		}

		/*private void SaveNewTransition (Transition transition) 
		{
			if (!mainNodeCanvas.nodes.Contains (transition.startNode) || !mainNodeCanvas.nodes.Contains (transition.endNode))
				throw new UnityException ("Cache system: Writing new Transition to save file failed as Node members are not part of the Cache!");
			string path = tempSessionPath + "/LastSession.asset";
			if (AssetDatabase.GetAssetPath (mainNodeCanvas) != path)
				throw new UnityException ("Cache system error: Current Canvas is not saved as the temporary cache!");
			NodeEditorSaveManager.AddSubAsset (transition, path);

			AssetDatabase.SaveAssets ();
			AssetDatabase.Refresh ();
		}*/

		private void SaveCache () 
		{
			//DeleteCache (); // Delete old cache
			string canvasName = mainNodeCanvas.name;
			EditorPrefs.SetString ("NodeEditorLastSession", canvasName);
			NodeEditorSaveManager.SaveNodeCanvas (tempSessionPath + "/LastSession.asset", false, mainNodeCanvas, mainEditorState);
			mainNodeCanvas.name = canvasName;

			AssetDatabase.SaveAssets ();
			AssetDatabase.Refresh ();
		}

		private void LoadCache () 
		{
			string lastSessionName = EditorPrefs.GetString ("NodeEditorLastSession");
			string path = tempSessionPath + "/LastSession.asset";
			mainNodeCanvas = NodeEditorSaveManager.LoadNodeCanvas (path, false);
			if (mainNodeCanvas == null)
				NewNodeCanvas ();
			else 
			{
				mainNodeCanvas.name = lastSessionName;
				List<NodeEditorState> editorStates = NodeEditorSaveManager.LoadEditorStates (path, false);
				if (editorStates == null || editorStates.Count == 0 || (mainEditorState = editorStates.Find (x => x.name == "MainEditorState")) == null )
				{ // New NodeEditorState
					mainEditorState = CreateInstance<NodeEditorState> ();
					mainEditorState.canvas = mainNodeCanvas;
					mainEditorState.name = "MainEditorState";
					NodeEditorSaveManager.AddSubAsset (mainEditorState, path);
					AssetDatabase.SaveAssets ();
					AssetDatabase.Refresh ();
				}
			}
		}

		private void DeleteCache () 
		{
			string lastSession = EditorPrefs.GetString ("NodeEditorLastSession");
			if (!String.IsNullOrEmpty (lastSession))
			{
				AssetDatabase.DeleteAsset (tempSessionPath + "/" + lastSession);
				AssetDatabase.Refresh ();
			}
			EditorPrefs.DeleteKey ("NodeEditorLastSession");
		}

		#endregion

		#region Save/Load
		
		/// <summary>
		/// Saves the mainNodeCanvas and it's associated mainEditorState as an asset at path
		/// </summary>
		private void SaveNodeCanvas (string path) 
		{
			NodeEditorSaveManager.SaveNodeCanvas (path, true, NodeEditor.curEditorState, NodeEditor.curNodeCanvas);
			//SaveCache ();
			Repaint ();
		}
		
		/// <summary>
		/// Loads the mainNodeCanvas and it's associated mainEditorState from an asset at path
		/// </summary>
		private void LoadNodeCanvas (string path) 
		{
			// Else it will be stuck forever
			NodeEditor.StopTransitioning (mainNodeCanvas);

			// Load the NodeCanvas
			mainNodeCanvas = NodeEditorSaveManager.LoadNodeCanvas (path, true);
			if (mainNodeCanvas == null) 
			{
				Debug.Log ("Error loading NodeCanvas from '" + path + "'!");
				NewNodeCanvas ();
				return;
			}
			
			// Load the associated MainEditorState
			List<NodeEditorState> editorStates = NodeEditorSaveManager.LoadEditorStates (path, true);
			if (editorStates.Count == 0) 
			{
				mainEditorState = ScriptableObject.CreateInstance<NodeEditorState> ();
				Debug.LogError ("The save file '" + path + "' did not contain an associated NodeEditorState!");
			}
			else 
			{
				mainEditorState = editorStates.Find (x => x.name == "MainEditorState");
				if (mainEditorState == null) mainEditorState = editorStates[0];
			}
			mainEditorState.canvas = mainNodeCanvas;

			openedCanvasPath = path;
			NodeEditor.RecalculateAll (mainNodeCanvas);
			SaveCache ();
			Repaint ();
		}

		/// <summary>
		/// Creates and opens a new empty node canvas
		/// </summary>
		private void NewNodeCanvas () 
		{
			// Else it will be stuck forever
			NodeEditor.StopTransitioning (mainNodeCanvas);

			// New NodeCanvas
			mainNodeCanvas = CreateInstance<NodeCanvas> ();
			mainNodeCanvas.name = "New Canvas";
			// New NodeEditorState
			mainEditorState = CreateInstance<NodeEditorState> ();
			mainEditorState.canvas = mainNodeCanvas;
			mainEditorState.name = "MainEditorState";

			openedCanvasPath = "";
			SaveCache ();
		}
		
		#endregion
	}
}