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
using System.Linq;
using System.Collections.Generic;

namespace Parseq
{
    public static class LazyExtensions
    {
        public static Lazy<U> Select<T, U>(this Lazy<T> lazyVal, Func<T, U> selector)
        {
            if (lazyVal == null)
                throw new ArgumentNullException("lazyVal");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return new Lazy<U>(() => selector(lazyVal.Value));
        }

        public static Lazy<U> SelectMany<T, U>(this Lazy<T> lazyVal, Func<T, Lazy<U>> selector)
        {
            if (lazyVal == null)
                throw new ArgumentNullException("lazyVal");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return new Lazy<U>(() => selector(lazyVal.Force()).Force());
        }

        public static Lazy<V> SelectMany<T, U, V>(this Lazy<T> lazyVal, Func<T, Lazy<U>> selector, Func<T, U, V> projector)
        {
            if (lazyVal == null)
                throw new ArgumentNullException("lazyVal");
            if (selector == null)
                throw new ArgumentNullException("selector");
            if (projector == null)
                throw new ArgumentNullException("projector");

            return lazyVal.SelectMany(x => selector(x).Select(y => projector(x, y)));
        }
    }
}
