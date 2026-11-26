using System;
using System.Diagnostics;
using Oathx.Expression;

/// <summary>Dependency-free behavioral and allocation checks shared with the Unity probe.</summary>
public static class ExpressionChecks
{
    private static int checks;
    private static int calls;
    private static ExpressionEngine engine;
    private static ExpressionContext context;

    /// <summary>Runs representative task, formula, cache and method lifecycle checks.</summary>
    public static string RunAll()
    {
        checks = 0;
        calls = 0;
        engine = new ExpressionEngine();
        context = engine.CreateContext();
        context.Set("hp", new ExpressionValue(100.0));
        context.Set("maxHp", new ExpressionValue(200.0));
        Number("hp * 0.1", 10);
        CompiledExpression program = engine.Compile("hp * 0.1");
        long compiled = engine.compilationCount;
        context.Set("hp", new ExpressionValue(250.0));
        Number("hp * 0.1", 25);
        Check(ReferenceEquals(program, engine.Compile("hp * 0.1")), "Cache identity");
        Check(engine.compilationCount == compiled, "Parameter changes do not parse");
        Number("2 + 3 * 4", 14);
        Number("(2 + 3) * 4", 20);
        Number("20 / 2 / 2", 5);
        Number("-2 * -3 + +4", 10);
        Number("17 % 5", 2);
        Number(".5 + 1e-2 + 2E+1", 20.51);
        Number("min(abs(-20), max(2, 5))", 5);
        Number("clamp(hp, 0, maxHp)", 200);
        Boolean("false || true && false", false);
        Boolean("!(1 < 2) || 2 >= 2 && 3 != 4", true);
        Boolean("false && missing > 0", false);
        Boolean("true || 1 / 0 > 1", true);
        Boolean("false && true || true && true", true);
        Boolean("'hero,one' == \"hero,one\"", true);
        Boolean("'a\\n\\u4E2D' == \"a\\n中\"", true);
        Boolean("2 <= 2 && 2 > 1 && 1 == 1", true);

        engine.Register("Count", ExpressionFunctionKind.Query, ExpressionValueKind.Number, Count);
        Boolean("false && Count() > 0", false);
        Boolean("true || Count() > 0", true);
        Check(calls == 0, "Skipped functions must not run");
        Number("Count() + Count()", 3);
        engine.Register("Heal", ExpressionFunctionKind.Command, ExpressionValueKind.Number, Heal,
            ExpressionValueKind.Number);
        Throws(delegate { engine.Evaluate("Heal(10)", context); }, "Pure mode rejects commands");
        Check(engine.Execute("Heal(hp * 0.1)", context).AsNumber() == 25, "Command expression");
        Check(engine.Execute("{\"Heal\", hp * 0.1}", context).AsNumber() == 25, "Command tuple");
        Number("{\"min\", 3, max(4, 5)}", 3);
        Check(engine.Execute("{\"Heal\", {\"max\", 2, 6}}", context).AsNumber() == 6, "Nested tuple");
        Throws(delegate { engine.Evaluate("{\"Heal\", 1}", context); }, "Tuple respects query permission");
        CompiledExpression tuple = engine.Compile("{\"Heal\", hp * 0.1}", ExpressionMode.Execute);
        Check(ReferenceEquals(tuple, engine.Compile(tuple.source, ExpressionMode.Execute)), "Tuple cache");
        Check(engine.Invoke("Heal", new ExpressionValue(12.0)).AsNumber() == 12, "Direct invocation");
        Throws(delegate { engine.Compile("Heal()"); }, "Arity");
        Throws(delegate { engine.Execute("Heal(true)", context); }, "Argument type");

        engine.Register("Nested", ExpressionFunctionKind.Query, ExpressionValueKind.Number, Nested);
        Number("10 + Nested() * 2", 60);
        engine.Register("Broken", ExpressionFunctionKind.Query, ExpressionValueKind.Number, Broken);
        Throws(delegate { engine.Evaluate("1 + Broken()", context); }, "Host exception");
        Number("hp * 0.1", 25);
        engine.Register("WrongResult", ExpressionFunctionKind.Query, ExpressionValueKind.Number, WrongResult);
        Throws(delegate { engine.Evaluate("WrongResult()", context); }, "Return type");

        CompiledExpression method = engine.Compile("Count()");
        long before = engine.compilationCount;
        Check(engine.Unregister("Count"), "Unregister");
        Check(!engine.Unregister("Count"), "Repeated unregister");
        Throws(delegate { engine.Run(method, context); }, "Cached calls observe unregister");
        engine.Register("Count", ExpressionFunctionKind.Query, ExpressionValueKind.Number, FortyTwo);
        Number("Count()", 42);
        Check(engine.compilationCount == before, "Rebind without parsing");
        Throws(delegate
        {
            engine.Register("Count", ExpressionFunctionKind.Query, ExpressionValueKind.Number, FortyTwo);
        }, "Duplicate registration");
        engine.Unregister("Count");
        engine.Register("Count", ExpressionFunctionKind.Command, ExpressionValueKind.Number, FortyTwo);
        Throws(delegate { engine.Run(method, context); }, "Rebind cannot bypass pure mode");

        string[] invalid = { "", "1 +", "(1 + 2", "1 2", "1e+", "1..2", "1 = 2", "1 & 2",
            "'unterminated", "'\\q'", "min(1,)", "min(1)", "unknown()", "1e999", "2 ** 3",
            "{min, 1, 2}", "{\"min\",}", "{\"min\" 1, 2}", "{\"min\", 1, 2", "{\"min\", 1, 2,}" };
        foreach (string source in invalid)
            Throws(delegate { engine.Compile(source); }, "Invalid syntax: " + source);

        string[] badValues = { "1 / 0", "3 % 0", "1 + true", "true == 1", "missing", "true && 2",
            "1e308 * 1e308", "clamp(1, 10, 0)" };
        foreach (string source in badValues)
            Throws(delegate { engine.Evaluate(source, context); }, "Invalid value: " + source);

        try
        {
            engine.Evaluate("10 + (3 / 0)", context);
            throw new Exception("Expected located failure");
        }
        catch (ExpressionException error)
        {
            Check(error.position == 8, "Runtime operator position");
        }

        var other = new ExpressionEngine();
        Throws(delegate { other.Run(program, other.CreateContext()); }, "Compiled owner");
        Throws(delegate { engine.Run(program, other.CreateContext()); }, "Context owner");
        ExpressionContext second = engine.CreateContext();
        second.Set("hp", new ExpressionValue(500.0));
        Check(engine.Run(program, second).AsNumber() == 50, "Independent parameter context");
        second.Clear();
        Throws(delegate { engine.Run(program, second); }, "Cleared parameter");

        var random = new Random(173);
        CompiledExpression formula = engine.Compile("(a + b * 2) / c - a % c");
        for (int index = 0; index < 1000; index++)
        {
            double a = random.Next(-1000, 1000);
            double b = random.Next(-1000, 1000);
            double c = random.Next(1, 50);
            context.Set("a", new ExpressionValue(a));
            context.Set("b", new ExpressionValue(b));
            context.Set("c", new ExpressionValue(c));
            double expected = (a + b * 2) / c - a % c;
            Check(Math.Abs(engine.Run(formula, context).AsNumber() - expected) < 0.0000001, "Formula oracle");
        }

        for (int index = 0; index < 10000; index++) engine.Run(program, context);
        const int iterations = 100000;
        long allocation = GC.GetAllocatedBytesForCurrentThread();
        var timer = Stopwatch.StartNew();
        long timerAllocation = GC.GetAllocatedBytesForCurrentThread();
        double sum = 0;
        for (int index = 0; index < iterations; index++) sum += engine.Run(program, context).AsNumber();
        long bytes = GC.GetAllocatedBytesForCurrentThread() - timerAllocation;
        timer.Stop();
        Check(sum == iterations * 25.0, "Benchmark result");
        Check(bytes == 0, "Warm execution must allocate zero managed bytes");
        string benchmark = "checks=" + checks + "; iterations=" + iterations + "; elapsedMs="
            + timer.Elapsed.TotalMilliseconds + "; executionAllocatedBytes=" + bytes
            + "; timerAllocatedBytes=" + (timerAllocation - allocation);

        engine.ClearCache();
        Check(engine.cacheCount == 0, "Clear cache");
        Check(engine.Run(program, context).AsNumber() == 25, "Eviction keeps held handle valid");
        Check(!ReferenceEquals(program, engine.Compile("hp * 0.1")), "Eviction allows new compilation");
        engine.Clear();
        Check(engine.cacheCount == 0, "Clear all");
        Throws(delegate { engine.Invoke("abs", new ExpressionValue(1.0)); }, "Clear includes builtins");
        Throws(delegate { engine.Run(method, context); }, "Clear invalidates old method handles");
        engine.Register("Count", ExpressionFunctionKind.Query, ExpressionValueKind.Number, FortyTwo);
        Check(engine.Run(method, context).AsNumber() == 42, "Rebind after clear");

        return "PASS " + benchmark + "; totalChecks=" + checks;
    }

    private static void Check(bool condition, string message)
    {
        checks++;
        if (!condition) throw new Exception("FAILED: " + message);
    }

    private static void Number(string source, double expected)
    {
        Check(Math.Abs(engine.Evaluate(source, context).AsNumber() - expected) < 0.0000001, source);
    }

    private static void Boolean(string source, bool expected)
    {
        Check(engine.Evaluate(source, context).AsBoolean() == expected, source);
    }

    private static void Throws(Action action, string description)
    {
        bool failed = false;
        try { action(); }
        catch (ExpressionException) { failed = true; }
        catch (InvalidOperationException) { failed = true; }
        catch (ArgumentException) { failed = true; }
        Check(failed, description);
    }

    private static ExpressionValue Count(ExpressionArguments arguments)
    {
        return new ExpressionValue((double)++calls);
    }

    private static ExpressionValue FortyTwo(ExpressionArguments arguments)
    {
        return new ExpressionValue(42.0);
    }

    private static ExpressionValue Heal(ExpressionArguments arguments)
    {
        return arguments[0];
    }

    private static ExpressionValue Nested(ExpressionArguments arguments)
    {
        return engine.Evaluate("hp * 0.1", context);
    }

    private static ExpressionValue Broken(ExpressionArguments arguments)
    {
        throw new Exception("Host method failed");
    }

    private static ExpressionValue WrongResult(ExpressionArguments arguments)
    {
        return new ExpressionValue(true);
    }
}
