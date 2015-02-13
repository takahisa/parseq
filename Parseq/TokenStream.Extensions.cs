/*
 * Copyright (C) 2012 - 2015 Takahisa Watanabe <linerlock@outlook.com> All rights reserved.
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
using System.IO;
using System.Collections.Generic;

namespace Parseq
{
    public static partial class TokenStream
    {
        public static CharStream AsStream(this String inputString)
        {
            return new CharStream(inputString);
        }

        public static CharStream AsStream(this TextReader inputStringReader)
        {
            return new CharStream(inputStringReader);
        }

        public static CharStream AsStream(this IEnumerable<Char> enumerable)
        {
            return TokenStream.AsStream(new TextReaderAdapter(enumerable));
        }

        public static ITokenStream<T> AsStream<T>(this IEnumerable<T> enumerable)
        {
            return new TokenStreamImpl<T>(enumerable);
        }
    }

    public static partial class TokenStream
    {
        class TokenStreamImpl<T>
            : ITokenStream<T>
        {
            public IOption<IPair<T, Position>> Current
            {
                get;
                private set;
            }
            
            private IEnumerator<T> enumerator;
            private IDelayed<ITokenStream<T>> restStream;

            public TokenStreamImpl(IEnumerable<T> enumerable)
                : this(enumerable.GetEnumerator())
            {
                
            }

            public TokenStreamImpl(IEnumerator<T> enumerator)
                : this(enumerator, Position.Zero)
            {

            }

            TokenStreamImpl(IEnumerator<T> enumerator, Position currentPosition)
            {
                this.enumerator = enumerator;
                this.Current = this.enumerator.MoveNext()
                    ? Option.Some<IPair<T, Position>>(Pair.Return(this.enumerator.Current, currentPosition))
                    : Option.None<IPair<T, Position>>();
                this.restStream = Delayed.Return(() =>
                    new TokenStreamImpl<T>(enumerator, new Position(currentPosition.Line, currentPosition.Column + 1)));
            }

            public ITokenStream<T> MoveNext()
            {
                return this.restStream.Force();
            }
        }

        class TextReaderAdapter
            : TextReader
        {
            public const Int32 EOF = -1;

            private IEnumerator<Char> enumerator;
            private IOption<Char> current;

            public TextReaderAdapter(IEnumerable<Char> enumerable)
                : this(enumerable.GetEnumerator())
            {

            }

            public TextReaderAdapter(IEnumerator<Char> enumerator)
            {
                this.enumerator = enumerator;
                this.current = this.enumerator.MoveNext()
                    ? Option.Some<Char>(this.enumerator.Current)
                    : Option.None<Char>();
            }

            public override Int32 Peek()
            {
                if (this.enumerator == null)
                    throw new ObjectDisposedException("enumerator");
                
                return this.current.Case(
                    none: () => EOF,
                    some: value => (Int32)value);
            }

            public override Int32 Read()
            {
                if (this.enumerator == null)
                    throw new ObjectDisposedException("enumerator");

                var c = this.Peek();

                this.current = this.enumerator.MoveNext()
                    ? Option.Some<Char>(this.enumerator.Current)
                    : Option.None<Char>();
                return c;
            }

            protected override void Dispose(Boolean disposing)
            {
                if (disposing && this.enumerator != null)
                {
                    this.enumerator.Dispose();
                    this.enumerator = null;
                }
            }
        }
    }
}
