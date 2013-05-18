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
    public static class OptionExtensions
    {
        public static Boolean TryGetValue<T>(this IOption<T> self, out T value)
        {
            // TODO: Assumed that self is Option<T> implicitly
            return ((Option<T>)self).TryGetValue(out value);
        }

        public static Boolean Exists<T>(this IOption<T> option)
        {
            T value;
            return option.TryGetValue(out value);
        }

        public static T Otherwise<T>(this IOption<T> option, Func<T> selector)
        {
            if (option == null)
                throw new ArgumentNullException("option");
            if (selector == null)
                throw new ArgumentNullException("selector");

            T value;
            return option.TryGetValue(out value) ?
                value :
                selector();
        }

        public static IOption<T> Where<T>(this IOption<T> option, Func<T, Boolean> predicate)
        {
            if (option == null)
                throw new ArgumentNullException("option");
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            T value;
            return option.TryGetValue(out value) && predicate(value) ?
                Option.Just<T>(value) :
                Option.None<T>();
        }

        public static IOption<U> Select<T, U>(this IOption<T> option, Func<T, U> selector)
        {
            if (option == null)
                throw new ArgumentNullException("option");
            if (selector == null)
                throw new ArgumentNullException("selector");

            T value;
            return option.TryGetValue(out value) ?
                Option.Just(selector(value)) :
                Option.None<U>();
        }

        public static IOption<U> SelectMany<T, U>(this IOption<T> option, Func<T, IOption<U>> selector)
        {
            if (option == null)
                throw new ArgumentNullException("option");
            if (selector == null)
                throw new ArgumentNullException("selector");

            T value;
            return option.TryGetValue(out value) ?
                selector(value) :
                Option.None<U>();
        }

        public static IOption<V> SelectMany<T, U, V>(this IOption<T> option, Func<T, IOption<U>> selector, Func<T, U, V> projector)
        {
            if (option == null)
                throw new ArgumentNullException("option");
            if (selector == null)
                throw new ArgumentNullException("selector");
            if (projector == null)
                throw new ArgumentNullException("projector");

            return option.SelectMany(x => selector(x).Select(y => projector(x, y)));
        }
    }
}
