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
    public static partial class Either
    {
        public static T Force<TException, T>(this IEither<TException, T> either)
            where TException : Exception
        {
            return either.Case(
                left: exception => { throw exception; },
                right: value => value);
        }

        public static IEither<TException, T> Throw<TException, T>(TException exception)
        {
            return Either.Left<TException, T>(exception);
        }

        public static IEither<TException, T> Catch<TException, T>(Func<T> valueFactory)
            where TException : Exception
        {
            try
            {
                return Either.Right<TException, T>(valueFactory());
            }
            catch (TException exception)
            {
                return Either.Left<TException, T>(exception);
            }
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class EitherExtensions
    {
        public static IEither<TException, T1> Select<TException, T0, T1>(
            this IEither<TException, T0> either,
                 Func<T0, T1> selector)
        {
            return either.Case(
                left: exception => Either.Left<TException, T1>(exception),
                right: value => Either.Right<TException, T1>(selector(value)));
        }

        public static IEither<TException, T1> SelectMany<TException, T0, T1>(
            this IEither<TException, T0> either,
                 Func<T0, IEither<TException, T1>> selector)
        {
            return either.Case(
                left: exception => Either.Left<TException, T1>(exception),
                right: value => selector(value));
        }

        public static IEither<TException, T2> SelectMany<TException, T0, T1, T2>(
            this IEither<TException, T0> either,
                 Func<T0, IEither<TException, T1>> selector,
                 Func<T0, T1, T2> projector)
        {
            return either.SelectMany(value0 => selector(value0).Select(value1 => projector(value0, value1)));
        }
    }
}
