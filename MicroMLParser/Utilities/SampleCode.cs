using System.Collections.Generic;

namespace MicroMLParser
{
    public static class SampleCode
    {
        /// <summary>
        /// Simple arithmetic expressions
        /// </summary>
        public static string SimpleExpression =>
@"let x = 5 in
let y = 10 in
x + y * 2";

        /// <summary>
        /// Basic function definition and application
        /// </summary>
        public static string BasicFunction =>
@"let add = fn x y -> x + y in
add 5 10";

        /// <summary>
        /// Conditional expression with if-then-else
        /// </summary>
        public static string Conditional =>
@"let max = fn a b -> 
  if a > b then a else b
in
max 42 17";

        /// <summary>
        /// Get all sample code examples
        /// </summary>
        public static Dictionary<string, string> GetAllExamples()
        {
            return new Dictionary<string, string>
            {
                { "Simple Expression", SimpleExpression },
                { "Basic Function", BasicFunction },
                { "Conditional", Conditional }
            };
        }
    }
}