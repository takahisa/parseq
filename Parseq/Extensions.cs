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
            var enumerator = enumerable.GetEnumerator();
            return (!enumerator.MoveNext())
                ? thenSelector().With(_ => enumerator.Dispose())
                : elseSelector(enumerator.Current.Concat(enumerator.EnumerateTail(e => e.Dispose())));
        }

        internal static Tuple<T, IEnumerable<T>> HeadAndTail<T>(this IEnumerable<T> enumerable)
        {
            var enumerator = enumerable.GetEnumerator();
            if (!enumerator.MoveNext())
                throw new InvalidOperationException();

            var head = enumerator.Current;
            var tail = enumerator.EnumerateTail(e => e.Dispose());

            return Tuple.Create(head, tail);
        }

        internal static Tuple<T, IEnumerable<T>> LastAndInit<T>(this IEnumerable<T> enumerable)
        {
            var enumerator = enumerable.Reverse().GetEnumerator();
            if (!enumerator.MoveNext())
                throw new InvalidOperationException();

            var last = enumerator.Current;
            var init = enumerator.EnumerateInit(e => e.Dispose());

            return Tuple.Create(last, init);
        }

        internal static Tuple<IEnumerable<T>, IEnumerable<T>> Partition<T>(this IEnumerable<T> enumerable, Int32 count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            var enumerator = enumerable.GetEnumerator();
            var par0 = enumerator.EnumerateCount(count, e => { });
            var par1 = enumerator.EnumerateTail(e => e.Dispose());
            return Tuple.Create(par0, par1);
        }

        private static IEnumerable<T> EnumerateTail<T>(this IEnumerator<T> enumerator, Action<IEnumerator<T>> callback)
        {
            if (enumerator == null)
                throw new ArgumentNullException("enumerator");

            while (enumerator.MoveNext())
                yield return enumerator.Current;

            callback(enumerator);
        }

        private static IEnumerable<T> EnumerateInit<T>(this IEnumerator<T> enumerator, Action<IEnumerator<T>> callback)
        {
            if (enumerator == null)
                throw new ArgumentNullException("enumerator");

            if (enumerator.MoveNext())
            {
                do yield return enumerator.Current; while (enumerator.MoveNext());
            }

            callback(enumerator);
        }

        private static IEnumerable<T> EnumerateCount<T>(this IEnumerator<T> enumerator, Int32 count, Action<IEnumerator<T>> callback)
        {
            foreach(var i in Enumerable.Range(0, count)) {
                if (!enumerator.MoveNext())
                    break;
                yield return enumerator.Current;
            }
            callback(enumerator);
        }
    }
}