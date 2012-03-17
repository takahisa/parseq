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
        public abstract bool CanNext();
        public abstract bool CanRewind();
        public abstract Stream<TToken> Next();
        public abstract Stream<TToken> Rewind();
        public abstract void Dispose();
    }

    partial class Stream<TToken>
    {
        public virtual int CompareTo(Stream<TToken> other){
            return (this.Position.Index - other.Position.Index);
        }

        public static bool operator >(Stream<TToken> x, Stream<TToken> y){
            return x.Position > y.Position;
        }

        public static bool operator >=(Stream<TToken> x, Stream<TToken> y){
            return x.Position >= y.Position;
        }

        public static bool operator <(Stream<TToken> x, Stream<TToken> y){
            return x.Position < y.Position;
        }

        public static bool operator <=(Stream<TToken> x, Stream<TToken> y){
            return x.Position <= y.Position;
        }

        public static Stream<TToken> operator >>(Stream<TToken> stream, int count){
            return Enumerable
                .Range(1, count)
                .Aggregate(stream, (s, i) => s.Next());
        }

        public static Stream<TToken> operator <<(Stream<TToken> stream, int count){
            return Enumerable
                .Range(1, count)
                .Aggregate(stream, (s, i) => s.Rewind());
        }
    }

}
