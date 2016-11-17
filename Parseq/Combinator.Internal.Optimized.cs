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
using System.Collections.Generic;
using System.Linq;

namespace Parseq
{
    internal static partial class InternalCombinator
    {
        public static Parser<TToken, IEnumerable<T>> Sequence_Optimized<TToken, T>(
            IEnumerable<Parser<TToken, T>> parsers)
        {
            return stream =>
                {
                    var resultStream = stream;
                    var resultValue = new List<T>();
                    String errorMessage = null;
                    foreach (var parser in parsers)
                    {
                        var successful = parser(resultStream)
                            .Case(
                                failure: (restStream, value) =>
                                    {
                                        resultStream = restStream;
                                        errorMessage = value;
                                        return false;
                                    },
                                success: (restStream, value) =>
                                    {
                                        resultStream = restStream;
                                        resultValue.Add(value);
                                        return true;
                                    });

                        if (!successful)
                            return Reply.Failure<TToken, IEnumerable<T>>(resultStream, errorMessage);
                    }
                    return Reply.Success<TToken, IEnumerable<T>>(resultStream, resultValue);
                };
        }

        public static Parser<TToken, T> Choice_Optimized<TToken, T>(
            IEnumerable<Parser<TToken, T>> parsers)
        {
            return stream =>
            {
                var resultStream = stream;
                var resultValue = default(T);
                String errorMessage = null;
                foreach (var parser in parsers)
                {
                    var successful = parser(resultStream)
                        .Case(
                            failure: (restStream, value) =>
                            {
                                resultStream = stream;
                                errorMessage = value;
                                return false;
                            },
                            success: (restStream, value) =>
                            {
                                resultStream = restStream;
                                resultValue = value;
                                return true;
                            });

                    if (successful)
                        return Reply.Success<TToken, T>(resultStream, resultValue);
                }
                return Reply.Failure<TToken, T>(resultStream, errorMessage);
            };
        }

        public static Parser<TToken, IEnumerable<T>> Many0_Optimized<TToken, T>(
            Parser<TToken, T> parser)
        {
            return stream =>
            {
                var resultStream = stream;
                var resultValue = new List<T>();
                while(true)
                {
                    var successful = parser(resultStream)
                        .Case(
                            failure: (restStream, value) =>
                            {
                                return false;
                            },
                            success: (restStream, value) =>
                            {
                                resultStream = restStream;
                                resultValue.Add(value);
                                return true;
                            });

                    if (!successful)
                        return Reply.Success<TToken, IEnumerable<T>>(resultStream, resultValue);
                }
            };
        }

        public static Parser<TToken, IEnumerable<T>> Many1_Optimized<TToken, T>(
            Parser<TToken, T> parser)
        {
            return parser.Pipe(InternalCombinator.Many0_Optimized(parser),
                (head, tail) => Enumerable.Concat(new[] { head }, tail));
        }

        public static Parser<TToken, IEnumerable<T>> ManyTill_Optimized<TToken, T>(
            Parser<TToken, T> parser,
            Parser<TToken, Unit> terminater)
        {
            return stream =>
            {
                var resultStream = stream;
                var resultValue = new List<T>();
                String errorMessage = null;
                while (true)
                {
                    Boolean successful;
                    successful = terminater(resultStream)
                        .Case(
                            failure: (restStream, value) => false,
                            success: (restStream, value) =>
                                {
                                    resultStream = restStream;
                                    return true;
                                });

                    if (successful)
                        return Reply.Success<TToken, IEnumerable<T>>(resultStream, resultValue);

                    successful = parser(resultStream)
                        .Case(
                            failure: (restStream, value) =>
                                {
                                    resultStream = restStream;
                                    errorMessage = value;
                                    return false;
                                },
                            success: (restStream, value) =>
                                {
                                    resultStream = restStream;
                                    resultValue.Add(value);
                                    return true;
                                });
                    if (!successful)
                        return Reply.Failure<TToken, IEnumerable<T>>(resultStream, errorMessage);
                }
            };
        }

        public static Parser<TToken, IEnumerable<T>> Repeat_Optimized<TToken, T>(
            Parser<TToken, T> parser,
            Int32 count)
        {
            return InternalCombinator.Sequence_Optimized(Enumerable.Repeat(parser, count));
        }

        public static Parser<TToken, IEnumerable<T>> SepBy0_Optimized<TToken, T>(
            Parser<TToken, T> parser,
            Parser<TToken, Unit> sep)
        {
            return InternalCombinator.SepBy1_Optimized(parser, sep)
                .Or(Parser.Return<TToken, IEnumerable<T>>(Enumerable.Empty<T>()));
        }

        public static Parser<TToken, IEnumerable<T>> SepBy1_Optimized<TToken, T>(
            Parser<TToken, T> parser,
            Parser<TToken, Unit> sep)
        {
            return parser.Pipe(InternalCombinator.Many0_Optimized(sep.Bindr(parser)),
                (head, tail) => Enumerable.Concat(new[] { head }, tail));
        }

        public static Parser<TToken, IEnumerable<T>> EndBy0_Optimized<TToken, T>(
            Parser<TToken, T> parser,
            Parser<TToken, Unit> sep)
        {
            return InternalCombinator.Many0_Optimized(parser.Bindl(sep));
        }

        public static Parser<TToken, IEnumerable<T>> EndBy1_Optimized<TToken, T>(
            Parser<TToken, T> parser,
            Parser<TToken, Unit> sep)
        {
            return InternalCombinator.Many1_Optimized(parser.Bindl(sep));
        }

        public static Parser<TToken, IEnumerable<T>> SepEndBy0_Optimized<TToken, T>(
            Parser<TToken, T> parser,
            Parser<TToken, Unit> sep)
        {
            return InternalCombinator.SepBy0_Optimized(parser, sep)
                .Bindl(sep.Optional());
        }

        public static Parser<TToken, IEnumerable<T>> SepEndBy1_Optimized<TToken, T>(
           Parser<TToken, T> parser,
           Parser<TToken, Unit> sep)
        {
            return InternalCombinator.SepBy1_Optimized(parser, sep)
                .Bindl(sep.Optional());
        }
    }
}
