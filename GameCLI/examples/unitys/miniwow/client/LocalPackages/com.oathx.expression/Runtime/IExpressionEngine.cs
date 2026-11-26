using System;

namespace Oathx.Expression
{
    /// <summary>Controls whether side-effecting methods may be called.</summary>
    public enum ExpressionMode
    {
        /// <summary>Only registered query functions may run.</summary>
        Evaluate,
        /// <summary>Registered queries and commands may run.</summary>
        Execute
    }

    /// <summary>Declares a registered method's side-effect contract.</summary>
    public enum ExpressionFunctionKind
    {
        /// <summary>A function declared by its host to have no side effects.</summary>
        Query,
        /// <summary>A method permitted to change host state.</summary>
        Command
    }

    /// <summary>Calls a host method with a temporary read-only argument view.</summary>
    /// <param name="arguments">Arguments valid only during this invocation.</param>
    /// <returns>The declared result, or default for void commands.</returns>
    public delegate ExpressionValue ExpressionFunction(ExpressionArguments arguments);

    /// <summary>A non-allocating view of invocation arguments; never retain it.</summary>
    public readonly struct ExpressionArguments
    {
        /// <summary>Backing execution stack.</summary>
        private readonly ExpressionValue[] values;

        /// <summary>First argument position.</summary>
        private readonly int offset;

        /// <summary>Argument count.</summary>
        public int count
        { get; }

        /// <summary>Reads an argument within this invocation.</summary>
        /// <param name="index">Zero-based argument index.</param>
        /// <returns>The argument value.</returns>
        public ExpressionValue this[int index]
        {
            get
            {
                if (index < 0 || index >= count)
                    throw new IndexOutOfRangeException("Argument index is outside this invocation.");

                return values[offset + index];
            }
        }

        /// <summary>Creates an invocation view.</summary>
        /// <param name="values">Backing argument array.</param>
        /// <param name="offset">First argument offset.</param>
        /// <param name="count">Number of arguments in the view.</param>
        internal ExpressionArguments(ExpressionValue[] values, int offset, int count)
        {
            this.values = values;
            this.offset = offset;
            this.count = count;
        }
    }

    /// <summary>Public expression, registration and cache operations.</summary>
    public interface IExpressionEngine
    {
        /// <summary>Creates mutable parameters owned by this engine.</summary>
        /// <returns>An independent parameter context.</returns>
        ExpressionContext CreateContext();

        /// <summary>Compiles once per exact source and execution mode.</summary>
        /// <param name="source">Expression source text.</param>
        /// <param name="mode">Permission for query or command execution.</param>
        /// <returns>The immutable compiled program.</returns>
        CompiledExpression Compile(string source, ExpressionMode mode = ExpressionMode.Evaluate);

        /// <summary>Evaluates cached text with current parameters, without commands.</summary>
        /// <param name="source">Expression source text.</param>
        /// <param name="context">Current variable values and reusable execution stacks.</param>
        /// <returns>The computed typed value.</returns>
        ExpressionValue Evaluate(string source, ExpressionContext context);

        /// <summary>Executes cached text with current parameters and command permission.</summary>
        /// <param name="source">Expression source text.</param>
        /// <param name="context">Current variable values and reusable execution stacks.</param>
        /// <returns>The computed result, or a void value for commands.</returns>
        ExpressionValue Execute(string source, ExpressionContext context);

        /// <summary>Runs a previously compiled handle using its original execution mode.</summary>
        /// <param name="expression">Compiled program created by this engine.</param>
        /// <param name="context">Current variable values and reusable execution stacks.</param>
        /// <returns>The final stack value.</returns>
        ExpressionValue Run(CompiledExpression expression, ExpressionContext context);

        /// <summary>Calls a registered method directly without parsing source text.</summary>
        /// <param name="name">Case-sensitive variable or method identifier.</param>
        /// <param name="arguments">Arguments valid only for the duration of the call.</param>
        /// <returns>The registered method result.</returns>
        ExpressionValue Invoke(string name, params ExpressionValue[] arguments);

        /// <summary>Registers a method; duplicate active names are rejected.</summary>
        /// <param name="name">Case-sensitive variable or method identifier.</param>
        /// <param name="kind">Whether the host method is a query or command.</param>
        /// <param name="result">Declared return type.</param>
        /// <param name="function">Host callback implementation.</param>
        /// <param name="parameters">Ordered argument types, copied during registration.</param>
        void Register(string name, ExpressionFunctionKind kind, ExpressionValueKind result,
            ExpressionFunction function, params ExpressionValueKind[] parameters);

        /// <summary>Removes a method and invalidates its cached call sites.</summary>
        /// <param name="name">Case-sensitive variable or method identifier.</param>
        /// <returns>Whether an active registration was removed.</returns>
        bool Unregister(string name);

        /// <summary>Evicts cached source programs without changing methods or context parameters.</summary>
        void ClearCache();

        /// <summary>Clears all methods and cached programs, including built-in functions.</summary>
        void Clear();
    }
}
