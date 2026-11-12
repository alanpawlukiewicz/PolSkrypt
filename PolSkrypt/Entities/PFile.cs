using PolSkrypt.Errors.Exceptions;
using System.Text;

namespace PolSkrypt.Entities
{
    public class PFile : IDisposable
    {
        private readonly FileStream _stream;
        private readonly StreamReader? _reader;
        private readonly StreamWriter? _writer;
        public bool Closed { get; private set; }

        public PFile(FileStream stream)
        {
            _stream = stream;
            var enc = new UTF8Encoding(false);
            if (stream.CanRead) _reader = new StreamReader(stream, enc, false, 1024, leaveOpen: true);
            if (stream.CanWrite) _writer = new StreamWriter(stream, enc, 1024, leaveOpen: true);
        }

        public string? ReadLine()
        {
            EnsureOpen(); RequireReader();
            _writer?.Flush();
            return _reader!.ReadLine();
        }

        public void Write(string text)
        {
            EnsureOpen(); RequireWriter();
            _writer!.Write(text);
        }

        public bool Eof()
        {
            EnsureOpen(); RequireReader();
            return _reader!.Peek() == -1;
        }

        public void Seek(long pos)
        {
            EnsureOpen();
            if (pos < 0) 
                throw new RuntimeException("Pozycja nie może być ujemna.");
            _writer?.Flush();
            _reader?.DiscardBufferedData();
            _stream.Seek(pos, SeekOrigin.Begin);
        }

        public void Rewind() => Seek(0);

        public void Close()
        {
            if (Closed) return;
            _writer?.Flush();
            _stream.Dispose();
            Closed = true;
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
