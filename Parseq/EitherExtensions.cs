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
    public static class EitherExtensions
    {
        public static T Merge<TLeft, TRight, T>(
            this Either<TLeft, TRight> either,
            Func<TLeft, T> lselector,
            Func<TRight, T> rselector)
        {
            if (either == null)
                throw new ArgumentNullException("either");
            if (lselector == null)
                throw new ArgumentNullException("lselector");
            if (rselector == null)
                throw new ArgumentNullException("rselector");

            TLeft left; TRight right;
            return either.TryGetValue(out left, out right) == Hand.Left
                ? lselector(left)
                : rselector(right);
        }

        public static TLeft MergeLeft<TLeft, TRight>(
            this Either<TLeft, TRight> either,
            Func<TRight, TLeft> selector)
        {
            if (either == null)
                throw new ArgumentNullException("either");
            if (selector == null)
                throw new ArgumentNullException("selector");

            TLeft left; TRight right;
            return either.TryGetValue(out left, out right) == Hand.Left
                ? left
                : selector(right);
        }

        public static TRight MergeRight<TLeft, TRight>(
            this Either<TLeft, TRight> either,
            Func<TLeft, TRight> selector)
        {
            if (either == null)
                throw new ArgumentNullException("either");
            if (selector == null)
                throw new ArgumentNullException("selector");

            TLeft left; TRight right;
            return either.TryGetValue(out left, out right) == Hand.Left
                ? selector(left)
                : right;
        }
    }
}
