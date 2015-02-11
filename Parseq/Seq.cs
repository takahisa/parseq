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
    public interface ISeq<T>
        : IEnumerable<T>
    {
        TResult Case<TResult>(
            Func<TResult> empty,
            Func<IPair<T, IDelayed<ISeq<T>>>, TResult> headAndTail);
    }

    public partial class Seq
    {
        public static IDelayed<ISeq<T>> Of<T>(IEnumerable<T> enumerable)
        {
            return Seq.Of(enumerable.GetEnumerator());
        }

        public static IDelayed<ISeq<T>> Of<T>(IEnumerator<T> enumerator)
        {
            return Delayed.Return(() => enumerator.MoveNext()
                ? Seq.Cons(enumerator.Current, Delayed.Return(() => Seq.Of(enumerator).Force()))
                    .Force()
                : Seq.Empty<T>()
                    .Force());
        }

        public static IDelayed<ISeq<T>> Empty<T>()
        {
            return Delayed.Return(SingletonClassHelper<Seq.EmptyImpl<T>>.Instance);
        }

        public static IDelayed<ISeq<T>> Cons<T>(T head, IDelayed<ISeq<T>> tail)
        {
            return Delayed.Return(new ConsImpl<T>(head, tail));
        }
    }

    public partial class Seq
    {
        class EmptyImpl<T>
            : ISeq<T>
        {
            public TResult Case<TResult>(Func<TResult> empty, Func<IPair<T, IDelayed<ISeq<T>>>, TResult> cons)
            {
                return empty();
            }

            public IEnumerator<T> GetEnumerator()
            {
                yield break;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        class ConsImpl<T>
            : ISeq<T>
        {
            private readonly IPair<T, IDelayed<ISeq<T>>> headAndTail;

            public ConsImpl(T head, IDelayed<ISeq<T>> tail)
            {
                this.headAndTail = Pair.Return(head, tail);
            }

            public TResult Case<TResult>(Func<TResult> empty, Func<IPair<T, IDelayed<ISeq<T>>>, TResult> headAndTail)
            {
                return headAndTail(this.headAndTail);
            }

            public IEnumerator<T> GetEnumerator()
            {
                yield return this.headAndTail.Item0;

                foreach (var item in this.headAndTail.Item1.Force())
                    yield return item;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
    }
}
