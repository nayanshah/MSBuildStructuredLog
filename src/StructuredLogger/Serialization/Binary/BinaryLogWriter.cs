﻿using System;

namespace Microsoft.Build.Logging.StructuredLogger
{
    public class BinaryLogWriter : IDisposable
    {
        private readonly string filePath;
        private TreeBinaryWriter writer;

        public static void Write(Build build, string filePath)
        {
            using (var binaryLogWriter = new BinaryLogWriter(filePath))
            {
                binaryLogWriter.WriteNode(build);
            }
        }

        private BinaryLogWriter(string filePath)
        {
            this.filePath = filePath;
            this.writer = new TreeBinaryWriter(filePath);
        }

        private void WriteNode(object node)
        {
            writer.WriteNode(Serialization.GetNodeName(node));
            WriteAttributes(node);
            writer.WriteEndAttributes();
            WriteChildren(node);
        }

        private void WriteChildren(object node)
        {
            var treeNode = node as TreeNode;
            if (treeNode != null && treeNode.HasChildren)
            {
                writer.WriteChildrenCount(treeNode.Children.Count);
                foreach (var child in treeNode.Children)
                {
                    WriteNode(child);
                }
            }
            else
            {
                writer.WriteChildrenCount(0);
            }
        }

        private void WriteAttributes(object node)
        {
            var metadata = node as Metadata;
            if (metadata != null)
            {
                SetString(nameof(Metadata.Name), metadata.Name);
                SetString(nameof(Metadata.Value), metadata.Value);
                return;
            }

            var property = node as Property;
            if (property != null)
            {
                SetString(nameof(Property.Name), property.Name);
                SetString(nameof(Property.Value), property.Value);
                return;
            }

            var message = node as Message;
            if (message != null)
            {
                SetString(nameof(Message.IsLowRelevance), message.IsLowRelevance.ToString());
                SetString(nameof(Message.Timestamp), ToString(message.Timestamp));
                SetString(nameof(Message.Text), message.Text);
                return;
            }

            var folder = node as Folder;
            if (folder != null)
            {
                SetString(nameof(Folder.IsLowRelevance), folder.IsLowRelevance.ToString());
                return;
            }

            var namedNode = node as NamedNode;
            if (namedNode != null)
            {
                SetString(nameof(NamedNode.Name), namedNode.Name?.Replace("\"", ""));
            }

            var textNode = node as TextNode;
            if (textNode != null)
            {
                SetString(nameof(TextNode.Text), textNode.Text);
            }

            if (node is TimedNode)
            {
                AddStartAndEndTime((TimedNode)node);
            }

            var task = node as Task;
            if (task != null)
            {
                SetString(nameof(Task.FromAssembly), task.FromAssembly);
                SetString(nameof(Task.CommandLineArguments), task.CommandLineArguments);
                return;
            }

            var target = node as Target;
            if (target != null)
            {
                SetString(nameof(Target.DependsOnTargets), target.DependsOnTargets);
                SetString(nameof(Target.IsLowRelevance), target.IsLowRelevance.ToString());
                return;
            }

            var diagnostic = node as AbstractDiagnostic;
            if (diagnostic != null)
            {
                SetString(nameof(AbstractDiagnostic.Code), diagnostic.Code);
                SetString(nameof(AbstractDiagnostic.File), diagnostic.File);
                SetString(nameof(AbstractDiagnostic.LineNumber), diagnostic.LineNumber.ToString());
                SetString(nameof(AbstractDiagnostic.ColumnNumber), diagnostic.ColumnNumber.ToString());
                SetString(nameof(AbstractDiagnostic.EndLineNumber), diagnostic.EndLineNumber.ToString());
                SetString(nameof(AbstractDiagnostic.EndColumnNumber), diagnostic.EndColumnNumber.ToString());
                SetString(nameof(AbstractDiagnostic.ProjectFile), diagnostic.ProjectFile);
                return;
            }

            var project = node as Project;
            if (project != null)
            {
                SetString(nameof(Project.ProjectFile), project.ProjectFile);
                return;
            }

            var build = node as Build;
            if (build != null)
            {
                SetString(nameof(Build.Succeeded), build.Succeeded.ToString());
                SetString(nameof(Build.IsAnalyzed), build.IsAnalyzed.ToString());
                return;
            }
        }

        private void SetString(string name, string value)
        {
            writer.WriteAttributeValue(value);
        }

        private void AddStartAndEndTime(TimedNode node)
        {
            SetString(nameof(TimedNode.StartTime), ToString(node.StartTime));
            SetString(nameof(TimedNode.EndTime), ToString(node.EndTime));
        }

        private string ToString(DateTime time)
        {
            return time.ToString("o");
        }

        public void Dispose()
        {
            if (writer != null)
            {
                writer.Dispose();
                writer = null;
            }
        }
    }
}
