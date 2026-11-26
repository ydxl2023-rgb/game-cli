namespace Oathx.Expression
{
    /// <summary>Opcodes emitted in postfix order, with branches for lazy boolean operations.</summary>
    internal enum ExpressionOp
    {
        /// <summary>Pushes a constant.</summary>
        Literal,
        /// <summary>Reads a variable slot.</summary>
        Variable,
        /// <summary>Invokes a registered method.</summary>
        Call,
        /// <summary>Checks a numeric unary operand.</summary>
        Positive,
        /// <summary>Negates a number.</summary>
        Negative,
        /// <summary>Negates a boolean.</summary>
        Not,
        /// <summary>Adds numeric operands.</summary>
        Add,
        /// <summary>Subtracts numeric operands.</summary>
        Subtract,
        /// <summary>Multiplies numeric operands.</summary>
        Multiply,
        /// <summary>Divides numeric operands.</summary>
        Divide,
        /// <summary>Computes a numeric remainder.</summary>
        Modulo,
        /// <summary>Compares equality.</summary>
        Equal,
        /// <summary>Compares inequality.</summary>
        NotEqual,
        /// <summary>Compares a strict lower bound.</summary>
        Less,
        /// <summary>Compares an inclusive lower bound.</summary>
        LessEqual,
        /// <summary>Compares a strict upper bound.</summary>
        Greater,
        /// <summary>Compares an inclusive upper bound.</summary>
        GreaterEqual,
        /// <summary>Skips a conjunction's right operand when false.</summary>
        AndBranch,
        /// <summary>Skips a disjunction's right operand when true.</summary>
        OrBranch,
        /// <summary>Checks the remaining logical operand.</summary>
        Boolean
    }

    /// <summary>An internal instruction that never captures parameter values.</summary>
    internal struct ExpressionInstruction
    {
        /// <summary>Operation to execute.</summary>
        internal ExpressionOp op;

        /// <summary>Variable slot, argument count or jump target.</summary>
        internal int operand;

        /// <summary>Original source offset.</summary>
        internal int position;

        /// <summary>Literal payload.</summary>
        internal ExpressionValue literal;

        /// <summary>Live method binding.</summary>
        internal ExpressionMethod method;
    }

    /// <summary>A stable method slot that can be unregistered and rebound.</summary>
    internal sealed class ExpressionMethod
    {
        /// <summary>Case-sensitive registration name.</summary>
        internal string name;

        /// <summary>Active callback, or null after unregistering.</summary>
        internal ExpressionFunction function;

        /// <summary>Side-effect permission.</summary>
        internal ExpressionFunctionKind kind;

        /// <summary>Declared result type.</summary>
        internal ExpressionValueKind result;

        /// <summary>Owned argument signature.</summary>
        internal ExpressionValueKind[] parameters;
    }

    /// <summary>An immutable compiled expression owned by its creating engine.</summary>
    public sealed class CompiledExpression
    {
        /// <summary>Owning engine.</summary>
        internal readonly ExpressionEngine owner;

        /// <summary>Private-to-assembly instruction sequence.</summary>
        internal readonly ExpressionInstruction[] instructions;

        /// <summary>Original source text.</summary>
        public string source
        { get; }

        /// <summary>Permission applied during compilation and every execution.</summary>
        public ExpressionMode mode
        { get; }

        /// <summary>Creates a compiled program without capturing a context.</summary>
        /// <param name="owner">Engine that owns the compiled slot identities.</param>
        /// <param name="source">Expression source text.</param>
        /// <param name="mode">Permission for query or command execution.</param>
        /// <param name="instructions">Compiled postfix and branch instructions.</param>
        internal CompiledExpression(ExpressionEngine owner, string source, ExpressionMode mode,
            ExpressionInstruction[] instructions)
        {
            this.owner = owner;
            this.source = source;
            this.mode = mode;
            this.instructions = instructions;
        }
    }
}
