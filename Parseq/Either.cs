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
    public enum Hand
    {
        Left = 1,
        Right = -1,
    }

    public interface IEither<out TLeft, out TRight>
    {
        Hand Hand { get; }
        IOption<TLeft> Left { get; }
        IOption<TRight> Right { get; }
    }

    public abstract partial class Either<TLeft, TRight>
        : IEither<TLeft, TRight>
        , IEquatable<IEither<TLeft, TRight>>
    {
        public abstract Hand Hand { get; }

        public abstract Hand TryGetValue(out TLeft left, out TRight right);

        public virtual Boolean Equals(IEither<TLeft, TRight> other)
        {
            return this.Equals<TLeft, TRight>(other);
        }
    }

    partial class Either<TLeft, TRight>
    {
        public sealed class LeftProduction : Either<TLeft, TRight>
        {
            private readonly TLeft _left;
            private readonly TRight _right;

            public LeftProduction(TLeft value)
            {
                this._left = value;
                this._right = default(TRight);
            }

            public override IOption<TLeft> Left
            {
                get { return Option.Just(_left); }
            }

            public override IOption<TRight> Right
            {
                get { return Option.None<TRight>(); }
            }

            public override Hand Hand
            {
                get { return Hand.Left; }
            }

            public override Hand TryGetValue(out TLeft left, out TRight right)
            {
                left = _left;
                right = _right;
                return Hand.Left;
            }
        }

        public sealed class RightProduction : Either<TLeft, TRight>
        {
            private readonly TLeft _left;
            private readonly TRight _right;

            public RightProduction(TRight value)
            {
                _left = default(TLeft);
                _right = value;
            }

            public override IOption<TLeft> Left
            {
                get { return Option.None<TLeft>(); }
            }

            public override IOption<TRight> Right
            {
                get { return Option.Just(this._right); }
            }

            public override Hand Hand
            {
                get { return Hand.Right; }
            }

            public override Hand TryGetValue(out TLeft left, out TRight right)
            {
                left = _left;
                right = _right;
                return Hand.Right;
            }
        }
    }

    partial class Either<TLeft, TRight>
    {
        public virtual Boolean TryGetLeft(out TLeft value)
        {
            TRight right;
            return this.TryGetValue(out value, out right) == Hand.Left;
        }

        public virtual Boolean TryGetRight(out TRight value)
        {
            TLeft left;
            return this.TryGetValue(out left, out value) == Hand.Left;
        }

        public virtual IOption<TLeft> Left
        {
            get
            {
                TLeft left; TRight right;
                return this.TryGetValue(out left, out right) == Hand.Left
                    ? Option.Just<TLeft>(left)
                    : Option.None<TLeft>();
            }
        }

        public virtual IOption<TRight> Right
        {
            get
            {
                TLeft left; TRight right;
                return this.TryGetValue(out left, out right) == Hand.Left
                    ? Option.None<TRight>()
                    : Option.Just<TRight>(right);
            }
        }
    }

    public static class Either
    {
        public static IEither<TLeft, TRight> Left<TLeft, TRight>(TLeft value)
        {
            return new Either<TLeft, TRight>.LeftProduction(value);
        }

        public static IEither<TLeft, TRight> Right<TLeft, TRight>(TRight value)
        {
            return new Either<TLeft, TRight>.RightProduction(value);
        }

        public static IEither<T, TException> Try<T, TException>(Func<T> selector)
            where TException : Exception
        {
            if (selector == null)
                throw new ArgumentNullException("selector");
            try
            {
                return Either.Left<T, TException>(selector());
            }
            catch (TException e)
            {
                return Either.Right<T, TException>(e);
            }
        }

        public static Boolean Equals<TLeft, TRight>(this IEither<TLeft, TRight> self, IEither<TLeft, TRight> other)
        {
            if (self == null)
                throw new ArgumentNullException("self");

            return other != null
                && self.Left.Equals(other.Left)
                && self.Right.Equals(other.Right);
        }

        public static Boolean Equals<TLeft, TRight>(this IEither<TLeft, TRight> self, IOption<TLeft> other)
        {
            if (self == null)
                throw new ArgumentNullException("self");

            return self.Left.Equals(other);
        }

        public static Boolean Equals<TLeft, TRight>(this IEither<TLeft, TRight> self, IOption<TRight> other)
        {
            if (self == null)
                throw new ArgumentNullException("self");

            return self.Right.Equals(other);
        }

        public static Boolean Equals<TLeft, TRight>(this IEither<TLeft, TRight> self, TLeft other)
        {
            if (self == null)
                throw new ArgumentNullException("self");

            return self.Left.Equals(other);
        }

        public static Boolean Equals<TLeft, TRight>(this IEither<TLeft, TRight> self, TRight other)
        {
            if (self == null)
                throw new ArgumentNullException("self");

            return self.Right.Equals(other);
        }
    }
}
