namespace ExpressionEvaluator.Core;

using System.Globalization;

public class Evaluator
{
    public static decimal Evaluate(string infix)
    {
        var tokens = Tokenize(infix);
        var postfix = InfixToPostfix(tokens);
        return EvaluatePostfix(postfix);
    }

    private static List<string> Tokenize(string input)
    {
        var tokens = new List<string>();
        string number = "";

        foreach (char c in input)
        {
            if (c == ' ') continue;

            if (char.IsDigit(c) || c == '.')
            {
                number += c;
            }
            else
            {
                if (number != "")
                {
                    tokens.Add(number);
                    number = "";
                }
                tokens.Add(c.ToString());
            }
        }

        if (number != "")
            tokens.Add(number);

        return tokens;
    }

    private static Queue<string> InfixToPostfix(List<string> tokens)
    {
        var output = new Queue<string>();
        var stack = new Stack<string>();

        foreach (var token in tokens)
        {
            if (decimal.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                output.Enqueue(token);
            }
            else if (token == "(")
            {
                stack.Push(token);
            }
            else if (token == ")")
            {
                while (stack.Count > 0 && stack.Peek() != "(")
                    output.Enqueue(stack.Pop());

                if (stack.Count == 0)
                    throw new Exception("Paréntesis desbalanceados");

                stack.Pop();
            }
            else
            {
                while (stack.Count > 0 &&
                       Priority(stack.Peek()) >= Priority(token))
                {
                    output.Enqueue(stack.Pop());
                }
                stack.Push(token);
            }
        }

        while (stack.Count > 0)
        {
            if (stack.Peek() == "(")
                throw new Exception("Paréntesis desbalanceados");

            output.Enqueue(stack.Pop());
        }

        return output;
    }

    private static int Priority(string op) => op switch
    {
        "^" => 3,
        "*" or "/" => 2,
        "+" or "-" => 1,
        _ => 0
    };

    private static decimal EvaluatePostfix(Queue<string> postfix)
    {
        var stack = new Stack<decimal>();

        while (postfix.Count > 0)
        {
            var token = postfix.Dequeue();

            if (decimal.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal num))
            {
                stack.Push(num);
            }
            else
            {
                if (stack.Count < 2)
                    throw new Exception("Expresión inválida");

                decimal b = stack.Pop();
                decimal a = stack.Pop();

                stack.Push(token switch
                {
                    "+" => a + b,
                    "-" => a - b,
                    "*" => a * b,
                    "/" => b == 0 ? throw new DivideByZeroException() : a / b,
                    "^" => (decimal)Math.Pow((double)a, (double)b),
                    _ => throw new Exception("Operador inválido")
                });
            }
        }

        var result = stack.Pop();
        return result;
    }
}