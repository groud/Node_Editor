using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

using NodeEditorFramework;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.core
{

	public abstract class Node : ScriptableObject
	{
		public Rect rect = new Rect ();

		[SerializeField]
		public List<Knob> knobs = new List<Knob> ();

		// Calculation graph
		[SerializeField, HideInInspector]
		public List<KnobInput> Inputs = new List<KnobInput>();
		[SerializeField, HideInInspector]
		public List<KnobOutput> Outputs = new List<KnobOutput>();
		[NonSerialized, HideInInspector]
		internal bool calculated = true;

		#region General

		/// <summary>
		/// Init the Node Base after the Node has been created. This includes adding to canvas, and to calculate for the first time
		/// </summary>
		protected internal void InitBase () 
		{
			Calculate ();
			if (!NodeEditor.canvas.nodes.Contains (this))
				NodeEditor.canvas.nodes.Add (this);
			#if UNITY_EDITOR
			if (name == "")
				name = UnityEditor.ObjectNames.NicifyVariableName ("YOLO");
			#endif
		}

		/// <summary>
		/// Deletes this Node from curNodeCanvas and the save file
		/// </summary>
		public void Delete () 
		{
			if (!NodeEditor.canvas.nodes.Contains (this))
				throw new UnityException ("The Node " + name + " does not exist on the Canvas " + NodeEditor.canvas.name + "!");
			NodeEditorCallbacks.IssueOnDeleteNode (this);
			NodeEditor.canvas.nodes.Remove (this);
			foreach (KnobOutput output in Outputs) 
			{
				while (output.connections.Count != 0)
					output.connections[0].RemoveConnection ();
				DestroyImmediate (output, true);
			}
			foreach (KnobInput input in Inputs) 
			{
				if (input.connection != null)
					input.connection.connections.Remove (input);
				DestroyImmediate (input, true);
			}
			foreach  (Knob knob in knobs) 
			{ // Inputs/Outputs need specific treatment, unfortunately
				if (knob != null)
					DestroyImmediate (knob, true);
			}
			DestroyImmediate (this, true);
		}

		public static Node Create (System.Type type, Vector2 position) 
		{
			Node node = (Node) ScriptableObject.CreateInstance (type);
			node = node.Create (position);
			node.InitBase ();

			NodeEditorCallbacks.IssueOnAddNode (node);
			return node;
		}

		/// <summary>
		/// Makes sure this Node has migrated from the previous save version of Knobs to the current mixed and generic one
		/// </summary>
		/*internal void CheckKnobMigration () 
		{ // TODO: Migration from previous Knob system; Remove later on
			if (nodeKnobs.Count == 0 && (Inputs.Count != 0 || Outputs.Count != 0)) 
			{
				nodeKnobs.AddRange (Inputs.Cast<Knob> ());
				nodeKnobs.AddRange (Outputs.Cast<Knob> ());
			}
		}
		*/

		#endregion

		#region Node Type methods (abstract)

		/// <summary>
		/// Get the ID of the Node
		/// </summary>
		//public abstract string GetID { get; }

		/// <summary>
		/// Create an instance of this Node at the given position
		/// </summary>
		public abstract Node Create (Vector2 pos);

		/// <summary>
		/// Draw the Node immediately
		/// </summary>
		protected internal abstract void NodeGUI ();
		
		/// <summary>
		/// Calculate the outputs of this Node
		/// Return Success/Fail
		/// Might be dependant on previous nodes
		/// </summary>
		public abstract bool Calculate ();

		#endregion

		#region Node Type Properties

		/// <summary>
		/// Does this node allow recursion? Recursion is allowed if atleast a single Node in the loop allows for recursion
		/// </summary>
		public virtual bool AllowRecursion { get { return false; } }

		/// <summary>
		/// Should the following Nodes be calculated after finishing the Calculation function of this node?
		/// </summary>
		public virtual bool ContinueCalculation { get { return true; } }

        #endregion

		#region Protected Callbacks

		/// <summary>
		/// Callback when the node is deleted
		/// </summary>
		protected internal virtual void OnDelete () {}

		/// <summary>
		/// Callback when the KnobInput was assigned a new connection
		/// </summary>
		protected internal virtual void OnAddInputConnection (KnobInput input) {}

		/// <summary>
		/// Callback when the KnobOutput was assigned a new connection (the last in the list)
		/// </summary>
		protected internal virtual void OnAddOutputConnection (KnobOutput output) {}

		/// <summary>
		/// Callback when the this Node is being transitioned to. 
		/// OriginTransition is the transition from which was transitioned to this node OR null if the transitioning process was started on this Node
		/// </summary>
		//protected internal virtual void OnEnter (Transition originTransition) {}

		/// <summary>
		/// Callback when the this Node is transitioning to another Node through the passed Transition
		/// </summary>
		// protected internal virtual void OnLeave (Transition transition) {}

		#endregion

		#region Additional Serialization

		/// <summary>
		/// Returns all additional ScriptableObjects this Node holds. 
		/// That means only the actual SOURCES, simple REFERENCES will not be returned
		/// This means all SciptableObjects returned here do not have it's source elsewhere
		/// </summary>
		protected internal virtual ScriptableObject[] GetScriptableObjects () { return new ScriptableObject[0]; }

		/// <summary>
		/// Replaces all REFERENCES aswell as SOURCES of any ScriptableObjects this Node holds with the cloned versions in the serialization process.
		/// </summary>
		protected internal virtual void CopyScriptableObjects (System.Func<ScriptableObject, ScriptableObject> replaceSerializableObject) {}

		#endregion


		#region Node and Knob Drawing

		/// <summary>
		/// Draws the node frame and calls NodeGUI. Can be overridden to customize drawing.
		/// </summary>
		protected internal virtual void Draw() 
		{
			// TODO: Node Editor Feature: Custom Windowing System
			// Create a rect that is adjusted to the editor zoom
			Rect nodeRect = rect;
			nodeRect.position += NodeEditor.curEditorState.zoomPanAdjust;
			GUI.Window (0, rect, DrawInside, name);

			/*
			Rect nodeRect = rect;
			nodeRect.position += NodeEditor.curEditorState.zoomPanAdjust;
			Vector2 contentOffset = new Vector2 (0, 20);

			// Mark the current transitioning node as such by outlining it
			if (NodeEditor.curNodeCanvas.currentNode == this)
				GUI.DrawTexture (new Rect (nodeRect.x-8, nodeRect.y-8, nodeRect.width+16, nodeRect.height+16), NodeEditorGUI.GUIBoxSelection);

			// Create a headerRect out of the previous rect and draw it, marking the selected node as such by making the header bold
			Rect headerRect = new Rect (nodeRect.x, nodeRect.y, nodeRect.width, contentOffset.y);
			GUI.Label (headerRect, name);

			// Begin the body frame around the NodeGUI
			Rect bodyRect = new Rect (nodeRect.x, nodeRect.y + contentOffset.y, nodeRect.width, nodeRect.height - contentOffset.y);
			GUI.BeginGroup (bodyRect, GUI.skin.box);
			bodyRect.position = Vector2.zero;
			GUILayout.BeginArea (bodyRect, GUI.skin.box);
			// Call NodeGUI
			GUI.changed = false;
			NodeGUI ();
			// End NodeGUI frame
			GUILayout.EndArea ();
			GUI.EndGroup ();*/
		}

		/// <summary>
		/// Draws inside
		/// </summary>
		protected internal virtual void DrawInside (int windowId) 
		{
			NodeGUI ();
		}


		/// <summary>
		/// Draws the nodeKnobs
		/// </summary>
		protected internal virtual void DrawKnobs () 
		{
			foreach  (Knob knob in knobs)
				knob.Draw();
		}

		/// <summary>
		/// Draws the node curves
		/// </summary>
		protected internal virtual void DrawConnections () 
		{
			//TODO: What is this ?
			if (Event.current.type != EventType.Repaint)
				return;
			
			foreach (KnobOutput output in Outputs) 
			{
				Vector2 startPos = output.GetGUIKnob ().center;
				Vector2 startDir = output.GetDirection ();

				foreach (KnobInput input in output.connections) 
				{
					NodeEditorGUI.DrawConnection (startPos,
					                           startDir,
					                           input.GetGUIKnob ().center,
					                           input.GetDirection (),
					                           ConnectionTypes.GetTypeData (output.type).col);
				}
			}
		}

        /// <summary>
        /// Used to display a custom node property editor in the side window of the NodeEditorWindow
        /// Optionally override this to implement
        /// </summary>
        public virtual void DrawNodePropertyEditor() { }

		#endregion
		
		#region Node Calculation Utility
		
		/// <summary>
		/// Checks if there are no unassigned and no null-value inputs.
		/// </summary>
		protected internal bool allInputsReady ()
		{
			foreach (KnobInput input in Inputs) 
			{
				if (input.connection == null || input.connection.IsValueNull)
					return false;
			}
			return true;
		}
		/// <summary>
		/// Checks if there are any unassigned inputs.
		/// </summary>
		protected internal bool hasUnassignedInputs () 
		{
			foreach (KnobInput input in Inputs) 
				if (input.connection == null)
					return true;
			return false;
		}
		
		/// <summary>
		/// Returns whether every direct descendant has been calculated
		/// </summary>
		protected internal bool descendantsCalculated () 
		{
			foreach (KnobInput input in Inputs) 
			{
				if (input.connection != null && !input.connection.parentNode.calculated)
					return false;
			}
			return true;
		}

		/// <summary>
		/// Returns whether the node acts as an input (no inputs or no inputs assigned)
		/// </summary>
		protected internal bool isInput () 
		{
			foreach (KnobInput input in Inputs) 
				if (input.connection != null)
					return false;
			return true;
		}

		#endregion

		#region Node Knob Utility

		// -- OUTPUTS --

		/// <summary>
		/// Creates and output on your Node of the given type.
		/// </summary>
		public void CreateOutput (string outputName, string outputType)
		{
			KnobOutput.Create (this, outputName, outputType);
		}
		/// <summary>
		/// Creates and output on this Node of the given type at the specified NodeSide.
		/// </summary>
		public void CreateOutput (string outputName, string outputType, Side nodeSide)
		{
			KnobOutput.Create (this, outputName, outputType, nodeSide);
		}
		/// <summary>
		/// Creates and output on this Node of the given type at the specified NodeSide and position.
		/// </summary>
		public void CreateOutput (string outputName, string outputType, Side nodeSide, float sidePosition)
		{
			KnobOutput.Create (this, outputName, outputType, nodeSide, sidePosition);
		}

		/// <summary>
		/// Aligns the OutputKnob on it's NodeSide with the last GUILayout control drawn.
		/// </summary>
		/// <param name="outputIdx">The index of the output in the Node's Outputs list</param>
		protected void OutputKnob (int outputIdx)
		{
			if (Event.current.type == EventType.Repaint)
				Outputs[outputIdx].SetPosition ();
		}

		/// <summary>
		/// Returns the output knob that is at the position on this node or null
		/// </summary>
		public KnobOutput GetOutputAtPos (Vector2 pos) 
		{
			foreach (KnobOutput output in Outputs) 
			{ // Search for an output at the position
				if (output.GetScreenKnob ().Contains (new Vector3 (pos.x, pos.y)))
					return output;
			}
			return null;
		}


		// -- INPUTS --

		/// <summary>
		/// Creates and input on your Node of the given type.
		/// </summary>
		public void CreateInput (string inputName, string inputType)
		{
			KnobInput.Create (this, inputName, inputType);
		}
		/// <summary>
		/// Creates and input on this Node of the given type at the specified NodeSide.
		/// </summary>
		public void CreateInput (string inputName, string inputType, Side nodeSide)
		{
			KnobInput.Create (this, inputName, inputType, nodeSide);
		}
		/// <summary>
		/// Creates and input on this Node of the given type at the specified NodeSide and position.
		/// </summary>
		public void CreateInput (string inputName, string inputType, Side nodeSide, float sidePosition)
		{
			KnobInput.Create (this, inputName, inputType, nodeSide, sidePosition);
		}

		/// <summary>
		/// Aligns the InputKnob on it's NodeSide with the last GUILayout control drawn.
		/// </summary>
		/// <param name="inputIdx">The index of the input in the Node's Inputs list</param>
		protected void InputKnob (int inputIdx)
		{
			if (Event.current.type == EventType.Repaint)
				Inputs[inputIdx].SetPosition ();
		}

		/// <summary>
		/// Returns the input knob that is at the position on this node or null
		/// </summary>
		public KnobInput GetInputAtPos (Vector2 pos) 
		{
			foreach (KnobInput input in Inputs) 
			{ // Search for an input at the position
				if (input.GetScreenKnob ().Contains (new Vector3 (pos.x, pos.y)))
					return input;
			}
			return null;
		}

		#endregion

		#region Recursive Search Utility

		/// <summary>
		/// Recursively checks whether this node is a child of the other node
		/// </summary>
		public bool isChildOf (Node otherNode)
		{
			if (otherNode == null || otherNode == this)
				return false;
			if (BeginRecursiveSearchLoop ()) return false;
			foreach (KnobInput input in Inputs) 
			{
				KnobOutput connection = input.connection;
				if (connection != null) 
				{
					if (connection.parentNode != startRecursiveSearchNode)
					{
						if (connection.parentNode == otherNode || connection.parentNode.isChildOf (otherNode))
						{
							StopRecursiveSearchLoop ();
							return true;
						}
					}
				}
			}
			EndRecursiveSearchLoop ();
			return false;
		}

		/// <summary>
		/// Recursively checks whether this node is in a loop
		/// </summary>
		internal bool isInLoop ()
		{
			if (BeginRecursiveSearchLoop ()) return this == startRecursiveSearchNode;
			foreach (KnobInput input in Inputs) 
			{
				KnobOutput connection = input.connection;
				if (connection != null) 
				{
					if (connection.parentNode.isInLoop ())
					{
						StopRecursiveSearchLoop ();
						return true;
					}
				}
			}
			EndRecursiveSearchLoop ();
			return false;
		}

		/// <summary>
		/// Recursively checks whether any node in the loop to be made allows recursion.
		/// Other node is the node this node needs connect to in order to fill the loop (other node being the node coming AFTER this node).
		/// That means isChildOf has to be confirmed before calling this!
		/// </summary>
		internal bool allowsLoopRecursion (Node otherNode)
		{
			if (AllowRecursion)
				return true;
			if (otherNode == null)
				return false;
			if (BeginRecursiveSearchLoop ()) return false;
			foreach (KnobInput input in Inputs)
			{
				KnobOutput connection = input.connection;
				if (connection != null) 
				{
					if (connection.parentNode != startRecursiveSearchNode)
					{
						if (connection.parentNode.allowsLoopRecursion (otherNode))
						{
							StopRecursiveSearchLoop ();
							return true;
						}
					}
				}
			}
			EndRecursiveSearchLoop ();
			return false;
		}

		/// <summary>
		/// A recursive function to clear all calculations depending on this node.
		/// Usually does not need to be called manually
		/// </summary>
		public void ClearCalculation () 
		{
			if (BeginRecursiveSearchLoop ()) return;
			calculated = false;
			foreach (KnobOutput output in Outputs)
			{
				foreach (KnobInput connection in output.connections)
					connection.parentNode.ClearCalculation ();
			}
			EndRecursiveSearchLoop ();
		}

		#region Recursive Search Helpers

		private List<Node> recursiveSearchSurpassed;
		private Node startRecursiveSearchNode; // Temporary start node for recursive searches

		/// <summary>
		/// Begins the recursive search loop and returns whether this node has already been searched
		/// </summary>
		internal bool BeginRecursiveSearchLoop ()
		{
			if (startRecursiveSearchNode == null || recursiveSearchSurpassed == null) 
			{ // Start search
				recursiveSearchSurpassed = new List<Node> ();
				startRecursiveSearchNode = this;
			}

			if (recursiveSearchSurpassed.Contains (this))
				return true;
			recursiveSearchSurpassed.Add (this);
			return false;
		}

		/// <summary>
		/// Ends the recursive search loop if this was the start node
		/// </summary>
		internal void EndRecursiveSearchLoop () 
		{
			if (startRecursiveSearchNode == this) 
			{ // End search
				recursiveSearchSurpassed = null;
				startRecursiveSearchNode = null;
			}
		}

		/// <summary>
		/// Stops the recursive search loop immediately. Call when you found what you needed.
		/// </summary>
		internal void StopRecursiveSearchLoop () 
		{
			recursiveSearchSurpassed = null;
			startRecursiveSearchNode = null;
		}

		#endregion

		#endregion

		#region Static Connection Utility

		/// <summary>
		/// Creates a transition from node to node
		/// </summary>
		/*public static void CreateTransition (Node fromNode, Node toNode) 
		{
			Transition trans = Transition.Create (fromNode, toNode);
			if (trans != null)
			{
				fromNode.OnAddTransition (trans);
				toNode.OnAddTransition (trans);
				NodeEditorCallbacks.IssueOnAddTransition (trans);
			}
		}*/

		#endregion
	}



}