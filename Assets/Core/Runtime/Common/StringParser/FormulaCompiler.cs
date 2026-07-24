using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;


namespace CrystalEngine
{
    public static class FormulaCompiler
    {
        private static int GetOpPrecedence(TokenType type) => type switch
        {
            TokenType.Question or TokenType.Colon => 1,
            TokenType.LessThan or TokenType.GreaterThan or
            TokenType.LessOrEqual or TokenType.GreaterOrEqual or
            TokenType.Equal or TokenType.NotEqual => 2,
            TokenType.Plus or TokenType.Minus => 3,
            TokenType.Multiply or TokenType.Divide => 4,
            TokenType.Function => 5,
            _ => 0
        };

        public static List<Token> Compile(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression)) return new List<Token>();

            var rawTokens = ArrayPool<Token>.Shared.Rent(expression.Length);
            int tokenCount = 0;
            
            try
            {
                tokenCount = Tokenize(expression.AsSpan(), rawTokens);
                return ShuntingYard(rawTokens.AsSpan(0, tokenCount));
            }
            finally
            {
                ArrayPool<Token>.Shared.Return(rawTokens);
            }
        }

        private static int Tokenize(ReadOnlySpan<char> expr, Span<Token> tokens)
        {
            int i = 0;
            int tokenIdx = 0;

            while (i < expr.Length)
            {
                char c = expr[i];
                if (char.IsWhiteSpace(c)) { i++; continue; }

                // Оптимизированный разбор переменных в стиле Game Creator через ReadOnlySpan (без Substring)
                if (c == 's' || c == 't')
                {
                    var rem = expr.Slice(i);
                    VariableContext context = VariableContext.None;
                    int offset = 0;

                    if (rem.StartsWith("source.stat[")) { context = VariableContext.Source; offset = 12; }
                    else if (rem.StartsWith("target.stat[")) { context = VariableContext.Target; offset = 12; }

                    if (context != VariableContext.None)
                    {
                        i += offset;
                        int start = i;
                        while (i < expr.Length && expr[i] != ']') i++;

                        // Сохраняем имя переменной. На будущее: здесь тоже лучше использовать строковый пул.
                        tokens[tokenIdx++] = new Token(TokenType.Variable)
                        {
                            VariableName = expr.Slice(start, i - start).ToString(),
                            Context = context
                        };
                        i++; // Пропускаем ']'
                        continue;
                    }
                }

                // Парсинг чисел без аллокаций через Int32.TryParse / Float.TryParse по Span
                if (char.IsDigit(c) || (c == '.' && i + 1 < expr.Length && char.IsDigit(expr[i + 1])))
                {
                    int start = i;
                    int dotCount = 0;
                    while (i < expr.Length && (char.IsDigit(expr[i]) || expr[i] == '.'))
                    {
                        if (expr[i] == '.') dotCount++;
                        i++;
                    }

                    var numSpan = expr.Slice(start, i - start);
                    if (dotCount > 1)
                    {
                        Debug.LogError($"[Parser Error] Invalid number format: '{numSpan.ToString()}'");
                        tokens[tokenIdx++] = new Token(TokenType.Number) { ConstantValue = new Union(0) };
                        continue;
                    }

                    Union val = dotCount > 0 ?
                        new Union(float.Parse(numSpan, CultureInfo.InvariantCulture)) :
                        new Union(int.Parse(numSpan));

                    tokens[tokenIdx++] = new Token(TokenType.Number) { ConstantValue = val };
                    continue;
                }

                // Исправленный парсинг функций
                if (char.IsLetter(c))
                {
                    int start = i;
                    while (i < expr.Length && char.IsLetter(expr[i])) i++;
                    var nameSpan = expr.Slice(start, i - start);

                    // Быстрое сравнение без выделения строк в куче через MemoryExtensions.Equals
                    FunctionType fType = FunctionType.None;
                    if (nameSpan.Equals("clamp", StringComparison.OrdinalIgnoreCase)) fType = FunctionType.Clamp;
                    else if (nameSpan.Equals("min", StringComparison.OrdinalIgnoreCase)) fType = FunctionType.Min;
                    else if (nameSpan.Equals("max", StringComparison.OrdinalIgnoreCase)) fType = FunctionType.Max;
                    else if (nameSpan.Equals("round", StringComparison.OrdinalIgnoreCase)) fType = FunctionType.Round;

                    if (fType != FunctionType.None)
                    {
                        tokens[tokenIdx++] = new Token(TokenType.Function) { FuncType = fType };
                    }
                    else
                    {
                        // Если это не функция, трактуем как глобальную/обычную переменную
                        tokens[tokenIdx++] = new Token(TokenType.Variable) { VariableName = nameSpan.ToString() };
                    }
                    continue;
                }

                // Односимвольные лексемы
                switch (c)
                {
                    case '+': tokens[tokenIdx++] = new Token(TokenType.Plus); i++; break;
                    case '-': tokens[tokenIdx++] = new Token(TokenType.Minus); i++; break;
                    case '*': tokens[tokenIdx++] = new Token(TokenType.Multiply); i++; break;
                    case '/': tokens[tokenIdx++] = new Token(TokenType.Divide); i++; break;
                    case '<':
                        if (i + 1 < expr.Length && expr[i + 1] == '=') { tokens[tokenIdx++] = new Token(TokenType.LessOrEqual); i += 2; }
                        else { tokens[tokenIdx++] = new Token(TokenType.LessThan); i++; }
                        break;
                    case '>':
                        if (i + 1 < expr.Length && expr[i + 1] == '=') { tokens[tokenIdx++] = new Token(TokenType.GreaterOrEqual); i += 2; }
                        else { tokens[tokenIdx++] = new Token(TokenType.GreaterThan); i++; }
                        break;
                    case '=':
                        if (i + 1 < expr.Length && expr[i + 1] == '=') i++;
                        tokens[tokenIdx++] = new Token(TokenType.Equal); i++; break;
                    case '!':
                        if (i + 1 < expr.Length && expr[i + 1] == '=') { tokens[tokenIdx++] = new Token(TokenType.NotEqual); i += 2; }
                        else i++; // Избегаем зависания на одиночном '!'
                        break;
                    case '?': tokens[tokenIdx++] = new Token(TokenType.Question); i++; break;
                    case ':': tokens[tokenIdx++] = new Token(TokenType.Colon); i++; break;
                    case '(': tokens[tokenIdx++] = new Token(TokenType.OpenParenthesis); i++; break;
                    case ')': tokens[tokenIdx++] = new Token(TokenType.CloseParenthesis); i++; break;
                    case ',': tokens[tokenIdx++] = new Token(TokenType.Comma); i++; break;
                    default: i++; break;
                }
            }

            return tokenIdx;
        }

        private static List<Token> ShuntingYard(ReadOnlySpan<Token> tokens)
        {
            var output = new List<Token>(tokens.Length);
            // Пул стака для алгоритма, чтобы избежать аллокации класса Stack
            var stackArray = ArrayPool<Token>.Shared.Rent(tokens.Length);
            int stackPtr = 0;

            try
            {
                foreach (ref readonly var token in tokens)
                {
                    if (token.Type == TokenType.Number || token.Type == TokenType.Variable)
                    {
                        output.Add(token);
                    }
                    else if (token.Type == TokenType.Function || token.Type == TokenType.OpenParenthesis)
                    {
                        stackArray[stackPtr++] = token;
                    }
                    else if (token.Type == TokenType.CloseParenthesis)
                    {
                        while (stackPtr > 0 && stackArray[stackPtr - 1].Type != TokenType.OpenParenthesis)
                            output.Add(stackArray[--stackPtr]);

                        if (stackPtr > 0) stackPtr--; // Убираем '('
                        if (stackPtr > 0 && stackArray[stackPtr - 1].Type == TokenType.Function)
                            output.Add(stackArray[--stackPtr]);
                    }
                    else if (token.Type == TokenType.Comma)
                    {
                        while (stackPtr > 0 && stackArray[stackPtr - 1].Type != TokenType.OpenParenthesis)
                            output.Add(stackArray[--stackPtr]);
                    }
                    else
                    {
                        while (stackPtr > 0 && GetOpPrecedence(stackArray[stackPtr - 1].Type) >= GetOpPrecedence(token.Type))
                        {
                            output.Add(stackArray[--stackPtr]);
                        }
                        stackArray[stackPtr++] = token;
                    }
                }

                while (stackPtr > 0) output.Add(stackArray[--stackPtr]);
            }
            finally
            {
                ArrayPool<Token>.Shared.Return(stackArray);
            }

            return output;
        }
    }
}


//using System;
//using System.Collections.Generic;
//using UnityEngine;

//namespace CrystalEngine
//{

//    public static class FormulaCompiler
//    {
//        private static readonly Dictionary<TokenType, int> OpPrecedence = new Dictionary<TokenType, int>
//        {
//            { TokenType.Question, 1 }, { TokenType.Colon, 1 },
//            { TokenType.LessThan, 2 }, { TokenType.GreaterThan, 2 },
//            { TokenType.LessOrEqual, 2 }, { TokenType.GreaterOrEqual, 2 },
//            { TokenType.Equal, 2 }, { TokenType.NotEqual, 2 },
//            { TokenType.Plus, 3 }, { TokenType.Minus, 3 },
//            { TokenType.Multiply, 4 }, { TokenType.Divide, 4 },
//            { TokenType.Function, 5 }
//        };

//        /// <summary>
//        /// Преобразует строковое выражение в оптимизированную постфиксную последовательность токенов.
//        /// <br/><br/>
//        /// Converts a string expression into an optimized postfix token sequence.
//        /// </summary>
//        public static List<Token> Compile(string expression)
//        {
//            return ShuntingYard(Tokenize(expression));
//        }

//        private static List<Token> Tokenize(string expr)
//        {
//            var result = new List<Token>();
//            int i = 0;

//            while (i < expr.Length)
//            {
//                char c = expr[i];
//                if (char.IsWhiteSpace(c)) { i++; continue; }

//                if (c == 's' || c == 't')
//                {
//                    string rem = expr.Substring(i);
//                    VariableContext context = VariableContext.None;
//                    int offset = 0;

//                    if (rem.StartsWith("source.stat[")) { context = VariableContext.Source; offset = 12; }
//                    else if (rem.StartsWith("target.stat[")) { context = VariableContext.Target; offset = 12; }

//                    if (context != VariableContext.None)
//                    {
//                        i += offset;
//                        int start = i;
//                        while (i < expr.Length && expr[i] != ']') i++;
//                        result.Add(new Token(TokenType.Variable) { VariableName = expr.Substring(start, i - start), Context = context });
//                        i++;
//                        continue;
//                    }
//                }

//                if (char.IsDigit(c) || (c == '.' && i + 1 < expr.Length && char.IsDigit(expr[i + 1])))
//                {
//                    int start = i;
//                    int dotCount = 0;
//                    while (i < expr.Length && (char.IsDigit(expr[i]) || expr[i] == '.'))
//                    {
//                        if (expr[i] == '.') dotCount++;
//                        i++;
//                    }
//                    string numStr = expr.Substring(start, i - start);

//                    if (dotCount > 1)
//                    {
//                        Debug.LogError($"[Parser Error] Invalid number format: '{numStr}'");
//                        result.Add(new Token(TokenType.Number) { ConstantValue = new AnyNumber(0) });
//                        continue;
//                    }

//                    AnyNumber val = numStr.Contains(".") ?
//                        new AnyNumber(float.Parse(numStr, System.Globalization.CultureInfo.InvariantCulture)) :
//                        new AnyNumber(int.Parse(numStr));

//                    result.Add(new Token(TokenType.Number) { ConstantValue = val });
//                    continue;
//                }

//                if (char.IsLetter(c))
//                {
//                    int start = i;
//                    while (i < expr.Length && char.IsLetter(expr[i])) i++;
//                    string name = expr.Substring(start, i - start).ToLower();

//                    Token t = new Token(TokenType.Function);
//                    if (name == "clamp") t.FuncType = FunctionType.Clamp;
//                    else if (name == "min") t.FuncType = FunctionType.Min;
//                    else if (name == "max") t.FuncType = FunctionType.Max;
//                    else if (name == "round") t.FuncType = FunctionType.Round;

//                    result.Add(t);
//                    continue;
//                }

//                switch (c)
//                {
//                    case '+': result.Add(new Token(TokenType.Plus)); i++; break;
//                    case '-': result.Add(new Token(TokenType.Minus)); i++; break;
//                    case '*': result.Add(new Token(TokenType.Multiply)); i++; break;
//                    case '/': result.Add(new Token(TokenType.Divide)); i++; break;
//                    case '<':
//                        if (i + 1 < expr.Length && expr[i + 1] == '=') { result.Add(new Token(TokenType.LessOrEqual)); i += 2; }
//                        else { result.Add(new Token(TokenType.LessThan)); i++; }
//                        break;
//                    case '>':
//                        if (i + 1 < expr.Length && expr[i + 1] == '=') { result.Add(new Token(TokenType.GreaterOrEqual)); i += 2; }
//                        else { result.Add(new Token(TokenType.GreaterThan)); i++; }
//                        break;
//                    case '=':
//                        if (i + 1 < expr.Length && expr[i + 1] == '=') i++;
//                        result.Add(new Token(TokenType.Equal)); i++; break;
//                    case '!':
//                        if (i + 1 < expr.Length && expr[i + 1] == '=') { result.Add(new Token(TokenType.NotEqual)); i += 2; }
//                        break;
//                    case '?': result.Add(new Token(TokenType.Question)); i++; break;
//                    case ':': result.Add(new Token(TokenType.Colon)); i++; break;
//                    case '(': result.Add(new Token(TokenType.OpenParenthesis)); i++; break;
//                    case ')': result.Add(new Token(TokenType.CloseParenthesis)); i++; break;
//                    case ',': result.Add(new Token(TokenType.Comma)); i++; break;
//                    default: i++; break;
//                }
//            }
//            return result;
//        }

//        private static List<Token> ShuntingYard(List<Token> tokens)
//        {
//            var output = new List<Token>();
//            var stack = new Stack<Token>();

//            foreach (var token in tokens)
//            {
//                if (token.Type == TokenType.Number || token.Type == TokenType.Variable)
//                {
//                    output.Add(token);
//                }
//                else if (token.Type == TokenType.Function || token.Type == TokenType.OpenParenthesis)
//                {
//                    stack.Push(token);
//                }
//                else if (token.Type == TokenType.CloseParenthesis)
//                {
//                    while (stack.Count > 0 && stack.Peek().Type != TokenType.OpenParenthesis)
//                        output.Add(stack.Pop());

//                    if (stack.Count > 0) stack.Pop();
//                    if (stack.Count > 0 && stack.Peek().Type == TokenType.Function)
//                        output.Add(stack.Pop());
//                }
//                else if (token.Type == TokenType.Comma)
//                {
//                    while (stack.Count > 0 && stack.Peek().Type != TokenType.OpenParenthesis)
//                        output.Add(stack.Pop());
//                }
//                else
//                {
//                    while (stack.Count > 0 && OpPrecedence.ContainsKey(stack.Peek().Type) &&
//                           OpPrecedence[stack.Peek().Type] >= OpPrecedence[token.Type])
//                    {
//                        output.Add(stack.Pop());
//                    }
//                    stack.Push(token);
//                }
//            }

//            while (stack.Count > 0) output.Add(stack.Pop());
//            return output;
//        }
//    }
//}