using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MicroMLParser.Models;

namespace MicroMLParser.Services
{
    public class MicroMLParser
    {
        private string _code = string.Empty;
        private string _currentToken = string.Empty;
        private List<string> _tokens = new List<string>();
        private int _tokenIndex;
        private int _lineNumber = 1;
        private Dictionary<int, int> _tokenToLineMap = new Dictionary<int, int>();

        // Regular regex patterns
        private static readonly Regex IdentifierRegex = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$", RegexOptions.Compiled);
        private static readonly Regex NumberRegex = new Regex(@"^\d+(\.\d+)?$", RegexOptions.Compiled);
        private static readonly Regex WhitespaceRegex = new Regex(@"^\s+$", RegexOptions.Compiled);
        private static readonly Regex CommentRegex = new Regex("--.*", RegexOptions.Compiled);

        // Array constants to prevent recreation
        private static readonly string[] NewlineChars = { "\r", "\n" };

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

        public ASTNode? Parse(string code)
        {
            try
            {
                _code = code;
                _tokenToLineMap.Clear();
                _tokens = Tokenize(code);
                _tokenIndex = 0;

                if (_tokens.Count == 0)
                    return null;

                _currentToken = _tokens[0];
                _lineNumber = GetLineNumber(_tokenIndex);

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
            // Get the line of code where the error occurred
            string[] lines = _code.Split(NewlineChars, StringSplitOptions.RemoveEmptyEntries);
            if (_lineNumber > 0 && _lineNumber <= lines.Length)
            {
                string line = lines[_lineNumber - 1];
                return line.Trim();
            }

            return string.Empty;
        }

        private List<string> Tokenize(string code)
        {
            // Map to track token positions to line numbers
            _tokenToLineMap = new Dictionary<int, int>();

            // Remove comments
            code = CommentRegex.Replace(code, "");

            // Define token patterns with specific order for correct tokenization
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
            int currentLine = 1;
            int lastPosition = 0;

            // Process matches and track line numbers
            foreach (Match match in matches)
            {
                // Count newlines before this token
                for (int i = lastPosition; i < match.Index; i++)
                {
                    if (i < code.Length && (code[i] == '\n' || (code[i] == '\r' && (i + 1 >= code.Length || code[i + 1] != '\n'))))
                    {
                        currentLine++;
                    }
                }

                var value = match.Value;

                // Count newlines in whitespace tokens
                if (WhitespaceRegex.IsMatch(value))
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        if (value[i] == '\n' || (value[i] == '\r' && (i + 1 >= value.Length || value[i + 1] != '\n')))
                        {
                            currentLine++;
                        }
                    }
                }
                else
                {
                    // Add non-whitespace tokens to our list
                    tokens.Add(value);
                    _tokenToLineMap[tokens.Count - 1] = currentLine;
                }

                lastPosition = match.Index + match.Length;
            }

            return tokens;
        }

        private int GetLineNumber(int tokenIndex)
        {
            if (_tokenToLineMap.TryGetValue(tokenIndex, out int line))
            {
                return line;
            }
            return 1; // Default to line 1 if not found
        }

        private ASTNode ParseExpression()
        {
            if (_currentToken == null)
            {
                ThrowParseError("Unexpected end of input");
            }

            _lineNumber = GetLineNumber(_tokenIndex);

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
                return ParseBinaryExpression();
            }
        }

        private ASTNode ParseBinaryExpression(int precedence = 0)
        {
            var operators = new Dictionary<string, int>
            {
                { "+", 1 }, { "-", 1 },
                { "*", 2 }, { "/", 2 },
                { "==", 0 }, { "!=", 0 }, { "<", 0 }, { ">", 0 }, { "<=", 0 }, { ">=", 0 }
            };

            ASTNode left = ParseApplicationOrPrimary();

            while (_currentToken != null &&
                   operators.TryGetValue(_currentToken, out int currentPrecedence) &&
                   currentPrecedence >= precedence)
            {
                string op = _currentToken;
                Consume(); // consume operator

                if (_currentToken == null)
                {
                    ThrowParseError($"Expected right operand after '{op}'");
                }

                ASTNode right = ParseBinaryExpression(currentPrecedence + 1);
                left = new BinaryOpNode(op, left, right);
            }

            return left;
        }

        private ASTNode ParseApplicationOrPrimary()
        {
            var function = ParsePrimary();

            // If there are arguments, parse as application
            List<ASTNode> arguments = new List<ASTNode>();

            while (_tokenIndex < _tokens.Count &&
                  !IsTerminator(_currentToken) &&
                  !IsBinaryOperator(_currentToken))
            {
                arguments.Add(ParsePrimary());
            }

            if (arguments.Count > 0)
            {
                return new ApplicationNode(function, arguments);
            }

            return function;
        }

        private LetNode ParseLetExpression()
        {
            int letLineNumber = GetLineNumber(_tokenIndex);
            Consume("let"); // consume 'let'

            if (_currentToken == null || !IsIdentifier(_currentToken))
            {
                ThrowParseError("Expected identifier after 'let'", letLineNumber);
            }

            var variable = _currentToken;
            Consume(); // consume variable name

            if (_currentToken != "=")
            {
                ThrowParseError("Expected '=' after variable name in let binding", letLineNumber);
            }

            Consume("="); // consume '='

            var value = ParseExpression();

            if (_currentToken != "in")
            {
                ThrowParseError("Expected 'in' after let binding", letLineNumber);
            }

            Consume("in"); // consume 'in'
            var body = ParseExpression();

            return new LetNode(variable, value, body);
        }

        private IfNode ParseIfExpression()
        {
            int ifLineNumber = GetLineNumber(_tokenIndex);
            Consume("if"); // consume 'if'

            var condition = ParseExpression();

            if (_currentToken != "then")
            {
                ThrowParseError("Expected 'then' after if condition", ifLineNumber);
            }

            Consume("then"); // consume 'then'
            var thenBranch = ParseExpression();

            if (_currentToken != "else")
            {
                ThrowParseError("Expected 'else' after then branch", ifLineNumber);
            }

            Consume("else"); // consume 'else'
            var elseBranch = ParseExpression();

            return new IfNode(condition, thenBranch, elseBranch);
        }

        private FunctionNode ParseFunctionDefinition()
        {
            int fnLineNumber = GetLineNumber(_tokenIndex);
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
                ThrowParseError("Expected '->' in function definition", fnLineNumber);
            }

            Consume("->"); // consume '->'

            if (_currentToken == null)
            {
                ThrowParseError("Expected function body after '->'", fnLineNumber);
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

            _lineNumber = GetLineNumber(_tokenIndex);

            if (NumberRegex.IsMatch(_currentToken))
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
                throw new InvalidOperationException("This code is unreachable");  // This will never be reached but satisfies the compiler
            }
        }

        private static bool IsIdentifier(string? token)
        {
            if (token == null)
                return false;

            return IdentifierRegex.IsMatch(token);
        }

        private static bool IsBinaryOperator(string token)
        {
            return token == "+" || token == "-" || token == "*" || token == "/" ||
                   token == "==" || token == "!=" || token == "<" || token == ">" ||
                   token == "<=" || token == ">=";
        }

        private static bool IsTerminator(string token)
        {
            return token == ")" || token == ";" || token == "in" || token == "then" || token == "else";
        }

        private void Consume(string? expected = null)
        {
            if (expected != null && (_currentToken == null || _currentToken != expected))
            {
                ThrowParseError($"Expected '{expected}', got '{_currentToken ?? "end of input"}'");
            }

            _tokenIndex++;

            if (_tokenIndex < _tokens.Count)
            {
                _currentToken = _tokens[_tokenIndex];
                _lineNumber = GetLineNumber(_tokenIndex);
            }
            else
            {
                _currentToken = string.Empty;
            }
        }

        private void ThrowParseError(string message, int? specificLineNumber = null)
        {
            int line = specificLineNumber ?? _lineNumber;
            string context = GetContextForLine(line);
            throw new ParseException(message, line, _tokenIndex, context);
        }

        private string GetContextForLine(int lineNumber)
        {
            string[] lines = _code.Split(NewlineChars, StringSplitOptions.RemoveEmptyEntries);
            if (lineNumber > 0 && lineNumber <= lines.Length)
            {
                return lines[lineNumber - 1].Trim();
            }
            return string.Empty;
        }
    }
}