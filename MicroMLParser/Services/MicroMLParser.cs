using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MicroMLParser.Models;

namespace MicroMLParser.Services
{
    public class MicroMLParser
    {
        private string _code;
        private int _position;
        private string _currentToken;
        private List<string> _tokens;
        private int _tokenIndex;

        public ASTNode Parse(string code)
        {
            _code = code;
            _tokens = Tokenize(code);
            _tokenIndex = 0;

            if (_tokens.Count == 0)
                return null;

            _currentToken = _tokens[0];
            return ParseExpression();
        }

        private List<string> Tokenize(string code)
        {
            // Remove comments
            code = Regex.Replace(code, "--.*", "");

            // Replace newlines with spaces
            code = code.Replace("\n", " ").Replace("\r", " ");

            // Define token patterns
            var tokenPatterns = new Dictionary<string, string>
            {
                { "KEYWORD", @"\b(let|in|if|then|else|true|false|fn)\b" },
                { "IDENTIFIER", @"[a-zA-Z_][a-zA-Z0-9_]*" },
                { "NUMBER", @"\d+(\.\d+)?" },
                { "OPERATOR", @"[\+\-\*/=<>!&|]+" },
                { "PUNCTUATION", @"[\(\)\{\};,]" },
                { "WHITESPACE", @"\s+" }
            };

            var combinedPattern = string.Join("|", tokenPatterns.Values);
            var regex = new Regex(combinedPattern);
            var matches = regex.Matches(code);

            var tokens = new List<string>();
            foreach (Match match in matches)
            {
                var value = match.Value;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    tokens.Add(value);
                }
            }

            return tokens.Where(t => !Regex.IsMatch(t, @"^\s+$")).ToList();
        }

        private ASTNode ParseExpression()
        {
            if (_currentToken == "let")
            {
                return ParseLetExpression();
            }
            else if (_currentToken == "if")
            {
                return ParseIfExpression();
            }
            else if (_currentToken == "fn")
            {
                return ParseFunctionDefinition();
            }
            else
            {
                return ParseApplicationOrPrimary();
            }
        }

        private ASTNode ParseLetExpression()
        {
            Consume("let"); // consume 'let'
            var variable = _currentToken;
            Consume(); // consume variable name
            Consume("="); // consume '='
            var value = ParseExpression();
            Consume("in"); // consume 'in'
            var body = ParseExpression();
            return new LetNode(variable, value, body);
        }

        private ASTNode ParseIfExpression()
        {
            Consume("if"); // consume 'if'
            var condition = ParseExpression();
            Consume("then"); // consume 'then'
            var thenBranch = ParseExpression();
            Consume("else"); // consume 'else'
            var elseBranch = ParseExpression();
            return new IfNode(condition, thenBranch, elseBranch);
        }

        private ASTNode ParseFunctionDefinition()
        {
            Consume("fn"); // consume 'fn'
            var parameters = new List<string>();

            // Parse parameters
            while (_currentToken != "->")
            {
                parameters.Add(_currentToken);
                Consume(); // consume parameter name
            }

            Consume("->"); // consume '->'
            var body = ParseExpression();

            return new FunctionNode(parameters, body);
        }

        private ASTNode ParseApplicationOrPrimary()
        {
            var function = ParsePrimary();

            // If there are arguments, parse as application
            if (_tokenIndex < _tokens.Count && !IsTerminator(_currentToken))
            {
                var arguments = new List<ASTNode>();

                while (_tokenIndex < _tokens.Count && !IsTerminator(_currentToken))
                {
                    arguments.Add(ParsePrimary());
                }

                return new ApplicationNode(function, arguments);
            }

            return function;
        }

        private ASTNode ParsePrimary()
        {
            if (Regex.IsMatch(_currentToken, @"^\d+(\.\d+)?$"))
            {
                var value = _currentToken;
                Consume(); // consume number
                return new LiteralNode(value, "Number");
            }
            else if (_currentToken == "true" || _currentToken == "false")
            {
                var value = _currentToken;
                Consume(); // consume boolean
                return new LiteralNode(value, "Boolean");
            }
            else if (_currentToken == "(")
            {
                Consume("("); // consume '('
                var expression = ParseExpression();
                Consume(")"); // consume ')'
                return expression;
            }
            else if (Regex.IsMatch(_currentToken, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
            {
                var name = _currentToken;
                Consume(); // consume identifier
                return new IdentifierNode(name);
            }
            else
            {
                throw new Exception($"Unexpected token: {_currentToken}");
            }
        }

        private void Consume(string expected = null)
        {
            if (expected != null && _currentToken != expected)
            {
                throw new Exception($"Expected '{expected}', got '{_currentToken}'");
            }

            _tokenIndex++;
            if (_tokenIndex < _tokens.Count)
            {
                _currentToken = _tokens[_tokenIndex];
            }
            else
            {
                _currentToken = null;
            }
        }

        private bool IsTerminator(string token)
        {
            return token == ")" || token == ";" || token == "in" || token == "then" || token == "else";
        }
    }
}