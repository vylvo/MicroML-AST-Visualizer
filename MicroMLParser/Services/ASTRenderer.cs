using System;
using System.Collections.Generic;
using System.Text;
using MicroMLParser.Models;

namespace MicroMLParser.Services
{
    public class ASTRenderer
    {
        private const int NodeWidth = 120;
        private const int NodeHeight = 40;
        private const int LevelHeight = 80;
        private const int NodeHorizontalSpacing = 20;

        public string RenderToSvg(ASTNode root)
        {
            if (root == null)
                return "<svg width=\"400\" height=\"100\"><text x=\"10\" y=\"50\" fill=\"red\">No AST available</text></svg>";

            // Calculate tree dimensions
            var treeDepth = CalculateTreeDepth(root);
            var treeWidth = CalculateTreeWidth(root) * (NodeWidth + NodeHorizontalSpacing);
            var treeHeight = treeDepth * LevelHeight + 50;

            // Adjust width if it's too small
            treeWidth = Math.Max(treeWidth, 400);

            var svgBuilder = new StringBuilder();
            svgBuilder.AppendLine($"<svg width=\"{treeWidth}\" height=\"{treeHeight}\" xmlns=\"http://www.w3.org/2000/svg\">");

            // Define styles
            svgBuilder.AppendLine("<defs>");
            svgBuilder.AppendLine("  <style>");
            svgBuilder.AppendLine("    .node { fill: #e8f4f8; stroke: #4a90e2; stroke-width: 2px; }");
            svgBuilder.AppendLine("    .node-text { font-family: Arial, sans-serif; font-size: 12px; fill: #333; }");
            svgBuilder.AppendLine("    .edge { stroke: #4a90e2; stroke-width: 1.5px; }");
            svgBuilder.AppendLine("  </style>");
            svgBuilder.AppendLine("</defs>");

            // Draw the tree
            var horizontalPositions = new Dictionary<ASTNode, float>();
            CalculateHorizontalPositions(root, horizontalPositions);

            DrawNode(svgBuilder, root, treeWidth / 2, 40, 0, horizontalPositions);

            svgBuilder.AppendLine("</svg>");
            return svgBuilder.ToString();
        }

        private void DrawNode(StringBuilder svgBuilder, ASTNode node, float x, float y, int level, Dictionary<ASTNode, float> horizontalPositions)
        {
            if (horizontalPositions.ContainsKey(node))
            {
                x = horizontalPositions[node];
            }

            // Draw node
            svgBuilder.AppendLine($"<rect class=\"node\" x=\"{x - NodeWidth / 2}\" y=\"{y}\" width=\"{NodeWidth}\" height=\"{NodeHeight}\" rx=\"5\" />");

            // Draw node text
            var nodeText = GetNodeLabel(node);
            svgBuilder.AppendLine($"<text class=\"node-text\" x=\"{x}\" y=\"{y + NodeHeight / 2 + 5}\" text-anchor=\"middle\">{nodeText}</text>");

            // Draw connections and child nodes
            if (node.Children.Count > 0)
            {
                var childY = y + LevelHeight;

                foreach (var child in node.Children)
                {
                    float childX = horizontalPositions[child];

                    // Draw edge
                    svgBuilder.AppendLine($"<line class=\"edge\" x1=\"{x}\" y1=\"{y + NodeHeight}\" x2=\"{childX}\" y2=\"{childY}\" />");

                    // Draw child node
                    DrawNode(svgBuilder, child, childX, childY, level + 1, horizontalPositions);
                }
            }
        }

        private void CalculateHorizontalPositions(ASTNode node, Dictionary<ASTNode, float> positions, float startX = 0)
        {
            var nodeQueue = new Queue<Tuple<ASTNode, int>>();
            nodeQueue.Enqueue(new Tuple<ASTNode, int>(node, 0));

            var levelNodes = new Dictionary<int, List<ASTNode>>();

            // Group nodes by level
            while (nodeQueue.Count > 0)
            {
                var (currentNode, level) = nodeQueue.Dequeue();

                if (!levelNodes.ContainsKey(level))
                {
                    levelNodes[level] = new List<ASTNode>();
                }

                levelNodes[level].Add(currentNode);

                foreach (var child in currentNode.Children)
                {
                    nodeQueue.Enqueue(new Tuple<ASTNode, int>(child, level + 1));
                }
            }

            // Process level by level, bottom-up
            var maxLevel = levelNodes.Keys.Count > 0 ? levelNodes.Keys.Max() : 0;

            for (int level = maxLevel; level >= 0; level--)
            {
                var nodesInLevel = levelNodes[level];
                var spacing = 0;

                if (level == maxLevel)
                {
                    // Position leaf nodes
                    spacing = NodeWidth + NodeHorizontalSpacing;
                    float currentX = startX;

                    foreach (var leafNode in nodesInLevel)
                    {
                        positions[leafNode] = currentX;
                        currentX += spacing;
                    }
                }
                else
                {
                    // Position parent nodes
                    foreach (var parentNode in nodesInLevel)
                    {
                        if (parentNode.Children.Count > 0)
                        {
                            // Position parent at center of children
                            float minChildX = float.MaxValue;
                            float maxChildX = float.MinValue;

                            foreach (var child in parentNode.Children)
                            {
                                if (positions.ContainsKey(child))
                                {
                                    minChildX = Math.Min(minChildX, positions[child]);
                                    maxChildX = Math.Max(maxChildX, positions[child]);
                                }
                            }

                            if (minChildX != float.MaxValue)
                            {
                                positions[parentNode] = (minChildX + maxChildX) / 2;
                            }
                            else
                            {
                                positions[parentNode] = startX;
                            }
                        }
                        else
                        {
                            // No children, position arbitrarily
                            positions[parentNode] = startX;
                        }
                    }
                }

                startX += spacing * nodesInLevel.Count;
            }
        }

        private int CalculateTreeDepth(ASTNode node)
        {
            if (node == null || node.Children.Count == 0)
                return 1;

            int maxChildDepth = 0;
            foreach (var child in node.Children)
            {
                maxChildDepth = Math.Max(maxChildDepth, CalculateTreeDepth(child));
            }

            return 1 + maxChildDepth;
        }

        private int CalculateTreeWidth(ASTNode node)
        {
            if (node == null)
                return 0;

            if (node.Children.Count == 0)
                return 1;

            int totalChildrenWidth = 0;
            foreach (var child in node.Children)
            {
                totalChildrenWidth += CalculateTreeWidth(child);
            }

            return Math.Max(1, totalChildrenWidth);
        }

        private string GetNodeLabel(ASTNode node)
        {
            switch (node)
            {
                case LiteralNode literal:
                    return $"{literal.LiteralType}: {literal.Value}";

                case IdentifierNode identifier:
                    return $"Var: {identifier.Name}";

                case BinaryOpNode binaryOp:
                    return $"Op: {binaryOp.Operator}";

                case FunctionNode function:
                    return $"Function ({function.Parameters.Count})";

                case ApplicationNode application:
                    return $"Apply ({application.Arguments.Count})";

                case LetNode let:
                    return $"Let: {let.Variable}";

                case IfNode ifNode:
                    return "If";

                default:
                    return node.NodeType;
            }
        }
    }
}