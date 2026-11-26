using System;
using System.Collections.Generic;

namespace Oathx.Expression
{
    /// <summary>Mutable variable slots and reusable, reentrant execution stacks.</summary>
    public sealed class ExpressionContext
    {
        /// <summary>Owner of variable slot identities.</summary>
        private readonly ExpressionEngine engine;

        /// <summary>Current variable values.</summary>
        private ExpressionValue[] values = new ExpressionValue[8];

        /// <summary>Distinguishes missing values from explicit zero values.</summary>
        private bool[] assigned = new bool[8];

        /// <summary>One reusable stack per nested evaluation.</summary>
        private readonly List<ExpressionValue[]> stacks = new List<ExpressionValue[]>();

        /// <summary>Current nested evaluation depth.</summary>
        private int depth;

        /// <summary>Binds a parameter context to its engine.</summary>
        /// <param name="engine">Engine that supplies variable and method bindings.</param>
        internal ExpressionContext(ExpressionEngine engine)
        {
            this.engine = engine;
        }

        /// <summary>Assigns a parameter without recompiling expressions.</summary>
        /// <param name="name">Case-sensitive variable name.</param>
        /// <param name="value">Current typed value.</param>
        public void Set(string name, ExpressionValue value)
        {
            int slot = engine.VariableSlot(name);
            if (slot >= values.Length)
            {
                int length = Math.Max(slot + 1, values.Length * 2);
                Array.Resize(ref values, length);
                Array.Resize(ref assigned, length);
            }

            values[slot] = value;
            assigned[slot] = true;
        }

        /// <summary>Clears parameters while retaining reusable capacity.</summary>
        public void Clear()
        {
            Array.Clear(values, 0, values.Length);
            Array.Clear(assigned, 0, assigned.Length);
        }

        /// <summary>Checks that compiled slots belong to this context.</summary>
        /// <param name="owner">Engine that owns the compiled slot identities.</param>
        internal void CheckOwner(ExpressionEngine owner)
        {
            if (!ReferenceEquals(engine, owner))
                throw new InvalidOperationException("Expression context belongs to a different engine.");
        }

        /// <summary>Reads a required parameter slot.</summary>
        /// <param name="slot">Stable variable slot index.</param>
        /// <returns>The assigned variable value.</returns>
        internal ExpressionValue Read(int slot)
        {
            if (slot >= assigned.Length || !assigned[slot])
                throw new InvalidOperationException("Variable has not been assigned.");

            return values[slot];
        }

        /// <summary>Acquires a reusable stack isolated from enclosing evaluations.</summary>
        /// <param name="capacity">Minimum execution stack capacity.</param>
        /// <returns>An isolated reusable execution stack.</returns>
        internal ExpressionValue[] Enter(int capacity)
        {
            if (depth == stacks.Count)
                stacks.Add(new ExpressionValue[capacity]);
            else if (stacks[depth].Length < capacity)
                stacks[depth] = new ExpressionValue[capacity];

            return stacks[depth++];
        }

        /// <summary>Releases temporary operands even when execution fails.</summary>
        internal void Leave()
        {
            ExpressionValue[] stack = stacks[--depth];
            Array.Clear(stack, 0, stack.Length);
        }
    }
}
