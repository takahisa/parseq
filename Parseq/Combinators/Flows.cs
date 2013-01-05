/*
 * Parseq - a monadic parser combinator library for C#
 *
 * Copyright (c) 2012 - 2013 WATANABE TAKAHISA <x.linerlock@gmail.com> All rights reserved.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq.Combinators
{
    public static class Flows
    {
        public static Parser<TToken, IEnumerable<TResult>> While<TToken, TResult, TCond>(
            this Parser<TToken, TResult> parser, Parser<TToken, TCond> condition)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (condition == null)
                throw new ArgumentNullException("condition");

            return (condition.And().Right(parser)).Many();
        }

        public static Parser<TToken, IEnumerable<TResult>> Until<TToken, TResult, TCond>(
            this Parser<TToken, TResult> parser, Parser<TToken, TCond> condition)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (condition == null)
                throw new ArgumentNullException("condition");

            return (condition.Not().Right(parser)).Many();
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

            return (condition.And().Right(thenParser)).Or(elseParser);
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

            return Flows.If<TToken, TCond, Either<TResult0, TResult1>>(condition,
                thenParser.Select(_ => Either.Left<TResult0, TResult1>(_)),
                elseParser.Select(_ => Either.Right<TResult0, TResult1>(_)));
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

            return (condition.Not().Right(thenParser)).Or(elseParser);
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

            return Flows.Unless<TToken, TCond, Either<TResult0, TResult1>>(condition,
                thenParser.Select(_ => Either.Left<TResult0, TResult1>(_)),
                elseParser.Select(_ => Either.Right<TResult0, TResult1>(_)));
        }
    }
}
