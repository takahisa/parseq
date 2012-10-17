using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public static Parser<Char, IEnumerable<Char>> Sequence(params Char[] seq)
        {
            if (seq == null)
                throw new ArgumentNullException("seq");

            return Combinator.Sequence(seq.Select(c => c.Satisfy()).ToArray());
        }

        public static Parser<Char, IEnumerable<Char>> Sequence(String seq)
        {
            if (seq == null)
                throw new ArgumentNullException("seq");

            return Chars.Sequence(seq.ToArray());
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
