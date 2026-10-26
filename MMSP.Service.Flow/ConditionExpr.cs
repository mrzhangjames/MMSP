using System.Globalization;

namespace MMSP.Service.Flow
{
    /// <summary>
    /// 条件表达式。只做布尔运算，不访问设备。
    /// </summary>
    /// <remarks>
    /// 用法：{变量名} 取已声明变量的当前文本。与、或两侧的名称都会核对，没有这个名称时失败。
    /// <para>{条码} == SN001：文本相等，区分大小写。!= 是文本不等。相等不按数值，10 和 10.0 不相等。</para>
    /// <para>{工位状态} &gt; 9：两边都是有限数字时按数值比，否则按码点比。&gt;=、&lt;、&lt;= 相同。</para>
    /// <para>{条码} 长度 &gt; 8：长度是该文本的字符数，再按上面的规则比较。</para>
    /// <para>{条码} 包含 SN：文本包含，区分大小写。右侧也可以是 {变量名}。</para>
    /// <para>{条码} 长度 &gt; 8 &amp;&amp; {工位状态} == 1：与优先于或。|| 是或，! 是非，可以用括号。</para>
    /// <para>!{工位状态} == 0 表示工位状态不等于 0，非作用在后面的整个比较上。</para>
    /// </remarks>
    public static class ConditionExpr
    {
        /// <summary>
        /// 计算公式。成功时 passed 是结果。失败时 error 是「变量未声明」或「条件无效」。
        /// </summary>
        public static bool TryEval(string? expression, FlowVariables variables, out bool passed, out string error)
        {
            passed = false;
            error = "";
            ArgumentNullException.ThrowIfNull(variables);
            if (!Parser.TryParse(expression ?? "", out var node))
            {
                error = "条件无效";
                return false;
            }

            if (!node.Check(variables))
            {
                error = "变量未声明";
                return false;
            }

            passed = node.Eval(variables);
            return true;
        }

        /// <summary>
        /// 有限数字才按数值比。NaN 和无穷大仍当文本。
        /// </summary>
        private static bool TryNumber(string text, out double value)
        {
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
                && double.IsFinite(value))
                return true;

            value = 0;
            return false;
        }

        /// <summary>
        /// 关系运算。两边都是有限数字时按数值，否则按码点。
        /// </summary>
        private static bool Relate(string left, string right, Kind op)
        {
            if (TryNumber(left, out var a) && TryNumber(right, out var b))
            {
                return op switch
                {
                    Kind.Gt => a > b,
                    Kind.Ge => a >= b,
                    Kind.Lt => a < b,
                    Kind.Le => a <= b,
                    _ => false
                };
            }

            var cmp = string.Compare(left, right, StringComparison.Ordinal);
            return op switch
            {
                Kind.Gt => cmp > 0,
                Kind.Ge => cmp >= 0,
                Kind.Lt => cmp < 0,
                Kind.Le => cmp <= 0,
                _ => false
            };
        }

        /// <summary>
        /// 词法单元种类。
        /// </summary>
        private enum Kind
        {
            End,
            Var,
            Lit,
            Length,
            Contains,
            Eq,
            Ne,
            Gt,
            Ge,
            Lt,
            Le,
            And,
            Or,
            Not,
            LParen,
            RParen
        }

        /// <summary>
        /// 一个词法单元。变量和字面值带文本。
        /// </summary>
        private readonly struct Token
        {
            private Token(Kind kind, string text)
            {
                Kind = kind;
                Text = text;
            }

            public Kind Kind { get; }

            public string Text { get; }

            public static Token Of(Kind kind)
            {
                return new Token(kind, "");
            }

            public static Token TextOf(Kind kind, string text)
            {
                return new Token(kind, text);
            }
        }

        /// <summary>
        /// 把表达式拆成词法单元。拆不开时返回 false。
        /// </summary>
        private static bool TryLex(string text, out List<Token> tokens)
        {
            tokens = [];
            var index = 0;
            while (index < text.Length)
            {
                if (char.IsWhiteSpace(text[index]))
                {
                    index++;
                    continue;
                }

                var current = text[index];
                if (current == '{')
                {
                    var end = text.IndexOf('}', index + 1);
                    if (end < 0)
                        return false;

                    var name = text[(index + 1)..end].Trim();
                    if (name.Length == 0 || name.Contains('{'))
                        return false;

                    tokens.Add(Token.TextOf(Kind.Var, name));
                    index = end + 1;
                    continue;
                }

                if (current == '}')
                    return false;

                if (current == '(')
                {
                    tokens.Add(Token.Of(Kind.LParen));
                    index++;
                    continue;
                }

                if (current == ')')
                {
                    tokens.Add(Token.Of(Kind.RParen));
                    index++;
                    continue;
                }

                if (current is '=' or '!' or '>' or '<' or '&' or '|')
                {
                    if (!TryOperator(text, ref index, out var op))
                        return false;

                    tokens.Add(Token.Of(op));
                    continue;
                }

                var start = index;
                while (index < text.Length && !Breaks(text[index]))
                    index++;

                var word = text[start..index];
                if (word == "长度")
                    tokens.Add(Token.Of(Kind.Length));
                else if (word == "包含")
                    tokens.Add(Token.Of(Kind.Contains));
                else
                    tokens.Add(Token.TextOf(Kind.Lit, word));
            }

            tokens.Add(Token.Of(Kind.End));
            return true;
        }

        /// <summary>
        /// 读一个运算符。单独的 =、&amp;、| 无效。
        /// </summary>
        private static bool TryOperator(string text, ref int index, out Kind kind)
        {
            var current = text[index];
            var next = index + 1 < text.Length ? text[index + 1] : '\0';
            if (current == '=' && next == '=')
            {
                kind = Kind.Eq;
                index += 2;
                return true;
            }

            if (current == '!' && next == '=')
            {
                kind = Kind.Ne;
                index += 2;
                return true;
            }

            if (current == '>' && next == '=')
            {
                kind = Kind.Ge;
                index += 2;
                return true;
            }

            if (current == '<' && next == '=')
            {
                kind = Kind.Le;
                index += 2;
                return true;
            }

            if (current == '&' && next == '&')
            {
                kind = Kind.And;
                index += 2;
                return true;
            }

            if (current == '|' && next == '|')
            {
                kind = Kind.Or;
                index += 2;
                return true;
            }

            if (current == '>')
            {
                kind = Kind.Gt;
                index++;
                return true;
            }

            if (current == '<')
            {
                kind = Kind.Lt;
                index++;
                return true;
            }

            if (current == '!')
            {
                kind = Kind.Not;
                index++;
                return true;
            }

            kind = Kind.End;
            return false;
        }

        /// <summary>
        /// 字面值在空白、括号、花括号或运算符处断开。
        /// </summary>
        private static bool Breaks(char value)
        {
            return char.IsWhiteSpace(value) || value is '{' or '}' or '(' or ')' or '=' or '!' or '>' or '<' or '&' or '|';
        }

        /// <summary>
        /// 布尔式。
        /// </summary>
        private abstract class BoolNode
        {
            public abstract bool Eval(FlowVariables variables);

            public abstract bool Check(FlowVariables variables);
        }

        /// <summary>
        /// 一个文本值。变量可以取长度。
        /// </summary>
        private abstract class ValueNode
        {
            public abstract string Text(FlowVariables variables);

            public abstract bool Check(FlowVariables variables);
        }

        /// <summary>
        /// 递归下降。与优先于或，非作用在后面的比较上。
        /// </summary>
        private sealed class Parser
        {
            private readonly List<Token> _tokens;
            private int _at;

            private Parser(List<Token> tokens)
            {
                _tokens = tokens;
            }

            public static bool TryParse(string text, out BoolNode node)
            {
                node = null!;
                if (!TryLex(text, out var tokens))
                    return false;

                var parser = new Parser(tokens);
                var parsed = parser.ParseOr();
                if (parsed is null || parser.Peek() != Kind.End)
                    return false;

                node = parsed;
                return true;
            }

            private BoolNode? ParseOr()
            {
                var left = ParseAnd();
                if (left is null)
                    return null;

                while (Eat(Kind.Or))
                {
                    var right = ParseAnd();
                    if (right is null)
                        return null;

                    left = new OrNode(left, right);
                }

                return left;
            }

            private BoolNode? ParseAnd()
            {
                var left = ParseNot();
                if (left is null)
                    return null;

                while (Eat(Kind.And))
                {
                    var right = ParseNot();
                    if (right is null)
                        return null;

                    left = new AndNode(left, right);
                }

                return left;
            }

            private BoolNode? ParseNot()
            {
                if (!Eat(Kind.Not))
                    return ParsePrimary();

                var inner = ParseNot();
                return inner is null ? null : new NotNode(inner);
            }

            private BoolNode? ParsePrimary()
            {
                if (Eat(Kind.LParen))
                {
                    var inner = ParseOr();
                    if (inner is null || !Eat(Kind.RParen))
                        return null;

                    return inner;
                }

                return ParseComparison();
            }

            private BoolNode? ParseComparison()
            {
                var left = ParseValue();
                if (left is null)
                    return null;

                if (Eat(Kind.Contains))
                {
                    var right = ParseValue();
                    return right is null ? null : new ContainsNode(left, right);
                }

                if (!TryRelation(out var op))
                    return null;

                var other = ParseValue();
                return other is null ? null : new CompareNode(left, op, other);
            }

            private ValueNode? ParseValue()
            {
                if (Peek() == Kind.Var)
                {
                    var name = Take().Text;
                    var length = Eat(Kind.Length);
                    return new VarNode(name, length);
                }

                if (Peek() != Kind.Lit)
                    return null;

                return new LitNode(Take().Text);
            }

            private bool TryRelation(out Kind op)
            {
                op = Peek();
                if (op is not (Kind.Eq or Kind.Ne or Kind.Gt or Kind.Ge or Kind.Lt or Kind.Le))
                    return false;

                _at++;
                return true;
            }

            private Kind Peek()
            {
                return _tokens[_at].Kind;
            }

            private Token Take()
            {
                return _tokens[_at++];
            }

            private bool Eat(Kind kind)
            {
                if (Peek() != kind)
                    return false;

                _at++;
                return true;
            }
        }

        /// <summary>
        /// 或。两边的变量名都会核对。
        /// </summary>
        private sealed class OrNode : BoolNode
        {
            private readonly BoolNode _left;
            private readonly BoolNode _right;

            public OrNode(BoolNode left, BoolNode right)
            {
                _left = left;
                _right = right;
            }

            public override bool Eval(FlowVariables variables)
            {
                return _left.Eval(variables) || _right.Eval(variables);
            }

            public override bool Check(FlowVariables variables)
            {
                var left = _left.Check(variables);
                var right = _right.Check(variables);
                return left && right;
            }
        }

        /// <summary>
        /// 与。两边的变量名都会核对。
        /// </summary>
        private sealed class AndNode : BoolNode
        {
            private readonly BoolNode _left;
            private readonly BoolNode _right;

            public AndNode(BoolNode left, BoolNode right)
            {
                _left = left;
                _right = right;
            }

            public override bool Eval(FlowVariables variables)
            {
                return _left.Eval(variables) && _right.Eval(variables);
            }

            public override bool Check(FlowVariables variables)
            {
                var left = _left.Check(variables);
                var right = _right.Check(variables);
                return left && right;
            }
        }

        /// <summary>
        /// 非。
        /// </summary>
        private sealed class NotNode : BoolNode
        {
            private readonly BoolNode _inner;

            public NotNode(BoolNode inner)
            {
                _inner = inner;
            }

            public override bool Eval(FlowVariables variables)
            {
                return !_inner.Eval(variables);
            }

            public override bool Check(FlowVariables variables)
            {
                return _inner.Check(variables);
            }
        }

        /// <summary>
        /// 文本包含。
        /// </summary>
        private sealed class ContainsNode : BoolNode
        {
            private readonly ValueNode _left;
            private readonly ValueNode _right;

            public ContainsNode(ValueNode left, ValueNode right)
            {
                _left = left;
                _right = right;
            }

            public override bool Eval(FlowVariables variables)
            {
                return _left.Text(variables).Contains(_right.Text(variables), StringComparison.Ordinal);
            }

            public override bool Check(FlowVariables variables)
            {
                return _left.Check(variables) && _right.Check(variables);
            }
        }

        /// <summary>
        /// 相等、不等或关系比较。
        /// </summary>
        private sealed class CompareNode : BoolNode
        {
            private readonly ValueNode _left;
            private readonly Kind _op;
            private readonly ValueNode _right;

            public CompareNode(ValueNode left, Kind op, ValueNode right)
            {
                _left = left;
                _op = op;
                _right = right;
            }

            public override bool Eval(FlowVariables variables)
            {
                var left = _left.Text(variables);
                var right = _right.Text(variables);
                if (_op == Kind.Eq)
                    return string.Equals(left, right, StringComparison.Ordinal);

                if (_op == Kind.Ne)
                    return !string.Equals(left, right, StringComparison.Ordinal);

                return Relate(left, right, _op);
            }

            public override bool Check(FlowVariables variables)
            {
                return _left.Check(variables) && _right.Check(variables);
            }
        }

        /// <summary>
        /// 已声明变量的当前文本。带长度时改成字符数。
        /// </summary>
        private sealed class VarNode : ValueNode
        {
            private readonly string _name;
            private readonly bool _length;

            public VarNode(string name, bool length)
            {
                _name = name;
                _length = length;
            }

            public override string Text(FlowVariables variables)
            {
                var text = variables.Get(_name);
                return _length ? text.Length.ToString(CultureInfo.InvariantCulture) : text;
            }

            public override bool Check(FlowVariables variables)
            {
                return variables.Contains(_name);
            }
        }

        /// <summary>
        /// 表达式里直接写出的文本。
        /// </summary>
        private sealed class LitNode : ValueNode
        {
            private readonly string _text;

            public LitNode(string text)
            {
                _text = text;
            }

            public override string Text(FlowVariables variables)
            {
                return _text;
            }

            public override bool Check(FlowVariables variables)
            {
                return true;
            }
        }
    }
}
