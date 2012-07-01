using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq
{
    class CharStream
        : Stream<Char>
    {
        private CharBuffer _buffer;
        private Option<Char> _current;
        private Position _position;
        private Option<Stream<Char>> _upper;
        private Option<Stream<Char>> _lower;

        public CharStream(CharBuffer buffer)
            : this(buffer,
                Option.Try(() =>
                    !(buffer.EndOfBuffer)
                    ? Option.Just<Char>((Char)buffer.Read())
                    : Option.None<Char>()),
                new Position(1, 1, 0),
                Option.None<Stream<Char>>(),
                Option.None<Stream<Char>>())
        {

        }

        public CharStream(System.IO.TextReader reader)
            : this(new CharBuffer(reader))
        {

        }

        private CharStream(
            CharBuffer buffer,
            Option<Char> current,
            Position position,
            Option<Stream<Char>> upper,
            Option<Stream<Char>> lower)
        {
            _buffer = buffer;
            _current = current;
            _position = position;
            _upper = upper;
            _lower = lower;
        }

        public override Position Position
        {
            get { return _position; }
        }

        public override Boolean CanNext()
        {
            if (_lower.Exists())
                return true;
            if (!_current.Exists())
                return false;

            var position = (_buffer.Peek(0) == '\n' || _buffer.Peek(0) == '\r' && _buffer.Peek(1) != '\n')
                ? new Position(1, _position.Line + 1, _position.Index + 1)
                : new Position(_position.Column + 1, _position.Line, _position.Index + 1);

            var upper = Option.Just<Stream<Char>>(this);
            var lower = Option.None<Stream<Char>>();

            _lower = !(_buffer.EndOfBuffer)
               ? new CharStream(_buffer, (Char)_buffer.Read(), position, upper, lower)
               : new CharStream(_buffer, Option.None<Char>(), position, upper, lower);
            return true;
        }

        public override Boolean CanRewind()
        {
            return _upper.Exists();
        }

        public override Stream<Char> Next()
        {
            Stream<Char> stream;
            if (this.CanNext() && _lower.TryGetValue(out stream))
                return stream;
            else
                throw new InvalidOperationException();
        }

        public override Stream<Char> Rewind()
        {
            Stream<Char> stream;
            if (this.CanRewind() && _upper.TryGetValue(out stream))
                return stream;
            else
                throw new InvalidOperationException();
        }

        public override Boolean TryGetValue(out Char value)
        {
            return _current.TryGetValue(out value);
        }

        public override Char Perform()
        {
            return _current.Perform();
        }

        public override void Dispose()
        {
            if(_buffer != null)
            {
                _buffer.Dispose();
                _buffer = null;
                _current = null;
                _upper = null;
                _lower = null;
            }
        }
    }
}
