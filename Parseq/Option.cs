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
        public abstract bool TryGetValue(out T value);
    }

    public abstract partial class Option<T>
        : IEquatable<Option<T>>
        , IEquatable<T>
    {
        public sealed class Just : Option<T>{
            private readonly T _value;

            public Just(T value){
                this._value = value;
            }

            public T Value {
                get { return _value; }
            }

            public override T Perform(){
                return this.Value;
            }

            public override bool TryGetValue(out T value){
                value = this.Value;
                return true;
            }
        }

        public sealed class None : Option<T>{
            public None() { }

            public override T Perform(){
                throw new InvalidOperationException();
            }

            public override bool TryGetValue(out T value){
                value = default(T);
                return false;
            }
        }

    }

    public abstract partial class Option<T>
        : IEquatable<Option<T>>
        , IEquatable<T>
    {
        public virtual bool Equals(Option<T> other){
            if (other == null)
                throw new ArgumentNullException("other");

            T a, b;
            var ra = this.TryGetValue(out a);
            var rb = other.TryGetValue(out b);
            return (ra == rb) && ( !ra || EqualityComparer<T>.Default.Equals(a, b));
        }

        public virtual bool Equals(T other){
            return this.Equals(Option.Just(other));
        }

        public override int GetHashCode(){
            return base.GetHashCode();
        }

        public override bool Equals(object obj){
            return ((obj is Option<T>) && this.Equals((Option<T>)obj))
                || ((obj is T) && this.Equals((T)obj));
        }

        public override string ToString(){
            return base.ToString();
        }

        public static implicit operator Option<T>(T value){
            return new Option<T>.Just(value);
        }

        public static explicit operator T(Option<T> option){
            T value;
            if (option.TryGetValue(out value))
                return value;
            else
                throw new InvalidCastException();
        }
    }

    public static class Option {
        public static Option<T> Just<T>(T value){
            return new Option<T>.Just(value);
        }

        public static Option<T> None<T>(){
            return new Option<T>.None();
        }
    }
}
