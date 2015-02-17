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

namespace Parseq.Combinators
{
    public static class Prims
    {
        public static Parser<TToken, Unit> EndOfInput<TToken>()
        {
            return stream => stream.Current.HasValue
                ? Reply.Failure<TToken, Unit>(stream, "Failure: Prims.EndOfInput")
                : Reply.Success<TToken, Unit>(stream, Parseq.Unit.Instance);
        }

        public static Parser<TToken, Unit> Unit<TToken>()
        {
            return Parser.Return<TToken, Unit>(Parseq.Unit.Instance);
        }

        public static Parser<TToken, TToken> Any<TToken>()
        {
            return stream => stream.Current.HasValue
                ? Reply.Success<TToken, TToken>(stream.MoveNext(), stream.Current.Value.Item0)
                : Reply.Failure<TToken, TToken>(stream, "Failure: Prims.Any");
        }

        public static Parser<TToken, TToken> Satisfy<TToken>(TToken token)
            where TToken : IEquatable<TToken>
        {
            return stream => stream.Current.HasValue && stream.Current.Value.Item0.Equals(token)
                ? Reply.Success<TToken, TToken>(stream.MoveNext(), stream.Current.Value.Item0)
                : Reply.Failure<TToken, TToken>(stream, "Failure: Prims.Satisfy");
        }

        public static Parser<TToken, TToken> Satisfy<TToken>(Func<TToken, Boolean> predicate)
        {
            return stream => stream.Current.HasValue && predicate(stream.Current.Value.Item0)
                ? Reply.Success<TToken, TToken>(stream.MoveNext(), stream.Current.Value.Item0)
                : Reply.Failure<TToken, TToken>(stream, "Failure: Prims.Satisfy");
        }

        public static Parser<TToken, TToken> OneOf<TToken>(IEnumerable<TToken> candidates)
            where TToken : IEquatable<TToken>
        {
            return Combinator.Choice(candidates.Select(Prims.Satisfy<TToken>));
        }

        public static Parser<TToken, TToken> OneOf<TToken>(params TToken[] candidates)
            where TToken : IEquatable<TToken>
        {
            return Prims.OneOf(candidates.AsEnumerable());
        }

        public static Parser<TToken, TToken> NoneOf<TToken>(IEnumerable<TToken> candidates)
            where TToken : IEquatable<TToken>
        {
            return Combinator.Sequence(candidates.Select(token => Prims.Satisfy(token).Not())).Bindr(Prims.Any<TToken>());
        }

        public static Parser<TToken, TToken> NoneOf<TToken>(params TToken[] candidates)
            where TToken : IEquatable<TToken>
        {
            return Prims.NoneOf(candidates.AsEnumerable());
        }

        public static Parser<TToken, IEnumerable<T>> Empty<TToken, T>()
        {
            return Parser.Return<TToken, IEnumerable<T>>(Enumerable.Empty<T>());
        }

        public static Parser<TToken, IEnumerable<T>> Append<TToken, T>(
            this Parser<TToken, IEnumerable<T>> parser0,
                 Parser<TToken, IEnumerable<T>> parser1)
        {
            return InternalCombinator.Append(parser0.Map(Seq.Of), parser1.Map(Seq.Of))
                .Map(Seq.AsEnumerable);
        }

        public static Parser<TToken, IEnumerable<T>> Append<TToken, T>(
            this Parser<TToken, IEnumerable<T>> parser0,
                 Parser<TToken, T> parser1)
        {
            return InternalCombinator.Append(parser0.Map(Seq.Of), parser1)
                .Map(Seq.AsEnumerable);
        }

        public static Parser<TToken, IEnumerable<T>> Append<TToken, T>(
            this Parser<TToken, IEnumerable<T>> parser0,
                 Parser<TToken, IOption<IEnumerable<T>>> parser1)
        {
            return InternalCombinator.Append(
                parser0.Map(Seq.Of),
                parser1.Map(optionValue => optionValue.Map(Seq.Of)))
                .Map(Seq.AsEnumerable);
        }

        public static Parser<TToken, IEnumerable<T>> Append<TToken, T>(
            this Parser<TToken, IEnumerable<T>> parser0,
                 Parser<TToken, IOption<T>> parser1)
        {
            return InternalCombinator.Append(parser0.Map(Seq.Of), parser1)
                .Map(Seq.AsEnumerable);
        }
    }
}
