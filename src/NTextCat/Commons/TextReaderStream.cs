using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NTextCat.Commons.IO
{
    public class TextReaderStream : Stream
    {
        private TextReader _textReader;

        private Encoding _encoding;
        private Encoder _encoder;
        private readonly int _maxByteCountPerChar;
        char[] _charBuffer;

        public TextReaderStream(TextReader textReader, Encoding encoding, int bufferSize = 4096)
        {
            _textReader = textReader;
            _encoding = encoding;
            _maxByteCountPerChar = _encoding.GetMaxByteCount(1);
            _encoder = encoding.GetEncoder();
            if (bufferSize <= 0) throw new ArgumentOutOfRangeException("bufferSize", "zero or negative");
            _charBuffer = new char[bufferSize];
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count <= 0) throw new ArgumentOutOfRangeException("count", "zero or negative");
            int charsToReadLeft = count/_maxByteCountPerChar;
            if (charsToReadLeft == 0)
                throw new ArgumentOutOfRangeException("count", "too small count, read at least " + _maxByteCountPerChar + " bytes");
            int totalBytesWritten = 0;
            while (charsToReadLeft > 0)
            {
                int currentChunkSize = Math.Min(charsToReadLeft, _charBuffer.Length);
                int charsRead = _textReader.ReadBlock(_charBuffer, 0, currentChunkSize);
                int bytes = _encoder.GetBytes(_charBuffer, 0, charsRead, buffer, offset + totalBytesWritten, false);
                totalBytesWritten += bytes;
                charsToReadLeft -= charsRead;
                if (charsRead < currentChunkSize)
                    break;
            }
            return totalBytesWritten;
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override void Close()
        {
            try
            {
                if (_textReader != null)
                {
                    _textReader.Close();
                    _textReader = null;
                }
            }
            finally
            {
                base.Close();
            }
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        #region Not supperted

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        #endregion
    }
}