using PolSkrypt.Constants;
using PolSkrypt.Errors.Exceptions;
using System.Reflection.PortableExecutable;
using System.Text;

namespace PolSkrypt.Interpretation.NativeObjects
{
    // Claude: generating class
    public class PFile : IDisposable
    {
        private static readonly UTF8Encoding Utf8 = new(false);

        private readonly FileStream _stream;
        public bool Closed { get; private set; }

        public long Length { get { EnsureOpen(); return _stream.Length; } }

        public PFile(FileStream stream)
        {
            _stream = stream;
        }

        public string? ReadLine()
        {
            EnsureOpen(); RequireReader();

            bool atStart = _stream.Position == 0;
            var buffer = new List<byte>();
            int b;

            while ((b = _stream.ReadByte()) != -1)
            {
                if (b == '\n')
                {
                    if (buffer.Count > 0 && buffer[^1] == '\r')
                        buffer.RemoveAt(buffer.Count - 1);

                    return Decode(buffer, atStart);
                }
                buffer.Add((byte)b);
            }

            return buffer.Count == 0 ? null : Decode(buffer, atStart);
        }

        public string ReadAll()
        {
            EnsureOpen(); RequireReader();

            bool atStart = _stream.Position == 0;
            using var ms = new MemoryStream();
            _stream.CopyTo(ms);

            return Decode(ms.ToArray(), atStart);
        }

        public void Write(string text)
        {
            EnsureOpen(); RequireWriter();

            var bytes = Utf8.GetBytes(text);
            _stream.Write(bytes, 0, bytes.Length);
            _stream.Flush();
        }

        public bool Eof()
        {
            EnsureOpen(); RequireReader();
            return _stream.Position >= _stream.Length;
        }

        public long Tell()
        {
            EnsureOpen();
            return _stream.Position;
        }

        public void Seek(long pos)
        {
            EnsureOpen();
            if (pos < 0)
                throw new RuntimeException("Pozycja nie może być ujemna.");

            _stream.Seek(pos, SeekOrigin.Begin);
        }

        public void Rewind() => Seek(0);

        public void Close()
        {
            if (Closed) return;
            Closed = true;

            try { _stream.Flush(); }
            finally { _stream.Dispose(); }
        }

        public void Flush()
        {
            EnsureOpen();
            _stream.Flush();
        }

        public void Truncate()
        {
            EnsureOpen(); RequireWriter();
            _stream.SetLength(_stream.Position);
        }

        public string ReadChars(int count)
        {
            EnsureOpen(); RequireReader();

            bool atStart = _stream.Position == 0;
            var bytes = new List<byte>();
            int charsRead = 0;

            while (charsRead < count)
            {
                long posBeforeChar = _stream.Position;
                int b = _stream.ReadByte();
                if (b == -1) break;
                bytes.Add((byte)b);

                int extra = (b & 0xE0) == 0xC0 ? 1
                          : (b & 0xF0) == 0xE0 ? 2
                          : (b & 0xF8) == 0xF0 ? 3
                          : 0;

                for (int j = 0; j < extra; j++)
                {
                    int next = _stream.ReadByte();
                    if (next == -1) break;
                    bytes.Add((byte)next);
                }

                if (atStart && posBeforeChar == 0)
                {
                    var tempText = Utf8.GetString(bytes.ToArray());
                    if (tempText == "\uFEFF")
                    {
                        bytes.Clear();
                        continue;
                    }
                }

                charsRead++;
            }

            return Decode(bytes, atStart);
        }

        private static string Decode(IEnumerable<byte> bytes, bool atFileStart)
        {
            var text = Utf8.GetString(bytes.ToArray());

            if (atFileStart && text.Length > 0 && text[0] == '\uFEFF')
                text = text[1..];

            return text;
        }

        private void EnsureOpen()
        {
            if (Closed)
                throw new RuntimeException("Plik jest zamknięty.");
        }

        private void RequireReader()
        {
            if (!_stream.CanRead)
                throw new RuntimeException("Plik nie jest otwarty w trybie czytania.");
        }

        private void RequireWriter()
        {
            if (!_stream.CanWrite)
                throw new RuntimeException("Plik nie jest otwarty w trybie zapisu.");
        }


        public void Dispose() => Close();

        public override string ToString() => Keywords.FILE_STRING;
    }
}
