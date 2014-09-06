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
    public static class Lazy
    {
        public static Lazy<T> Return<T>(Func<T> func)
        {
            if (func == null)
                throw new ArgumentNullException("func");

            return new Lazy<T>(func);
        }

        public static Lazy<T> Return<T>(T value)
        {
            return new Lazy<T>(() => value);
        }

        public static T Force<T>(this Lazy<T> lazyVal)
        {
            if (lazyVal == null)
                throw new ArgumentNullException("lazyVal");

            return lazyVal.Value;
        }

        public static Boolean TryGetValue<T>(this Lazy<T> lazyVal, out T value)
        {
            if (lazyVal.IsValueCreated)
            {
                value = lazyVal.Value;
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }
    }
}
