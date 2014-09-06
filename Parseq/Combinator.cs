/*
 * Parseq - monadic parser combinator library for C#
 * 
 * Copyright (C) 2012 - 2014 Takahisa Watanabe <linerlock@outlook.com> All rights reserved.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */
using System;
using System.Linq;
using System.Collections.Generic;

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

            return parsers.IfEmpty(
                () => Prims.Empty<TToken, TResult>(),
                xs => xs.HeadAndTail()
                        .Case((head, tail) => head.SelectMany(x => Combinator.Sequence(tail).Select(y => x.Concat(y)))));
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

            return parsers.IfEmpty(
                () => Prims.Fail<TToken, TResult>(),
                xs => xs.HeadAndTail()
                        .Case((head, tail) => head.Or(Combinator.Lazy(() => Combinator.Choice(tail)))));
        }

        public static Parser<TToken, IEither<TResult0, TResult1>> Fork<TToken, TResult0, TResult1>(
            this Parser<TToken, TResult0> parser0,
            Parser<TToken, TResult1> parser1)
        {
            if (parser0 == null)
                throw new ArgumentNullException("parser0");
            if (parser1 == null)
                throw new ArgumentNullException("parser1");

            return parser0.Select(_ => Either.Left<TResult0, TResult1>(_))
                .Or(parser1.Select(_ => Either.Right<TResult0, TResult1>(_)));
        }

        public static Parser<TToken, TResult> Or<TToken, TResult>(
            this Parser<TToken, TResult> parser0,
            Parser<TToken, TResult> parser1)
        {
            if (parser0 == null)
                throw new ArgumentNullException("parser0");
            if (parser1 == null)
                throw new ArgumentNullException("parser1");

            return stream =>
            {
                IReply<TToken, TResult> reply;
                TResult result; ErrorMessage message;
                switch ((reply = parser0(stream)).TryGetValue(out result, out message))
                {
                    case ReplyStatus.Success:
                        return Reply.Success<TToken, TResult>(reply.Stream, result);
                    case ReplyStatus.Error:
                        return Reply.Error<TToken, TResult>(stream, message);
                    default:
                        return parser1(stream);
                }
            };
        }

        public static Parser<TToken, TResult> And<TToken, TResult>(
            this Parser<TToken, TResult> parser)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");

            return stream =>
            {
                IReply<TToken, TResult> reply;
                TResult result; ErrorMessage message;
                switch ((reply = parser(stream)).TryGetValue(out result, out message))
                {
                    case ReplyStatus.Success: return Reply.Success<TToken, TResult>(stream, result);
                    case ReplyStatus.Failure: return Reply.Failure<TToken, TResult>(stream);
                    default:
                        return Reply.Error<TToken, TResult>(stream, message);
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
                IReply<TToken, TResult> reply;
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

        public static Parser<TToken, IEnumerable<TResult>> Greed<TToken, TResult>(
            this IEnumerable<Parser<TToken, TResult>> parsers)
        {
            if (parsers == null)
                throw new ArgumentNullException("parsers");

            return parsers.IfEmpty(
                () => Prims.Empty<TToken, TResult>(),
                xs => xs.HeadAndTail()
                        .Case((head, tail) => head.SelectMany(x => Combinator.Greed(tail).Select(y => x.Concat(y)))
                            .Or(Prims.Empty<TToken, TResult>())));
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

            return parser.SelectMany(x => Combinator.Many(parser).Select(y => x.Concat(y)))
                .Or(Prims.Empty<TToken, TResult>());
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

        private static Parser<TToken, IEnumerable<TResult>> Min<TToken, TResult>(
            this Parser<TToken, TResult> parser, Int32 min)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (min < 0)
                throw new ArgumentOutOfRangeException("min");

            return Combinator.Repeat(parser, min).SelectMany(x => Combinator.Many(parser).Select(y => x.Concat(y)));
        }

        private static Parser<TToken, IEnumerable<TResult>> Max<TToken, TResult>(
            this Parser<TToken, TResult> parser, Int32 max)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (max < 0)
                throw new ArgumentOutOfRangeException("max");

            return Combinator.Greed(parser.Replicate().Take(max));
        }

        public static Parser<TToken, IOption<TResult>> Maybe<TToken, TResult>(
            this Parser<TToken, TResult> parser)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");

            return stream =>
            {
                IReply<TToken, TResult> reply;
                TResult result; ErrorMessage message;
                switch ((reply = parser(stream)).TryGetValue(out result, out message))
                {
                    case ReplyStatus.Success: return Reply.Success<TToken, IOption<TResult>>(reply.Stream, Option.Just(result));
                    case ReplyStatus.Failure: return Reply.Success<TToken, IOption<TResult>>(stream, Option.None<TResult>());
                    default:
                        return Reply.Error<TToken, IOption<TResult>>(stream, message);
                }
            };
        }

        public static Parser<TToken, TResult> Lazy<TToken, TResult>(
            this Func<Parser<TToken, TResult>> func)
        {
            if (func == null)
                throw new ArgumentNullException("parser");

            var cache = Option.None<Parser<TToken, TResult>>();
            return stream =>
            {
                Parser<TToken, TResult> result;
                if (!cache.TryGetValue(out result))
                    cache = Option.Just(result = func());
                return result.Run(stream);
            };
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
