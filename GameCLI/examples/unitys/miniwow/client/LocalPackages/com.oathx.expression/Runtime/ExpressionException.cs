using System;

namespace Oathx.Expression
{
    /// <summary>Reports a compile or evaluation failure at a zero-based source offset.</summary>
    public sealed class ExpressionException : Exception
    {
        /// <summary>Offset in the original expression, or minus one for direct invocation.</summary>
        public int position
        { get; }

        /// <summary>Creates a located expression failure.</summary>
        /// <param name="message">Failure description.</param>
        /// <param name="position">Zero-based source offset.</param>
        /// <param name="inner">Optional originating failure.</param>
        public ExpressionException(string message, int position, Exception inner = null)
            : base(message + " (position " + position + ")", inner)
        {
            this.position = position;
        }
    }
}
