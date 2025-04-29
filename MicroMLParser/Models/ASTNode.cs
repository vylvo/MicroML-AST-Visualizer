using System.Collections.Generic;

namespace MicroMLParser.Models
{
    // Base class for all AST nodes
    public abstract class ASTNode
    {
        public string NodeType { get; }
        public virtual List<ASTNode> Children { get; } = new List<ASTNode>();

        public ASTNode(string nodeType)
        {
            NodeType = nodeType;
        }
    }

    // Represents a literal value (number, boolean, etc.)
    public class LiteralNode : ASTNode
    {
        public string Value { get; }
        public string LiteralType { get; }

        public LiteralNode(string value, string literalType) : base("Literal")
        {
            Value = value;
            LiteralType = literalType;
        }
    }

    // Represents a variable reference
    public class IdentifierNode : ASTNode
    {
        public string Name { get; }

        public IdentifierNode(string name) : base("Identifier")
        {
            Name = name;
        }
    }

    // Represents a binary operation (e.g., +, -, *, /)
    public class BinaryOpNode : ASTNode
    {
        public string Operator { get; }
        public ASTNode Left { get; }
        public ASTNode Right { get; }

        public BinaryOpNode(string op, ASTNode left, ASTNode right) : base("BinaryOp")
        {
            Operator = op;
            Left = left;
            Right = right;
            Children.Add(left);
            Children.Add(right);
        }
    }

    // Represents a function definition
    public class FunctionNode : ASTNode
    {
        public List<string> Parameters { get; }
        public ASTNode Body { get; }

        public FunctionNode(List<string> parameters, ASTNode body) : base("Function")
        {
            Parameters = parameters;
            Body = body;
            Children.Add(body);
        }
    }

    // Represents a function application
    public class ApplicationNode : ASTNode
    {
        public ASTNode Function { get; }
        public List<ASTNode> Arguments { get; }

        public ApplicationNode(ASTNode function, List<ASTNode> arguments) : base("Application")
        {
            Function = function;
            Arguments = arguments;
            Children.Add(function);
            Children.AddRange(arguments);
        }
    }

    // Represents a let binding
    public class LetNode : ASTNode
    {
        public string Variable { get; }
        public ASTNode Value { get; }
        public ASTNode Body { get; }

        public LetNode(string variable, ASTNode value, ASTNode body) : base("Let")
        {
            Variable = variable;
            Value = value;
            Body = body;
            Children.Add(value);
            Children.Add(body);
        }
    }

    // Represents an if-then-else expression
    public class IfNode : ASTNode
    {
        public ASTNode Condition { get; }
        public ASTNode ThenBranch { get; }
        public ASTNode ElseBranch { get; }

        public IfNode(ASTNode condition, ASTNode thenBranch, ASTNode elseBranch) : base("If")
        {
            Condition = condition;
            ThenBranch = thenBranch;
            ElseBranch = elseBranch;
            Children.Add(condition);
            Children.Add(thenBranch);
            Children.Add(elseBranch);
        }
    }
}