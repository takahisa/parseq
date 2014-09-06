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
    public interface IOption<out T>
    {
        Boolean HasValue { get; }

        T Value { get; }
    }

    public abstract partial class Option<T>
        : IOption<T>
        , IEquatable<IOption<T>>
        , IEquatable<T>
    {
        public abstract Boolean HasValue { get; }
        public abstract T Value { get; }
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

            public override Boolean HasValue
            {
                get { return true; }
            }

            public override T Value
            {
                get { return _value; }
            }
        }

        public sealed class None : Option<T>
        {
            public None() { }

            public override Boolean HasValue
            {
                get { return false; }
            }

            public override T Value
            {
                get { throw new InvalidOperationException(); }
            }
        }
    }

    partial class Option<T>
    {
        public virtual Boolean Equals(IOption<T> other)
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

        public override Boolean Equals(object obj)
        {
            return ((obj is IOption<T>) && this.Equals((IOption<T>)obj))
                || ((obj is T) && this.Equals((T)obj));
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
        public static IOption<T> Just<T>(T value)
        {
            return new Option<T>.Just(value);
        }

        public static IOption<T> None<T>()
        {
            return new Option<T>.None();
        }

        public static IOption<T> Try<T, TException>(Func<T> selector)
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

        public static IOption<T> Try<T, TException>(Func<IOption<T>> selector)
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

        public static IOption<T> Try<T>(Func<T> selector)
        {
            return Option.Try<T, Exception>(selector);
        }

        public static IOption<T> Try<T>(Func<IOption<T>> selector)
        {
            return Option.Try<T, Exception>(selector);
        }
    }
}
