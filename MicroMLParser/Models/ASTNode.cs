using System.Collections.Generic;

namespace MicroMLParser.Models
{
    // Base class for all AST nodes
    public abstract class ASTNode
    {
        public string NodeType { get; set; }
        public virtual List<ASTNode> Children { get; } = new List<ASTNode>();

        public ASTNode(string nodeType)
        {
            NodeType = nodeType;
        }
    }

    // Represents a literal value (number, boolean, etc.)
    public class LiteralNode : ASTNode
    {
        public string Value { get; set; }
        public string LiteralType { get; set; }

        public LiteralNode(string value, string literalType) : base("Literal")
        {
            Value = value;
            LiteralType = literalType;
        }
    }

    // Represents a variable reference
    public class IdentifierNode : ASTNode
    {
        public string Name { get; set; }

        public IdentifierNode(string name) : base("Identifier")
        {
            Name = name;
        }
    }

    // Represents a binary operation (e.g., +, -, *, /)
    public class BinaryOpNode : ASTNode
    {
        public string Operator { get; set; }
        public ASTNode Left { get; set; }
        public ASTNode Right { get; set; }

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
        public List<string> Parameters { get; set; }
        public ASTNode Body { get; set; }

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
        public ASTNode Function { get; set; }
        public List<ASTNode> Arguments { get; set; }

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
        public string Variable { get; set; }
        public ASTNode Value { get; set; }
        public ASTNode Body { get; set; }

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
        public ASTNode Condition { get; set; }
        public ASTNode ThenBranch { get; set; }
        public ASTNode ElseBranch { get; set; }

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