using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq.Combinators
{
    public static class Flows
    {
        public static Boolean Success<TToken, TResult>(this Parser<TToken, TResult> parser, Stream<TToken> stream)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (stream == null)
                throw new ArgumentNullException("stream");

            TResult result; ErrorMessage message;
            return ReplyStatus.Success == parser(stream).TryGetValue(out result, out message);
        }

        public static Boolean Failure<TToken, TResult>(this Parser<TToken, TResult> parser, Stream<TToken> stream)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");

            TResult result; ErrorMessage message;
            return ReplyStatus.Failure == parser(stream).TryGetValue(out result, out message);
        }

        public static Boolean Error<TToken, TResult>(this Parser<TToken, TResult> parser, Stream<TToken> stream)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (stream == null)
                throw new ArgumentNullException("stream");
            TResult result; ErrorMessage message;
            return ReplyStatus.Error == parser(stream).TryGetValue(out result, out message);
        }

        public static Parser<TToken, IEnumerable<TResult>> While<TToken, TResult, TCond>(
            this Parser<TToken, TResult> parser, Parser<TToken, TCond> condition)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (condition == null)
                throw new ArgumentNullException("condition");

            return stream =>
            {
                Reply<TToken, TResult> reply;
                TResult result; ErrorMessage message;
                var results = new List<TResult>();
                var current = stream;
                while (condition.Success(current))
                {
                    switch ((reply = parser(current)).TryGetValue(out result, out message))
                    {
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

        public static Parser<TToken, IEnumerable<TResult>> Until<TToken, TResult, TCond>(
            this Parser<TToken, TResult> parser, Parser<TToken, TCond> condition)
        {
            return parser.While(condition.Not());
        }

        public static Parser<TToken, TResult> DoWhenSuccess<TToken, TResult>(
            this Parser<TToken, TResult> parser, Action<TResult> action)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (action == null)
                throw new ArgumentNullException("action");

            return stream =>
            {
                Reply<TToken, TResult> reply;
                TResult result; ErrorMessage message;
                switch ((reply = parser(stream)).TryGetValue(out result, out message))
                {
                    case ReplyStatus.Success:
                        action(result);
                        return Reply.Success<TToken, TResult>(reply.Stream, result);
                    case ReplyStatus.Failure:
                        return Reply.Failure<TToken, TResult>(stream);
                    default:
                        return Reply.Error<TToken, TResult>(stream, message);
                }
            };
        }

        public static Parser<TToken, TResult> DoWhenFailure<TToken, TResult>(
            this Parser<TToken, TResult> parser, Action action)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (action == null)
                throw new ArgumentNullException("action");

            return stream =>
            {
                Reply<TToken, TResult> reply;
                TResult result; ErrorMessage message;
                switch ((reply = parser(stream)).TryGetValue(out result, out message))
                {
                    case ReplyStatus.Success:
                        return Reply.Success<TToken, TResult>(reply.Stream, result);
                    case ReplyStatus.Failure:
                        action();
                        return Reply.Failure<TToken, TResult>(stream);
                    default:
                        return Reply.Error<TToken, TResult>(stream, message);
                }
            };
        }

        public static Parser<TToken, TResult> DoWhenError<TToken, TResult>(
            this Parser<TToken, TResult> parser, Action<ErrorMessage> action)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (action == null)
                throw new ArgumentNullException("action");

            return stream =>
            {
                Reply<TToken, TResult> reply;
                TResult result; ErrorMessage message;
                switch ((reply = parser(stream)).TryGetValue(out result, out message))
                {
                    case ReplyStatus.Success:
                        return Reply.Success<TToken, TResult>(reply.Stream, result);
                    case ReplyStatus.Failure:
                        return Reply.Failure<TToken, TResult>(stream);
                    default:
                        action(message);
                        return Reply.Error<TToken, TResult>(stream, message);
                }
            };
        }

        public static Parser<TToken, TResult> If<TToken, TCond, TResult>(
            Parser<TToken, TCond> condition,
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

        public static Parser<TToken, Either<TResult0, TResult1>> If<TToken, TCond, TResult0, TResult1>(
            Parser<TToken, TCond> condition,
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

        public static Parser<TToken, TResult> Unless<TToken, TCond, TResult>(
            Parser<TToken, TCond> condition,
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

        public static Parser<TToken, Either<TResult0, TResult1>> Unless<TToken, TCond, TResult0, TResult1>(
            Parser<TToken, TCond> condition,
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
