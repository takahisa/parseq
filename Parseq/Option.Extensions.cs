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

namespace Parseq
{
    public static partial class Option
    {
        public static T1 Case<T0, T1>(
            this IOption<T0> option,
                 Func<T1> none,
                 Func<T0, T1> some)
        {
            return option.HasValue
                ? some(option.Value)
                : none();
        }

        public static IOption<T1> Map<T0, T1>(
            this IOption<T0> option,
                 Func<T0, T1> func)
        {
            return option.Select(func);
        }

        public static IOption<T1> FlatMap<T0, T1>(
            this IOption<T0> option,
                 Func<T0, IOption<T1>> func)
        {
            return option.SelectMany(func);
        }

        public static IOption<T> Filter<T>(
            this IOption<T> option,
                 Func<T, Boolean> predicate)
        {
            return option.Where(predicate);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class OptionExtensions
    {
        public static IOption<T> Where<T>(
            this IOption<T> option,
                 Func<T, Boolean> predicate)
        {
            return option.HasValue && predicate(option.Value)
                ? Option.Some<T>(option.Value)
                : Option.None<T>();
        }
        
        public static IOption<T1> Select<T0, T1>(
            this IOption<T0> option,
                 Func<T0, T1> selector)
        {
            return option.Case(
                none: Option.None<T1>,
                some: value =>
                    Option.Some<T1>(selector(value)));
        }

        public static IOption<T1> SelectMany<T0, T1>(
            this IOption<T0> option,
                 Func<T0, IOption<T1>> selector)
        {
            return option.HasValue
                ? selector(option.Value)
                : Option.None<T1>();
        }

        public static IOption<T2> SelectMany<T0, T1, T2>(
           this IOption<T0> option,
                Func<T0, IOption<T1>> selector,
                Func<T0, T1, T2> projector)
        {
            return option.SelectMany(value0 => selector(value0).Select(value1 => projector(value0, value1)));
        }
    }
}
