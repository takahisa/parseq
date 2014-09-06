/*
 * Parseq - monadic parser combinator library for C#
 * 
 * Copyright (C) 2012 - 2014 Takahisa Watanabe <linerlock@outlook.com> All rights reserved.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Parseq
{
    public static class StreamExtensions
    {
        public static IStream<T> Where<T>(this IStream<T> stream, Func<T, Boolean> predicate)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (predicate == null)
                throw new ArgumentNullException("stream");

            T result;
            while (stream.Current.TryGetValue(out result) && predicate(result))
                stream = stream.Next();
            return stream;
        }

        public static IStream<T> Where<T>(this IStream<T> stream, Func<IStream<T>, T, Boolean> predicate)
        {
            return stream.Where(_ => predicate(stream, _));
        }

        public static IStream<U> Select<T, U>(this IStream<T> stream, Func<T, U> selector)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return new StreamMapper<T, U>(stream, selector);
        }

        public static IStream<U> Select<T, U>(this IStream<T> stream, Func<IStream<T>, U> selector)
        {
            return stream.Select((T _) => selector(stream));
        }

        public static IStream<U> Select<T, U>(this IStream<T> stream, Func<IStream<T>, T, U> selector)
        {
            return stream.Select((T _) => selector(stream, _));
        }

        public static IStream<U> SelectMany<T, U>(this IStream<T> stream, Func<T, IStream<U>> selector)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (selector == null)
                throw new ArgumentNullException("selector");

            T result;
            if (stream.Current.TryGetValue(out result))
                return selector(result);
            else
                throw new InvalidOperationException();
        }

        public static IStream<U> SelectMany<T, U>(this IStream<T> stream, Func<IStream<T>, IStream<U>> selector)
        {
            return stream.SelectMany((T _) => selector(stream));
        }

        public static IStream<U> SelectMany<T, U>(this IStream<T> stream, Func<IStream<T>, T, IStream<U>> selector)
        {
            return stream.SelectMany((T _) => selector(stream, _));
        }

        public static IStream<T> AsStream<T>(this IEnumerable<T> enumerable)
        {
            return new StreamAdapter<T>(enumerable);
        }

        public static CharStream AsStream(this IEnumerable<Char> enumerable)
        {
            return new CharStream(new TextReaderAdapter(enumerable));
        }

        public static CharStream AsStream(this TextReader reader)
        {
            return new CharStream(new CharBuffer(reader));
        }

        private class StreamMapper<T, U>
            : Stream<U>
        {
            private readonly IStream<T> _stream;
            private readonly Func<T, U> _selector;

            public StreamMapper(IStream<T> stream, Func<T, U> selector)
            {
                if (stream == null)
                    throw new ArgumentNullException("stream");
                if (selector == null)
                    throw new ArgumentNullException("selector");
                _stream = stream;
                _selector = selector;
            }

            public override Position Position
            {
                get { return _stream.Position; }
            }

            public override IOption<U> Current
            {
                get { return _stream.Current.Select(_selector); }
            }

            public override Boolean CanNext()
            {
                return _stream.CanNext();
            }

            public override Boolean CanRewind()
            {
                return _stream.CanRewind();
            }

            public override IStream<U> Next()
            {
                return new StreamMapper<T, U>(_stream.Next(), _selector);
            }

            public override IStream<U> Rewind()
            {
                return new StreamMapper<T, U>(_stream.Rewind(), _selector);
            }
        }

        private class StreamAdapter<T>
            : Stream<T>
        {
            private readonly IEnumerator<T> _enumerator;
            private readonly IOption<T> _current;
            private readonly Position _position;
            private IOption<IStream<T>> _upper;
            private IOption<IStream<T>> _lower;

            public StreamAdapter(IEnumerable<T> enumerable)
                : this(enumerable.GetEnumerator())
            {

            }

            public StreamAdapter(IEnumerator<T> enumerator)
                : this(
                    enumerator,
                    Option.Try(() =>
                        enumerator.MoveNext()
                        ? Option.Just<T>(enumerator.Current)
                        : Option.None<T>()),
                    new Position(1, 1, 0),
                    Option.None<IStream<T>>(),
                    Option.None<IStream<T>>())
            {

            }

            private StreamAdapter(
                IEnumerator<T> enumerator,
                IOption<T> current,
                Position position,
                IOption<IStream<T>> upper,
                IOption<IStream<T>> lower)
            {
                _enumerator = enumerator;
                _current = current;
                _position = position;
                _upper = upper;
                _lower = lower;
            }

            public override Position Position
            {
                get { return _position; }
            }

            public override IOption<T> Current
            {
                get { return _current; }
            }

            public override Boolean CanNext()
            {
                if (_lower.Exists())
                    return true;
                if (!_current.Exists())
                    return false;

                var position = new Position(
                        _position.Line,
                        _position.Column + 1,
                        _position.Index + 1);

                var upper = Option.Just<IStream<T>>(this);
                var lower = Option.None<IStream<T>>();

                _lower = Option.Just(new StreamAdapter<T>(
                   _enumerator, _enumerator.MoveNext() ? Option.Just(_enumerator.Current) : Option.None<T>(), position, upper, lower)
                );
                return true;
            }

            public override Boolean CanRewind()
            {
                return _upper.Exists();
            }

            public override IStream<T> Next()
            {
                IStream<T> stream;
                if (this.CanNext() && _lower.TryGetValue(out stream))
                    return stream;
                else
                    throw new InvalidOperationException();
            }

            public override IStream<T> Rewind()
            {
                IStream<T> stream;
                if (this.CanRewind() && _upper.TryGetValue(out stream))
                    return stream;
                else
                    throw new InvalidOperationException();
            }
        }

        private class TextReaderAdapter
            : TextReader
        {
            private IEnumerator<Char> _enumerator;
            private IOption<Char> _current;

            public TextReaderAdapter(IEnumerable<Char> enumerable)
                : this(enumerable.GetEnumerator())
            {

            }

            public TextReaderAdapter(IEnumerator<Char> enumerator)
            {
                if (enumerator == null)
                    throw new ArgumentNullException("enumerator");

                _enumerator = enumerator;

                this.MoveNext();
            }

            public override Int32 Read()
            {
                var ch = this.Peek();
                this.MoveNext();
                return ch;
            }

            public override Int32 Peek()
            {
                return _current.Select(t => (Int32)t).Otherwise(() => -1);
            }

            private void MoveNext()
            {
                if (_enumerator == null)
                    throw new ObjectDisposedException("enumerator");

                _current = Option.Try(() =>
                    _enumerator.MoveNext()
                    ? Option.Just<Char>(_enumerator.Current)
                    : Option.None<Char>());
            }

            protected override void Dispose(Boolean disposing)
            {
                if (disposing && _enumerator != null)
                {
                    _enumerator.Dispose();
                    _enumerator = null;
                }
            }

        }
    }
}
