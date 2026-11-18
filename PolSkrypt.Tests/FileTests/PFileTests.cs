using FluentAssertions;
using PolSkrypt.Constants;
using PolSkrypt.Errors.Exceptions;
using PolSkrypt.Interpretation.NativeObjects;
using System.Text;
using Xunit;

namespace PolSkrypt.Tests.FileTests
{
    // Claude: generating class
    /// <summary>
    /// Baza testów: tworzy katalog tymczasowy i sprząta po każdym teście.
    /// </summary>
    public abstract class TempFileTestBase : IDisposable
    {
        private readonly string _dir;
        private readonly List<PFile> _opened = new();

        protected TempFileTestBase()
        {
            _dir = Path.Combine(Path.GetTempPath(), "polskrypt_tests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
        }

        protected string PathFor(string name) => Path.Combine(_dir, name);

        protected string CreateFile(string name, string content)
        {
            var path = PathFor(name);
            File.WriteAllText(path, content, new UTF8Encoding(false));
            return path;
        }

        protected string CreateFileBytes(string name, byte[] content)
        {
            var path = PathFor(name);
            File.WriteAllBytes(path, content);
            return path;
        }

        protected PFile Open(string path, string mode)
        {
            var (fileMode, access) = mode switch
            {
                "r" => (FileMode.Open, FileAccess.Read),
                "w" => (FileMode.Create, FileAccess.Write),
                "a" => (FileMode.Append, FileAccess.Write),
                "rw" => (FileMode.OpenOrCreate, FileAccess.ReadWrite),
                _ => throw new ArgumentException(mode)
            };

            var file = new PFile(File.Open(path, fileMode, access, FileShare.ReadWrite));
            _opened.Add(file);
            return file;
        }

        protected string ReadDisk(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var sr = new StreamReader(fs, new UTF8Encoding(false));
            return sr.ReadToEnd();
        }

        public void Dispose()
        {
            foreach (var f in _opened) f.Dispose();
            try { Directory.Delete(_dir, recursive: true); } catch { /* ignorujemy */ }
        }
    }

    public class PFileTests : TempFileTestBase
    {
        // ---------- ReadLine ----------

        [Fact]
        public void ReadLine_ReturnsLinesSeparatedByLf()
        {
            var f = Open(CreateFile("a.txt", "jeden\ndwa\ntrzy"), "r");

            f.ReadLine().Should().Be("jeden");
            f.ReadLine().Should().Be("dwa");
            f.ReadLine().Should().Be("trzy");
            f.ReadLine().Should().BeNull();
        }

        [Fact]
        public void ReadLine_StripsCarriageReturn()
        {
            var f = Open(CreateFile("a.txt", "jeden\r\ndwa\r\n"), "r");

            f.ReadLine().Should().Be("jeden");
            f.ReadLine().Should().Be("dwa");
            f.ReadLine().Should().BeNull();
        }

        [Fact]
        public void ReadLine_EmptyFile_ReturnsNull()
        {
            var f = Open(CreateFile("a.txt", ""), "r");

            f.ReadLine().Should().BeNull();
        }

        [Fact]
        public void ReadLine_EmptyLineInTheMiddle_ReturnsEmptyString()
        {
            var f = Open(CreateFile("a.txt", "a\n\nb"), "r");

            f.ReadLine().Should().Be("a");
            f.ReadLine().Should().Be("");
            f.ReadLine().Should().Be("b");
            f.ReadLine().Should().BeNull();
        }

        [Fact]
        public void ReadLine_TrailingNewline_DoesNotProduceExtraEmptyLine()
        {
            var f = Open(CreateFile("a.txt", "a\n"), "r");

            f.ReadLine().Should().Be("a");
            f.ReadLine().Should().BeNull();
        }

        [Fact]
        public void ReadLine_PreservesPolishCharacters()
        {
            var f = Open(CreateFile("a.txt", "zażółć gęślą jaźń\nŻÓŁW"), "r");

            f.ReadLine().Should().Be("zażółć gęślą jaźń");
            f.ReadLine().Should().Be("ŻÓŁW");
        }

        [Fact]
        public void ReadLine_StripsBomAtStartOfFile()
        {
            var bytes = new byte[] { 0xEF, 0xBB, 0xBF }
                .Concat(Encoding.UTF8.GetBytes("abc\ndef")).ToArray();
            var f = Open(CreateFileBytes("bom.txt", bytes), "r");

            f.ReadLine().Should().Be("abc");
            f.ReadLine().Should().Be("def");
        }

        // ---------- ReadAll ----------

        [Fact]
        public void ReadAll_ReturnsWholeContentUnchanged()
        {
            var f = Open(CreateFile("a.txt", "a\r\nb\nżółw\n"), "r");

            f.ReadAll().Should().Be("a\r\nb\nżółw\n");
        }

        [Fact]
        public void ReadAll_EmptyFile_ReturnsEmptyString()
        {
            var f = Open(CreateFile("a.txt", ""), "r");

            f.ReadAll().Should().BeEmpty();
        }

        [Fact]
        public void ReadAll_StripsBomAtStart()
        {
            var bytes = new byte[] { 0xEF, 0xBB, 0xBF }
                .Concat(Encoding.UTF8.GetBytes("abc")).ToArray();
            var f = Open(CreateFileBytes("bom.txt", bytes), "r");

            f.ReadAll().Should().Be("abc");
        }

        [Fact]
        public void ReadAll_ReadsOnlyRemainderAfterReadLine()
        {
            var f = Open(CreateFile("a.txt", "a\nb\nc"), "r");

            f.ReadLine();

            f.ReadAll().Should().Be("b\nc");
        }

        // ---------- Write ----------

        [Fact]
        public void Write_InWriteMode_CreatesFileWithContent()
        {
            var path = PathFor("w.txt");
            var f = Open(path, "w");

            f.Write("zażółć");
            f.Close();

            ReadDisk(path).Should().Be("zażółć");
        }

        [Fact]
        public void Write_InWriteMode_TruncatesExistingFile()
        {
            var path = CreateFile("w.txt", "stara długa zawartość");
            var f = Open(path, "w");

            f.Write("nowa");
            f.Close();

            ReadDisk(path).Should().Be("nowa");
        }

        [Fact]
        public void Write_InAppendMode_AppendsToExistingContent()
        {
            var path = CreateFile("a.txt", "abc");
            var f = Open(path, "a");

            f.Write("def");
            f.Close();

            ReadDisk(path).Should().Be("abcdef");
        }

        [Fact]
        public void Write_FlushesImmediately_WithoutClose()
        {
            var path = PathFor("w.txt");
            var f = Open(path, "w");

            f.Write("dane");

            ReadDisk(path).Should().Be("dane");
        }

        [Fact]
        public void Write_InReadMode_Throws()
        {
            var f = Open(CreateFile("a.txt", "abc"), "r");

            var act = () => f.Write("x");

            act.Should().Throw<RuntimeException>();
        }

        [Fact]
        public void ReadLine_InWriteMode_Throws()
        {
            var f = Open(PathFor("w.txt"), "w");

            var act = () => f.ReadLine();

            act.Should().Throw<RuntimeException>();
        }

        [Fact]
        public void ReadAll_InWriteMode_Throws()
        {
            var f = Open(PathFor("w.txt"), "w");

            var act = () => f.ReadAll();

            act.Should().Throw<RuntimeException>();
        }

        // ---------- tryb "rw" (regresja błędu z buforem StreamReadera) ----------

        [Fact]
        public void ReadWrite_WriteAfterReadLine_OverwritesRightAfterReadLine()
        {
            var path = CreateFile("rw.txt", "AAA\nBBB\nCCC");
            var f = Open(path, "rw");

            f.ReadLine().Should().Be("AAA");
            f.Write("XYZ");
            f.Close();

            ReadDisk(path).Should().Be("AAA\nXYZ\nCCC");
        }

        [Fact]
        public void ReadWrite_ReadAfterWrite_ReadsFromPositionAfterWrite()
        {
            var path = CreateFile("rw.txt", "AAA\nBBB\nCCC");
            var f = Open(path, "rw");

            f.Write("XXX");

            f.ReadLine().Should().BeEmpty();   // zostało "\n"
            f.ReadLine().Should().Be("BBB");
        }

        [Fact]
        public void ReadWrite_OpenOrCreate_DoesNotTruncateExistingFile()
        {
            var path = CreateFile("rw.txt", "zawartość");
            var f = Open(path, "rw");

            f.ReadAll().Should().Be("zawartość");
        }

        [Fact]
        public void ReadWrite_CreatesMissingFile()
        {
            var path = PathFor("new.txt");

            var f = Open(path, "rw");
            f.Write("abc");
            f.Close();

            File.Exists(path).Should().BeTrue();
            ReadDisk(path).Should().Be("abc");
        }

        // ---------- Eof ----------

        [Fact]
        public void Eof_EmptyFile_IsTrue()
        {
            var f = Open(CreateFile("a.txt", ""), "r");

            f.Eof().Should().BeTrue();
        }

        [Fact]
        public void Eof_TracksReadingProgress()
        {
            var f = Open(CreateFile("a.txt", "a\nb"), "r");

            f.Eof().Should().BeFalse();
            f.ReadLine();
            f.Eof().Should().BeFalse();
            f.ReadLine();
            f.Eof().Should().BeTrue();
        }

        [Fact]
        public void Eof_InWriteMode_Throws()
        {
            var f = Open(PathFor("w.txt"), "w");

            var act = () => f.Eof();

            act.Should().Throw<RuntimeException>();
        }

        // ---------- Tell / Seek / Rewind / Length ----------

        [Fact]
        public void Tell_StartsAtZero_AndAdvancesWithReading()
        {
            var f = Open(CreateFile("a.txt", "abc\ndef"), "r");

            f.Tell().Should().Be(0);
            f.ReadLine();
            f.Tell().Should().Be(4);
        }

        [Fact]
        public void Tell_CountsBytesNotCharacters()
        {
            var f = Open(CreateFile("a.txt", "ż"), "r");

            f.ReadAll();

            f.Tell().Should().Be(2);
        }

        [Fact]
        public void Seek_MovesToGivenPosition()
        {
            var f = Open(CreateFile("a.txt", "abcdef"), "r");

            f.Seek(3);

            f.Tell().Should().Be(3);
            f.ReadAll().Should().Be("def");
        }

        [Fact]
        public void Seek_Negative_Throws()
        {
            var f = Open(CreateFile("a.txt", "abc"), "r");

            var act = () => f.Seek(-1);

            act.Should().Throw<RuntimeException>();
        }

        [Fact]
        public void Seek_BeyondEnd_AllowsExtendingFileByWriting()
        {
            var path = CreateFile("rw.txt", "ab");
            var f = Open(path, "rw");

            f.Seek(4);
            f.Write("c");
            f.Close();

            File.ReadAllBytes(path).Should().Equal(
                (byte)'a', (byte)'b', (byte)0, (byte)0, (byte)'c');
        }

        [Fact]
        public void Rewind_ReturnsToStart()
        {
            var f = Open(CreateFile("a.txt", "abc\ndef"), "r");
            f.ReadAll();

            f.Rewind();

            f.Tell().Should().Be(0);
            f.ReadLine().Should().Be("abc");
        }

        [Fact]
        public void Length_ReturnsSizeInBytes()
        {
            var f = Open(CreateFile("a.txt", "żółw"), "r");

            f.Length.Should().Be(Encoding.UTF8.GetByteCount("żółw"));
        }

        [Fact]
        public void Length_GrowsAfterWrite()
        {
            var f = Open(PathFor("w.txt"), "w");

            f.Length.Should().Be(0);
            f.Write("abc");
            f.Length.Should().Be(3);
        }

        // ---------- Truncate ----------

        [Fact]
        public void Truncate_CutsFileAtCurrentPosition()
        {
            var path = CreateFile("rw.txt", "AAA\nBBB\nCCC");
            var f = Open(path, "rw");

            f.ReadLine();
            f.Truncate();
            f.Close();

            ReadDisk(path).Should().Be("AAA\n");
        }

        [Fact]
        public void Truncate_InReadMode_Throws()
        {
            var f = Open(CreateFile("a.txt", "abc"), "r");

            var act = () => f.Truncate();

            act.Should().Throw<RuntimeException>();
        }

        // ---------- ReadChars ----------

        [Fact]
        public void ReadChars_ReadsGivenNumberOfCharacters()
        {
            var f = Open(CreateFile("a.txt", "abcdef"), "r");

            f.ReadChars(3).Should().Be("abc");
            f.ReadChars(2).Should().Be("de");
        }

        [Fact]
        public void ReadChars_CountsCharactersNotBytes()
        {
            var f = Open(CreateFile("a.txt", "żółw"), "r");

            f.ReadChars(3).Should().Be("żół");
            f.ReadChars(1).Should().Be("w");
        }

        [Fact]
        public void ReadChars_ZeroCount_ReturnsEmptyString()
        {
            var f = Open(CreateFile("a.txt", "abc"), "r");

            f.ReadChars(0).Should().BeEmpty();
            f.Tell().Should().Be(0);
        }

        [Fact]
        public void ReadChars_MoreThanAvailable_ReturnsWhatIsLeft()
        {
            var f = Open(CreateFile("a.txt", "ab"), "r");

            f.ReadChars(10).Should().Be("ab");
            f.ReadChars(10).Should().BeEmpty();
        }

        [Fact]
        public void ReadChars_HandlesFourByteCharacters()
        {
            var f = Open(CreateFile("a.txt", "a😀b"), "r");

            f.ReadChars(1).Should().Be("a");
            f.ReadChars(1).Should().Be("😀");
            f.ReadChars(1).Should().Be("b");
        }

        [Fact]
        public void ReadChars_StripsBomAtStart()
        {
            var bytes = new byte[] { 0xEF, 0xBB, 0xBF }
                .Concat(Encoding.UTF8.GetBytes("abc")).ToArray();
            var f = Open(CreateFileBytes("bom.txt", bytes), "r");

            f.ReadChars(3).Should().Be("abc");
        }

        // ---------- Close / Dispose / Flush ----------

        [Fact]
        public void Closed_IsFalseInitially_TrueAfterClose()
        {
            var f = Open(CreateFile("a.txt", "abc"), "r");

            f.Closed.Should().BeFalse();
            f.Close();
            f.Closed.Should().BeTrue();
        }

        [Fact]
        public void Close_CalledTwice_DoesNotThrow()
        {
            var f = Open(CreateFile("a.txt", "abc"), "r");
            f.Close();

            var act = () => f.Close();

            act.Should().NotThrow();
        }

        [Fact]
        public void Dispose_ClosesFile()
        {
            var f = Open(CreateFile("a.txt", "abc"), "r");

            f.Dispose();

            f.Closed.Should().BeTrue();
        }

        [Fact]
        public void Close_ReleasesFileHandle()
        {
            var path = CreateFile("a.txt", "abc");
            var f = Open(path, "r");
            f.Close();

            var act = () =>
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            };

            act.Should().NotThrow();
        }

        [Fact]
        public void Operations_AfterClose_Throw()
        {
            var f = Open(CreateFile("a.txt", "abc"), "rw");
            f.Close();

            var actions = new List<Action>
            {
                () => f.ReadLine(),
                () => f.ReadAll(),
                () => f.Write("x"),
                () => f.Eof(),
                () => f.Tell(),
                () => f.Seek(0),
                () => f.Rewind(),
                () => { var _ = f.Length; },
                () => f.Flush(),
                () => f.Truncate(),
                () => f.ReadChars(1)
            };

            foreach (var action in actions)
            {
                action.Should().Throw<RuntimeException>();
            }
        }

        [Fact]
        public void Flush_OpenFile_DoesNotThrow()
        {
            var f = Open(PathFor("w.txt"), "w");

            var act = () => f.Flush();

            act.Should().NotThrow();
        }

        // ---------- Tryb dopisywania ----------

        [Fact]
        public void Seek_BeforeAppendStart_InAppendMode_ThrowsIOException()
        {
            var f = Open(CreateFile("a.txt", "abc"), "a");

            var act = () => f.Seek(0);

            act.Should().Throw<IOException>();
        }

        // ---------- ToString ----------

        [Fact]
        public void ToString_ReturnsFileKeyword()
        {
            var f = Open(CreateFile("a.txt", "abc"), "r");

            f.ToString().Should().Be(Keywords.FILE_STRING);
        }
    }
}