using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Oathx.Expression
{
    /// <summary>Parses precedence and emits postfix instructions with short-circuit branches.</summary>
    internal sealed class ExpressionCompiler
    {
        /// <summary>Owner of variable and method bindings.</summary>
        private readonly ExpressionEngine engine;

        /// <summary>Original source text.</summary>
        private readonly string source;

        /// <summary>Requested execution permission.</summary>
        private readonly ExpressionMode mode;

        /// <summary>Instructions assembled during one compilation.</summary>
        private readonly List<ExpressionInstruction> code = new List<ExpressionInstruction>();

        /// <summary>Next source offset.</summary>
        private int cursor;

        /// <summary>Current token offset.</summary>
        private int position;

        /// <summary>Current token text.</summary>
        private string token;

        /// <summary>True when the current token is a quoted literal.</summary>
        private bool quoted;

        /// <summary>Current recursive parser depth.</summary>
        private int depth;

        /// <summary>Creates a single-use compiler.</summary>
        /// <param name="engine">Engine that supplies variable and method bindings.</param>
        /// <param name="source">Expression source text.</param>
        /// <param name="mode">Permission for query or command execution.</param>
        internal ExpressionCompiler(ExpressionEngine engine, string source, ExpressionMode mode)
        {
            this.engine = engine;
            this.source = source;
            this.mode = mode;
        }

        /// <summary>Compiles a complete expression and rejects trailing input.</summary>
        /// <returns>The immutable compiled program.</returns>
        internal CompiledExpression Compile()
        {
            Next();
            Parse(1);
            if (token.Length != 0)
                throw Error("Unexpected token: " + token);

            return new CompiledExpression(engine, source, mode, code.ToArray());
        }

        /// <summary>Checks identifier syntax shared by variables and registered methods.</summary>
        /// <param name="name">Case-sensitive variable or method identifier.</param>
        internal static void ValidateName(string name)
        {
            if (string.IsNullOrEmpty(name) || !NameStart(name[0]) || name == "true" || name == "false")
                throw new ArgumentException("Invalid expression identifier: " + name);

            for (int index = 1; index < name.Length; index++)
            {
                if (!NamePart(name[index]))
                    throw new ArgumentException("Invalid expression identifier: " + name);
            }
        }

        /// <summary>Parses a precedence level with left-associative binary operations.</summary>
        /// <param name="minimum">Lowest binary precedence accepted at this position.</param>
        private void Parse(int minimum)
        {
            depth++;
            if (depth > 128)
                throw Error("Expression nesting exceeds 128 levels.");

            Primary();
            while (!quoted && Precedence(token) >= minimum)
            {
                string operation = token;
                int precedence = Precedence(operation);
                int location = position;
                Next();
                if (operation == "&&" || operation == "||")
                {
                    int branch = code.Count;
                    Emit(operation == "&&" ? ExpressionOp.AndBranch : ExpressionOp.OrBranch, location);
                    Parse(precedence + 1);
                    Emit(ExpressionOp.Boolean, location);
                    ExpressionInstruction instruction = code[branch];
                    instruction.operand = code.Count;
                    code[branch] = instruction;
                }
                else
                {
                    Parse(precedence + 1);
                    Emit(BinaryOp(operation), location);
                }
            }

            depth--;
        }

        /// <summary>Parses a literal, variable, call, unary operation or grouped expression.</summary>
        private void Primary()
        {
            int location = position;
            string value = token;
            if (quoted)
            {
                Literal(new ExpressionValue(value), location);
                Next();

                return;
            }

            if (value == "+" || value == "-" || value == "!")
            {
                Next();
                Parse(7);
                Emit(value == "+" ? ExpressionOp.Positive : value == "-" ? ExpressionOp.Negative : ExpressionOp.Not,
                    location);

                return;
            }

            if (value == "(")
            {
                Next();
                Parse(1);
                Expect(")");

                return;
            }

            if (value == "true" || value == "false")
            {
                Literal(new ExpressionValue(value == "true"), location);
                Next();

                return;
            }

            if (value.Length > 0 && (char.IsDigit(value[0]) || value[0] == '.'))
            {
                if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double number)
                    || double.IsNaN(number) || double.IsInfinity(number))
                    throw Error("Invalid finite number: " + value);

                Literal(new ExpressionValue(number), location);
                Next();

                return;
            }

            if (value == "{")
            {
                Next();
                if (!quoted || token.Length == 0)
                    throw Error("Command name must be a quoted string.");

                string name = token;
                Next();
                if (token != "}" || quoted)
                {
                    Expect(",");
                    if (token == "}" && !quoted)
                        throw Error("Expected an argument after the comma.");
                }

                ParseCall(name, location, "}");

                return;
            }

            if (value.Length == 0 || !NameStart(value[0]))
                throw Error("Expected a value, variable or function call.");

            Next();
            if (token != "(" || quoted)
            {
                Emit(ExpressionOp.Variable, location, engine.VariableSlot(value));

                return;
            }

            Next();
            ParseCall(value, location, ")");
        }

        /// <summary>Compiles a function call or command tuple using the same typed method binding.</summary>
        /// <param name="value">Registered method name.</param>
        /// <param name="location">Source offset of the call.</param>
        /// <param name="closing">Required closing delimiter.</param>
        private void ParseCall(string value, int location, string closing)
        {
            ExpressionMethod method;
            try
            {
                method = engine.FindMethod(value);
            }
            catch (InvalidOperationException error)
            {
                throw new ExpressionException(error.Message, location, error);
            }

            if (mode == ExpressionMode.Evaluate && method.kind == ExpressionFunctionKind.Command)
                throw new ExpressionException("Command is forbidden in Evaluate: " + value, location);

            int count = 0;
            if (token != closing || quoted)
            {
                while (true)
                {
                    Parse(1);
                    count++;
                    if (token != "," || quoted)
                        break;

                    Next();
                }
            }

            Expect(closing);
            if (count != method.parameters.Length)
                throw new ExpressionException("Argument count does not match " + value, location);

            var instruction = new ExpressionInstruction {
                op = ExpressionOp.Call,
                operand = count,
                position = location,
                method = method
            };

            code.Add(instruction);
        }

        /// <summary>Requires a punctuation token.</summary>
        /// <param name="expected">Required punctuation token.</param>
        private void Expect(string expected)
        {
            if (quoted || token != expected)
                throw Error("Expected '" + expected + "'.");

            Next();
        }

        /// <summary>Emits an operation or variable read.</summary>
        /// <param name="op">Operation to execute.</param>
        /// <param name="location">Zero-based source offset.</param>
        /// <param name="operand">Variable slot or instruction operand.</param>
        private void Emit(ExpressionOp op, int location, int operand = 0)
        {
            var instruction = new ExpressionInstruction {
                op = op,
                operand = operand,
                position = location
            };

            code.Add(instruction);
        }

        /// <summary>Emits an immutable literal.</summary>
        /// <param name="value">Operand or token being processed.</param>
        /// <param name="location">Zero-based source offset.</param>
        private void Literal(ExpressionValue value, int location)
        {
            var instruction = new ExpressionInstruction {
                op = ExpressionOp.Literal,
                literal = value,
                position = location
            };

            code.Add(instruction);
        }

        /// <summary>Scans the next token without accepting unsupported punctuation.</summary>
        private void Next()
        {
            while (cursor < source.Length && char.IsWhiteSpace(source[cursor])) cursor++;
            position = cursor;
            quoted = false;
            if (cursor == source.Length)
            {
                token = string.Empty;

                return;
            }

            char current = source[cursor++];
            if (current == '"' || current == '\'')
            {
                token = ReadString(current);
                quoted = true;

                return;
            }

            if (NameStart(current))
            {
                while (cursor < source.Length && NamePart(source[cursor])) cursor++;
            }
            else if (char.IsDigit(current) || current == '.')
            {
                while (cursor < source.Length && (char.IsDigit(source[cursor]) || source[cursor] == '.')) cursor++;
                if (cursor < source.Length && (source[cursor] == 'e' || source[cursor] == 'E'))
                {
                    cursor++;
                    if (cursor < source.Length && (source[cursor] == '+' || source[cursor] == '-')) cursor++;
                    while (cursor < source.Length && char.IsDigit(source[cursor])) cursor++;
                }
            }
            else
            {
                string pair = cursor < source.Length ? source.Substring(position, 2) : string.Empty;
                if (pair == "&&" || pair == "||" || pair == "==" || pair == "!=" || pair == "<=" || pair == ">=")
                    cursor++;
                else if ("+-*/%()!,<>{}".IndexOf(current) < 0)
                    throw Error("Unsupported character: " + current);
            }

            token = source.Substring(position, cursor - position);
        }

        /// <summary>Reads quoted strings with common escapes and four-digit Unicode escapes.</summary>
        /// <param name="quote">Opening quote character.</param>
        /// <returns>The decoded string literal.</returns>
        private string ReadString(char quote)
        {
            var result = new StringBuilder();
            while (cursor < source.Length)
            {
                char current = source[cursor++];
                if (current == quote)
                    return result.ToString();

                if (current == '\\')
                {
                    if (cursor == source.Length) throw Error("Unfinished string escape.");
                    current = source[cursor++];
                    switch (current)
                    {
                        case 'n': current = '\n'; break;
                        case 'r': current = '\r'; break;
                        case 't': current = '\t'; break;
                        case '\\': case '\'': case '"': break;
                        case 'u':
                            if (cursor + 4 > source.Length || !ushort.TryParse(source.Substring(cursor, 4),
                                NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort unicode))
                                throw Error("Invalid Unicode escape.");
                            current = (char)unicode;
                            cursor += 4;
                            break;
                        default: throw Error("Unsupported string escape.");
                    }
                }

                result.Append(current);
            }

            throw Error("Unterminated string.");
        }

        /// <summary>Creates a failure at the current token.</summary>
        /// <param name="message">Description of the expression failure.</param>
        /// <returns>A failure located at the current source token.</returns>
        private ExpressionException Error(string message)
        {
            return new ExpressionException(message, position);
        }

        /// <summary>Reports binary precedence; zero denotes punctuation or an operand.</summary>
        /// <param name="value">Operand or token being processed.</param>
        /// <returns>Binary precedence, or zero for other tokens.</returns>
        private static int Precedence(string value)
        {
            switch (value)
            {
                case "||": return 1;
                case "&&": return 2;
                case "==": case "!=": return 3;
                case "<": case "<=": case ">": case ">=": return 4;
                case "+": case "-": return 5;
                case "*": case "/": case "%": return 6;
                default: return 0;
            }
        }

        /// <summary>Maps a recognized binary token to its opcode.</summary>
        /// <param name="value">Operand or token being processed.</param>
        /// <returns>The opcode for the recognized operator.</returns>
        private static ExpressionOp BinaryOp(string value)
        {
            switch (value)
            {
                case "+": return ExpressionOp.Add;
                case "-": return ExpressionOp.Subtract;
                case "*": return ExpressionOp.Multiply;
                case "/": return ExpressionOp.Divide;
                case "%": return ExpressionOp.Modulo;
                case "==": return ExpressionOp.Equal;
                case "!=": return ExpressionOp.NotEqual;
                case "<": return ExpressionOp.Less;
                case "<=": return ExpressionOp.LessEqual;
                case ">": return ExpressionOp.Greater;
                case ">=": return ExpressionOp.GreaterEqual;
                default: throw new InvalidOperationException("Unknown binary operator.");
            }
        }

        /// <summary>Accepts Unicode letters and underscores at the start of identifiers.</summary>
        /// <param name="value">Operand or token being processed.</param>
        /// <returns>Whether the character may begin an identifier.</returns>
        private static bool NameStart(char value)
        {
            return char.IsLetter(value) || value == '_';
        }

        /// <summary>Accepts letters, digits and underscores within identifiers.</summary>
        /// <param name="value">Operand or token being processed.</param>
        /// <returns>Whether the character may occur within an identifier.</returns>
        private static bool NamePart(char value)
        {
            return char.IsLetterOrDigit(value) || value == '_';
        }
    }
}
