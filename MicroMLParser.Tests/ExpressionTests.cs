using Xunit;
using MicroMLParser.Models;

namespace MicroMLParser.Tests
{
    public class ExpressionTests : ParserTests
    {
        [Fact]
        public void Parser_ShouldParse_LiteralNumbers()
        {
            // Arrange
            string code = "42";
            
            // Act
            var result = _parser.Parse(code);
            
            // Assert
            Assert.NotNull(result);
            Assert.IsType<LiteralNode>(result);
            var literal = (LiteralNode)result;
            Assert.Equal("42", literal.Value);
            Assert.Equal("Number", literal.LiteralType);
        }
        
        [Fact]
        public void Parser_ShouldParse_BooleanLiterals()
        {
            // Arrange
            string code = "true";
            
            // Act
            var result = _parser.Parse(code);
            
            // Assert
            Assert.NotNull(result);
            Assert.IsType<LiteralNode>(result);
            var literal = (LiteralNode)result;
            Assert.Equal("true", literal.Value);
            Assert.Equal("Boolean", literal.LiteralType);
        }
        
        [Fact]
        public void Parser_ShouldParse_IdentifierReferences()
        {
            // Arrange
            string code = "myVariable";
            
            // Act
            var result = _parser.Parse(code);
            
            // Assert
            Assert.NotNull(result);
            Assert.IsType<IdentifierNode>(result);
            var id = (IdentifierNode)result;
            Assert.Equal("myVariable", id.Name);
        }
        
        [Fact]
        public void Parser_ShouldParse_BinaryExpressions()
        {
            // Arrange
            string code = "x + y";
            
            // Act
            var result = _parser.Parse(code);
            
            // Assert
            Assert.NotNull(result);
            Assert.IsType<BinaryOpNode>(result);
            var binary = (BinaryOpNode)result;
            Assert.Equal("+", binary.Operator);
            Assert.IsType<IdentifierNode>(binary.Left);
            Assert.IsType<IdentifierNode>(binary.Right);
        }
        
        [Fact]
        public void Parser_ShouldParse_NestedBinaryExpressions()
        {
            // Arrange
            string code = "a + b * c";
            
            // Act
            var result = _parser.Parse(code);
            
            // Assert
            Assert.NotNull(result);
            Assert.IsType<BinaryOpNode>(result);
            var binary = (BinaryOpNode)result;
            Assert.Equal("+", binary.Operator);
            Assert.IsType<IdentifierNode>(binary.Left);
            Assert.IsType<BinaryOpNode>(binary.Right);
        }
        
        [Fact]
        public void Parser_ShouldRespect_OperatorPrecedence()
        {
            // Arrange
            string code = "a * b + c";
            
            // Act
            var result = _parser.Parse(code);
            
            // Assert
            Assert.NotNull(result);
            Assert.IsType<BinaryOpNode>(result);
            var binary = (BinaryOpNode)result;
            Assert.Equal("+", binary.Operator);
            Assert.IsType<BinaryOpNode>(binary.Left);
            Assert.IsType<IdentifierNode>(binary.Right);
        }
        
        [Fact]
        public void Parser_ShouldHandle_Parentheses()
        {
            // Arrange
            string code = "(a + b) * c";
            
            // Act
            var result = _parser.Parse(code);
            
            // Assert
            Assert.NotNull(result);
            Assert.IsType<BinaryOpNode>(result);
            var binary = (BinaryOpNode)result;
            Assert.Equal("*", binary.Operator);
            Assert.IsType<BinaryOpNode>(binary.Left);
            Assert.IsType<IdentifierNode>(binary.Right);
        }
    }
}