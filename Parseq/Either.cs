using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq
{
    public enum Hand {
        Left = 1,
        Right = -1,
    }

    public abstract partial class Either<TLeft,TRight>
        : IEquatable<Either<TLeft,TRight>>
    {
        public abstract Hand TryGetValue(out TLeft left, out TRight right);
    }

    partial class Either<TLeft, TRight>
    {
        public sealed class LeftProduction : Either<TLeft,TRight>{
            private readonly TLeft _left;
            private readonly TRight _right;

            public LeftProduction(TLeft value){
                this._left = value;
                this._right = default(TRight);
            }

            public override Option<TLeft> Left {
                get { return _left; }
            }

            public override Option<TRight> Right {
                get { return Option.None<TRight>(); }
            }

            public override Hand TryGetValue(out TLeft left, out TRight right){
                left = _left;
                right = _right;
                return Hand.Left;
            }
        }

        public sealed class RightProduction : Either<TLeft, TRight> {
            private readonly TLeft _left;
            private readonly TRight _right;

            public RightProduction(TRight value){
                _left = default(TLeft);
                _right = value;
            }

            public override Option<TLeft> Left{
                get { return Option.None<TLeft>(); }
            }

            public override Option<TRight> Right{
                get { return this._right; }
            }

            public override Hand TryGetValue(out TLeft left, out TRight right){
                left = _left;
                right = _right;
                return Hand.Right;
            }
        }
    }

    partial class Either<TLeft, TRight>
    {
        public virtual bool TryGetLeft(out TLeft value){
            TRight right;
            return this.TryGetValue(out value, out right) == Hand.Left;
        }

        public virtual bool TryGetRight(out TRight value){
            TLeft left;
            return this.TryGetValue(out left, out value) == Hand.Left;
        }

        public virtual Option<TLeft> Left {
            get {
                TLeft left; TRight right;
                return this.TryGetValue(out left, out right) == Hand.Left ?
                    Option.Just<TLeft>(left) :
                    Option.None<TLeft>();
            }
        }

        public virtual Option<TRight> Right {
            get {
                TLeft left; TRight right;
                return this.TryGetValue(out left, out right) == Hand.Left ?
                    Option.None<TRight>() :
                    Option.Just<TRight>(right);
            }
        }

        public bool Equals(Either<TLeft, TRight> other){
            return other != null 
                && this.Left.Equals(other.Left)
                && this.Right.Equals(other.Right);
        }

        public virtual bool Equals(Option<TLeft> other){
            return this.Left.Equals(other);
        }

        public virtual bool Equals(Option<TRight> other){
            return this.Right.Equals(other);
        }

        public virtual bool Equals(TLeft other){
            return this.Left.Equals(other);
        }

        public virtual bool Equals(TRight other){
            return this.Right.Equals(other);
        }
    }

    public static class Either {
        public static Either<TLeft, TRight> Left<TLeft, TRight>(TLeft value){
            return new Either<TLeft, TRight>.LeftProduction(value);
        }

        public static Either<TLeft, TRight> Right<TLeft, TRight>(TRight value){
            return new Either<TLeft, TRight>.RightProduction(value);
        }

        public static Either<T, TException> Try<T,TException>(Func<T> selector)
            where TException : Exception
        {
            if (selector == null)
                throw new ArgumentNullException("selector");
            try { 
                return Either.Left<T, TException>(selector());
            }
            catch (TException e){
                return Either.Right<T, TException>(e);
            }
        }
    }
}
