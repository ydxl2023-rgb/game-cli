using System;
using System.Collections.Generic;

namespace Oathx.Expression
{
    /// <summary>Compiles, caches and executes expressions without reflection or generated IL.</summary>
    public sealed class ExpressionEngine : IExpressionEngine
    {
        /// <summary>Pure expression cache.</summary>
        private readonly Dictionary<string, CompiledExpression> queries =
            new Dictionary<string, CompiledExpression>(StringComparer.Ordinal);

        /// <summary>Command-enabled expression cache.</summary>
        private readonly Dictionary<string, CompiledExpression> commands =
            new Dictionary<string, CompiledExpression>(StringComparer.Ordinal);

        /// <summary>Stable binding slots for registered names.</summary>
        private readonly Dictionary<string, ExpressionMethod> methods =
            new Dictionary<string, ExpressionMethod>(StringComparer.Ordinal);

        /// <summary>Variable names resolved once while compiling or assigning parameters.</summary>
        private readonly Dictionary<string, int> variables = new Dictionary<string, int>(StringComparer.Ordinal);

        /// <summary>Number of currently cached programs.</summary>
        public int cacheCount
        { get { return queries.Count + commands.Count; } }

        /// <summary>Number of successful compilations, including programs later evicted.</summary>
        public long compilationCount
        { get; private set; }

        /// <summary>Installs the standard pure math functions.</summary>
        public ExpressionEngine()
        {
            RegisterMath();
        }

        /// <summary>Creates reusable parameters for this engine.</summary>
        /// <returns>A new independent context.</returns>
        public ExpressionContext CreateContext()
        {
            return new ExpressionContext(this);
        }

        /// <summary>Compiles each exact source and mode once until cache eviction.</summary>
        /// <param name="source">Expression source.</param>
        /// <param name="mode">Allowed method category.</param>
        /// <returns>The shared immutable compiled handle.</returns>
        public CompiledExpression Compile(string source, ExpressionMode mode = ExpressionMode.Evaluate)
        {
            if (mode != ExpressionMode.Evaluate && mode != ExpressionMode.Execute)
                throw new ArgumentOutOfRangeException(nameof(mode));

            var cache = mode == ExpressionMode.Evaluate ? queries : commands;
            if (cache.TryGetValue(source, out var program))
                return program;

            var compiler = new ExpressionCompiler(this, source, mode);
            program = compiler.Compile();
            cache.Add(source, program);
            compilationCount++;

            return program;
        }

        /// <summary>Computes a pure expression using the latest parameters.</summary>
        /// <param name="source">Expression source text.</param>
        /// <param name="context">Current variable values and reusable execution stacks.</param>
        /// <returns>The computed typed value.</returns>
        public ExpressionValue Evaluate(string source, ExpressionContext context)
        {
            return Run(Compile(source), context);
        }

        /// <summary>Executes an expression that may invoke commands.</summary>
        /// <param name="source">Expression source text.</param>
        /// <param name="context">Current variable values and reusable execution stacks.</param>
        /// <returns>The computed result, or a void value for commands.</returns>
        public ExpressionValue Execute(string source, ExpressionContext context)
        {
            return Run(Compile(source, ExpressionMode.Execute), context);
        }

        /// <summary>Runs compiled instructions with a reusable, reentrant stack.</summary>
        /// <param name="expression">Compiled program created by this engine.</param>
        /// <param name="context">Current variable values and reusable execution stacks.</param>
        /// <returns>The final stack value.</returns>
        public ExpressionValue Run(CompiledExpression expression, ExpressionContext context)
        {
            if (!ReferenceEquals(expression.owner, this))
                throw new InvalidOperationException("Compiled expression belongs to a different engine.");

            context.CheckOwner(this);
            ExpressionInstruction[] code = expression.instructions;
            ExpressionValue[] stack = context.Enter(code.Length + 1);
            int top = 0;
            int position = 0;
            try
            {
                for (int index = 0; index < code.Length; index++)
                {
                    ExpressionInstruction instruction = code[index];
                    position = instruction.position;
                    switch (instruction.op)
                    {
                        case ExpressionOp.Literal:
                            stack[top++] = instruction.literal;
                            break;
                        case ExpressionOp.Variable:
                            stack[top++] = context.Read(instruction.operand);
                            break;
                        case ExpressionOp.Call:
                            int start = top - instruction.operand;
                            var arguments = new ExpressionArguments(stack, start, instruction.operand);
                            stack[start] = Call(instruction.method, arguments, expression.mode);
                            top = start + 1;
                            break;
                        case ExpressionOp.AndBranch:
                        case ExpressionOp.OrBranch:
                            bool value = stack[top - 1].AsBoolean();
                            bool skip = instruction.op == ExpressionOp.AndBranch ? !value : value;
                            if (skip)
                                index = instruction.operand - 1;
                            else
                                top--;
                            break;
                        case ExpressionOp.Positive:
                            stack[top - 1].AsNumber();
                            break;
                        case ExpressionOp.Negative:
                            stack[top - 1] = new ExpressionValue(-stack[top - 1].AsNumber());
                            break;
                        case ExpressionOp.Not:
                            stack[top - 1] = new ExpressionValue(!stack[top - 1].AsBoolean());
                            break;
                        case ExpressionOp.Boolean:
                            stack[top - 1].AsBoolean();
                            break;
                        default:
                            ExpressionValue right = stack[--top];
                            stack[top - 1] = Binary(instruction.op, stack[top - 1], right);
                            break;
                    }
                }

                return stack[0];
            }
            catch (Exception error)
            {
                throw new ExpressionException(error.Message, position, error);
            }
            finally
            {
                context.Leave();
            }
        }

        /// <summary>Invokes a registered query or command directly; pass a reusable array to avoid allocations.</summary>
        /// <param name="name">Case-sensitive variable or method identifier.</param>
        /// <param name="arguments">Arguments valid only for the duration of the call.</param>
        /// <returns>The registered method result.</returns>
        public ExpressionValue Invoke(string name, params ExpressionValue[] arguments)
        {
            try
            {
                ExpressionMethod method = FindMethod(name);
                var view = new ExpressionArguments(arguments, 0, arguments.Length);

                return Call(method, view, ExpressionMode.Execute);
            }
            catch (Exception error)
            {
                throw new ExpressionException(error.Message, -1, error);
            }
        }

        /// <summary>Registers a signature and callback without reflection; inactive names may be rebound.</summary>
        /// <param name="name">Case-sensitive variable or method identifier.</param>
        /// <param name="kind">Whether the host method is a query or command.</param>
        /// <param name="result">Declared return type.</param>
        /// <param name="function">Host callback implementation.</param>
        /// <param name="parameters">Ordered argument types, copied during registration.</param>
        public void Register(string name, ExpressionFunctionKind kind, ExpressionValueKind result,
            ExpressionFunction function, params ExpressionValueKind[] parameters)
        {
            ExpressionCompiler.ValidateName(name);
            if (function == null)
                throw new ArgumentException(
                    "A registered method requires a callable implementation.", nameof(function));

            if (kind != ExpressionFunctionKind.Query && kind != ExpressionFunctionKind.Command)
                throw new ArgumentOutOfRangeException(nameof(kind));

            if (result < ExpressionValueKind.Void || result > ExpressionValueKind.String)
                throw new ArgumentOutOfRangeException(nameof(result));

            foreach (ExpressionValueKind parameter in parameters)
            {
                if (parameter <= ExpressionValueKind.Void || parameter > ExpressionValueKind.String)
                    throw new ArgumentException("Method parameters require a concrete value type.");
            }

            if (!methods.TryGetValue(name, out var method))
            {
                method = new ExpressionMethod();
                method.name = name;
                methods.Add(name, method);
            }
            else if (method.function != null)
                throw new InvalidOperationException("Method is already registered: " + name);

            method.kind = kind;
            method.result = result;
            method.parameters = (ExpressionValueKind[])parameters.Clone();
            method.function = function;
        }

        /// <summary>Unregisters a callback, including from previously compiled call sites.</summary>
        /// <returns>Whether an active method was removed.</returns>
        /// <param name="name">Case-sensitive variable or method identifier.</param>
        public bool Unregister(string name)
        {
            if (!methods.TryGetValue(name, out var method) || method.function == null)
                return false;

            method.function = null;
            method.parameters = null;

            return true;
        }

        /// <summary>Evicts source caches; existing compiled handles remain executable.</summary>
        public void ClearCache()
        {
            queries.Clear();
            commands.Clear();
        }

        /// <summary>Removes every method and cache entry while retaining context slot identities.</summary>
        public void Clear()
        {
            foreach (ExpressionMethod method in methods.Values)
            {
                method.function = null;
                method.parameters = null;
            }

            ClearCache();
        }

        /// <summary>Resolves a variable name to a stable indexed slot.</summary>
        /// <param name="name">Case-sensitive variable or method identifier.</param>
        /// <returns>The stable variable index.</returns>
        internal int VariableSlot(string name)
        {
            ExpressionCompiler.ValidateName(name);
            if (!variables.TryGetValue(name, out int slot))
            {
                slot = variables.Count;
                variables.Add(name, slot);
            }

            return slot;
        }

        /// <summary>Finds an active registered method during compilation or direct invocation.</summary>
        /// <param name="name">Case-sensitive variable or method identifier.</param>
        /// <returns>The active method binding.</returns>
        internal ExpressionMethod FindMethod(string name)
        {
            if (!methods.TryGetValue(name, out var method) || method.function == null)
                throw new InvalidOperationException("Method is not registered: " + name);

            return method;
        }

        /// <summary>Checks the live method binding before every call.</summary>
        /// <param name="method">Live registered method slot.</param>
        /// <param name="arguments">Arguments valid only for the duration of the call.</param>
        /// <param name="mode">Permission for query or command execution.</param>
        /// <returns>The type-checked method result.</returns>
        private static ExpressionValue Call(ExpressionMethod method, ExpressionArguments arguments, ExpressionMode mode)
        {
            ExpressionFunction callback = method.function;
            if (callback == null)
                throw new InvalidOperationException("Method is not registered: " + method.name);

            if (mode == ExpressionMode.Evaluate && method.kind == ExpressionFunctionKind.Command)
                throw new InvalidOperationException("Commands are forbidden in Evaluate: " + method.name);

            if (arguments.count != method.parameters.Length)
                throw new InvalidOperationException("Argument count does not match " + method.name);

            for (int index = 0; index < arguments.count; index++)
            {
                if (arguments[index].kind != method.parameters[index])
                    throw new InvalidOperationException("Argument type does not match " + method.name);
            }

            ExpressionValueKind expected = method.result;
            ExpressionValue result = callback(arguments);
            if (result.kind != expected)
                throw new InvalidOperationException("Return type does not match " + method.name);

            return result;
        }

        /// <summary>Applies a strict binary operation without implicit type coercion.</summary>
        /// <param name="op">Operation to execute.</param>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>The computed binary result.</returns>
        private static ExpressionValue Binary(ExpressionOp op, ExpressionValue left, ExpressionValue right)
        {
            if (op == ExpressionOp.Equal || op == ExpressionOp.NotEqual)
            {
                bool equal = left.EqualTo(right);

                return new ExpressionValue(op == ExpressionOp.Equal ? equal : !equal);
            }

            double a = left.AsNumber();
            double b = right.AsNumber();
            switch (op)
            {
                case ExpressionOp.Add: return new ExpressionValue(a + b);
                case ExpressionOp.Subtract: return new ExpressionValue(a - b);
                case ExpressionOp.Multiply: return new ExpressionValue(a * b);
                case ExpressionOp.Divide:
                case ExpressionOp.Modulo:
                    if (b == 0) throw new DivideByZeroException("Expression divisor is zero.");

                    return new ExpressionValue(op == ExpressionOp.Divide ? a / b : a % b);
                case ExpressionOp.Less: return new ExpressionValue(a < b);
                case ExpressionOp.LessEqual: return new ExpressionValue(a <= b);
                case ExpressionOp.Greater: return new ExpressionValue(a > b);
                case ExpressionOp.GreaterEqual: return new ExpressionValue(a >= b);
                default: throw new InvalidOperationException("Unsupported instruction.");
            }
        }

        /// <summary>Registers the standard finite numeric functions.</summary>
        private void RegisterMath()
        {
            Register("min", ExpressionFunctionKind.Query, ExpressionValueKind.Number, Minimum,
                ExpressionValueKind.Number, ExpressionValueKind.Number);
            Register("max", ExpressionFunctionKind.Query, ExpressionValueKind.Number, Maximum,
                ExpressionValueKind.Number, ExpressionValueKind.Number);
            Register("clamp", ExpressionFunctionKind.Query, ExpressionValueKind.Number, Clamp,
                ExpressionValueKind.Number, ExpressionValueKind.Number, ExpressionValueKind.Number);
            Register("abs", ExpressionFunctionKind.Query, ExpressionValueKind.Number, Absolute,
                ExpressionValueKind.Number);
        }

        /// <summary>Returns the smaller operand.</summary>
        /// <param name="arguments">Arguments valid only for the duration of the call.</param>
        /// <returns>The smaller numeric operand.</returns>
        private static ExpressionValue Minimum(ExpressionArguments arguments)
        {
            return new ExpressionValue(Math.Min(arguments[0].AsNumber(), arguments[1].AsNumber()));
        }

        /// <summary>Returns the larger operand.</summary>
        /// <param name="arguments">Arguments valid only for the duration of the call.</param>
        /// <returns>The larger numeric operand.</returns>
        private static ExpressionValue Maximum(ExpressionArguments arguments)
        {
            return new ExpressionValue(Math.Max(arguments[0].AsNumber(), arguments[1].AsNumber()));
        }

        /// <summary>Clamps a value to an ordered inclusive interval.</summary>
        /// <param name="arguments">Arguments valid only for the duration of the call.</param>
        /// <returns>The value restricted to the specified bounds.</returns>
        private static ExpressionValue Clamp(ExpressionArguments arguments)
        {
            double value = arguments[0].AsNumber();
            double lower = arguments[1].AsNumber();
            double upper = arguments[2].AsNumber();
            if (lower > upper)
                throw new ArgumentException("Clamp lower bound exceeds its upper bound.");

            return new ExpressionValue(Math.Min(upper, Math.Max(lower, value)));
        }

        /// <summary>Returns the absolute numeric value.</summary>
        /// <param name="arguments">Arguments valid only for the duration of the call.</param>
        /// <returns>The absolute numeric value.</returns>
        private static ExpressionValue Absolute(ExpressionArguments arguments)
        {
            return new ExpressionValue(Math.Abs(arguments[0].AsNumber()));
        }
    }
}
