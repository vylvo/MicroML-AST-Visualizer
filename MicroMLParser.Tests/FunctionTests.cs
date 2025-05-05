using Xunit;
using MicroMLParser.Models;
using System.Linq;

namespace MicroMLParser.Tests
{
    public class FunctionTests : ParserTests
    {
        [Fact]
        public void Parser_ShouldParse_SimpleFunctionDefinition()
        {
            // Arrange
            string code = "fn x -> x + 1";
            
            // Act
            var result = _parser.Parse(code);
            
            // Assert
            Assert.NotNull(result);
            Assert.IsType<FunctionNode>(result);
            var func = (FunctionNode)result;
            Assert.Single(func.Parameters);
            Assert.Equal("x", func.Parameters.First());
            Assert.IsType<BinaryOpNode>(func.Body);
        }
        
        [Fact]
        public void Parser_ShouldParse_MultiParameterFunction()
        {
            // Arrange
            string code = "fn x y z -> x + y + z";
            
            // Act
            var result = _parser.Parse(code);
            
            // Assert
            Assert.NotNull(result);
            Assert.IsType<FunctionNode>(result);
            var func = (FunctionNode)result;
            Assert.Equal(3, func.Parameters.Count);
            Assert.Equal("x", func.Parameters[0]);
            Assert.Equal("y", func.Parameters[1]);
            Assert.Equal("z", func.Parameters[2]);
        }
        
        [Fact]
        public void Parser_ShouldParse_FunctionApplication()
        {
            // Arrange
            string code = "add 5 10";
            
            // Act
            var result = _parser.Parse(code);
            
            // Assert
            Assert.NotNull(result);
            Assert.IsType<ApplicationNode>(result);
            var app = (ApplicationNode)result;
            Assert.IsType<IdentifierNode>(app.Function);
            Assert.Equal("add", ((IdentifierNode)app.Function).Name);
            Assert.Equal(2, app.Arguments.Count);
            Assert.IsType<LiteralNode>(app.Arguments[0]);
            Assert.IsType<LiteralNode>(app.Arguments[1]);
        }
        
        [Fact]
        public void Parser_ShouldParse_NestedFunctionApplication()
        {
            // Arrange
            string code = "add (mul 2 3) 10";
            
            // Act
            var result = _parser.Parse(code);
            
            // Assert
            Assert.NotNull(result);
            Assert.IsType<ApplicationNode>(result);
            var app = (ApplicationNode)result;
            Assert.Equal(2, app.Arguments.Count);
            Assert.IsType<ApplicationNode>(app.Arguments[0]);
        }
        
        [Fact]
        public void Parser_ShouldParse_LetWithFunctionDefinition()
        {
            // Arrange
            string code = "let add = fn x y -> x + y in add 5 10";
            
            // Act
            var result = _parser.Parse(code);
            
            // Assert
            Assert.NotNull(result);
            Assert.IsType<LetNode>(result);
            var let = (LetNode)result;
            Assert.Equal("add", let.Variable);
            Assert.IsType<FunctionNode>(let.Value);
            Assert.IsType<ApplicationNode>(let.Body);
        }
    }
}