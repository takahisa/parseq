/*
 * Copyright (C) 2012 - 2015 Takahisa Watanabe <linerlock@outlook.com> All rights reserved.
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
using System.ComponentModel;

namespace Parseq
{
    public static partial class Parser
    {
        public static IReply<TToken, T> Run<TToken, T>(
            this Parser<TToken, T> parser,
                 ITokenStream<TToken> stream)
        {
            try
            {
                return parser(stream);
            }
            catch(FailFastImpl<TToken, T> reply)
            {
                return reply;
            }
        }

        public static Parser<TToken, T> Return<TToken, T>(T value)
        {
            return stream =>
                Reply.Success<TToken, T>(stream, value);
        }

        public static Parser<TToken, T> Fail<TToken, T>(String errorMessage)
        {
            return stream =>
                Reply.Failure<TToken, T>(stream, errorMessage);
        }

        public static Parser<TToken, T> FailFast<TToken, T>(String errorMessage)
        {
            return stream =>
            { throw new Parser.FailFastImpl<TToken, T>(stream, errorMessage); };
        }
    }

    public static partial class Parser
    {
        class FailFastImpl<TToken, T>
            : Exception
            , IReply<TToken, T>
        {
            private readonly ITokenStream<TToken> restStream;
            private readonly String errorMessage;

            public FailFastImpl(ITokenStream<TToken> restStream, String errorMessage)
            {
                this.restStream = restStream;
                this.errorMessage = errorMessage;
            }

            public TResult Case<TResult>(
                Func<ITokenStream<TToken>, String, TResult> failure,
                Func<ITokenStream<TToken>, T, TResult> success)
            {
                return failure(this.restStream, this.errorMessage);
            }

            TResult IEither<String, T>.Case<TResult>(
                Func<String, TResult> left,
                Func<T, TResult> right)
            {
                return left(this.errorMessage);
            }
        }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static class ParserExtensions
    {
        public static Parser<TToken, T1> Select<TToken, T0, T1>(
            this Parser<TToken, T0> parser,
                 Func<T0, T1> selector)
        {
            return stream0 => parser(stream0).Case(
                failure: (stream1, errorMessage) =>
                    Reply.Failure<TToken, T1>(stream1, errorMessage),
                success: (stream1, value) =>
                    Reply.Success<TToken, T1>(stream1, selector(value)));
        }

        public static Parser<TToken, T1> SelectMany<TToken, T0, T1>(
            this Parser<TToken, T0> parser,
                 Func<T0, Parser<TToken, T1>> selector)
        {
            return stream0 => parser(stream0).Case(
                failure: (stream1, errorMessage) =>
                    Reply.Failure<TToken, T1>(stream1, errorMessage),
                success: (stream1, value) =>
                    selector(value)(stream1));
        }

        public static Parser<TToken, T2> SelectMany<TToken, T0, T1, T2>(
            this Parser<TToken, T0> parser,
                 Func<T0, Parser<TToken, T1>> selector,
                 Func<T0, T1, T2> projector)
        {
            return parser.SelectMany(value0 => selector(value0).Select(value1 => projector(value0, value1)));
        }
    }
}
