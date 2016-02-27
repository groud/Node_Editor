using UnityEngine;
using System.Collections.Generic;

namespace NodeEditorFramework.core
{
	/// <summary>
	/// NodeInput accepts one connection to a NodeOutput by default
	/// </summary>
	public class KnobInput : Knob
	{
		// NodeKnob Members
		protected override Side defaultSide { get { return Side.Left; } }

		// NodeInput Members
		public KnobOutput connection;
		public string type;
		[System.NonSerialized]
		internal TypeData typeData;
		// Multiple connections
//		public List<NodeOutput> connections;

		#region Contructors

		/// <summary>
		/// Creates a new NodeInput in NodeBody of specified type
		/// </summary>
		public static KnobInput Create (Node parentNode, string inputName, string inputType)
		{
			return Create (parentNode, inputName, inputType, Side.Left, 20);
		}

		/// <summary>
		/// Creates a new NodeInput in NodeBody of specified type at the specified NodeSide
		/// </summary>
		public static KnobInput Create (Node parentNode, string inputName, string inputType, Side side)
		{
			return Create (parentNode, inputName, inputType, side, 20);
		}

		/// <summary>
		/// Creates a new NodeInput in NodeBody of specified type at the specified NodeSide and position
		/// </summary>
		public static KnobInput Create (Node parentNode, string inputName, string inputType, Side side, float sidePosition)
		{
			KnobInput input = CreateInstance <KnobInput> ();
			input.type = inputType;
			input.InitBase (parentNode, side, sidePosition, inputName);
			parentNode.Inputs.Add (input);
			return input;
		}

		#endregion

		#region Additional Serialization

		protected internal override void CopyScriptableObjects (System.Func<ScriptableObject, ScriptableObject> replaceSerializableObject) 
		{
			connection = replaceSerializableObject.Invoke (connection) as KnobOutput;
			// Multiple connections
//			for (int conCnt = 0; conCnt < connections.Count; conCnt++) 
//				connections[conCnt] = replaceSerializableObject.Invoke (connections[conCnt]) as NodeOutput;
		}

		#endregion

		#region KnobType

		protected override void ReloadTexture () 
		{
			CheckType ();
			knobTexture = typeData.InputKnob;
		}

		private void CheckType () 
		{
			if (typeData.declaration == null || typeData.Type == null) 
				typeData = ConnectionTypes.GetTypeData (type);
		}

		#endregion

		#region Value

		/// <summary>
		/// Gets the value of the connection or the default value
		/// </summary>
		public T GetValue<T> ()
		{
			return connection != null? connection.GetValue<T> () : KnobOutput.GetDefault<T> ();
		}

		/// <summary>
		/// Sets the value of the connection if the type matches
		/// </summary>
		public void SetValue<T> (T value)
		{
			if (connection != null)
				connection.SetValue<T> (value);
		}

		#endregion

		#region Connecting Utility

		/// <summary>
		/// Check if the passed NodeOutput can be connected to this NodeInput
		/// </summary>
		public bool CanApplyConnection (KnobOutput output)
		{
			if (output == null || parentNode == output.parentNode || connection == output || typeData.Type != output.typeData.Type)
				return false;

			if (output.parentNode.isChildOf (parentNode)) 
			{ // Recursive
				if (!output.parentNode.allowsLoopRecursion (parentNode))
				{
					// TODO: Generic Notification
					Debug.LogWarning ("Cannot apply connection: Recursion detected!");
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Applies a connection between the passed NodeOutput and this NodeInput. 'CanApplyConnection' has to be checked before to avoid interferences!
		/// </summary>
		public void ApplyConnection (KnobOutput output)
		{
			if (output == null) 
				return;
			
			if (connection != null) 
			{
				NodeEditorCallbacks.IssueOnRemoveConnection (this);
				connection.connections.Remove (this);
			}
			connection = output;
			output.connections.Add (this);

			NodeEditor.RecalculateFrom (parentNode);
			output.parentNode.OnAddOutputConnection (output);
			parentNode.OnAddInputConnection (this);
			NodeEditorCallbacks.IssueOnAddConnection (this);
		}

		/// <summary>
		/// Removes the connection from this NodeInput
		/// </summary>
		public void RemoveConnection ()
		{
			if (connection == null)
				return;
			
			NodeEditorCallbacks.IssueOnRemoveConnection (this);
			connection.connections.Remove (this);
			connection = null;

			NodeEditor.RecalculateFrom (parentNode);
		}


		#endregion
	}
}