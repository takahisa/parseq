using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Parseq.Combinators;

namespace Parseq
{
    public static class Combinator {

        public static Parser<TToken, IEnumerable<TResult>> Sequence<TToken, TResult>(
            params Parser<TToken, TResult>[] parsers)
        {
            if (parsers == null)
                throw new ArgumentNullException("parsers");

            return stream => {
                Reply<TToken, TResult> reply;
                TResult result; ErrorMessage message;
                var results = new List<TResult>();
                var current = stream;

                foreach(var parser in parsers){
                    switch ((reply = parser(current)).TryGetValue(out result, out message)){
                        case ReplyStatus.Success:
                            results.Add(result);
                            current = reply.Stream;
                            break;
                        case ReplyStatus.Failure:
                            return Reply.Failure<TToken, IEnumerable<TResult>>(stream);
                        case ReplyStatus.Error:
                            return Reply.Error<TToken, IEnumerable<TResult>>(stream, message);
                    }
                }
                return Reply.Success<TToken,IEnumerable<TResult>>(current, results);
            };
        }

        public static Parser<TToken, TResult> Choice<TToken, TResult>(
            params Parser<TToken, TResult>[] parsers)
        {
            if (parsers == null)
                throw new ArgumentNullException("parsers");

            return stream => {
                Reply<TToken, TResult> reply;
                TResult result; ErrorMessage message;

                foreach (var parser in parsers) {
                    switch ((reply = parser(stream)).TryGetValue(out result, out message)) {
                        case ReplyStatus.Success: 
                            return Reply.Success<TToken, TResult>(reply.Stream, result);
                        case ReplyStatus.Error:
                            return Reply.Error<TToken, TResult>(stream, message);
                    }
                }

                return Reply.Failure<TToken, TResult>(stream);
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

            return stream => {
                Reply<TToken, TResult> reply;
                TResult result; ErrorMessage message;

                switch ((reply = parser0(stream)).TryGetValue(out result, out message)){
                    case ReplyStatus.Success:
                        return Reply.Success<TToken, TResult>(reply.Stream, result);
                    case ReplyStatus.Error:
                        return Reply.Error<TToken, TResult>(stream, message);
                    default:
                        return parser1(stream);
                }
            };
        }

        public static Parser<TToken, Unit> And<TToken,TResult>(
            this Parser<TToken, TResult> parser)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");

            return stream => {
                Reply<TToken, TResult> reply;
                TResult result; ErrorMessage message;
                switch ((reply = parser(stream)).TryGetValue(out result,out message)){
                    case ReplyStatus.Success: return Reply.Success<TToken, Unit>(stream, Unit.Instance);
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

            return stream => {
                Reply<TToken, TResult> reply;
                TResult result; ErrorMessage message;
                switch ((reply = parser(stream)).TryGetValue(out result, out message)){
                    case ReplyStatus.Success: return Reply.Failure<TToken, Unit>(stream);
                    case ReplyStatus.Failure: return Reply.Success<TToken, Unit>(stream, Unit.Instance);
                    default:
                        return Reply.Error<TToken, Unit>(stream, message);
                }
            };
        }

        public static Parser<TToken, IEnumerable<TResult>> Repeat<TToken, TResult>(
            this Parser<TToken, TResult> parser, int count)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            return stream => {
                Reply<TToken, TResult> reply;
                TResult result; ErrorMessage message;
                var results = new List<TResult>();
                var current = stream;

                foreach (var i in Enumerable.Range(0, count)){
                    switch ((reply = parser(current)).TryGetValue(out result, out message)){
                        case ReplyStatus.Success:
                            results.Add(result);
                            current = reply.Stream;
                            break;
                        case ReplyStatus.Failure:
                            return Reply.Failure<TToken, IEnumerable<TResult>>(stream);
                        case ReplyStatus.Error:
                            return Reply.Error<TToken, IEnumerable<TResult>>(stream, message);
                    }
                }
                return Reply.Success<TToken, IEnumerable<TResult>>(current, results);
            };
        }

        public static Parser<TToken, IEnumerable<TResult>> Many<TToken, TResult>(
            this Parser<TToken, TResult> parser)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            return stream => {
                Reply<TToken, TResult> reply;
                TResult result; ErrorMessage message;
                var results = new List<TResult>();
                var current = stream;

                while (true){
                    switch ((reply = parser(current)).TryGetValue(out result, out message)){
                        case ReplyStatus.Success:
                            results.Add(result);
                            current = reply.Stream;
                            break;
                        case ReplyStatus.Failure:
                            return Reply.Success<TToken, IEnumerable<TResult>>(current, results);
                        case ReplyStatus.Error:
                            return Reply.Error<TToken, IEnumerable<TResult>>(stream, message);
                    }
                }
            };
        }

        public static Parser<TToken, IEnumerable<TResult>> Many<TToken, TResult>(
            this Parser<TToken, TResult> parser, int min)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (min < 0)
                throw new ArgumentOutOfRangeException("min");

            return from x in parser.Repeat(min)
                   from y in parser.Many()
                   select x.Concat(y);
        }

        public static Parser<TToken, IEnumerable<TResult>> Many<TToken, TResult>(
            this Parser<TToken, TResult> parser, int min, int max)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (min < 0)
                throw new ArgumentOutOfRangeException("min");
            if (max < min)
                throw new ArgumentOutOfRangeException("max");

            Parser<TToken,IEnumerable<TResult>> cont = stream => {
                var results = new List<TResult>();
                var current = stream;
                foreach (var i in Enumerable.Range(0, max - min)){
                    Reply<TToken,TResult> reply;
                    TResult result; ErrorMessage message;
                    switch ((reply = parser(stream)).TryGetValue(out result, out message)){
                        case ReplyStatus.Success:
                            results.Add(result);
                            current = reply.Stream;
                            break;
                        case ReplyStatus.Failure:
                            return Reply.Success<TToken,IEnumerable<TResult>>(current,results);
                        default:
                            return Reply.Error<TToken, IEnumerable<TResult>>(stream, message);
                    }
                }
                return Reply.Success<TToken, IEnumerable<TResult>>(current, results);
            };

            return from x in parser.Many(min)
                   from y in cont
                   select x.Concat(y);
        }

        public static Parser<TToken, Option<TResult>> Maybe<TToken, TResult>(
            this Parser<TToken, TResult> parser)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");

            return stream => {
                Reply<TToken, TResult> reply;
                TResult result; ErrorMessage message;
                switch ((reply = parser(stream)).TryGetValue(out result, out message)){
                    case ReplyStatus.Success: return Reply.Success<TToken, Option<TResult>>(reply.Stream, Option.Just(result));
                    case ReplyStatus.Failure: return Reply.Success<TToken, Option<TResult>>(stream, Option.None<TResult>());
                    default:
                        return Reply.Error<TToken, Option<TResult>>(stream, message);
                }
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
