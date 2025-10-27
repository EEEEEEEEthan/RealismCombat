using System;
using System.Text;
using Godot;
using RealismCombat.Data;
namespace RealismCombat.Nodes;
partial class BodyPartNode : Node
{
	public BodyPartData bodyPartData;
	[Export] Label name;
	[Export] Label hitPoint;
	public override void _Process(double delta)
	{
		name.Text = bodyPartData.bodyPart switch
		{
			BodyPartCode.Head => "头部",
			BodyPartCode.Chest => "胸部",
			BodyPartCode.RightArm => "右臂",
			BodyPartCode.LeftArm => "左臂",
			BodyPartCode.RightLeg => "右腿",
			BodyPartCode.LeftLeg => "左腿",
			_ => throw new ArgumentOutOfRangeException(),
		};
		var builder = new StringBuilder();
		for (var i = 0; i < bodyPartData.hp; i++) builder.Append("▮");
		for (var i = bodyPartData.hp; i < bodyPartData.maxHp; i++) builder.Append("▯");
	}
}
