using UnityEngine;
using NodeEditorFramework.core;
using NodeEditorFramework;

[Node (false, "AllAround Node")]
public class AllAroundNode : Node 
{
	public override bool AllowRecursion { get { return true; } }
	public override bool ContinueCalculation { get { return true; } }

	public override Node Create (Vector2 pos) 
	{
		AllAroundNode node = CreateInstance<AllAroundNode> ();
		
		node.rect = new Rect (pos.x, pos.y, 60, 60);
		node.name = "AllAround Node";
		
		node.CreateInput ("Input Top", "Float", Side.Top, 20);
		node.CreateInput ("Input Bottom", "Float", Side.Bottom, 20);
		node.CreateInput ("Input Right", "Float", Side.Right, 20);
		node.CreateInput ("Input Left", "Float", Side.Left, 20);
		
		node.CreateOutput ("Output Top", "Float", Side.Top, 40);
		node.CreateOutput ("Output Bottom", "Float", Side.Bottom, 40);
		node.CreateOutput ("Output Right", "Float", Side.Right, 40);
		node.CreateOutput ("Output Left", "Float", Side.Left, 40);
		
		return node;
	}
	
	protected internal override void Draw() 
	{
		Rect nodeRect = rect;
		nodeRect.position += NodeEditor.curEditorState.zoomPanAdjust;
		
		Rect bodyRect = new Rect (nodeRect.x, nodeRect.y, nodeRect.width, nodeRect.height);
		
		GUI.changed = false;
		GUILayout.BeginArea (bodyRect, GUI.skin.box);
		NodeGUI ();
		GUILayout.EndArea ();
	}
	
	protected internal override void NodeGUI () 
	{
		
	}
	
	public override bool Calculate () 
	{
		Outputs [0].SetValue<float> (Inputs [0].GetValue<float> ());
		Outputs [1].SetValue<float> (Inputs [1].GetValue<float> ());
		Outputs [2].SetValue<float> (Inputs [2].GetValue<float> ());
		Outputs [3].SetValue<float> (Inputs [3].GetValue<float> ());

		return true;
	}
}