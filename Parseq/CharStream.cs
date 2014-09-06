/*
 * Parseq - a monadic parser combinator library for C#
 *
 * Copyright (c) 2012 - 2013 WATANABE TAKAHISA <x.linerlock@gmail.com> All rights reserved.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */
using System;
using System.Linq;
using System.Collections.Generic;

namespace Parseq
{
    public class CharStream
        : Stream<Char>
        , IDisposable
    {
        private CharBuffer _buffer;
        private IOption<Char> _current;
        private Position _position;
        private IOption<IStream<Char>> _upper;
        private IOption<IStream<Char>> _lower;

        public CharStream(System.IO.TextReader reader)
            : this(new CharBuffer(reader))
        {

        }

        private CharStream(CharBuffer buffer)
            : this(buffer,
                Option.Try(() =>
                    !(buffer.Peek() == CharBuffer.EOF)
                    ? Option.Just<Char>((Char)buffer.Read())
                    : Option.None<Char>()),
                new Position(1, 1, 0),
                Option.None<IStream<Char>>(),
                Option.None<IStream<Char>>())
        {

        }

        private CharStream(
            CharBuffer buffer,
            IOption<Char> current,
            Position position,
            IOption<IStream<Char>> upper,
            IOption<IStream<Char>> lower)
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

        public override IOption<Char> Current
        {
            get { return _current; }
        }

        public override Boolean CanNext()
        {
            if (_lower.Exists())
                return true;
            if (!_current.Exists())
                return false;

            var position = (_buffer.Peek(0) == '\n' || _buffer.Peek(0) == '\r' && _buffer.Peek(1) != '\n')
                ? new Position(_position.Line + 1, 1, _position.Index + 1)
                : new Position(_position.Line, _position.Column + 1, _position.Index + 1);

            var upper = Option.Just<IStream<Char>>(this);
            var lower = Option.None<IStream<Char>>();

            _lower = Option.Just(new CharStream(
                _buffer, !(_buffer.Peek() == CharBuffer.EOF) ? Option.Just((Char)_buffer.Read()) : Option.None<Char>(), position, upper, lower
            ));
            return true;
        }

        public override Boolean CanRewind()
        {
            return _upper.Exists();
        }

        public override IStream<Char> Next()
        {
            IStream<Char> stream;
            if (this.CanNext() && _lower.TryGetValue(out stream))
                return stream;
            else
                throw new InvalidOperationException();
        }

        public override IStream<Char> Rewind()
        {
            IStream<Char> stream;
            if (this.CanRewind() && _upper.TryGetValue(out stream))
                return stream;
            else
                throw new InvalidOperationException();
        }

        public virtual void Dispose()
        {
            if (_buffer != null)
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
