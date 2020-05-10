namespace SimpleGraphQL.GraphQLParser
{
    public readonly struct Token
    {
        public Token(TokenKind kind, string value, int start, int end)
        {
            Kind = kind;
            Value = value;
            Start = start;
            End = end;
        }

        public int End { get; }

        public TokenKind Kind { get; }

        public int Start { get; }

        public string Value { get; }

        public static string GetTokenKindDescription(TokenKind kind)
        {
            switch (kind)
            {
                case TokenKind.EOF:
                    return "EOF";
                case TokenKind.BANG:
                    return "!";
                case TokenKind.DOLLAR:
                    return "$";
                case TokenKind.PAREN_L:
                    return "(";
                case TokenKind.PAREN_R:
                    return ")";
                case TokenKind.SPREAD:
                    return "...";
                case TokenKind.COLON:
                    return ":";
                case TokenKind.EQUALS:
                    return "=";
                case TokenKind.AT:
                    return "@";
                case TokenKind.BRACKET_L:
                    return "[";
                case TokenKind.BRACKET_R:
                    return "]";
                case TokenKind.BRACE_L:
                    return "{";
                case TokenKind.PIPE:
                    return "|";
                case TokenKind.BRACE_R:
                    return "}";
                case TokenKind.NAME:
                    return "Name";
                case TokenKind.INT:
                    return "Int";
                case TokenKind.FLOAT:
                    return "Float";
                case TokenKind.STRING:
                    return "String";
                case TokenKind.COMMENT:
                    return "#";
                default:
                    return string.Empty;
            }
        }

        public override string ToString()
        {
            return Value != null
                ? $"{GetTokenKindDescription(Kind)} \"{Value}\""
                : GetTokenKindDescription(Kind);
        }
    }
}