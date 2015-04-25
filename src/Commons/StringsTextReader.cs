using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace IvanAkcheurov.Commons
{
    public class StringsTextReader : TextReader
    {
        private IEnumerator<string> _strings;

        private string _s;
        private string _sNext;
        private int _pos;
        private int _length;
        private bool _finished;
        private bool _currentLineIsNewLine;
        private bool _disposed;

        public StringsTextReader(IEnumerable<string> strings)
        {
            if (strings == null) throw new ArgumentNullException("strings");
            _strings = strings.GetEnumerator();
            _currentLineIsNewLine = true;
            if (_strings.MoveNext() == false)
                _finished = true;
            else
                _sNext = _strings.Current;
            _length = _sNext.Length;
            _pos = _length;
        }

        protected override void Dispose(bool disposing)
        {
            if (_strings != null)
                _strings.Dispose();
            _strings = null;
            _disposed = true;
            _s = null;
            _pos = 0;
            _length = 0;
            base.Dispose(disposing);
        }
        public void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("This object has been already disposed");
        }

        public override int Peek()
        {
            CheckDisposed();
            if (HasNoTextLeftSkippingEmpties())
                return -1;
            return _s[_pos];
        }

        public override int Read()
        {
            int result = Peek();
            _pos++;
            return result;
        }

        public override int Read(char[] buffer, int index, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (index < 0)
                throw new ArgumentOutOfRangeException("index");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if (buffer.Length - index < count)
                throw new ArgumentException("should have: (buffer.Length - index < count) true");
            
            CheckDisposed();
            if (HasNoTextLeftSkippingEmpties())
                return 0;
            int leftChars = _length - _pos;
            if (leftChars > count)
                leftChars = count;
            _s.CopyTo(_pos, buffer, index, leftChars);
            _pos += leftChars;
            return leftChars;
        }

        public override string ReadToEnd()
        {
            CheckDisposed();
            var sb = new StringBuilder();
            var buffer = new char[4*1024];
            int readChars;
            while ((readChars = Read(buffer, 0, buffer.Length)) > 0)
                sb.Append(buffer, 0, readChars);
            return sb.ToString();
        }

        public override string ReadLine()
        {
            CheckDisposed();

            if (HasNoTextLeftForReadLine())
                return null;

            int index;
            for (index = _pos; index < _length; ++index)
            {
                char ch = _s[index];
                switch (ch)
                {
                    case '\r':
                    case '\n':
                        string str = _s.Substring(_pos, index - _pos);
                        _pos = index + 1;
                        if (ch == 13 && _pos < _length && _s[_pos] == 10)
                            _pos++;
                        return str;
                }
            }
            if(index > _pos)
            {
                string str = _s.Substring(_pos, index - _pos);
                _pos = index;
                return str;
            }
            return string.Empty;
        }
        private bool HasNoTextLeftSkippingEmpties()
        {
            do
            {
                if (HasNoTextLeft())
                    return true;
            } while (_pos == _length);
            return false;
        }

        private bool HasNoTextLeft()
        {
            if (_finished)
                return true;
            if (_pos == _length)
            {
                if (_currentLineIsNewLine)
                {
                    _s = _sNext;
                }
                else
                {
                    if (_strings.MoveNext() == false)
                    {
                        _finished = true;
                        return true;
                    }
                    _sNext = _strings.Current;
                    _s = Environment.NewLine;
                }
                _pos = 0;
                _length = _s.Length;
                _currentLineIsNewLine = !_currentLineIsNewLine;
            }
            return false;
        }


        private bool HasNoTextLeftForReadLine()
        {
            if (_finished)
                return true;
            if (_pos == _length)
            {
                if (_sNext != null)
                {
                    _s = _sNext;
                    _sNext = null;
                    _currentLineIsNewLine = false;
                }
                else if (_strings.MoveNext() == false)
                {
                    _finished = true;
                    return true;
                }
                else
                {
                    _s = _strings.Current;
                }
                _pos = 0;
                _length = _s.Length;
            }
            return false;
        }
    }
}
