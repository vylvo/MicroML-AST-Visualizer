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
        private const int NodeHorizontalSpacing = 40; // Increased spacing
        private const int LeftPadding = 50; // Added padding to left side
        private const int TopPadding = 20; // Added padding to top

        public string RenderToSvg(ASTNode? root)
        {
            if (root == null)
                return "<svg width=\"400\" height=\"100\"><text x=\"10\" y=\"50\" fill=\"red\">No AST available</text></svg>";

            // Calculate tree dimensions
            var treeDepth = CalculateTreeDepth(root);
            var treeWidth = Math.Max(600, CalculateTreeWidth(root) * (NodeWidth + NodeHorizontalSpacing) + LeftPadding * 2);
            var treeHeight = treeDepth * LevelHeight + TopPadding * 2;

            var svgBuilder = new StringBuilder();
            svgBuilder.AppendLine($"<svg width=\"{treeWidth}\" height=\"{treeHeight}\" xmlns=\"http://www.w3.org/2000/svg\">");

            // Define styles
            svgBuilder.AppendLine("<defs>");
            svgBuilder.AppendLine("  <style>");
            svgBuilder.AppendLine("    .node { fill: #e8f4f8; stroke: #4a90e2; stroke-width: 2px; }");
            svgBuilder.AppendLine("    .node-text { font-family: Arial, sans-serif; font-size: 12px; fill: #333; text-anchor: middle; dominant-baseline: middle; }");
            svgBuilder.AppendLine("    .edge { stroke: #4a90e2; stroke-width: 1.5px; }");
            svgBuilder.AppendLine("  </style>");
            svgBuilder.AppendLine("</defs>");

            // Draw the tree
            var horizontalPositions = new Dictionary<ASTNode, float>();
            CalculateHorizontalPositions(root, horizontalPositions, LeftPadding);

            DrawNode(svgBuilder, root, treeWidth / 2, TopPadding + NodeHeight, 0, horizontalPositions);

            svgBuilder.AppendLine("</svg>");
            return svgBuilder.ToString();
        }

        private void DrawNode(StringBuilder svgBuilder, ASTNode node, float x, float y, int level, Dictionary<ASTNode, float> horizontalPositions)
        {
            if (horizontalPositions.TryGetValue(node, out float posX))
            {
                x = posX;
            }

            // Draw node
            svgBuilder.AppendLine($"<rect class=\"node\" x=\"{x - NodeWidth / 2}\" y=\"{y}\" width=\"{NodeWidth}\" height=\"{NodeHeight}\" rx=\"5\" />");

            // Draw node text - centered both horizontally and vertically
            var nodeText = GetNodeLabel(node);
            svgBuilder.AppendLine($"<text class=\"node-text\" x=\"{x}\" y=\"{y + NodeHeight / 2}\">{nodeText}</text>");

            // Draw connections and child nodes
            if (node.Children.Count > 0)
            {
                var childY = y + LevelHeight;

                foreach (var child in node.Children)
                {
                    if (horizontalPositions.TryGetValue(child, out float childX))
                    {
                        // Draw edge
                        svgBuilder.AppendLine($"<line class=\"edge\" x1=\"{x}\" y1=\"{y + NodeHeight}\" x2=\"{childX}\" y2=\"{childY}\" />");

                        // Draw child node
                        DrawNode(svgBuilder, child, childX, childY, level + 1, horizontalPositions);
                    }
                }
            }
        }

        private void CalculateHorizontalPositions(ASTNode node, Dictionary<ASTNode, float> positions, float startX = 0)
        {
            // First, calculate the total width needed for leaf nodes
            var leafNodes = new List<ASTNode>();
            GetLeafNodes(node, leafNodes);

            // If no leaf nodes, handle the case
            if (leafNodes.Count == 0)
            {
                positions[node] = startX + NodeWidth;
                return;
            }

            // Place leaf nodes first with consistent spacing
            float leafSpacing = NodeWidth + NodeHorizontalSpacing;
            float currentX = startX;

            // Place leaf nodes
            foreach (var leaf in leafNodes)
            {
                positions[leaf] = currentX + NodeWidth / 2;
                currentX += leafSpacing;
            }

            // Now work up from bottom to top
            PositionInternalNodes(node, positions);
        }

        private void GetLeafNodes(ASTNode node, List<ASTNode> leafNodes)
        {
            if (node.Children.Count == 0)
            {
                leafNodes.Add(node);
                return;
            }

            foreach (var child in node.Children)
            {
                GetLeafNodes(child, leafNodes);
            }
        }

        private void PositionInternalNodes(ASTNode node, Dictionary<ASTNode, float> positions)
        {
            // Process children first (bottom-up)
            foreach (var child in node.Children)
            {
                if (!positions.ContainsKey(child))
                {
                    PositionInternalNodes(child, positions);
                }
            }

            // Position this node based on children
            if (node.Children.Count > 0)
            {
                float minX = float.MaxValue;
                float maxX = float.MinValue;

                foreach (var child in node.Children)
                {
                    if (positions.TryGetValue(child, out float childX))
                    {
                        minX = Math.Min(minX, childX);
                        maxX = Math.Max(maxX, childX);
                    }
                }

                if (minX != float.MaxValue && maxX != float.MinValue)
                {
                    positions[node] = (minX + maxX) / 2;
                }
                else if (minX != float.MaxValue)
                {
                    positions[node] = minX;
                }
                else if (maxX != float.MinValue)
                {
                    positions[node] = maxX;
                }
            }

            // If node has no position yet (no children)
            if (!positions.ContainsKey(node))
            {
                positions[node] = NodeWidth; // Default position
            }
        }

        private int CalculateTreeDepth(ASTNode? node)
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

        private int CalculateTreeWidth(ASTNode? node)
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

        private static string GetNodeLabel(ASTNode node)
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

                case IfNode _:
                    return "If";

                default:
                    return node.NodeType;
            }
        }
    }
}