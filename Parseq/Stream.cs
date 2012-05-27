using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq
{
    public abstract partial class Stream<TToken> 
        : Option<TToken>
        , IComparable<Stream<TToken>>
        , IDisposable
    {
        public abstract Position Position { get; }
        public abstract Boolean CanNext();
        public abstract Boolean CanRewind();
        public abstract Stream<TToken> Next();
        public abstract Stream<TToken> Rewind();
        public abstract void Dispose();
    }

    partial class Stream<TToken>
    {
        public virtual Int32 CompareTo(Stream<TToken> other){
            return (this.Position.Index - other.Position.Index);
        }

        public static Boolean operator >(Stream<TToken> x, Stream<TToken> y){
            return x.Position > y.Position;
        }

        public static Boolean operator >=(Stream<TToken> x, Stream<TToken> y){
            return x.Position >= y.Position;
        }

        public static Boolean operator <(Stream<TToken> x, Stream<TToken> y){
            return x.Position < y.Position;
        }

        public static Boolean operator <=(Stream<TToken> x, Stream<TToken> y){
            return x.Position <= y.Position;
        }

        public static Stream<TToken> operator >>(Stream<TToken> stream, Int32 count){
            return Enumerable
                .Range(1, count)
                .Aggregate(stream, (s, i) => s.Next());
        }

        public static Stream<TToken> operator <<(Stream<TToken> stream, Int32 count){
            return Enumerable
                .Range(1, count)
                .Aggregate(stream, (s, i) => s.Rewind());
        }
    }

}
