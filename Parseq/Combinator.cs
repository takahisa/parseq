using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Parseq.Combinators;

namespace Parseq
{
    public static partial class Combinator
    {
        public static Parser<TToken, IEnumerable<TResult>> Sequence<TToken, TResult>(
            params Parser<TToken, TResult>[] parsers)
        {
            return Combinator.Sequence(parsers.AsEnumerable());
        }

        public static Parser<TToken, IEnumerable<TResult>> Sequence<TToken, TResult>(
            this IEnumerable<Parser<TToken, TResult>> parsers)
        {
            if (parsers == null)
                throw new ArgumentNullException("parsers");

            return stream => parsers.Match(
                () => Reply.Success<TToken, IEnumerable<TResult>>(stream, Enumerable.Empty<TResult>()),
                (head, tail) => head.SelectMany(x => Combinator.Sequence(tail).Select(y => x.Concat(y)))(stream));
        }

        public static Parser<TToken, TResult> Choice<TToken, TResult>(
            params Parser<TToken, TResult>[] parsers)
        {
            return Combinator.Choice(parsers.AsEnumerable());
        }

        public static Parser<TToken, TResult> Choice<TToken, TResult>(
            this IEnumerable<Parser<TToken, TResult>> parsers)
        {
            if (parsers == null)
                throw new ArgumentNullException("parsers");

            return stream => parsers.Match(
                () => Reply.Failure<TToken, TResult>(stream),
                (head, tail) => head.Or(() => Combinator.Choice(tail))(stream));
        }

        public static Parser<TToken, TResult> Or<TToken, TResult>(
            this Parser<TToken, TResult> parser0,
            Func<Parser<TToken, TResult>> parser1)
        {
            if (parser0 == null)
                throw new ArgumentNullException("parser0");
            if (parser1 == null)
                throw new ArgumentNullException("parser1");

            return stream =>
            {
                Reply<TToken, TResult> reply;
                TResult result; ErrorMessage message;
                switch ((reply = parser0(stream)).TryGetValue(out result, out message))
                {
                    case ReplyStatus.Success:
                        return Reply.Success<TToken, TResult>(reply.Stream, result);
                    case ReplyStatus.Error:
                        return Reply.Error<TToken, TResult>(stream, message);
                    default:
                        return parser1()(stream);
                }
            };
        }

        public static Parser<TToken, TResult> Or<TToken, TResult>(
            this Parser<TToken, TResult> parser0,
            Parser<TToken, TResult> parser1)
        {
            if (parser0 == null)
                throw new ArgumentNullException("parser0");
            if (parser1 == null)
                throw new ArgumentNullException("parser1");

            return Combinator.Or(parser0, () => parser1);
        }

        public static Parser<TToken, Unit> And<TToken, TResult>(
            this Parser<TToken, TResult> parser)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");

            return stream =>
            {
                Reply<TToken, TResult> reply;
                TResult result; ErrorMessage message;
                switch ((reply = parser(stream)).TryGetValue(out result, out message))
                {
                    case ReplyStatus.Success: return Reply.Success<TToken, Unit>(stream,Unit.Instance);
                    case ReplyStatus.Failure: return Reply.Failure<TToken, Unit>(stream);
                    default:
                        return Reply.Error<TToken, Unit>(stream, message);
                }
            };
        }

        public static Parser<TToken, Unit> Not<TToken, TResult>(
            this Parser<TToken, TResult> parser)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");

            return stream =>
            {
                Reply<TToken, TResult> reply;
                TResult result; ErrorMessage message;
                switch ((reply = parser(stream)).TryGetValue(out result, out message))
                {
                    case ReplyStatus.Success: return Reply.Failure<TToken, Unit>(stream);
                    case ReplyStatus.Failure: return Reply.Success<TToken, Unit>(stream, Unit.Instance);
                    default:
                        return Reply.Error<TToken, Unit>(stream, message);
                }
            };
        }

        public static Parser<TToken, IEnumerable<TResult>> Repeat<TToken, TResult>(
            this Parser<TToken, TResult> parser, Int32 count)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            return Combinator.Sequence(parser.Replicate().Take(count));
        }

        public static Parser<TToken, IEnumerable<TResult>> Many<TToken, TResult>(
            this Parser<TToken, TResult> parser)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");

            return parser.Maybe().SelectMany(t => t.Select(x => Many(parser)
                    .Select(y => x.Concat(y)))
                    .Otherwise(() => s => Reply.Success(s, Enumerable.Empty<TResult>())));
        }

        public static Parser<TToken, IEnumerable<TResult>> Many<TToken, TResult>(
            this Parser<TToken, TResult> parser, Int32 min)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (min < 0)
                throw new ArgumentOutOfRangeException("min");

            return Combinator.Min(parser, min);
        }

        public static Parser<TToken, IEnumerable<TResult>> Many<TToken, TResult>(
            this Parser<TToken, TResult> parser, Int32 min, Int32 max)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (min < 0)
                throw new ArgumentOutOfRangeException("min");
            if (max < min)
                throw new ArgumentOutOfRangeException("max");

            return Combinator.Repeat(parser, min).SelectMany(x => Max(parser, (max - min)).Select(y => x.Concat(y)));
        }

        private static Parser<TToken,IEnumerable<TResult>> Min<TToken,TResult>(
            this Parser<TToken, TResult> parser, Int32 min)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (min < 0)
                throw new ArgumentOutOfRangeException("min");

            return (min == 0)
                ? parser.Many()
                : parser.SelectMany(x => Min(parser, min - 1).Select(y => x.Concat(y)));
        }

        private static Parser<TToken, IEnumerable<TResult>> Max<TToken, TResult>(
            this Parser<TToken, TResult> parser, Int32 max)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (max < 0)
                throw new ArgumentOutOfRangeException("max");

            return (max == 0)
                ? Enumerable.Empty<TResult>().Return<TToken, IEnumerable<TResult>>()
                : parser.SelectMany(x => Max(parser, max - 1).Select(y => x.Concat(y)));
        }

        public static Parser<TToken, Option<TResult>> Maybe<TToken, TResult>(
            this Parser<TToken, TResult> parser)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");

            return stream =>
            {
                Reply<TToken, TResult> reply;
                TResult result; ErrorMessage message;
                switch ((reply = parser(stream)).TryGetValue(out result, out message))
                {
                    case ReplyStatus.Success: return Reply.Success<TToken, Option<TResult>>(reply.Stream, Option.Just(result));
                    case ReplyStatus.Failure: return Reply.Success<TToken, Option<TResult>>(stream, Option.None<TResult>());
                    default:
                        return Reply.Error<TToken, Option<TResult>>(stream, message);
                }
            };
        }

        public static Parser<TToken, TResult> Lazy<TToken, TResult>(
            this Func<Parser<TToken, TResult>> parser)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");

            return stream => parser()(stream);
        }

        public static Parser<TToken, TResult> Lazy<TToken, TResult>(
            this Parser<TToken, TResult> parser)
        {
            return Combinator.Lazy(() => parser);
        }

        public static Parser<TToken, Unit> Ignore<TToken, TResult>(
            this Parser<TToken, TResult> parser)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");

            return parser.Select(_ => Unit.Instance);
        }
    }
}
