using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Parseq;
using Parseq.Combinators;

namespace Example
{
    public static class Json
    {
        /*
         * 
         * JSON Parser
         * ( Forked from http://d.hatena.ne.jp/liner_lock/20120404/1333561060 )
         *
         */

        static Json()
        {
            Parser<Char, dynamic> jValueRef = null;
            var jValue = Combinator.Lazy(() => jValueRef);

            var ws = Chars.OneOf(' ', '\t', '\r', '\n');
            var wsChar = (Func<Char, Parser<Char, Char>>)(c => Chars.Satisfy(c).Between(ws.Many(), ws.Many()));

            var sign = Chars.OneOf('+', '-').Maybe()
                .Select(_ => _.Select(e => e.ToString()).Otherwise(() => String.Empty));

            var jChar = Combinator.Choice(
                Chars.NoneOf('\"', '\\'),
                Chars.Satisfy('\\').Right(Combinator.Choice(
                        '\"'.Satisfy(),
                        '\\'.Satisfy(),
                        'b'.Satisfy().Select(e => '\b'),
                        'f'.Satisfy().Select(e => '\f'),
                        'n'.Satisfy().Select(e => '\n'),
                        'r'.Satisfy().Select(e => '\r'),
                        't'.Satisfy().Select(e => '\t'),
                        'u'.Satisfy().Right(Chars.Hex().Repeat(4))
                            .Select(_ => Char.Parse("\\u" + new String(_.ToArray()))),
                        Chars.Any())));

            var jString = jChar.Many()
                .Between('\"'.Satisfy(), '\"'.Satisfy())
                .Select(_ => (dynamic)new String(_.ToArray()));

            var jBool = Combinator.Choice(
                Chars.Sequence("true").Select(_ => (dynamic)true),
                Chars.Sequence("false").Select(_ => (dynamic)false));

            var jNull = Chars.Sequence("null").Select(_ => (dynamic)null);

            var jMember = 
                from x in jString
                from y in wsChar(':')
                from z in jValue
                select Tuple.Create(x, z);

            var jObject = jMember.SepBy(wsChar(','))
                .Between(wsChar('{'), wsChar('}'))
                .Select(_ => (dynamic)_.ToDictionary(e => e.Item1, e => e.Item2));

            var jFraction = 
                from x in '.'.Satisfy()
                from y in Chars.Digit().Many(1)
                select String.Concat(x, new String(y.ToArray()));

            var jExponent = 
                from x in 'e'.Satisfy()
                from y in sign
                from z in Chars.Digit().Many(1)
                select String.Concat(x, sign, new String(z.ToArray()));

            var jNumber =
                from w in sign
                from x in Chars.Digit().Many(1)
                from y in jFraction.Maybe().Select(_ => _.Otherwise(() => String.Empty))
                from z in jExponent.Maybe().Select(_ => _.Otherwise(() => String.Empty))
                select (dynamic)Int32.Parse(String.Concat(w, new String(x.ToArray()), y, z));

            jValueRef = Combinator.Choice(
                jBool,
                jNull,
                jString,
                jObject,
                jNumber);

            var jArray = jValue.SepBy(wsChar(','))
                .Between(wsChar('['), wsChar(']'))
                .Select(_ => (dynamic)_.ToList());

            Json.Parser = jObject.Or(jArray)
                .Left(Errors.FollowedBy(Chars.Eof()));
        }

        public static Parser<Char, dynamic> Parser
        {
            get;
            private set;
        }
    }
}