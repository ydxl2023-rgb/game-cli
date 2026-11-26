using System;
using System.Globalization;

namespace Oathx.Expression
{
    /// <summary>Supported expression value types.</summary>
    public enum ExpressionValueKind
    {
        /// <summary>No return value.</summary>
        Void,
        /// <summary>A finite double precision number.</summary>
        Number,
        /// <summary>A logical value.</summary>
        Boolean,
        /// <summary>An ordinal text value.</summary>
        String
    }

    /// <summary>A typed value that does not box numeric or logical operands.</summary>
    public readonly struct ExpressionValue
    {
        /// <summary>Stored numeric value.</summary>
        private readonly double number;

        /// <summary>Stored logical value.</summary>
        private readonly bool boolean;

        /// <summary>Stored text value.</summary>
        private readonly string text;

        /// <summary>The active value type.</summary>
        public ExpressionValueKind kind
        { get; }

        /// <summary>Creates a finite numeric value.</summary>
        /// <param name="value">Numeric operand.</param>
        public ExpressionValue(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArithmeticException("Expression numbers must be finite.");

            number = value;
            boolean = false;
            text = null;
            kind = ExpressionValueKind.Number;
        }

        /// <summary>Creates a logical value.</summary>
        /// <param name="value">Logical operand.</param>
        public ExpressionValue(bool value)
        {
            number = 0;
            boolean = value;
            text = null;
            kind = ExpressionValueKind.Boolean;
        }

        /// <summary>Creates a text value; null is not an expression literal.</summary>
        /// <param name="value">Text operand.</param>
        public ExpressionValue(string value)
        {
            if (value == null)
                throw new ArgumentException("Use a string value; null is not supported.", nameof(value));

            number = 0;
            boolean = false;
            text = value;
            kind = ExpressionValueKind.String;
        }

        /// <summary>Reads a numeric operand without coercion.</summary>
        /// <returns>The stored number.</returns>
        public double AsNumber()
        {
            Require(ExpressionValueKind.Number);

            return number;
        }

        /// <summary>Reads a logical operand without coercion.</summary>
        /// <returns>The stored boolean.</returns>
        public bool AsBoolean()
        {
            Require(ExpressionValueKind.Boolean);

            return boolean;
        }

        /// <summary>Reads a text operand without coercion.</summary>
        /// <returns>The stored text.</returns>
        public string AsString()
        {
            Require(ExpressionValueKind.String);

            return text;
        }

        /// <summary>Compares values with identical non-void types.</summary>
        /// <param name="other">The second operand.</param>
        /// <returns>Whether the values are equal.</returns>
        public bool EqualTo(ExpressionValue other)
        {
            Require(other.kind);
            switch (kind)
            {
                case ExpressionValueKind.Number: return number == other.number;
                case ExpressionValueKind.Boolean: return boolean == other.boolean;
                case ExpressionValueKind.String: return string.Equals(text, other.text, StringComparison.Ordinal);
                default: throw new InvalidOperationException("Void values cannot be compared.");
            }
        }

        /// <summary>Formats a diagnostic representation using invariant culture.</summary>
        /// <returns>A readable representation.</returns>
        public override string ToString()
        {
            switch (kind)
            {
                case ExpressionValueKind.Number: return number.ToString("R", CultureInfo.InvariantCulture);
                case ExpressionValueKind.Boolean: return boolean ? "true" : "false";
                case ExpressionValueKind.String: return text;
                default: return "void";
            }
        }

        /// <summary>Enforces the requested operand type.</summary>
        /// <param name="expected">Required type.</param>
        private void Require(ExpressionValueKind expected)
        {
            if (kind != expected)
                throw new InvalidOperationException("Expected " + expected + ", received " + kind + ".");
        }
    }
}
