using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Parseq;
using Parseq.Combinators;

namespace Parseq.Test.Combinators
{
    [TestClass]
    public class CharsTest
    {
        [TestMethod]
        public void Chars_EofTest()
        {
            Chars.Eof().SuccessTest("".AsStream(), Unit.Instance);
            Chars.Eof().FailureTest("foobar".AsStream());
        }

        [TestMethod]
        public void Chars_AnyTest()
        {
            Chars.Any().SuccessTest("foobar".AsStream(), 'f');
            Chars.Any().FailureTest("".AsStream());
        }

        [TestMethod]
        public void Chars_SpaceTest()
        {
            Chars_TestHelper(Chars.Space(), Char.IsWhiteSpace);
        }
        
        [TestMethod]
        public void Chars_LowerTest()
        {
            Chars_TestHelper(Chars.Lower(), Char.IsLower);
        }

        [TestMethod]
        public void Chars_UpperTest()
        {
            Chars_TestHelper(Chars.Upper(), Char.IsUpper);
        }

        [TestMethod]
        public void Chars_DigitTest()
        {
            Chars_TestHelper(Chars.Digit(), Char.IsDigit);
        }

        [TestMethod]
        public void Chars_HexTest()
        {
            Chars_TestHelper(Chars.Hex(), c =>
                c == '0' || c == '1' ||
                c == '2' || c == '3' ||
                c == '4' || c == '5' ||
                c == '6' || c == '7' ||
                c == '8' || c == '9' ||
                c == 'a' || c == 'A' ||
                c == 'b' || c == 'B' ||
                c == 'c' || c == 'C' ||
                c == 'd' || c == 'D' ||
                c == 'e' || c == 'E' ||
                c == 'f' || c == 'F' );
        }

        [TestMethod]
        public void Chars_OctTest()
        {
            Chars_TestHelper(Chars.Oct(), c =>
                c == '0' || c == '1' ||
                c == '2' || c == '3' ||
                c == '4' || c == '5' ||
                c == '6' || c == '7' );
        }

        [TestMethod]
        public void Chars_NumberTest()
        {
            Chars_TestHelper(Chars.Number(), c =>
                c == '0' || c == '1' ||
                c == '2' || c == '3' ||
                c == '4' || c == '5' ||
                c == '6' || c == '7' ||
                c == '8' || c == '9' );
        }

        [TestMethod]
        public void Chars_SymbolTest()
        {
            Chars_TestHelper(Chars.Symbol(), Char.IsSymbol);
        }

        [TestMethod]
        public void Chars_ControlTest()
        {
            Chars_TestHelper(Chars.Control(), Char.IsControl);
        }

        [TestMethod]
        public void Chars_SeparatorTest()
        {
            Chars_TestHelper(Chars.Separator(), Char.IsSeparator);
        }

        [TestMethod]
        public void Chars_PunctuationTest()
        {
            Chars_TestHelper(Chars.Punctuation(), Char.IsPunctuation);
        }

        [TestMethod]
        public void Chars_SatisfyTest()
        {
            Chars.Satisfy("foobar".First()).SuccessTest("foobar".AsStream(), 'f');
            Chars.Satisfy("xyzzy".First()).FailureTest("foobar".AsStream());

            Chars.Satisfy(_ => _ == "foobar".First()).SuccessTest("foobar".AsStream(), 'f');
            Chars.Satisfy(_ => _ != "foobar".First()).FailureTest("foobar".AsStream());
        }

        [TestMethod]
        public void Chars_SequenceTest()
        {
            Chars.Sequence("").SuccessTest("foobar".AsStream(), Enumerable.Empty<Char>(), Enumerable.SequenceEqual);

            Chars.Sequence("xyzzy").FailureTest("foobar".AsStream());

            Chars.Sequence("f").SuccessTest("foobar".AsStream(), "f".AsEnumerable(),Enumerable.SequenceEqual);
            Chars.Sequence("fo").SuccessTest("foobar".AsStream(), "fo".AsEnumerable(), Enumerable.SequenceEqual);
            Chars.Sequence("foo").SuccessTest("foobar".AsStream(), "foo".AsEnumerable(), Enumerable.SequenceEqual);
            Chars.Sequence("foobar").SuccessTest("foobar".AsStream(), "foobar".AsEnumerable(), Enumerable.SequenceEqual);

            Chars.Sequence("foobar_").FailureTest("foobar".AsStream());
        }

        [TestMethod]
        public void Chars_OneOfTest()
        {
            Chars.OneOf().FailureTest("foobar".AsStream());

            Chars.OneOf('x', 'y', 'z').FailureTest("foobar".AsStream());

            Chars.OneOf("foobar".First(), 'y', 'z').SuccessTest("foobar".AsStream(), "foobar".First());
            Chars.OneOf('x', "foobar".First(), 'z').SuccessTest("foobar".AsStream(), "foobar".First());
            Chars.OneOf('x', 'y', "foobar".First()).SuccessTest("foobar".AsStream(), "foobar".First());

        }

        [TestMethod]
        public void Chars_NoneOfTest()
        {
            Chars.NoneOf().SuccessTest("foobar".AsStream(), 'f');

            Chars.NoneOf('x', 'y', 'z').SuccessTest("foobar".AsStream(), 'f');

            Chars.NoneOf("foobar".First(), 'y', 'z').FailureTest("foobar".AsStream());
            Chars.NoneOf('x', "foobar".First(), 'z').FailureTest("foobar".AsStream());
            Chars.NoneOf('x', 'y', "foobar".First()).FailureTest("foobar".AsStream());
        }

        private void Chars_TestHelper(Parser<Char, Char> parser, Func<Char, Boolean> predicate)
        {
            Enumerable.Range(Char.MinValue, Char.MaxValue)
                .Select(t => (Char)t)
                .Where(predicate)
                .ForEach(t => parser.SuccessTest(t.Enumerate().AsStream(), t));
        }

    }
}
