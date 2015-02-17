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

namespace Parseq.Combinators
{
    public static class Chars
    {
        public static Parser<Char, Unit> EndOfInput()
        {
            return stream => stream.Current.HasValue
                ? Reply.Failure<Char, Unit>(stream, "Failure: Chars.EndOfInput")
                : Reply.Success<Char, Unit>(stream, Unit.Instance);
        }

        public static Parser<Char, Char> Any()
        {
            return stream => stream.Current.HasValue
                ? Reply.Success<Char, Char>(stream.MoveNext(), stream.Current.Value.Item0)
                : Reply.Failure<Char, Char>(stream, "Failure: Chars.Any");
        }

        public static Parser<Char, Char> Char(Char c)
        {
            return stream => stream.Current.HasValue && stream.Current.Value.Item0 == c
                ? Reply.Success<Char, Char>(stream.MoveNext(), c)
                : Reply.Failure<Char, Char>(stream, "Failure: Chars.Char");
        }

        public static Parser<Char, Char> Char(Func<Char, Boolean> predicate)
        {
            return stream => stream.Current.HasValue && predicate(stream.Current.Value.Item0)
                ? Reply.Success<Char, Char>(stream.MoveNext(), stream.Current.Value.Item0)
                : Reply.Failure<Char, Char>(stream, "Failure: Chars.Char");
        }

        public static Parser<Char, IEnumerable<Char>> Sequence(IEnumerable<Char> enumerble)
        {
            return Combinator.Attempt(Combinator.Sequence(enumerble.Select(Chars.Char)));
        }

        public static Parser<Char, IEnumerable<Char>> Sequence(params Char[] charArray)
        {
            return Chars.Sequence(charArray.AsEnumerable());
        }

        public static Parser<Char, IEnumerable<Char>> Sequence(String s)
        {
            return Chars.Sequence(s.ToCharArray());
        }

        public static Parser<Char, Char> OneOf(IEnumerable<Char> candidates)
        {
            return Combinator.Choice(candidates.Select(Chars.Char));
        }

        public static Parser<Char, Char> OneOf(params Char[] candidates)
        {
            return Chars.OneOf(candidates.AsEnumerable());
        }

        public static Parser<Char, Char> NoneOf(IEnumerable<Char> candidates)
        {
            return Combinator.Sequence(candidates.Select(c => Chars.Char(c).Not())).Bindr(Chars.Any());
        }

        public static Parser<Char, Char> NoneOf(params Char[] candidates)
        {
            return Chars.NoneOf(candidates.AsEnumerable());
        }

        public static Parser<Char, Char> Digit()
        {
            return Chars.Char(System.Char.IsDigit);
        }

        public static Parser<Char, Char> Letter()
        {
            return Chars.Char(System.Char.IsLetter);
        }

        public static Parser<Char, Char> LetterOrDigit()
        {
            return Chars.Char(System.Char.IsLetterOrDigit);
        }

        public static Parser<Char, Char> Lower()
        {
            return Chars.Char(System.Char.IsLower);
        }

        public static Parser<Char, Char> Upper()
        {
            return Chars.Char(System.Char.IsUpper);
        }

        public static Parser<Char, Char> WhiteSpace()
        {
            return Chars.Char(System.Char.IsWhiteSpace);
        }

        public static Parser<Char, Char> Oct()
        {
            return Chars.OneOf('0', '1', '2', '3', '4', '5', '6', '7');
        }

        public static Parser<Char, Char> Hex()
        {
            return Chars.OneOf(
                '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
                'a', 'b', 'c', 'd', 'e', 'f',
                'A', 'B', 'C', 'D', 'E', 'F');
        }

        public static Parser<Char, Char> Satisfy(Char c)
        {
            return Chars.Char(c);
        }

        public static Parser<Char, Char> Satisfy(Func<Char, Boolean> predicate)
        {
            return Chars.Char(predicate);
        }
    }
}
