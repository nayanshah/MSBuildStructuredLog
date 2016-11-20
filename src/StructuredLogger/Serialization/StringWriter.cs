﻿using System.Text;

namespace Microsoft.Build.Logging.StructuredLogger
{
    public class StringWriter
    {
        public static string GetString(object rootNode)
        {
            var sb = new StringBuilder();

            WriteNode(rootNode, sb, 0);

            return sb.ToString();
        }

        private static void WriteNode(object rootNode, StringBuilder sb, int indent = 0)
        {
            Indent(sb, indent);
            var text = rootNode.ToString() ?? "";

            // when we injest strings we normalize on \n to save space.
            // when the strings leave our app via clipboard, bring \r\n back so that notepad works
            text = text.Replace("\n", "\r\n");

            sb.AppendLine(text);

            var treeNode = rootNode as TreeNode;
            if (treeNode != null && treeNode.HasChildren)
            {
                if (treeNode.HasChildren)
                {
                    foreach (var child in treeNode.Children)
                    {
                        WriteNode(child, sb, indent + 1);
                    }
                }
            }
        }

        private static void Indent(StringBuilder sb, int indent)
        {
            for (int i = 0; i < indent * 4; i++)
            {
                sb.Append(' ');
            }
        }
    }
}
