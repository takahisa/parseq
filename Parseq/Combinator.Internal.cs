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
        public static Parser<TToken, IDelayed<ISeq<T>>> Sequence<TToken, T>(
            IDelayed<ISeq<Parser<TToken, T>>> parsers)
        {
            return parsers.Force().Case(
                    empty: () =>
                        Parser.Return<TToken, IDelayed<ISeq<T>>>(Seq.Empty<T>()),
                    headAndTail: pair =>
                        pair.Item0.SelectMany(value0 => InternalCombinator.Sequence(pair.Item1).Select(value1 => Seq.Cons(value0, value1))));
        }

        public static Parser<TToken, T> Choice<TToken, T>(
            IDelayed<ISeq<Parser<TToken, T>>> parsers)
        {
            return parsers.Force().Case(
                    empty: () =>
                        Parser.Fail<TToken, T>("Failure: Combinator.Choice"),
                    headAndTail: pair =>
                        pair.Item0.Or(InternalCombinator.Choice(pair.Item1)));
        }

        public static Parser<TToken, IDelayed<ISeq<T>>> Repeat<TToken, T>(
            Parser<TToken, T> parser,
            Int32 count)
        {
            return InternalCombinator.Sequence(Seq.Of(Enumerable.Repeat(parser, count)));
        }

        public static Parser<TToken, IDelayed<ISeq<T>>> Many0<TToken, T>(
            Parser<TToken, T> parser)
        {
            return stream => parser(stream).Case(
                    failure: (restStream, errorMessage) =>
                        Reply.Success<TToken, IDelayed<ISeq<T>>>(stream, Seq.Empty<T>()),
                    success: (restStream, head) =>
                        InternalCombinator.Many0(parser).Select(tail => Seq.Cons(head, tail))(restStream));
        }

        public static Parser<TToken, IDelayed<ISeq<T>>> Many1<TToken, T>(
            Parser<TToken, T> parser)
        {
            return parser.SelectMany(head => InternalCombinator.Many0(parser).Select(tail => Seq.Cons(head, tail)));
        }

        public static Parser<TToken, IDelayed<ISeq<T>>> ManyTill<TToken, T>(
            this Parser<TToken, T> parser,
                 Parser<TToken, Unit> terminator)
        {
            return terminator.Select(_ => Seq.Empty<T>())
                .Or(parser.SelectMany(head => InternalCombinator.ManyTill(parser, terminator)
                    .Select(tail => Seq.Cons(head, tail))));
        }

        public static Parser<TToken, IDelayed<ISeq<T>>> SepBy0<TToken, T>(
            Parser<TToken, T> parser,
            Parser<TToken, Unit> sep)
        {
            return InternalCombinator.SepBy1(parser, sep)
                    .Or(Parser.Return<TToken, IDelayed<ISeq<T>>>(Seq.Empty<T>()));
        }

        public static Parser<TToken, IDelayed<ISeq<T>>> SepBy1<TToken, T>(
            Parser<TToken, T> parser,
            Parser<TToken, Unit> sep)
        {
            return parser.SelectMany(head => InternalCombinator.Many0(sep.SelectMany(_ => parser)).Select(tail => Seq.Cons(head, tail)));
        }

        public static Parser<TToken, IDelayed<ISeq<T>>> EndBy0<TToken, T>(
            Parser<TToken, T> parser,
            Parser<TToken, Unit> sep)
        {
            return InternalCombinator.Many0(parser.SelectMany(value => sep.Select(_ => value)));
        }

        public static Parser<TToken, IDelayed<ISeq<T>>> EndBy1<TToken, T>(
            Parser<TToken, T> parser,
            Parser<TToken, Unit> sep)
        {
            return InternalCombinator.Many1(parser.SelectMany(value => sep.Select(_ => value)));
        }

        public static Parser<TToken, IDelayed<ISeq<T>>> SepEndBy0<TToken, T>(
            Parser<TToken, T> parser,
            Parser<TToken, Unit> sep)
        {
            return InternalCombinator.SepBy0(parser, sep).SelectMany(value => sep.Optional().Select(_ => value));
        }

        public static Parser<TToken, IDelayed<ISeq<T>>> SepEndBy1<TToken, T>(
            Parser<TToken, T> parser,
            Parser<TToken, Unit> sep)
        {
            return InternalCombinator.SepBy1(parser, sep).SelectMany(value => sep.Optional().Select(_ => value));
        }

        public static Parser<TToken, T> Chainl<TToken, T>(
            Parser<TToken, T> parser,
            Parser<TToken, Func<T, T, T>> op)
        {
            return from lhs in parser
                   from resultValue in InternalCombinator.ChainlRest<TToken, T>(parser, op, lhs)
                   select resultValue;
        }

        public static Parser<TToken, T> ChainlRest<TToken, T>(
            Parser<TToken, T> parser,
            Parser<TToken, Func<T, T, T>> op,
            T lhs)
        {
            return (from func in op
                    from rhs in parser
                    from resultValue in InternalCombinator.ChainlRest(parser, op, func(lhs, rhs))
                    select resultValue)
                    .Or(Parser.Return<TToken, T>(lhs));
        }

        public static Parser<TToken, T> Chainr<TToken, T>(
            Parser<TToken, T> parser,
            Parser<TToken, Func<T, T, T>> op)
        {
            return from lhs in parser
                   from resultValue in InternalCombinator.ChainrRest<TToken, T>(parser, op, lhs)
                   select resultValue;
        }

        public static Parser<TToken, T> ChainrRest<TToken, T>(
            Parser<TToken, T> parser,
            Parser<TToken, Func<T, T, T>> op,
            T lhs)
        {
            return (from func in op
                    from rhs in InternalCombinator.Chainr(parser, op)
                    select func(lhs, rhs))
                    .Or(Parser.Return<TToken, T>(lhs));
        }

        public static Parser<TToken, IDelayed<ISeq<T>>> Append<TToken, T>(
            Parser<TToken, IDelayed<ISeq<T>>> parser0,
            Parser<TToken, IDelayed<ISeq<T>>> parser1)
        {
            return parser0.Pipe(parser1, Seq.Concat);
        }

        public static Parser<TToken, IDelayed<ISeq<T>>> Append<TToken, T>(
            Parser<TToken, IDelayed<ISeq<T>>> parser0,
            Parser<TToken, T> parser1)
        {
            return InternalCombinator.Append(
                parser0,
                parser1.Map(Seq.Singleton));
        }

        public static Parser<TToken, IDelayed<ISeq<T>>> Append<TToken, T>(
            Parser<TToken, IDelayed<ISeq<T>>> parser0,
            Parser<TToken, IOption<IDelayed<ISeq<T>>>> parser1)
        {
            return InternalCombinator.Append(
                parser0, 
                parser1.Map(optionValue => optionValue.Case(
                    none: Seq.Empty<T>,
                    some: value => value)));
        }

        public static Parser<TToken, IDelayed<ISeq<T>>> Append<TToken, T>(
            Parser<TToken, IDelayed<ISeq<T>>> parser0,
            Parser<TToken, IOption<T>> parser1)
        {
            return InternalCombinator.Append(
                parser0, 
                parser1.Map(optionValue => optionValue.Case(
                    none: Seq.Empty<T>,
                    some: Seq.Singleton<T>)));
        }
    }
}
