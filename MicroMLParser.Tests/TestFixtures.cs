namespace MicroMLParser.Tests
{
    public static class TestFixtures
    {
        public static class Expressions
        {
            public const string Simple = "42";
            public const string Variable = "x";
            public const string BinaryOp = "x + y";
            public const string ComplexExpression = "a * (b + c) / d";
        }

        public static class Functions
        {
            public const string Simple = "fn x -> x + 1";
            public const string MultiParam = "fn a b c -> a + b + c";
            public const string Nested = "fn f g x -> f (g x)";
        }

        public static class Conditionals
        {
            public const string Simple = "if a > b then a else b";
            public const string Nested = "if a > b then if a > c then a else c else b";
        }

        public static class HigherOrder
        {
            public const string Twice = "let twice = fn f x -> f (f x) in twice";
            public const string Compose = "let compose = fn f g x -> f (g x) in compose";
        }
    }
}