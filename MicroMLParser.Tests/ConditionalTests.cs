using Xunit;
using MicroMLParser.Models;

namespace MicroMLParser.Tests
{
    public class ConditionalTests : ParserTests
    {
        [Fact]
        public void Parser_ShouldParse_SimpleConditional()
        {
            // Arrange
            string code = "if true then 1 else 0";
            
            // Act
            var result = _parser.Parse(code);
            
            // Assert
            Assert.NotNull(result);
            Assert.IsType<IfNode>(result);
            var ifNode = (IfNode)result;
            Assert.IsType<LiteralNode>(ifNode.Condition);
            Assert.IsType<LiteralNode>(ifNode.ThenBranch);
            Assert.IsType<LiteralNode>(ifNode.ElseBranch);
        }
        
        [Fact]
        public void Parser_ShouldParse_ConditionalWithExpressions()
        {
            // Arrange
            string code = "if x > 10 then x * 2 else x / 2";
            
            // Act
            var result = _parser.Parse(code);
            
            // Assert
            Assert.NotNull(result);
            Assert.IsType<IfNode>(result);
            var ifNode = (IfNode)result;
            Assert.IsType<BinaryOpNode>(ifNode.Condition);
            Assert.IsType<BinaryOpNode>(ifNode.ThenBranch);
            Assert.IsType<BinaryOpNode>(ifNode.ElseBranch);
        }
        
        [Fact]
        public void Parser_ShouldParse_NestedConditionals()
        {
            // Arrange
            string code = "if x > 10 then if x > 20 then x * 3 else x * 2 else x";
            
            // Act
            var result = _parser.Parse(code);
            
            // Assert
            Assert.NotNull(result);
            Assert.IsType<IfNode>(result);
            var ifNode = (IfNode)result;
            Assert.IsType<IfNode>(ifNode.ThenBranch);
        }
        
        [Fact]
        public void Parser_ShouldParse_ConditionalWithFunctions()
        {
            // Arrange
            string code = "let max = fn a b -> if a > b then a else b in max 5 10";
            
            // Act
            var result = _parser.Parse(code);
            
            // Assert
            Assert.NotNull(result);
            Assert.IsType<LetNode>(result);
            var letNode = (LetNode)result;
            Assert.IsType<FunctionNode>(letNode.Value);
            var funcNode = (FunctionNode)letNode.Value;
            Assert.IsType<IfNode>(funcNode.Body);
        }
        
        [Fact]
        public void Parser_ShouldParse_MultilineConditional()
        {
            // Arrange
            string code = @"if x > 10 then 
                              x * 2 
                            else 
                              x / 2";
            
            // Act
            var result = _parser.Parse(code);
            
            // Assert
            Assert.NotNull(result);
            Assert.IsType<IfNode>(result);
        }
    }
}