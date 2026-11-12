using System.Text;

namespace PolSkrypt.Helpers
{
    // Claude: generating class
    public sealed class LineTrackingWriter : TextWriter
    {
        private static LineTrackingWriter? _current;
        private readonly TextWriter _inner;

        private bool AtLineStart { get; set; } = true;

        private LineTrackingWriter(TextWriter inner) => _inner = inner;

        public override Encoding Encoding => _inner.Encoding;

        public static void Install()
        {
            _current = new LineTrackingWriter(Console.Out);
            Console.SetOut(_current);
        }

        public static void EnsureNewLine()
        {
            if (_current is { AtLineStart: false })
            {
                _current.WriteLine();
            }
        }

        public static void MarkLineStart()
        {
            if (_current != null)
            {
                _current.AtLineStart = true;
            }
        }

        public override void Write(char value)
        {
            _inner.Write(value);
            AtLineStart = value == '\n';
        }

        public override void Write(string? value)
        {
            if (string.IsNullOrEmpty(value)) return;
            _inner.Write(value);
            AtLineStart = value[^1] == '\n';
        }

        public override void Write(char[] buffer, int index, int count)
        {
            if (count == 0) return;
            _inner.Write(buffer, index, count);
            AtLineStart = buffer[index + count - 1] == '\n';
        }

        public override void Flush() => _inner.Flush();
    }
}