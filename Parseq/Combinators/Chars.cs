using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq.Combinators
{
    public static class Chars {

        public static Parser<char, char> Match(this char c){
            return Chars.Satisfy(_ => _ == c);
        }

        public static Parser<char, char> Any(){
            return Prims.Any<char>();
        }

        public static Parser<char, char> Space(){
            return Prims.Satisfy<char>(char.IsWhiteSpace);
        }

        public static Parser<char, char> Lower(){
            return Prims.Satisfy<char>(char.IsLower);
        }

        public static Parser<char, char> Upper(){
            return Prims.Satisfy<char>(char.IsUpper);
        }

        public static Parser<char, char> Digit(){
            return Prims.Satisfy<char>(char.IsDigit);
        }

        public static Parser<char, char> Hex(){
            return Prims.Satisfy<char>(c =>
                ('0' <= c && c <= '9') ||
                ('a' <= c && c <= 'f') ||
                ('A' <= c && c <= 'F'));
        }

        public static Parser<char, char> Oct(){
            return Prims.Satisfy<char>(c => '0' <= c && c <= '7');
        }

        public static Parser<char, char> Number(){
            return Prims.Satisfy<char>(char.IsNumber);
        }

        public static Parser<char, char> Symbol(){
            return Prims.Satisfy<char>(char.IsSymbol);
        }

        public static Parser<char, char> Control(){
            return Prims.Satisfy<char>(char.IsControl);
        }

        public static Parser<char, char> Satisfy(Func<char, bool> predicate){
            return Prims.Satisfy<char>(predicate);
        }

        public static Parser<char, char> OneOf(params char[] candidates){
            return Prims.OneOf<char>(candidates);
        }

        public static Parser<char, char> NoneOf(params char[] candidates)
        {
            return Prims.NoneOf<char>(candidates);
        }

    }
}
