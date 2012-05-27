using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq
{
    public static class StreamExtensions {

        public static Stream<T> Where<T>(this Stream<T> stream, Func<T, Boolean> predicate){
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (predicate == null)
                throw new ArgumentNullException("stream");

            T result;
            var current = stream;
            while (current.TryGetValue(out result) && predicate(result))
                current = stream.Next();
            return current;
        }

        public static Stream<T> Where<T>(this Stream<T> stream, Func<Stream<T>, T, Boolean> predicate){
            return stream.Where(_ => predicate(stream, _));
        }

        public static Stream<U> Select<T, U>(this Stream<T> stream, Func<T, U> selector){
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return new StreamMapper<T, U>(stream, selector);
        }

        public static Stream<U> Select<T, U>(this Stream<T> stream, Func<Stream<T>, U> selector){
            return stream.Select((T _) => selector(stream));
        }

        public static Stream<U> Select<T, U>(this Stream<T> stream, Func<Stream<T>, T, U> selector){
            return stream.Select((T _) => selector(stream, _));
        }

        public static Stream<U> SelectMany<T, U>(this Stream<T> stream, Func<T, Stream<U>> selector){
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (selector == null)
                throw new ArgumentNullException("selector");

            T result;
            if (stream.TryGetValue(out result))
                return selector(result);
            else
                throw new InvalidOperationException();
        }

        public static Stream<U> SelectMany<T, U>(this Stream<T> stream, Func<Stream<T>, Stream<U>> selector){
            return stream.SelectMany((T _) => selector(stream));
        }

        public static Stream<U> SelectMany<T, U>(this Stream<T> stream, Func<Stream<T>, T, Stream<U>> selector){
            return stream.SelectMany((T _) => selector(stream, _));
        }

        private class StreamMapper<T, U> : Stream<U> {
            private readonly Stream<T> _stream;
            private readonly Func<T, U> _selector;

            public StreamMapper(Stream<T> stream, Func<T, U> selector){
                if (stream == null)
                    throw new ArgumentNullException("stream");
                if (selector == null)
                    throw new ArgumentNullException("selector");
                _stream = stream;
                _selector = selector;
            }

            public override Position Position {
                get { return _stream.Position; }
            }

            public override Boolean CanNext() {
                return _stream.CanNext();
            }

            public override Boolean CanRewind() {
                return _stream.CanRewind();
            }

            public override Stream<U> Next() {
                return new StreamMapper<T, U>(_stream.Next(), _selector);
            }

            public override Stream<U> Rewind(){
                return new StreamMapper<T, U>(_stream.Rewind(), _selector);
            }

            public override Boolean TryGetValue(out U value) {
                T result;
                if (_stream.TryGetValue(out result)) {
                    value = _selector(result);
                    return true;
                }
                else {
                    value = default(U);
                    return false;
                }
            }

            public override U Perform() {
                T result;
                if (_stream.TryGetValue(out result))
                    return _selector(result);
                else
                    throw new InvalidOperationException();
            }

            public override void Dispose() {
                _stream.Dispose();
            }
        }
    }
}
