/*
 * Parseq - a monadic parser combinator library for C#
 *
 * Copyright (c) 2012 WATANABE TAKAHISA <x.linerlock@gmail.com> All rights reserved.
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
    public static class Errors
    {
        public static Parser<TToken, TResult> Rescue<TToken, TResult>(
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
                    case ReplyStatus.Success: 
                        return Reply.Success<TToken, TResult>(reply.Stream, result);
                    default:
                        return Reply.Failure<TToken, TResult>(stream);
                }
            };
        }

        public static Parser<TToken, TResult> Rescue<TToken, TResult>(
            this Parser<TToken, TResult> parser,
            ErrorMessageType messageTypeFilter)
        {
            return stream =>
            {
                Reply<TToken, TResult> reply;
                TResult result; ErrorMessage message;
                switch ((reply = parser(stream)).TryGetValue(out result, out message))
                {
                    case ReplyStatus.Success: return Reply.Success<TToken, TResult>(reply.Stream, result);
                    case ReplyStatus.Failure: return Reply.Failure<TToken, TResult>(stream);
                    default:
                        return ((message.MessageType & messageTypeFilter) != 0)
                            ? Reply.Failure<TToken, TResult>(stream)
                            : Reply.Error<TToken, TResult>(stream, message);
                }
            };
        }

        public static Parser<TToken, TResult> Error<TToken, TResult>(String message)
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

        public static Parser<TToken, TResult> ErrorWhenSuccess<TToken, TResult>(
            this Parser<TToken, TResult> parser, String message)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (message == null)
                throw new ArgumentNullException("message");

            return parser.WhenSuccess(Errors.Error<TToken, TResult>(message));
        }

        public static Parser<TToken, TResult> ErrorWhenFailure<TToken, TResult>(
            this Parser<TToken, TResult> parser, String message)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (message == null)
                throw new ArgumentNullException("message");

            return parser.WhenFailure(Errors.Error<TToken, TResult>(message));
        }

        public static Parser<TToken, TResult> ErrorWhenError<TToken, TResult>(
            this Parser<TToken, TResult> parser, String message)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (message == null)
                throw new ArgumentNullException("message");

            return parser.WhenError(Errors.Error<TToken, TResult>(message));
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

        public static Parser<TToken, TResult> FollowedBy<TToken, TResult>(Parser<TToken, TResult> parser, String message)
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
                        return Reply.Success<TToken, TResult>(stream, result);
                    case ReplyStatus.Failure:
                        return Reply.Error<TToken, TResult>(stream,
                            new ErrorMessage(ErrorMessageType.Error, message, stream.Position, stream.Position));
                    default:
                        return Reply.Error<TToken, TResult>(stream, error);
                }
            };
           
        }

        public static Parser<TToken, TResult> FollowedBy<TToken, TResult>(Parser<TToken, TResult> parser)
        {
            return FollowedBy(parser, "Error: FollowedBy");
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
            return NotFollowedBy(parser, "Error: NotFollowedBy");
        }
    }
}
