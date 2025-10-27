using System.Collections.Generic;
using System.Text;
using Godot;
using RealismCombat.Nodes;
using System.Threading.Tasks;
namespace RealismCombat.Commands.DebugCommands;
class DebugShowNodeTreeCommand(ProgramRootNode rootNode, IReadOnlyDictionary<string, string> arguments) : Command(rootNode: rootNode, arguments: arguments)
{
	public const string name = "debug_show_node_tree";
    public override Task Execute()
	{
		var rootPath = arguments.GetValueOrDefault(key: "root", defaultValue: "/root");
		var root = rootNode.GetNode(rootPath);
		if (root == null)
		{
			Log.PrintError($"节点路径无效: {rootPath}");
			this.rootNode.McpCheckPoint();
            return Task.CompletedTask;
		}
		var json = BuildNodeTree(root);
		Log.Print(json);
		this.rootNode.McpCheckPoint();
        return Task.CompletedTask;
    }
	string BuildNodeTree(Node node)
	{
		var sb = new StringBuilder();
		BuildNodeTreeRecursive(node: node, sb: sb, indent: 0);
		return sb.ToString();
	}
	void BuildNodeTreeRecursive(Node node, StringBuilder sb, int indent)
	{
		var indentStr = new string(c: ' ', count: indent * 2);
		var nodeName = node.Name;
		var nodeType = node.GetType().Name;
		var childCount = node.GetChildCount();
		sb.Append(indentStr);
		sb.Append('"');
		sb.Append(nodeName);
		sb.Append('(');
		sb.Append(nodeType);
		sb.Append(')');
		sb.Append('"');
		if (childCount > 0)
		{
			sb.AppendLine(": {");
			for (var i = 0; i < childCount; i++)
			{
				var child = node.GetChild(i);
				BuildNodeTreeRecursive(node: child, sb: sb, indent: indent + 1);
				if (i < childCount - 1)
					sb.AppendLine(",");
				else
					sb.AppendLine();
			}
			sb.Append(indentStr);
			sb.Append('}');
		}
	}
}
