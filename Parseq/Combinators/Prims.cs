using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq.Combinators
{
    public static class Prims{

        public static Parser<TToken, TResult> Return<TToken, TResult>(this TResult value)
        {
            return stream => Reply.Success<TToken, TResult>(stream, value);
        }

        public static Parser<TToken, TResult> Fail<TToken,TResult>()
        {
            return stream => Reply.Failure<TToken, TResult>(stream);
        }

        public static Parser<TToken, TResult> Error<TToken, TResult>(String message)
        {
            return Errors.Error<TToken, TResult>(message);                
        }

        public static Parser<TToken, TResult> Error<TToken, TResult>()
        {
            return Prims.Error<TToken, TResult>("Error");
        }

        public static Parser<TToken, TToken> Satisfy<TToken>(
            Func<TToken, Boolean> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException("selector");

            TToken value;
            return stream => stream.TryGetValue(out value) && predicate(value)
                    ? Reply.Success<TToken, TToken>(stream.Next(), value)
                    : Reply.Failure<TToken, TToken>(stream);
        }

        public static Parser<TToken, TToken> Any<TToken>()
        {
            return Prims.Satisfy<TToken>(_ => true);
        }

        public static Parser<TToken, TToken> OneOf<TToken>(
            Func<TToken, TToken, Boolean> predicate,
            params TToken[] candidates)
        {
            if (predicate == null)
                throw new ArgumentNullException("predicate");
            if (candidates == null)
                throw new ArgumentNullException("candidates");

            return Combinator.Choice(candidates.Select(x => Prims.Satisfy<TToken>(y => predicate(x, y))));
        }

        public static Parser<TToken, TToken> OneOf<TToken>(
            params TToken[] candidates)
        {
            return Prims.OneOf<TToken>(
                EqualityComparer<TToken>.Default.Equals, candidates);
        }

        public static Parser<TToken, TToken> NoneOf<TToken>(
            Func<TToken, TToken, Boolean> predicate, params TToken[] candidates)
        {
            if (predicate == null)
                throw new ArgumentNullException("predicate");
            if (candidates == null)
                throw new ArgumentNullException("candidates");

            return Combinator.Choice(candidates.Select(x => Prims.Satisfy<TToken>(y => predicate(x, y)))).Not().Right(Prims.Any<TToken>());
        }

        public static Parser<TToken, TToken> NoneOf<TToken>(
            params TToken[] candidates)
        {
            return Prims.NoneOf<TToken>(
                EqualityComparer<TToken>.Default.Equals, candidates);
        }

        public static Parser<TToken, TResult2> Pipe<TToken, TResult0, TResult1, TResult2>(
            this Parser<TToken, TResult0> parser0,
            Parser<TToken, TResult1> parser1,
            Func<TResult0, TResult1, TResult2> selector)
        {
            if (parser0 == null)
                throw new ArgumentNullException("parser0");
            if (parser1 == null)
                throw new ArgumentNullException("parser1");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return from x in parser0
                   from y in parser1
                   select selector(x, y);
        }

        public static Parser<TToken, TResult3> Pipe<TToken, TResult0, TResult1, TResult2, TResult3>(
            this Parser<TToken, TResult0> parser0,
            Parser<TToken, TResult1> parser1,
            Parser<TToken, TResult2> parser2,
            Func<TResult0, TResult1, TResult2, TResult3> selector)
        {
            if (parser0 == null)
                throw new ArgumentNullException("parser0");
            if (parser1 == null)
                throw new ArgumentNullException("parser1");
            if (parser2 == null)
                throw new ArgumentNullException("parser2");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return from x in parser0
                   from y in parser1
                   from z in parser2
                   select selector(x, y, z);
        }

        public static Parser<TToken, TResult4> Pipe<TToken, TResult0, TResult1, TResult2, TResult3, TResult4>(
            this Parser<TToken, TResult0> parser0,
            Parser<TToken, TResult1> parser1,
            Parser<TToken, TResult2> parser2,
            Parser<TToken, TResult3> parser3,
            Func<TResult0, TResult1, TResult2, TResult3, TResult4> selector)
        {
            if (parser0 == null)
                throw new ArgumentNullException("parser0");
            if (parser1 == null)
                throw new ArgumentNullException("parser1");
            if (parser2 == null)
                throw new ArgumentNullException("parser2");
            if (parser3 == null)
                throw new ArgumentNullException("parser3");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return from x in parser0
                   from y in parser1
                   from z in parser2
                   from a in parser3
                   select selector(x, y, z, a);
        }

        public static Parser<TToken, TResult5> Pipe<TToken, TResult0, TResult1, TResult2, TResult3, TResult4, TResult5>(
            this Parser<TToken, TResult0> parser0,
            Parser<TToken, TResult1> parser1,
            Parser<TToken, TResult2> parser2,
            Parser<TToken, TResult3> parser3,
            Parser<TToken, TResult4> parser4,
            Func<TResult0, TResult1, TResult2, TResult3, TResult4, TResult5> selector)
        {
            if (parser0 == null)
                throw new ArgumentNullException("parser0");
            if (parser1 == null)
                throw new ArgumentNullException("parser1");
            if (parser2 == null)
                throw new ArgumentNullException("parser2");
            if (parser3 == null)
                throw new ArgumentNullException("parser3");
            if (parser4 == null)
                throw new ArgumentNullException("parser4");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return from x in parser0
                   from y in parser1
                   from z in parser2
                   from a in parser3
                   from b in parser4
                   select selector(x, y, z, a, b);
        }

        public static Parser<TToken, TResult0> Left<TToken, TResult0, TResult1>(
            this Parser<TToken, TResult0> parser0,
            Parser<TToken, TResult1> parser1)
        {
            if (parser0 == null)
                throw new ArgumentNullException("parser0");
            if (parser1 == null)
                throw new ArgumentNullException("parser1");

            return from x in parser0
                   from _ in parser1
                   select x;
        }

        public static Parser<TToken, TResult1> Right<TToken, TResult0, TResult1>(
            this Parser<TToken, TResult0> parser0,
            Parser<TToken, TResult1> parser1)
        {
            if (parser0 == null)
                throw new ArgumentNullException("parser0");
            if (parser1 == null)
                throw new ArgumentNullException("parser1");

            return from _ in parser0
                   from y in parser1
                   select y;
        }

        public static Parser<TToken, Tuple<TResult0, TResult1>> Both<TToken, TResult0, TResult1>(
            this Parser<TToken, TResult0> parser0,
            Parser<TToken, TResult1> parser1)
        {
            if (parser0 == null)
                throw new ArgumentNullException("parser0");
            if (parser1 == null)
                throw new ArgumentNullException("parser1");

            return from x in parser0
                   from y in parser1
                   select Tuple.Create(x, y);
        }

        public static Parser<TToken, TResult0> Between<TToken, TResult0, TResult1, TResult2>(
            this Parser<TToken, TResult0> parser0,
            Parser<TToken, TResult1> parser1,
            Parser<TToken, TResult2> parser2)
        {
            if (parser0 == null)
                throw new ArgumentNullException("parser0");
            if (parser1 == null)
                throw new ArgumentNullException("parser1");
            if (parser2 == null)
                throw new ArgumentNullException("parser2");

            return from x in parser1
                   from y in parser0
                   from z in parser2
                   select y;
        }

        public static Parser<TToken, IEnumerable<TResult>> SepBy<TToken, TResult, TSeparator>(
            this Parser<TToken, TResult> parser,
            Int32 count,
            Parser<TToken, TSeparator> separator)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if (separator == null)
                throw new ArgumentNullException("separator");

            return parser.SelectMany(x => separator.Right(parser).Many(count).Select(y => x.Concat(y)));
        }

        public static Parser<TToken, IEnumerable<TResult>> SepBy<TToken, TResult, TSeparator>(
            this Parser<TToken, TResult> parser,
            Parser<TToken, TSeparator> separator)
        {
            return parser.SepBy(0, separator);
        }

        public static Parser<TToken, IEnumerable<TResult>> SepEndBy<TToken, TResult, TSeparator>(
            this Parser<TToken, TResult> parser,
            Int32 count,
            Parser<TToken, TSeparator> separator)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if (separator == null)
                throw new ArgumentNullException("separator");

            return Prims.SepBy(parser, count, separator).Left(separator.Maybe());
        }

        public static Parser<TToken, IEnumerable<TResult>> SepEndBy<TToken, TResult, TSeparator>(
            this Parser<TToken, TResult> parser,
            Parser<TToken, TSeparator> separator)
        {
            return parser.SepEndBy(0, separator);
        }

        public static Parser<TToken, TResult> WhenSuccess<TToken, TResult>(
            this Parser<TToken, TResult> parser0, Parser<TToken, TResult> parser1)
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
                        return parser1(reply.Stream);
                    case ReplyStatus.Failure:
                        return Reply.Failure<TToken, TResult>(stream);
                    default:
                        return Reply.Error<TToken, TResult>(stream, message);
                }
            };
        }

        public static Parser<TToken, TResult> WhenFailure<TToken, TResult>(
            this Parser<TToken, TResult> parser0, Parser<TToken, TResult> parser1)
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
                    case ReplyStatus.Failure:
                        return parser1(stream);
                    default:
                        return Reply.Error<TToken, TResult>(stream, message);
                }
            };
        }

        public static Parser<TToken, TResult> WhenError<TToken, TResult>(
            this Parser<TToken, TResult> parser0, Parser<TToken, TResult> parser1)
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
                    case ReplyStatus.Failure:
                        return Reply.Failure<TToken, TResult>(stream);
                    default:
                        return parser1(stream);
                }
            };
        }
    }
}
