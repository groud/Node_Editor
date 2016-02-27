﻿using UnityEngine;
using System;
using System.Collections.Generic;

using NodeEditorFramework.core;

namespace NodeEditorFramework 
{
	public abstract class NodeEditorCallbackReceiver : MonoBehaviour
	{
		// Editor
		public virtual void OnEditorStartUp () {}
		// Save and Load
		public virtual void OnLoadCanvas (NodeCanvas canvas) {}
		public virtual void OnLoadEditorState (NodeEditorState editorState) {}
		public virtual void OnSaveCanvas (NodeCanvas canvas) {}
		public virtual void OnSaveEditorState (NodeEditorState editorState) {}
		// Node
		public virtual void OnAddNode (Node node) {}
		public virtual void OnDeleteNode (Node node) {}
		public virtual void OnMoveNode (Node node) {}
		// Connection
		public virtual void OnAddConnection (KnobInput input) {}
		public virtual void OnRemoveConnection (KnobInput input) {}
		// Transition
		//public virtual void OnAddTransition (Transition transition) {}
		//public virtual void OnRemoveTransition (Transition transition) {}
	}

	public static class NodeEditorCallbacks
	{
		private static int receiverCount;
		private static List<NodeEditorCallbackReceiver> callbackReceiver;

		public static void SetupReceivers () 
		{
			callbackReceiver = new List<NodeEditorCallbackReceiver> (MonoBehaviour.FindObjectsOfType<NodeEditorCallbackReceiver> ());
			receiverCount = callbackReceiver.Count;
		}

		#region Editor (1)

		public static Action OnEditorStartUp = null;
		public static void IssueOnEditorStartUp () 
		{
			if (OnEditorStartUp != null)
				OnEditorStartUp.Invoke ();
			for (int cnt = 0; cnt < receiverCount; cnt++) 
			{
				if (callbackReceiver [cnt] == null)
					callbackReceiver.RemoveAt (cnt--);
				else
					callbackReceiver [cnt].OnEditorStartUp ();
			}
		}

		#endregion

		#region Save and Load (4)

		public static Action<NodeCanvas> OnLoadCanvas;
		public static void IssueOnLoadCanvas (NodeCanvas canvas) 
		{
			if (OnLoadCanvas != null)
				OnLoadCanvas.Invoke (canvas);
			for (int cnt = 0; cnt < receiverCount; cnt++) 
			{
				if (callbackReceiver [cnt] == null)
					callbackReceiver.RemoveAt (cnt--);
				else
					callbackReceiver [cnt].OnLoadCanvas (canvas) ;
			}
		}

		public static Action<NodeEditorState> OnLoadEditorState;
		public static void IssueOnLoadEditorState (NodeEditorState editorState) 
		{
			if (OnLoadEditorState != null)
				OnLoadEditorState.Invoke (editorState);
			for (int cnt = 0; cnt < receiverCount; cnt++) 
			{
				if (callbackReceiver [cnt] == null)
					callbackReceiver.RemoveAt (cnt--);
				else
					callbackReceiver [cnt].OnLoadEditorState (editorState) ;
			}
		}

		public static Action<NodeCanvas> OnSaveCanvas;
		public static void IssueOnSaveCanvas (NodeCanvas canvas) 
		{
			if (OnSaveCanvas != null)
				OnSaveCanvas.Invoke (canvas);
			for (int cnt = 0; cnt < receiverCount; cnt++) 
			{
				if (callbackReceiver [cnt] == null)
					callbackReceiver.RemoveAt (cnt--);
				else
					callbackReceiver [cnt].OnSaveCanvas (canvas) ;
			}
		}

		public static Action<NodeEditorState> OnSaveEditorState;
		public static void IssueOnSaveEditorState (NodeEditorState editorState) 
		{
			if (OnSaveEditorState != null)
				OnSaveEditorState.Invoke (editorState);
			for (int cnt = 0; cnt < receiverCount; cnt++) 
			{
				if (callbackReceiver [cnt] == null)
					callbackReceiver.RemoveAt (cnt--);
				else
					callbackReceiver [cnt].OnSaveEditorState (editorState) ;
			}
		}

		#endregion

		#region Node (3)

		public static Action<Node> OnAddNode;
		public static void IssueOnAddNode (Node node) 
		{
			if (OnAddNode != null)
				OnAddNode.Invoke (node);
			for (int cnt = 0; cnt < receiverCount; cnt++) 
			{
				if (callbackReceiver [cnt] == null)
					callbackReceiver.RemoveAt (cnt--);
				else
					callbackReceiver [cnt].OnAddNode (node);
			}
		}

		public static Action<Node> OnDeleteNode;
		public static void IssueOnDeleteNode (Node node) 
		{
			if (OnDeleteNode != null)
				OnDeleteNode.Invoke (node);
			for (int cnt = 0; cnt < receiverCount; cnt++) 
			{
				if (callbackReceiver [cnt] == null)
					callbackReceiver.RemoveAt (cnt--);
				else 
				{
					callbackReceiver [cnt].OnDeleteNode (node);
					node.OnDelete ();
				}
			}
		}

		public static Action<Node> OnMoveNode;
		public static void IssueOnMoveNode (Node node) 
		{
			if (OnMoveNode != null)
				OnMoveNode.Invoke (node);
			for (int cnt = 0; cnt < receiverCount; cnt++) 
			{
				if (callbackReceiver [cnt] == null)
					callbackReceiver.RemoveAt (cnt--);
				else
					callbackReceiver [cnt].OnMoveNode (node);
			}
		}

		#endregion

		#region Connection (2)

		public static Action<KnobInput> OnAddConnection;
		public static void IssueOnAddConnection (KnobInput input) 
		{
			if (OnAddConnection != null)
				OnAddConnection.Invoke (input);
			for (int cnt = 0; cnt < receiverCount; cnt++) 
			{
				if (callbackReceiver [cnt] == null)
					callbackReceiver.RemoveAt (cnt--);
				else
					callbackReceiver [cnt].OnAddConnection (input);
			}
		}

		public static Action<KnobInput> OnRemoveConnection;
		public static void IssueOnRemoveConnection (KnobInput input) 
		{
			if (OnRemoveConnection != null)
				OnRemoveConnection.Invoke (input);
			for (int cnt = 0; cnt < receiverCount; cnt++) 
			{
				if (callbackReceiver [cnt] == null)
					callbackReceiver.RemoveAt (cnt--);
				else
					callbackReceiver [cnt].OnRemoveConnection (input);
			}
		}

		#endregion

		#region Transition (2)

		/*public static Action<Transition> OnAddTransition;
		public static void IssueOnAddTransition (Transition transition) 
		{
			if (OnAddTransition != null)
				OnAddTransition.Invoke (transition);
			for (int cnt = 0; cnt < receiverCount; cnt++) 
			{
				if (callbackReceiver [cnt] == null)
					callbackReceiver.RemoveAt (cnt--);
				else
					callbackReceiver [cnt].OnAddTransition (transition);
			}
		}

		public static Action<Transition> OnRemoveTransition;
		public static void IssueOnRemoveTransition (Transition transition) 
		{
			if (OnRemoveTransition != null)
				OnRemoveTransition.Invoke ( transition);
			for (int cnt = 0; cnt < receiverCount; cnt++) 
			{
				if (callbackReceiver [cnt] == null)
					callbackReceiver.RemoveAt (cnt--);
				else
					callbackReceiver [cnt].OnRemoveTransition ( transition);
			}
		}*/

		#endregion
	}
}