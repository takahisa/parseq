using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq
{
    public static class ParserExtensions {

        public static Reply<TToken,TResult> Run<TToken,TResult>(
            this Parser<TToken,TResult> parser,
            Stream<TToken> stream)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (stream == null)
                throw new ArgumentNullException("stream");
            return parser(stream);
        }

        public static Parser<TToken, T> Where<TToken, T>(
            this Parser<TToken, T> parser,
            Func<T, bool> predicate)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            return stream => parser(stream).Where(predicate);
        }

        public static Parser<TToken, U> Select<TToken, T, U>(
            this Parser<TToken, T> parser,
            Func<T, U> selector)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (selector == null)
                throw new ArgumentNullException("selector");
            return stream => {
                Reply<TToken, T> reply; T result; ErrorMessage message;
                switch ((reply = parser(stream)).TryGetValue(out result,out message)){
                    case ReplyStatus.Success: return Reply.Success<TToken, U>(reply.Stream, selector(result));
                    case ReplyStatus.Failure: return Reply.Failure<TToken, U>(stream);
                    default: return Reply.Error<TToken, U>(stream, message);
                }
            };
        }

        public static Parser<TToken, U> SelectMany<TToken, T, U>(
            this Parser<TToken, T> parser,
            Func<T, Parser<TToken,U>> selector)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (selector == null)
                throw new ArgumentNullException("selector");
            return stream =>
            {
                Reply<TToken, T> reply; T result; ErrorMessage message;
                switch ((reply = parser(stream)).TryGetValue(out result, out message)){
                    case ReplyStatus.Success: return selector(result)(reply.Stream);
                    case ReplyStatus.Failure: return Reply.Failure<TToken, U>(stream);
                    default: return Reply.Error<TToken, U>(stream, message);
                }
            };
        }

        public static Parser<TToken, V> SelectMany<TToken, T, U, V>(
            this Parser<TToken, T> parser,
            Func<T, Parser<TToken, U>> selector,
            Func<T, U, V> projector)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (selector == null)
                throw new ArgumentNullException("selector");
            if (projector == null)
                throw new ArgumentNullException("projector");
            return parser.SelectMany(x => selector(x).Select(y => projector(x, y)));
        }
    }
}
