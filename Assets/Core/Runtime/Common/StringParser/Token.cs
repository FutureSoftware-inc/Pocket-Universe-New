namespace CrystalEngine
{
    /// <summary>
    /// Легковесный токен математического выражения, не выделяющий управляемую память.
    /// <br/><br/>
    /// A lightweight mathematical expression token that does not allocate managed memory.
    /// </summary>
    public struct Token
    {
        public TokenType Type;
        public FunctionType FuncType;
        public Union ConstantValue;
        public string VariableName;
        public VariableContext Context;

        public Token(TokenType type)
        {
            Type = type;
            FuncType = FunctionType.None;
            ConstantValue = default;
            VariableName = null;
            Context = VariableContext.None;
        }
    }

    /// <summary>
    /// Типы лексем, поддерживаемые синтаксическим анализатором выражений.
    /// <br/><br/>
    /// Types of lexemes supported by the expression parser.
    /// </summary>
    public enum TokenType : byte
    {
        Number, Variable, Question, Colon,
        LessThan, GreaterThan, LessOrEqual, GreaterOrEqual, Equal, NotEqual,
        Plus, Minus, Multiply, Divide,
        Function, OpenParenthesis, CloseParenthesis, Comma
    }

    /// <summary>
    /// Поддерживаемые встроенные математические функции движка.
    /// <br/><br/>
    /// Supported built-in math functions of the engine.
    /// </summary>
    public enum FunctionType : byte
    {
        None, Clamp, Min, Max, Round
    }

    /// <summary>
    /// Контекст извлечения переменной в стиле плагина Game Creator 2.
    /// <br/><br/>
    /// Variable retrieval context in the style of Game Creator 2 plugin.
    /// </summary>
    public enum VariableContext : byte
    {
        None, Source, Target
    }
}