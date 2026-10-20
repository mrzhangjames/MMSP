using System.Windows;
using System.Windows.Media;

namespace MMSP.App.WpfBase.Services
{
    /// <summary>
    /// 用 Code 39 画出扫码结果。只负责显示，不连接扫码枪。
    /// </summary>
    public sealed class BarcodeBlock : FrameworkElement
    {
        private static readonly Dictionary<char, string> Patterns = new()
        {
            ['0'] = "000110100",
            ['1'] = "100100001",
            ['2'] = "001100001",
            ['3'] = "101100000",
            ['4'] = "000110001",
            ['5'] = "100110000",
            ['6'] = "001110000",
            ['7'] = "000100101",
            ['8'] = "100100100",
            ['9'] = "001100100",
            ['A'] = "100001001",
            ['B'] = "001001001",
            ['C'] = "101001000",
            ['D'] = "000011001",
            ['E'] = "100011000",
            ['F'] = "001011000",
            ['G'] = "000001101",
            ['H'] = "100001100",
            ['I'] = "001001100",
            ['J'] = "000011100",
            ['K'] = "100000011",
            ['L'] = "001000011",
            ['M'] = "101000010",
            ['N'] = "000010011",
            ['O'] = "100010010",
            ['P'] = "001010010",
            ['Q'] = "000000111",
            ['R'] = "100000110",
            ['S'] = "001000110",
            ['T'] = "000010110",
            ['U'] = "110000001",
            ['V'] = "011000001",
            ['W'] = "111000000",
            ['X'] = "010010001",
            ['Y'] = "110010000",
            ['Z'] = "011010000",
            ['-'] = "010000101",
            ['.'] = "110000100",
            [' '] = "011000100",
            ['*'] = "010010100",
            ['$'] = "010101000",
            ['/'] = "010100010",
            ['+'] = "010001010",
            ['%'] = "000101010"
        };

        /// <summary>
        /// 要画成条码的文字。
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(BarcodeBlock),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// 要画成条码的文字。
        /// </summary>
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        /// <summary>
        /// 按字符宽度画出黑条。空内容不画。
        /// </summary>
        protected override void OnRender(DrawingContext drawingContext)
        {
            var units = Encode(Text);
            if (units.Count == 0 || ActualWidth <= 1 || ActualHeight <= 1)
                return;

            var total = 0;
            foreach (var unit in units)
                total += unit.Width;

            var quiet = 16d;
            var usable = Math.Max(0, ActualWidth - quiet * 2);
            if (usable <= 0 || total == 0)
                return;

            var module = usable / total;
            var drawn = module * total;
            var x = (ActualWidth - drawn) / 2;
            var brush = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
            brush.Freeze();
            foreach (var unit in units)
            {
                var width = module * unit.Width;
                if (unit.Bar)
                    drawingContext.DrawRectangle(brush, null, new Rect(x, 0, width, ActualHeight));

                x += width;
            }
        }

        /// <summary>
        /// 编成窄条和宽条。不能编码的字符跳过。前后加起止符。
        /// </summary>
        private static List<BarUnit> Encode(string? text)
        {
            var source = (text ?? "").Trim().ToUpperInvariant();
            var units = new List<BarUnit>();
            if (source.Length == 0)
                return units;

            Append(units, '*');
            foreach (var ch in source)
            {
                if (ch == '*' || !Patterns.ContainsKey(ch))
                    continue;

                units.Add(new BarUnit(false, 1));
                Append(units, ch);
            }

            units.Add(new BarUnit(false, 1));
            Append(units, '*');
            return units;
        }

        /// <summary>
        /// 追加一个字符。条和空交替，1 是宽，0 是窄。
        /// </summary>
        private static void Append(List<BarUnit> units, char symbol)
        {
            var bits = Patterns[symbol];
            for (var index = 0; index < bits.Length; index++)
                units.Add(new BarUnit(index % 2 == 0, bits[index] == '1' ? 3 : 1));
        }

        /// <summary>
        /// 一条或一段空白，宽度按窄条的倍数计。
        /// </summary>
        private readonly record struct BarUnit(bool Bar, int Width);
    }
}
