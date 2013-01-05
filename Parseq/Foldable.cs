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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq
{
    public static class Foldable
    {
        public static U Foldl<T, U>(this IEnumerable<T> enumerable, U seed, Func<U, T, U> func)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");
            if (func == null)
                throw new ArgumentNullException("func");

            return enumerable.IfEmpty(
                () => seed,
                xs => xs.HeadAndTail()
                        .Case((y, ys) => Foldable.Foldl(ys, func(seed, y), func)));
        }

        public static T Foldl<T>(this IEnumerable<T> enumerable, Func<T, T, T> func)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");

            return enumerable.HeadAndTail()
                             .Case((x, xs) => Foldable.Foldl(xs, x, func));
        }

        public static U Foldr<T, U>(this IEnumerable<T> enumerable, U seed, Func<T, U, U> func)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");
            if (func == null)
                throw new ArgumentNullException("folder");

            return enumerable.IfEmpty(
                () => seed,
                xs => xs.LastAndInit()
                        .Case((y, ys) => Foldable.Foldr(ys, func(y, seed), func)));
        }

        public static T Foldr<T>(this IEnumerable<T> enumerable, Func<T, T, T> func)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");

            return enumerable.LastAndInit()
                             .Case((x, xs) => Foldable.Foldr(xs, x, func));
        }

        public static IEnumerable<T> Unfoldl<T, U>(U seed, Func<U, Option<Tuple<T, U>>> func)
        {
            return Foldable.Unfoldr(seed, (b => func(b).Select(_ => new Tuple<U, T>(_.Item2, _.Item1))))
                .Reverse();
        }

        public static IEnumerable<T> Unfoldr<T, U>(U seed, Func<U, Option<Tuple<U, T>>> func)
        {
            if (func == null)
                throw new ArgumentNullException("func");
            
            Tuple<U, T> result;
            while (func(seed).TryGetValue(out result))
            {
                seed = result.Item1;
                yield return result.Item2;
            }
        }
    }
}
