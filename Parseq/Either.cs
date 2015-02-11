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

namespace Parseq
{
    public interface IEither<out TLeft, out TRight>
    {
        TResult Case<TResult>(Func<TLeft, TResult> left, Func<TRight, TResult> right);
    }

    public partial class Either
    {
        public static IEither<TLeft, TRight> Left<TLeft, TRight>(TLeft value)
        {
            return new Either.LeftImpl<TLeft, TRight>(value);
        }

        public static IEither<TLeft, TRight> Right<TLeft, TRight>(TRight value)
        {
            return new Either.RightImpl<TLeft, TRight>(value);
        }
    }

    public partial class Either
    {
        class LeftImpl<TLeft, TRight>
            : IEither<TLeft, TRight>
        {
            private readonly TLeft value;

            public LeftImpl(TLeft value)
            {
                this.value = value;
            }

            public TResult Case<TResult>(Func<TLeft, TResult> left, Func<TRight, TResult> right)
            {
                return left(this.value);
            }
        }

        class RightImpl<TLeft, TRight>
            : IEither<TLeft, TRight>
        {
            private readonly TRight value;

            public RightImpl(TRight value)
            {
                this.value = value;
            }

            public TResult Case<TResult>(Func<TLeft, TResult> left, Func<TRight, TResult> right)
            {
                return right(this.value);
            }
        }
    }
}
