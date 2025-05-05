using System;
using Xunit;
using MicroMLParser.Services;
using MicroMLParser.Models;

namespace MicroMLParser.Tests
{
    public class ParserTests
    {
        protected readonly MicroMLParser.Services.MicroMLParser _parser;
        
        public ParserTests()
        {
            _parser = new MicroMLParser.Services.MicroMLParser();
        }
        
        [Fact]
        public void Parser_ShouldReturnNull_ForEmptyInput()
        {
            // Arrange
            string code = "";
            
            // Act
            var result = _parser.Parse(code);
            
            // Assert
            Assert.Null(result);
        }
        
        [Fact]
        public void Parser_ShouldHandleWhitespace()
        {
            // Arrange
            string code = "  \n  \t  ";
            
            // Act
            var result = _parser.Parse(code);
            
            // Assert
            Assert.Null(result);
        }
        
        [Fact]
        public void Parser_ShouldIgnoreComments()
        {
            // Arrange
            string code = "-- This is a comment\nlet x = 5 in x";
            
            // Act
            var result = _parser.Parse(code);
            
            // Assert
            Assert.NotNull(result);
            Assert.IsType<LetNode>(result);
            var letNode = (LetNode)result;
            Assert.Equal("x", letNode.Variable);
        }
        
        [Fact]
        public void Parser_ShouldThrowParseException_ForInvalidInput()
        {
            // Arrange
            string code = "let x = in";
            
            // Act & Assert
            var ex = Assert.Throws<MicroMLParser.Services.MicroMLParser.ParseException>(() => _parser.Parse(code));
            Assert.Contains("Expected", ex.Message);
        }
    }
}