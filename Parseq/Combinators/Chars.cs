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

namespace Parseq.Combinators
{
    public static class Chars
    {
        public static Parser<Char, Unit> Eof()
        {
            return Prims.Eof<Char>();
        }

        public static Parser<Char, Char> Any()
        {
            return Chars.Satisfy(_ => true);
        }

        public static Parser<Char, Char> Space()
        {
            return Chars.Satisfy(Char.IsWhiteSpace);
        }

        public static Parser<Char, Char> Lower()
        {
            return Chars.Satisfy(Char.IsLower);
        }

        public static Parser<Char, Char> Upper()
        {
            return Chars.Satisfy(Char.IsUpper);
        }

        public static Parser<Char, Char> Letter()
        {
            return Chars.Satisfy(Char.IsLetter);
        }

        public static Parser<Char, Char> Digit()
        {
            return Chars.Satisfy(Char.IsDigit);
        }

        public static Parser<Char, Char> Hex()
        {
            return Chars.Satisfy(c => ('0' <= c && c <= '9')
                || ('a' <= c && c <= 'f')
                || ('A' <= c && c <= 'F'));
        }

        public static Parser<Char, Char> Oct()
        {
            return Chars.Satisfy(c => '0' <= c && c <= '7');
        }

        public static Parser<Char, Char> Number()
        {
            return Chars.Satisfy(Char.IsNumber);
        }

        public static Parser<Char, Char> Symbol()
        {
            return Chars.Satisfy(Char.IsSymbol);
        }

        public static Parser<Char, Char> Control()
        {
            return Chars.Satisfy(Char.IsControl);
        }

        public static Parser<Char, Char> Separator()
        {
            return Chars.Satisfy(Char.IsSeparator);
        }

        public static Parser<Char, Char> Punctuation()
        {
            return Chars.Satisfy(Char.IsPunctuation);
        }

        public static Parser<Char, Char> Satisfy(this Char c)
        {
            return Chars.Satisfy(_ => _ == c);
        }

        public static Parser<Char, Char> Satisfy(Func<Char, Boolean> predicate)
        {
            return Prims.Satisfy<Char>(predicate);
        }

        public static Parser<Char, IEnumerable<Char>> Satisfy(this IEnumerable<Char> seq)
        {
            if (seq == null)
                throw new ArgumentNullException("seq");

            return Combinator.Sequence(seq.Select(Chars.Satisfy));
        }

        public static Parser<Char, IEnumerable<Char>> Sequence(params Char[] seq)
        {
            if (seq == null)
                throw new ArgumentNullException("seq");

            return Chars.Satisfy(seq.AsEnumerable());
        }

        public static Parser<Char, IEnumerable<Char>> Sequence(String seq)
        {
            if (seq == null)
                throw new ArgumentNullException("seq");

            return Chars.Satisfy(seq.AsEnumerable());
        }

        public static Parser<Char, Char> OneOf(params Char[] candidates)
        {
            return Prims.OneOf<Char>(candidates);
        }

        public static Parser<Char, Char> NoneOf(params Char[] candidates)
        {
            return Prims.NoneOf<Char>(candidates);
        }
    }
}
