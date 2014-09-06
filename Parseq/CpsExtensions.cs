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
using System.Linq;
using System.Collections.Generic;

namespace Parseq
{
    public static class Cps
    {
        public static Cps<TResult, T> Return<TResult, T>(T value)
        {
            return k => k(value);
        }

        public static Cps<TResult, U> CallCC<TResult, T, U>(Func<Func<U, Cps<TResult, T>>, Cps<TResult, U>> selector)
        {
            if (selector == null)
                throw new ArgumentNullException("f");

            return k => selector(x => _ => k(x))(k);
        }

        public static Cps<TResult, U> For<TResult, T, U>(this IEnumerable<T> enumerable, Func<T, Cps<TResult, U>> selector)
        {
            return enumerable.HeadAndTail()
                .Case((head, tail) => enumerable.Foldl(selector(head), (x, y) => x.SelectMany(_ => selector(y))));
        }

        public static Unit ForEach<T>(this IEnumerable<T> enumerable, Func<T, Cps<Unit, T>, Cps<Unit, T>, Cps<Unit, Unit>> f)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");
            if (f == null)
                throw new ArgumentNullException("f");

            Func<Unit, Unit> id = _ => _;

            return Cps.CallCC<Unit, T, Unit>(@break =>
                Cps.For(enumerable, i =>
                    Cps.CallCC<Unit, T, Unit>(@continue =>
                        f(i, @break(Unit.Instance), @continue(Unit.Instance)))))
                .Run(id);
        }
    }

    public static class CpsExtensions
    {
        public static TResult Run<TResult, T>(this Cps<TResult, T> cont, Func<T, TResult> f)
        {
            if (cont == null)
                throw new ArgumentNullException("cont");
            if (f == null)
                throw new ArgumentNullException("f");

            return cont(f);
        }

        public static Cps<TResult, U> Select<TResult, T, U>(
            this Cps<TResult, T> cont,
            Func<T, U> selector)
        {
            if (cont == null)
                throw new ArgumentNullException("cont");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return k => cont(t => k(selector(t)));
        }

        public static Cps<TResult, U> SelectMany<TResult, T, U>(
            this Cps<TResult, T> cont,
            Func<T, Cps<TResult, U>> selector)
        {
            if (cont == null)
                throw new ArgumentNullException("cont");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return k => cont(t => selector(t)(k));
        }

        public static Cps<TResult, V> SelectMany<TResult, T, U, V>(
            this Cps<TResult, T> cont,
            Func<T, Cps<TResult, U>> selector,
            Func<T, U, V> projector)
        {
            if (cont == null)
                throw new ArgumentNullException("cont");
            if (selector == null)
                throw new ArgumentNullException("selector");
            if (projector == null)
                throw new ArgumentNullException("projector");

            return cont.SelectMany(x => selector(x).Select(y => projector(x, y)));
        }
    }
}
