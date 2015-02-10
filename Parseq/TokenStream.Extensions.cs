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
using System.Collections.Generic;

namespace Parseq
{
    public static partial class TokenStream
    {
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
                : this(enumerator, new Position(1, 1))
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
    }
}
