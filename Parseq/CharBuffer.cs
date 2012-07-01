using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq
{
    class CharBuffer
        : System.IO.TextReader
    {
        private const Int32 DefaultBufferSize = 1024;
        private const Int32 MinBufferSize = 256;

        private System.IO.TextReader _baseReader;
        private Char[] _buffer;
        private Int32 _bufferPtrBegin;
        private Int32 _bufferPtrEnd;
        private Int32 _bufferSize;

        public CharBuffer(System.IO.TextReader reader, Int32 bufferSize)
        {
            _baseReader = reader;
            _bufferPtrBegin = 0;
            _bufferPtrEnd = 0;
            _bufferSize = Math.Max(bufferSize, MinBufferSize);
            _buffer = new Char[bufferSize];

            this.FillBuffer();
        }

        public CharBuffer(System.IO.TextReader reader)
            : this(reader, DefaultBufferSize)
        {

        }

        protected System.IO.TextReader BaseReader
        {
            get { return _baseReader; }
        }

        public Boolean EndOfBuffer
        {
            get { return _bufferPtrBegin >= _bufferPtrEnd; }
        }

        public override Int32 Read()
        {
            return this.Read(0);
        }

        public virtual Int32 Read(Int32 count)
        {
            return this.Peek(count).With(v => _bufferPtrBegin += count);
        }

        public override Int32 Read(Char[] buffer, Int32 index, Int32 count)
        {
            if (buffer == null)
                throw new ArgumentNullException();
            if (index < 0 || count < 0)
                throw new ArgumentOutOfRangeException();

            return (_bufferPtrBegin += this.Peek(buffer, index, count));
        }

        public override Int32 Peek()
        {
            return this.Peek(0);
        }

        public virtual Int32 Peek(Int32 count)
        {
            if (count < 0 || count >= _bufferSize)
                throw new ArgumentOutOfRangeException();

            return !(this.EndOfBuffer)
                ? _buffer[_bufferPtrBegin + count]
                : -1;
        }

        public virtual Int32 Peek(Char[] buffer, Int32 index, Int32 count)
        {
            if (buffer == null)
                throw new ArgumentNullException();
            if (index < 0 || count < 0)
                throw new ArgumentOutOfRangeException();

            var bufferedCount = this.Buffering(count);
            var bufferReadableCount = buffer.Count() - index;

            var readableCount = Math.Min(count, Math.Min(bufferReadableCount, bufferedCount));
            Array.Copy(_buffer, _bufferPtrBegin, buffer, index, readableCount);

            return readableCount;
        }

        public override String ReadLine()
        {
            var builder = new StringBuilder();

            if (this.EndOfBuffer)
                return null;

            Int32 ch;
            while ((ch = this.Peek()) >= 0)
            {
                builder.Append((Char)this.Read());
                if (ch == '\n' || (ch == '\r' && this.Peek() != '\n'))
                    return builder.ToString();
                
            }
            return builder.ToString();
        }

        private void FillBuffer()
        {

            _bufferPtrEnd = _baseReader.Read(_buffer, 0, _bufferSize);
            _bufferPtrBegin = 0;
        }

        private void ResizeBuffer(Int32 size)
        {
            var newBufferSize = Math.Max(size, MinBufferSize);
            var newBuffer = new Char[newBufferSize];

            Array.Copy(_buffer, 0, newBuffer, 0, _bufferSize);
            _bufferSize = newBufferSize;
            _buffer = newBuffer;
        }

        private Int32 Buffering(Int32 size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException();
            if (size > _bufferSize)
            {
                this.ResizeBuffer(size);
                return this.Buffering(size);
            }

            var bufferedCount = _bufferPtrEnd - _bufferPtrBegin;

            if (bufferedCount >= size)
                return size;
            else
            {
                var newBuffer = new Char[_bufferSize];
                Array.Copy(_buffer, _bufferPtrBegin, newBuffer, 0, bufferedCount);
                _bufferPtrEnd = bufferedCount +
                    _baseReader.Read(newBuffer, bufferedCount, _bufferSize - bufferedCount);
                _bufferPtrBegin = 0;
                _buffer = newBuffer;
                return Math.Min(size, _bufferPtrEnd);
            }
        }

        public override void Close()
        {
            this.Dispose(true);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _baseReader != null){
                _baseReader.Dispose();
                _baseReader = null;
                _buffer = null;
            }

            base.Dispose(disposing);
        }

    }
}
