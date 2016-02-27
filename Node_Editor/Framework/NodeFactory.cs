using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using NodeEditorFramework;

namespace NodeEditorFramework.core
{
	/// <summary>
	/// Handles fetching and storing of all NodeDeclarations
	/// </summary>
	public static class NodeFactory
	{
		//public static List<Node> nodes;

		/// <summary>
		/// Fetches every Node Declaration in the assembly and stores them in the nodes List.
		/// nodes List contains a default instance of each node type in the key and editor specific NodeData in the value
		/// </summary>
		public static System.Type[] FetchNodes() 
		{
			List<System.Type> result = new List<System.Type> ();
			Assembly[] scriptAssemblies = AppDomain.CurrentDomain.GetAssemblies ();

			foreach (Assembly assembly in scriptAssemblies) 
			{
				foreach (Type type in assembly.GetTypes ()) 
				{
					if(type.IsClass && !type.IsAbstract && type.IsSubclassOf (typeof (Node)))
					{
						result.Add (type);
					}

				}
			}
			return result.ToArray();
		}
	}

	/// <summary>
	/// The NodeAttribute is used to specify editor specific data for a node type, later stored using a NodeData
	/// </summary>
	public class NodeAttribute : Attribute 
	{
		public bool hide { get; private set; }
		public string contextText { get; private set; }

		public NodeAttribute (bool HideNode, string ReplacedContextText) 
		{
			hide = HideNode;
			contextText = ReplacedContextText;
		}
	}
}