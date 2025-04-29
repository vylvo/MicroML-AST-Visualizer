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
        private int _lineNumber = 1;

        public class ParseException : Exception
        {
            public int LineNumber { get; }
            public int Position { get; }
            public string Context { get; }

            public ParseException(string message, int lineNumber, int position, string context)
                : base(message)
            {
                LineNumber = lineNumber;
                Position = position;
                Context = context;
            }
        }

        public ASTNode Parse(string code)
        {
            try
            {
                _code = code;
                _tokens = Tokenize(code);
                _tokenIndex = 0;
                _lineNumber = 1;

                if (_tokens.Count == 0)
                    return null;

                _currentToken = _tokens[0];
                return ParseExpression();
            }
            catch (ParseException)
            {
                // Re-throw parse exceptions as they already have detailed info
                throw;
            }
            catch (Exception ex)
            {
                // Wrap generic exceptions with more context
                string context = GetErrorContext();
                throw new ParseException($"Error parsing MicroML code: {ex.Message}",
                    _lineNumber, _tokenIndex, context);
            }
        }

        private string GetErrorContext()
        {
            // Get the line of code
            string[] lines = _code.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (_lineNumber <= lines.Length)
            {
                string line = lines[_lineNumber - 1];
                return line.Trim();
            }

            return string.Empty;
        }

        private List<string> Tokenize(string code)
        {
            // Track line numbers
            _lineNumber = 1;
            for (int i = 0; i < code.Length; i++)
            {
                if (code[i] == '\n')
                {
                    _lineNumber++;
                }
            }

            // Reset line number for parsing
            _lineNumber = 1;

            // Remove comments
            code = Regex.Replace(code, "--.*", "");

            // Define token patterns
            var tokenPatterns = new Dictionary<string, string>
            {
                { "KEYWORD", @"\b(let|in|if|then|else|true|false|fn)\b" },
                { "OPERATOR", @"(->|<=|>=|==|!=|\+|-|\*|/|=|<|>)" },
                { "IDENTIFIER", @"[a-zA-Z_][a-zA-Z0-9_]*" },
                { "NUMBER", @"\d+(\.\d+)?" },
                { "PUNCTUATION", @"[\(\)\{\}\[\];,]" },
                { "WHITESPACE", @"\s+" }
            };

            var combinedPattern = string.Join("|", tokenPatterns.Values);
            var regex = new Regex(combinedPattern);
            var matches = regex.Matches(code);

            var tokens = new List<string>();
            foreach (Match match in matches)
            {
                var value = match.Value;
                if (!string.IsNullOrWhiteSpace(value) && !Regex.IsMatch(value, @"^\s+$"))
                {
                    tokens.Add(value);
                }
            }

            return tokens;
        }

        private ASTNode ParseExpression()
        {
            if (_currentToken == null)
            {
                ThrowParseError("Unexpected end of input");
            }

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
                return ParsePrimary();
            }
        }

        private ASTNode ParseLetExpression()
        {
            Consume("let"); // consume 'let'

            if (_currentToken == null || !IsIdentifier(_currentToken))
            {
                ThrowParseError("Expected identifier after 'let'");
            }

            var variable = _currentToken;
            Consume(); // consume variable name

            Consume("="); // consume '='

            var value = ParseExpression();

            if (_currentToken != "in")
            {
                ThrowParseError("Expected 'in' after let binding");
            }

            Consume("in"); // consume 'in'
            var body = ParseExpression();

            return new LetNode(variable, value, body);
        }

        private ASTNode ParseIfExpression()
        {
            Consume("if"); // consume 'if'
            var condition = ParseExpression();

            if (_currentToken != "then")
            {
                ThrowParseError("Expected 'then' after if condition");
            }

            Consume("then"); // consume 'then'
            var thenBranch = ParseExpression();

            if (_currentToken != "else")
            {
                ThrowParseError("Expected 'else' after then branch");
            }

            Consume("else"); // consume 'else'
            var elseBranch = ParseExpression();

            return new IfNode(condition, thenBranch, elseBranch);
        }

        private ASTNode ParseFunctionDefinition()
        {
            Consume("fn"); // consume 'fn'
            var parameters = new List<string>();

            // Parse parameters
            while (_currentToken != null && _currentToken != "->" && IsIdentifier(_currentToken))
            {
                parameters.Add(_currentToken);
                Consume(); // consume parameter name
            }

            if (_currentToken != "->")
            {
                ThrowParseError("Expected '->' in function definition");
            }

            Consume("->"); // consume '->'

            if (_currentToken == null)
            {
                ThrowParseError("Expected function body after '->'");
            }

            var body = ParseExpression();

            return new FunctionNode(parameters, body);
        }

        private ASTNode ParsePrimary()
        {
            if (_currentToken == null)
            {
                ThrowParseError("Unexpected end of input");
            }

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

                if (_currentToken != ")")
                {
                    ThrowParseError("Expected closing parenthesis ')'");
                }

                Consume(")"); // consume ')'
                return expression;
            }
            else if (IsIdentifier(_currentToken))
            {
                var name = _currentToken;
                Consume(); // consume identifier
                return new IdentifierNode(name);
            }
            else
            {
                ThrowParseError($"Unexpected token: {_currentToken}");
                return null; // This line will never be reached due to the exception, but it's required for compilation
            }
        }

        private bool IsIdentifier(string token)
        {
            return Regex.IsMatch(token, @"^[a-zA-Z_][a-zA-Z0-9_]*$");
        }

        private void Consume(string expected = null)
        {
            if (expected != null && (_currentToken == null || _currentToken != expected))
            {
                ThrowParseError($"Expected '{expected}', got '{_currentToken ?? "end of input"}'");
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

        private void ThrowParseError(string message)
        {
            string context = GetErrorContext();
            throw new ParseException(message, _lineNumber, _tokenIndex, context);
        }
    }
}