/*
 * Parseq - a monadic parser combinator library for C#
 *
 * Copyright (c) 2012 WATANABE TAKAHISA <x.linerlock@gmail.com> All rights reserved.
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

namespace Parseq
{
    public static class Extensions
    {
        internal static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var i in enumerable)
                action(i);
        }

        internal static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, Int32> action)
        {
            var index = 0;
            foreach (var i in enumerable)
                action(i, index++);
        }

        internal static T With<T>(this T value, Action<T> action)
        {
            action(value);
            return value;
        }

        internal static IEnumerable<T> Return<T>(this T value)
        {
            yield return value;
        }

        internal static IEnumerable<T> Concat<T>(this IEnumerable<T> first, T second)
        {
            foreach (var t in first)
                yield return t;

            yield return second;
        }

        internal static IEnumerable<T> Concat<T>(this T first, IEnumerable<T> second)
        {
            yield return first;

            foreach (var t in second)
                yield return t;
        }

        internal static IEnumerable<T> Replicate<T>(this T value)
        {
            return Extensions.Replicate(() => value);
        }

        internal static IEnumerable<T> Replicate<T>(this Func<T> selector)
        {
            while (true)
                yield return selector();
        }

        internal static TResult Case<T, TResult>(this Tuple<T, IEnumerable<T>> pattern,
            Func<T, IEnumerable<T>, TResult> selector)
        {
            if (pattern == null)
                throw new ArgumentNullException("pattern");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return selector(pattern.Item1, pattern.Item2);
        }

        internal static TResult Case<T, TResult>(this Tuple<IEnumerable<T>, IEnumerable<T>> pattern,
            Func<IEnumerable<T>, IEnumerable<T>, TResult> selector)
        {
            if (pattern == null)
                throw new ArgumentNullException("pattern");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return selector(pattern.Item1, pattern.Item2);
        }

        internal static TResult IfEmpty<T, TResult>(this IEnumerable<T> enumerable,
            Func<TResult> thenSelector,
            Func<IEnumerable<T>, TResult> elseSelector)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");

            var m = enumerable.Memoize();
            return (!m.Any()) ? thenSelector() : elseSelector(m);
        }

        internal static Tuple<T, IEnumerable<T>> HeadAndTail<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");

            var m = enumerable.Memoize();
            var head = m.First();
            var tail = m.Skip(1);
            return Tuple.Create(head, tail);
        }

        internal static Tuple<T, IEnumerable<T>> LastAndInit<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");

            var m = enumerable.Reverse().Memoize();
            var last = m.First();
            var init = m.Skip(1).Reverse();
            return Tuple.Create(last, init);
        }

        internal static Tuple<IEnumerable<T>, IEnumerable<T>> Partition<T>(this IEnumerable<T> enumerable, Int32 count)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            var m = enumerable.Memoize();
            var par0 = m.Take(count);
            var par1 = m.Skip(count);
            return Tuple.Create(par0, par1);
        }

        internal static IEnumerable<T> Memoize<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");

            return new MemoizeEnumerable<T>(enumerable);
        }

        private class MemoizeEnumerable<T>
            : IEnumerable<T>
        {
            private IEnumerable<T> _enumerable;
            private List<T> _buffer;

            public MemoizeEnumerable(IEnumerable<T> enumerable)
            {
                if (enumerable == null)
                    throw new ArgumentNullException("enumerable");

                _enumerable = enumerable;
                _buffer = new List<T>();
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new MemoizeEnumerator<T>(_enumerable.GetEnumerator(), _buffer);
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        private class MemoizeEnumerator<T>
            : IEnumerator<T>
        {
            private IEnumerator<T> _enumerator;
            private Option<T> _current;
            private Queue<T> _queue;

            public MemoizeEnumerator(IEnumerator<T> enumerator, List<T> buffer)
            {
                _enumerator = enumerator;
                _current = Option.None<T>();

                _queue = new Queue<T>(buffer);
            }

            public MemoizeEnumerator(IEnumerator<T> enumerator)
                : this(enumerator, new List<T>())
            {

            }

            public Boolean MoveNext()
            {
                if (_queue.Any())
                {
                    _current = Option.Just<T>(_queue.Dequeue());
                    return true;
                }

                if (_enumerator.MoveNext())
                {
                    _current = Option.Just<T>(_enumerator.Current);
                    return true;
                }

                return false;
            }

            public T Current
            {
                get
                {
                    T value;
                    if (_current.TryGetValue(out value))
                        return value;
                    else
                        throw new InvalidOperationException();
                }
            }

            public void Reset()
            {
                _enumerator.Reset();
            }

            public void Dispose()
            {
                _enumerator.Dispose();
                _queue.Clear();
            }

            Object System.Collections.IEnumerator.Current
            {
                get { return this.Current; }
            }
        }
    }
}