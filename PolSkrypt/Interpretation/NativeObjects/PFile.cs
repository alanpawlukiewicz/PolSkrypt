using PolSkrypt.Errors.Exceptions;
using System.Text;

namespace PolSkrypt.Interpretation.NativeObjects
{
    public class PFile : IDisposable
    {
        private readonly FileStream _stream;
        private readonly StreamReader? _reader;
        private readonly StreamWriter? _writer;
        public bool Closed { get; private set; }

        private static readonly UTF8Encoding _utf8 = new(false);

        public PFile(FileStream stream)
        {
            _stream = stream;
            if (stream.CanRead) _reader = new StreamReader(stream, _utf8, false, 1024, leaveOpen: true);
            if (stream.CanWrite) _writer = new StreamWriter(stream, _utf8, 1024, leaveOpen: true);
        }

        public void Write(string text)
        {
            EnsureOpen(); RequireWriter();
            _writer!.Write(text);
        }

        

        public string? ReadLine()
        {
            EnsureOpen(); RequireReader();
            _writer?.Flush();
            var buf = new List<byte>();
            int b;
            while ((b = _stream.ReadByte()) != -1)
            {
                if (b == '\n')
                {
                    if (buf.Count > 0 && buf[^1] == '\r') buf.RemoveAt(buf.Count - 1);
                    return _utf8.GetString(buf.ToArray());
                }
                buf.Add((byte)b);
            }
            return buf.Count == 0 ? null : _utf8.GetString(buf.ToArray());
        }

        public string ReadAll()
        {
            EnsureOpen(); RequireReader();
            _writer?.Flush();
            using var ms = new MemoryStream();
            _stream.CopyTo(ms);
            return _utf8.GetString(ms.ToArray());
        }

        public bool Eof() { EnsureOpen(); RequireReader(); _writer?.Flush(); return _stream.Position >= _stream.Length; }

        public void Seek(long pos) { EnsureOpen(); /*...*/ _writer?.Flush(); _stream.Seek(pos, SeekOrigin.Begin); }

        public void Rewind() => Seek(0);

        public void Close()
        {
            if (Closed) return;
            Closed = true;
            try { _writer?.Flush(); }
            finally { _stream.Dispose(); }
        }

        private void EnsureOpen()
        {
            if (Closed) 
                throw new RuntimeException("Plik jest zamknięty.");
        }
        private void RequireReader()
        {
            if (_reader == null) 
                throw new RuntimeException("Plik nie jest otwarty w trybie czytania.");
        }
        private void RequireWriter()
        {
            if (_writer == null) 
                throw new RuntimeException("Plik nie jest otwarty w trybie zapisu.");
        }

        public void Dispose() => Close();

        public override string ToString()
        {
            return "PLIK";
        }
    }
}
