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
using System.ComponentModel;
using System.Collections.Generic;

namespace Parseq
{
    public static partial class Delayed
    {
        public static IDelayed<T1> Map<T0, T1>(
            this IDelayed<T0> value,
                 Func<T0, T1> func)
        {
            return value.Select(func);
        }

        public static IDelayed<T1> FlatMap<T0, T1>(
            this IDelayed<T0> value,
                 Func<T0, IDelayed<T1>> func)
        {
            return value.SelectMany(func);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class DelayedExtensions
    {
        public static IDelayed<T1> Select<T0, T1>(
            this IDelayed<T0> value,
                 Func<T0, T1> selector)
        {
            return Delayed.Return(() => selector(value.Force()));
        }

        public static IDelayed<T1> SelectMany<T0, T1>(
            this IDelayed<T0> value,
                 Func<T0, IDelayed<T1>> selector)
        {
            return selector(value.Force());
        }

        public static IDelayed<T2> SelectMany<T0, T1, T2>(
            this IDelayed<T0> value,
                 Func<T0, IDelayed<T1>> selector,
                 Func<T0, T1, T2> projector)
        {
            return value.SelectMany(value0 => selector(value0).Select(value1 => projector(value0, value1)));
        }
    }
}
