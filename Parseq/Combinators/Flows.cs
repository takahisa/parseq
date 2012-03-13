using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq.Combinators
{
    public static class Flows {

        public static bool Success<TToken, TResult>(this Parser<TToken, TResult> parser, Stream<TToken> stream){
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (stream == null)
                throw new ArgumentNullException("stream");

            TResult result; ErrorMessage message;
            return ReplyStatus.Success == parser(stream).TryGetValue(out result, out message);
        }

        public static bool Failure<TToken, TResult>(this Parser<TToken, TResult> parser, Stream<TToken> stream){
            if (parser == null)
                throw new ArgumentNullException("parser");

            TResult result; ErrorMessage message;
            return ReplyStatus.Failure == parser(stream).TryGetValue(out result, out message);
        }

        public static bool Error<TToken, TResult>(this Parser<TToken, TResult> parser, Stream<TToken> stream){
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (stream == null)
                throw new ArgumentNullException("stream");
            TResult result; ErrorMessage message;
            return ReplyStatus.Error == parser(stream).TryGetValue(out result, out message);
        }

        public static Parser<TToken, IEnumerable<TResult>> While<TToken, T, TResult>(
            this Parser<TToken, TResult> parser, Parser<TToken, T> condition)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (condition == null)
                throw new ArgumentNullException("condition");

            return stream => {
                Reply<TToken, TResult> reply;
                TResult result; ErrorMessage message;

                var results = new List<TResult>();
                var current = stream;

                while (condition.Success(current)){
                    switch ((reply = parser(current)).TryGetValue(out result, out message)){
                        case ReplyStatus.Success:
                            results.Add(result);
                            current = reply.Stream;
                            break;
                        case ReplyStatus.Failure:
                            return Reply.Failure<TToken, IEnumerable<TResult>>(stream);
                        default:
                            return Reply.Error<TToken, IEnumerable<TResult>>(stream, message);
                    }
                }
                return Reply.Success<TToken, IEnumerable<TResult>>(current, results);
            };
        }

        public static Parser<TToken, IEnumerable<TResult>> Until<TToken, T, TResult>(
            this Parser<TToken, TResult> parser, Parser<TToken, T> condition)
        {
            return parser.While(condition.Not());
        }

        public static Parser<TToken, TResult> If<TToken, T, TResult>(
            Parser<TToken, T> condition, 
            Parser<TToken, TResult> thenParser,
            Parser<TToken, TResult> elseParser)
        {
            if (condition == null)
                throw new ArgumentNullException("condition");
            if (thenParser == null)
                throw new ArgumentNullException("thenParser");
            if (elseParser == null)
                throw new ArgumentNullException("elseParser");

            return stream => condition.Success(stream)
                ? thenParser(stream)
                : elseParser(stream);
        }

        public static Parser<TToken, Either<TResult0, TResult1>> If<TToken, T, TResult0, TResult1>(
            Parser<TToken, T> condition,
            Parser<TToken, TResult0> thenParser,
            Parser<TToken, TResult1> elseParser)
        {
            if (condition == null)
                throw new ArgumentNullException("condition");
            if (thenParser == null)
                throw new ArgumentNullException("thenParser");
            if (elseParser == null)
                throw new ArgumentNullException("elseParser");

            return stream => condition.Success(stream)
                ? thenParser(stream).Select(_ => Either.Left<TResult0, TResult1>(_))
                : elseParser(stream).Select(_ => Either.Right<TResult0, TResult1>(_));
        }

        public static Parser<TToken, TResult> Unless<TToken, T, TResult>(
            Parser<TToken, T> condition,
            Parser<TToken, TResult> thenParser,
            Parser<TToken, TResult> elseParser)
        {
            if (condition == null)
                throw new ArgumentNullException("condition");
            if (thenParser == null)
                throw new ArgumentNullException("thenParser");
            if (elseParser == null)
                throw new ArgumentNullException("elseParser");

            return stream => !condition.Success(stream)
                ? thenParser(stream)
                : elseParser(stream);
        }

        public static Parser<TToken, Either<TResult0, TResult1>> Unless<TToken, T, TResult0, TResult1>(
            Parser<TToken, T> condition,
            Parser<TToken, TResult0> thenParser,
            Parser<TToken, TResult1> elseParser)
        {
            if (condition == null)
                throw new ArgumentNullException("condition");
            if (thenParser == null)
                throw new ArgumentNullException("thenParser");
            if (elseParser == null)
                throw new ArgumentNullException("elseParser");

            return stream => !condition.Success(stream)
                ? thenParser(stream).Select(_ => Either.Left<TResult0, TResult1>(_))
                : elseParser(stream).Select(_ => Either.Right<TResult0, TResult1>(_));
        }
    }
}
