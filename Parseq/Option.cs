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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq
{
    public abstract partial class Option<T>
        : IEquatable<Option<T>>
        , IEquatable<T>
    {
        public abstract T Perform();
        public abstract Boolean TryGetValue(out T value);
    }

    partial class Option<T>
    {
        public sealed class Just : Option<T>
        {
            private readonly T _value;

            public Just(T value)
            {
                this._value = value;
            }

            public T Value
            {
                get { return _value; }
            }

            public override T Perform()
            {
                return this.Value;
            }

            public override Boolean TryGetValue(out T value)
            {
                value = this.Value;
                return true;
            }
        }

        public sealed class None : Option<T>
        {
            public None() { }

            public override T Perform()
            {
                throw new InvalidOperationException();
            }

            public override Boolean TryGetValue(out T value)
            {
                value = default(T);
                return false;
            }
        }
    }

    partial class Option<T>
    {
        public virtual Boolean Equals(Option<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            T a, b;
            var ra = this.TryGetValue(out a);
            var rb = other.TryGetValue(out b);
            return (ra == rb) && (!ra || EqualityComparer<T>.Default.Equals(a, b));
        }

        public virtual Boolean Equals(T other)
        {
            return this.Equals(Option.Just(other));
        }

        public override Int32 GetHashCode()
        {
            return base.GetHashCode();
        }

        public override Boolean Equals(object obj)
        {
            return ((obj is Option<T>) && this.Equals((Option<T>)obj))
                || ((obj is T) && this.Equals((T)obj));
        }

        public override String ToString()
        {
            return base.ToString();
        }

        public static implicit operator Option<T>(T value)
        {
            return new Option<T>.Just(value);
        }

        public static explicit operator T(Option<T> option)
        {
            T value;
            if (option.TryGetValue(out value))
                return value;
            else
                throw new InvalidCastException();
        }
    }

    public static class Option
    {
        public static Option<T> Just<T>(T value)
        {
            return new Option<T>.Just(value);
        }

        public static Option<T> None<T>()
        {
            return new Option<T>.None();
        }

        public static Option<T> Try<T, TException>(Func<T> selector)
            where TException : Exception
        {
            if (selector == null)
                throw new ArgumentNullException("selector");

            try
            {
                return Option.Just(selector());
            }
            catch (TException)
            {
                return Option.None<T>();
            }
        }

        public static Option<T> Try<T, TException>(Func<Option<T>> selector)
            where TException : Exception
        {
            if (selector == null)
                throw new ArgumentNullException("selector");

            try
            {
                return selector();
            }
            catch (TException)
            {
                return Option.None<T>();
            }
        }

        public static Option<T> Try<T>(Func<T> selector)
        {
            return Option.Try<T, Exception>(selector);
        }

        public static Option<T> Try<T>(Func<Option<T>> selector)
        {
            return Option.Try<T, Exception>(selector);
        }
    }
}
