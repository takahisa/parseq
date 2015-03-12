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
using System.Linq;
using System.Collections.Generic;
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
            catch (FailFastException<TToken> exception)
            {
                return Reply.Failure<TToken, T>(
                    exception.RestStream,
                    exception.Message);
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
                { throw new Parser.FailFastException<TToken>(stream, errorMessage); };
        }

        public static Parser<TToken, T1> Map<TToken, T0, T1>(
            this Parser<TToken, T0> parser,
                 Func<T0, T1> func)
        {
            return parser.Select(func);
        }

        public static Parser<TToken, T1> FlatMap<TToken, T0, T1>(
            this Parser<TToken, T0> parser,
                 Func<T0, Parser<TToken, T1>> func)
        {
            return parser.SelectMany(func);
        }

        public static Parser<TToken, T1> Bind<TToken, T0, T1>(
            this Parser<TToken, T0> parser,
                 Func<T0, Parser<TToken, T1>> func)
        {
            return parser.SelectMany(func);
        }

        public static Parser<TToken, T0> Bindl<TToken, T0, T1>(
            this Parser<TToken, T0> parser0,
                 Parser<TToken, T1> parser1)
        {
            return from value0 in parser0
                   from value1 in parser1
                   select value0;
        }

        public static Parser<TToken, T1> Bindr<TToken, T0, T1>(
            this Parser<TToken, T0> parser0,
                 Parser<TToken, T1> parser1)
        {
            return from value0 in parser0
                   from value1 in parser1
                   select value1;
        }

        public static Parser<TToken, T> DoWhenSuccess<TToken, T>(
            this Parser<TToken, T> parser,
                 Action<T> action)
        {
            IReply<TToken, T> reply;
            return stream => (reply = parser(stream)).Case(
                failure: (restStream, errorMessage) => reply,
                success: (restStream, value) =>
                    {
                        action(value);
                        return reply;
                    });
        }

        public static Parser<TToken, T> DoWhenFailure<TToken, T>(
            this Parser<TToken, T> parser,
                 Action<String> action)
        {
            IReply<TToken, T> reply;
            return stream => (reply = parser(stream)).Case(
                failure: (restStream, errorMessage) =>
                {
                    action(errorMessage);
                    return reply;
                },
                success: (restStream, value) => reply);
        }
    }

    public static partial class Parser
    {
        class FailFastException<TToken>
            : Exception
        {
            public ITokenStream<TToken> RestStream
            {
                get;
                private set;
            }

            public FailFastException(ITokenStream<TToken> restStream, String errorMessage)
                : base(errorMessage)
            {
                this.RestStream = restStream;
            }
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ParserExtensions
    {
        public static Parser<TToken, T1> Select<TToken, T0, T1>(
            this Parser<TToken, T0> parser,
                 Func<T0, T1> selector)
        {
            return stream => parser(stream).Case(
                failure: (restStream, errorMessage) =>
                    Reply.Failure<TToken, T1>(restStream, errorMessage),
                success: (restStream, value) =>
                    Reply.Success<TToken, T1>(restStream, selector(value)));
        }

        public static Parser<TToken, T1> SelectMany<TToken, T0, T1>(
            this Parser<TToken, T0> parser,
                 Func<T0, Parser<TToken, T1>> selector)
        {
            return stream => parser(stream).Case(
                failure: (restStream, errorMessage) =>
                    Reply.Failure<TToken, T1>(restStream, errorMessage),
                success: (restStream, value) =>
                    selector(value)(restStream));
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
