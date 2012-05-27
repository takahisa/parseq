using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq.Combinators
{
    public static class Errors
    {
        public static Parser<TToken, TResult> Fail<TToken, TResult>(String message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            return stream => Reply.Error<TToken, TResult>(
                stream, new ErrorMessage(ErrorMessageType.Error, message, stream.Position, stream.Position));
        }

        public static Parser<TToken, TResult> Warn<TToken, TResult>(String message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            return stream => Reply.Error<TToken, TResult>(
                stream, new ErrorMessage(ErrorMessageType.Warn, message, stream.Position, stream.Position));
        }

        public static Parser<TToken, TResult> Message<TToken, TResult>(String message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            return stream => Reply.Error<TToken, TResult>(
                stream, new ErrorMessage(ErrorMessageType.Message, message, stream.Position, stream.Position));
        }

        public static Parser<TToken, TResult> FailWhenSuccess<TToken, TResult>(
            this Parser<TToken, TResult> parser, String message)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (message == null)
                throw new ArgumentNullException("message");

            return parser.WhenSuccess(Errors.Fail<TToken, TResult>(message));
        }

        public static Parser<TToken, TResult> FailWhenFailure<TToken, TResult>(
            this Parser<TToken, TResult> parser, String message)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (message == null)
                throw new ArgumentNullException("message");

            return parser.WhenFailure(Errors.Fail<TToken, TResult>(message));
        }

        public static Parser<TToken, TResult> FailWhenError<TToken, TResult>(
            this Parser<TToken, TResult> parser, String message)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (message == null)
                throw new ArgumentNullException("message");

            return parser.WhenError(Errors.Fail<TToken, TResult>(message));
        }

        public static Parser<TToken, TResult> WarnWhenSuccess<TToken, TResult>(
            this Parser<TToken, TResult> parser, String message)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (message == null)
                throw new ArgumentNullException("message");

            return parser.WhenSuccess(Errors.Warn<TToken, TResult>(message));
        }

        public static Parser<TToken, TResult> WarnWhenFailure<TToken, TResult>(
            this Parser<TToken, TResult> parser, String message)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (message == null)
                throw new ArgumentNullException("message");

            return parser.WhenFailure(Errors.Warn<TToken, TResult>(message));
        }

        public static Parser<TToken, TResult> WarnWhenError<TToken, TResult>(
            this Parser<TToken, TResult> parser, String message)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (message == null)
                throw new ArgumentNullException("message");

            return parser.WhenError(Errors.Warn<TToken, TResult>(message));
        }

        public static Parser<TToken, TResult> MessageWhenSuccess<TToken, TResult>(
            this Parser<TToken, TResult> parser, String message)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (message == null)
                throw new ArgumentNullException("message");

            return parser.WhenSuccess(Errors.Message<TToken, TResult>(message));
        }

        public static Parser<TToken, TResult> MessageWhenFailure<TToken, TResult>(
            this Parser<TToken, TResult> parser, String message)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (message == null)
                throw new ArgumentNullException("message");

            return parser.WhenFailure(Errors.Message<TToken, TResult>(message));
        }

        public static Parser<TToken, TResult> MessageWhenError<TToken, TResult>(
            this Parser<TToken, TResult> parser, String message)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (message == null)
                throw new ArgumentNullException("message");

            return parser.WhenError(Errors.Message<TToken, TResult>(message));
        }

        public static Parser<TToken, Unit> FollowedBy<TToken, TResult>(Parser<TToken, TResult> parser, String message)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (message == null)
                throw new ArgumentNullException("message");

            return stream =>
            {
                TResult result; ErrorMessage error;
                switch (parser(stream).TryGetValue(out result, out error))
                {
                    case ReplyStatus.Success:
                        return Reply.Success<TToken, Unit>(stream, Unit.Instance);
                    case ReplyStatus.Failure:
                        return Reply.Error<TToken, Unit>(stream,
                            new ErrorMessage(ErrorMessageType.Error, message, stream.Position, stream.Position));
                    default:
                        return Reply.Error<TToken, Unit>(stream, error);
                }
            };
        }

        public static Parser<TToken, Unit> FollowedBy<TToken, TResult>(Parser<TToken, TResult> parser)
        {
            return FollowedBy(parser, "Syntax Error");
        }

        public static Parser<TToken, Unit> NotFollowedBy<TToken, TResult>(Parser<TToken, TResult> parser, String message)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (message == null)
                throw new ArgumentNullException("message");

            return stream =>
            {
                TResult result; ErrorMessage error;
                switch (parser(stream).TryGetValue(out result, out error))
                {
                    case ReplyStatus.Success:
                        return Reply.Error<TToken, Unit>(stream,
                            new ErrorMessage(ErrorMessageType.Error, message, stream.Position, stream.Position));
                    case ReplyStatus.Failure:
                        return Reply.Success<TToken, Unit>(stream, Unit.Instance);
                    default:
                        return Reply.Error<TToken, Unit>(stream, error);
                }
            };
        }

        public static Parser<TToken, Unit> NotFollowedBy<TToken, TResult>(Parser<TToken, TResult> parser)
        {
            return NotFollowedBy(parser, "Syntax Error");
        }

        public static Parser<TToken, Unit> Consume<TToken>(Func<TToken, Boolean> predicate, String message)
        {
            if (predicate == null)
                throw new ArgumentNullException("preducate");
            if (message == null)
                throw new ArgumentNullException("message");

            return stream =>
            {
                TToken result;
                return stream.TryGetValue(out result) && predicate(result)
                    ? Reply.Success<TToken, Unit>(stream.Next(), Unit.Instance)
                    : Reply.Error<TToken, Unit>(stream,
                        new ErrorMessage(ErrorMessageType.Error, message, stream.Position, stream.Position));
            };
        }

        public static Parser<TToken, Unit> Consume<TToken>(Func<TToken, Boolean> predicate)
        {
            return Consume(predicate, "Syntax Error");
        }

        public static Parser<TToken, Unit> Expect<TToken>(Func<TToken, Boolean> predicate, String message)
        {
            if (predicate == null)
                throw new ArgumentNullException("preducate");
            if (message == null)
                throw new ArgumentNullException("message");

            return stream =>
            {
                TToken result;
                return stream.TryGetValue(out result) && predicate(result)
                    ? Reply.Success<TToken, Unit>(stream, Unit.Instance)
                    : Reply.Error<TToken, Unit>(stream,
                        new ErrorMessage(ErrorMessageType.Error, message, stream.Position, stream.Position));
            };
        }

        public static Parser<TToken, Unit> Expect<TToken>(Func<TToken, Boolean> predicate)
        {
            return Expect(predicate, "Syntax Error");
        }
    }
}
