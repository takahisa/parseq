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
    public enum Hand
    {
        Left = 1,
        Right = -1,
    }

    public abstract partial class Either<TLeft, TRight>
        : IEquatable<Either<TLeft, TRight>>
    {
        public abstract Hand TryGetValue(out TLeft left, out TRight right);
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

            public override Option<TLeft> Left
            {
                get { return _left; }
            }

            public override Option<TRight> Right
            {
                get { return Option.None<TRight>(); }
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

            public override Option<TLeft> Left
            {
                get { return Option.None<TLeft>(); }
            }

            public override Option<TRight> Right
            {
                get { return this._right; }
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

        public virtual Option<TLeft> Left
        {
            get
            {
                TLeft left; TRight right;
                return this.TryGetValue(out left, out right) == Hand.Left 
                    ? Option.Just<TLeft>(left)
                    : Option.None<TLeft>();
            }
        }

        public virtual Option<TRight> Right
        {
            get
            {
                TLeft left; TRight right;
                return this.TryGetValue(out left, out right) == Hand.Left 
                    ? Option.None<TRight>()
                    : Option.Just<TRight>(right);
            }
        }

        public Boolean Equals(Either<TLeft, TRight> other)
        {
            return other != null
                && this.Left.Equals(other.Left)
                && this.Right.Equals(other.Right);
        }

        public virtual Boolean Equals(Option<TLeft> other)
        {
            return this.Left.Equals(other);
        }

        public virtual Boolean Equals(Option<TRight> other)
        {
            return this.Right.Equals(other);
        }

        public virtual Boolean Equals(TLeft other)
        {
            return this.Left.Equals(other);
        }

        public virtual Boolean Equals(TRight other)
        {
            return this.Right.Equals(other);
        }
    }

    public static class Either
    {
        public static Either<TLeft, TRight> Left<TLeft, TRight>(TLeft value)
        {
            return new Either<TLeft, TRight>.LeftProduction(value);
        }

        public static Either<TLeft, TRight> Right<TLeft, TRight>(TRight value)
        {
            return new Either<TLeft, TRight>.RightProduction(value);
        }

        public static Either<T, TException> Try<T, TException>(Func<T> selector)
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
    }
}
