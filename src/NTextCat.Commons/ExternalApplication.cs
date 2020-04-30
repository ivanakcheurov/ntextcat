using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace NTextCat.Commons.IO
{
    public static class ExternalApplication
    {
        public static Stream ToStream(string filename, string arguments, Stream input)
        {
            Process externalAppProc = null;
            try
            {
                ProcessStartInfo psi =
                new ProcessStartInfo
                {
                    StandardOutputEncoding = Encoding.GetEncoding(1250),
                    FileName = filename,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                externalAppProc = new Process();
                externalAppProc.StartInfo = psi;
                externalAppProc.Start();

                var outputStream = new ExtraDisposableStream(externalAppProc.StandardOutput.BaseStream, new IDisposable[] { externalAppProc });
                ThreadPool.QueueUserWorkItem(
                    parameter =>
                    {
                        //todo: May take down all application in case of exception
                        if (outputStream.Disposed)
                            return;
                        using (externalAppProc.StandardInput)
                        {
                            var buffer = new byte[64 * 1024];
                            int readBytes;
                            while ((readBytes = input.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                if (outputStream.Disposed)
                                    return;
                                externalAppProc.StandardInput.BaseStream.Write(buffer, 0, readBytes);
                                if (outputStream.Disposed)
                                    return;
                            }
                            externalAppProc.StandardInput.Flush();
                            externalAppProc.StandardInput.Close();
                        }
                    },
                    externalAppProc
                    );

                return outputStream;
            }
            catch
            {
                if (externalAppProc != null)
                    externalAppProc.Dispose();
                throw;
            }
            
        }

        public static Stream ToStream(string filename, string arguments, byte[] input)
        {
            return ToStream(filename, arguments, new MemoryStream(input));
        }

        public static Stream ToStream(string filename, string arguments, TextReader input, Encoding encoding = null, int bufferSize = 4096)
        {
            return ToStream(filename, arguments, new TextReaderStream(input, encoding ?? Encoding.UTF8, bufferSize));
        }

        public static Stream ToStream(string filename, string arguments, string input, Encoding encoding = null)
        {
            return ToStream(filename, arguments, new StringReader(input), encoding);
        }

        public static Stream ToStream(string filename, string arguments, IEnumerable<string> input, Encoding encoding = null)
        {
            return ToStream(filename, arguments, new StringsTextReader(input));
        }

        public static byte[] ToBytes(string filename, string arguments, Stream input, Encoding encoding = null)
        {
            using (var memStream = new MemoryStream())
            {
                var output = ToStream(filename, arguments, input);
#if NET35
                int readBytes;
                var buffer = new byte[4*1024];
                while ((readBytes = output.Read(buffer, 0, buffer.Length)) > 0)
                    memStream.Write(buffer, 0, readBytes);
#else
                output.CopyTo(memStream);
#endif
                return memStream.ToArray();
            }
        }

        public static byte[] ToBytes(string filename, string arguments, byte[] input)
        {
            return ToBytes(filename, arguments, new MemoryStream(input));
        }

        public static byte[] ToBytes(string filename, string arguments, TextReader input, Encoding encoding = null, int bufferSize = 4096)
        {
            return ToBytes(filename, arguments, new TextReaderStream(input, encoding ?? Encoding.UTF8, bufferSize), encoding);
        }

        public static byte[] ToBytes(string filename, string arguments, string input, Encoding encoding = null)
        {
            return ToBytes(filename, arguments, new StringReader(input), encoding);
        }

        public static byte[] ToBytes(string filename, string arguments, IEnumerable<string> input, Encoding encoding = null)
        {
            return ToBytes(filename, arguments, new StringsTextReader(input), encoding);
        }

        public static TextReader ToTextReader(string filename, string arguments, Stream input, Encoding encoding = null)
        {
            return new StreamReader(ToStream(filename, arguments, input), encoding ?? Encoding.UTF8);
        }

        public static TextReader ToTextReader(string filename, string arguments, byte[] input, Encoding encoding = null)
        {
            return ToTextReader(filename, arguments, new MemoryStream(input), encoding);
        }

        public static TextReader ToTextReader(string filename, string arguments, TextReader input, Encoding encoding = null, int bufferSize = 4096)
        {
            return ToTextReader(filename, arguments, new TextReaderStream(input, encoding ?? Encoding.UTF8, bufferSize), encoding);
        }

        public static TextReader ToTextReader(string filename, string arguments, string input, Encoding encoding = null)
        {
            return ToTextReader(filename, arguments, new StringReader(input), encoding);
        }

        public static TextReader ToTextReader(string filename, string arguments, IEnumerable<string> input, Encoding encoding = null)
        {
            return ToTextReader(filename, arguments, new StringsTextReader(input), encoding);
        }

        public static string ToString(string filename, string arguments, Stream input, Encoding encoding = null)
        {
            return new StreamReader(ToStream(filename, arguments, input), encoding ?? Encoding.UTF8).ReadToEnd();
        }

        public static string ToString(string filename, string arguments, byte[] input, Encoding encoding = null)
        {
            return ToString(filename, arguments, new MemoryStream(input), encoding);
        }

        public static string ToString(string filename, string arguments, TextReader input, Encoding encoding = null, int bufferSize = 4096)
        {
            return ToString(filename, arguments, new TextReaderStream(input, encoding ?? Encoding.UTF8, bufferSize), encoding);
        }

        public static string ToString(string filename, string arguments, string input, Encoding encoding = null)
        {
            return ToString(filename, arguments, new StringReader(input), encoding);
        }

        public static string ToString(string filename, string arguments, IEnumerable<string> input, Encoding encoding = null)
        {
            return ToString(filename, arguments, new StringsTextReader(input), encoding);
        }

        public static IEnumerable<string> ToStrings(string filename, string arguments, Stream input, Encoding encoding = null)
        {
            return new StreamReader(ToStream(filename, arguments, input), encoding ?? Encoding.UTF8).ReadLines();
        }

        public static IEnumerable<string> ToStrings(string filename, string arguments, byte[] input, Encoding encoding = null)
        {
            return ToStrings(filename, arguments, new MemoryStream(input), encoding);
        }

        public static IEnumerable<string> ToStrings(string filename, string arguments, TextReader input, Encoding encoding = null, int bufferSize = 4096)
        {
            return ToStrings(filename, arguments, new TextReaderStream(input, encoding ?? Encoding.UTF8, bufferSize), encoding);
        }

        public static IEnumerable<string> ToStrings(string filename, string arguments, string input, Encoding encoding = null)
        {
            return ToStrings(filename, arguments, new StringReader(input), encoding);
        }

        public static IEnumerable<string> ToStrings(string filename, string arguments, IEnumerable<string> input, Encoding encoding = null)
        {
            return ToStrings(filename, arguments, new StringsTextReader(input), encoding);
        }

        private class ExtraDisposableStream : Stream
        {
            private Stream _stream;

            private IDisposable[] _extraDisposable;

            public bool Disposed { get; private set; }

            public ExtraDisposableStream(Stream stream, IEnumerable<IDisposable> extraDisposable)
            {
                _stream = stream;
                _extraDisposable = extraDisposable.ToArray();
            }

            public override object InitializeLifetimeService()
            {
                return _stream.InitializeLifetimeService();
            }

            public override void Close()
            {
                Disposed = true;
                _stream.Close();
                if (_extraDisposable != null)
                {
                    foreach (IDisposable disposable in _extraDisposable)
                    {
                        if (disposable != null)
                            disposable.Dispose();
                    }
                }
            }

            public override void Flush()
            {
                _stream.Flush();
            }

            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                return _stream.BeginRead(buffer, offset, count, callback, state);
            }

            public override int EndRead(IAsyncResult asyncResult)
            {
                return _stream.EndRead(asyncResult);
            }

            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                return _stream.BeginWrite(buffer, offset, count, callback, state);
            }

            public override void EndWrite(IAsyncResult asyncResult)
            {
                _stream.EndWrite(asyncResult);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return _stream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                _stream.SetLength(value);
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return _stream.Read(buffer, offset, count);
            }

            public override int ReadByte()
            {
                return _stream.ReadByte();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                _stream.Write(buffer, offset, count);
            }

            public override void WriteByte(byte value)
            {
                _stream.WriteByte(value);
            }

            public override bool CanRead
            {
                get { return _stream.CanRead; }
            }

            public override bool CanSeek
            {
                get { return _stream.CanSeek; }
            }

            public override bool CanTimeout
            {
                get { return _stream.CanTimeout; }
            }

            public override bool CanWrite
            {
                get { return _stream.CanWrite; }
            }

            public override long Length
            {
                get { return _stream.Length; }
            }

            public override long Position
            {
                get { return _stream.Position; }
                set { _stream.Position = value; }
            }

            public override int ReadTimeout
            {
                get { return _stream.ReadTimeout; }
                set { _stream.ReadTimeout = value; }
            }

            public override int WriteTimeout
            {
                get { return _stream.WriteTimeout; }
                set { _stream.WriteTimeout = value; }
            }
        }

        //private void cmdExecute_Click(object sender, EventArgs e)
        //{
        //    string cmd = textInput.Text;
        //    _pythonProc.StandardInput.WriteLine(cmd);
        //    _pythonProc.StandardInput.Flush();
        //    textInput.Text = string.Empty;
        //}

        //private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        //{
        //    if (!_pythonProc.HasExited)
        //        _pythonProc.Kill();
        //}
    }
}