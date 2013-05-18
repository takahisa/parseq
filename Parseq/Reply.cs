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
using System.Linq;
using System.Collections.Generic;

namespace Parseq
{
    public enum ReplyStatus
    {
        Success = 1,
        Failure = 0,
        Error = -1,
    }

    public abstract partial class Reply<TToken, TResult>
        : Either<IOption<TResult>, ErrorMessage>
    {
        public abstract Stream<TToken> Stream { get; }
        public abstract ReplyStatus TryGetValue(out TResult result, out ErrorMessage error);
    }

    partial class Reply<TToken, TResult>
    {
        public sealed class Success : Reply<TToken, TResult>
        {
            private readonly Stream<TToken> _stream;
            private readonly TResult _value;

            public Success(Stream<TToken> stream, TResult value)
            {
                if (stream == null)
                    throw new ArgumentNullException("stream");

                _stream = stream;
                _value = value;
            }

            public override Stream<TToken> Stream
            {
                get { return _stream; }
            }

            public override Hand TryGetValue(out IOption<TResult> left, out ErrorMessage right)
            {
                left = Option.Just(_value);
                right = default(ErrorMessage);
                return Hand.Left;
            }

            public override ReplyStatus TryGetValue(out TResult result, out ErrorMessage error)
            {
                result = _value;
                error = default(ErrorMessage);
                return ReplyStatus.Success;
            }
        }

        public sealed class Failure : Reply<TToken, TResult>
        {
            private readonly Stream<TToken> _stream;

            public Failure(Stream<TToken> stream)
            {
                if (stream == null)
                    throw new ArgumentNullException("stream");

                _stream = stream;
            }

            public override Stream<TToken> Stream
            {
                get { return _stream; }
            }

            public override Hand TryGetValue(out IOption<TResult> left, out ErrorMessage right)
            {
                left = Option.Just(default(TResult));
                right = default(ErrorMessage);
                return Hand.Left;
            }

            public override ReplyStatus TryGetValue(out TResult result, out ErrorMessage error)
            {
                result = default(TResult);
                error = default(ErrorMessage);
                return ReplyStatus.Failure;
            }
        }

        public sealed class Error : Reply<TToken, TResult>
        {
            private readonly Stream<TToken> _stream;
            private readonly ErrorMessage _message;

            public Error(Stream<TToken> stream, ErrorMessage message)
            {
                if (stream == null)
                    throw new ArgumentNullException("stream");
                if (message == null)
                    throw new ArgumentNullException("message");

                _stream = stream;
                _message = message;
            }

            public override Stream<TToken> Stream
            {
                get { return _stream; }
            }

            public override Hand TryGetValue(out IOption<TResult> left, out ErrorMessage right)
            {
                left = Option.Just(default(TResult));
                right = _message;
                return Hand.Right;
            }

            public override ReplyStatus TryGetValue(out TResult result, out ErrorMessage error)
            {
                result = default(TResult);
                error = _message;
                return ReplyStatus.Error;
            }

            public override Boolean TryGetValue(out TResult result)
            {
                throw _message;
            }
        }
    }

    partial class Reply<TToken, TResult>
    {
        public virtual Boolean TryGetValue(out TResult result)
        {
            ErrorMessage message;
            switch (this.TryGetValue(out result, out message))
            {
                case ReplyStatus.Success: return true;
                case ReplyStatus.Failure: return false;
                default:
                    throw message;
            }
        }
    }

    public static class Reply
    {
        public static Reply<TToken, TResult> Success<TToken, TResult>(Stream<TToken> stream, TResult value)
        {
            return new Reply<TToken, TResult>.Success(stream, value);
        }

        public static Reply<TToken, TResult> Failure<TToken, TResult>(Stream<TToken> stream)
        {
            return new Reply<TToken, TResult>.Failure(stream);
        }

        public static Reply<TToken, TResult> Error<TToken, TResult>(Stream<TToken> stream, ErrorMessage message)
        {
            return new Reply<TToken, TResult>.Error(stream, message);
        }
    }
}
