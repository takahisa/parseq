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
    public class FlowsTest
    {
        [TestMethod]
        public void Flows_WhileTest()
        {
            Flows.While(Chars.Any(), Chars.Lower())
                .SuccessTest("foobar".AsStream(), "foobar".AsEnumerable(), Enumerable.SequenceEqual);
        }

        [TestMethod]
        public void Flows_UntilTest()
        {
            Flows.Until(Chars.Any(), Chars.Upper())
                .SuccessTest("foobar".AsStream(), "foobar".AsEnumerable(), Enumerable.SequenceEqual);
        }

        [TestMethod]
        public void Flows_IfTest()
        {
            Flows.If<Char, Char, Char>(Prims.Error<Char, Char>(), Chars.Any(), Chars.Any())
                .ErrorTest("foobar".AsStream());
            Flows.If<Char, Char, Char>(Chars.Lower(), Prims.Error<Char, Char>(), Chars.Any())
                .ErrorTest("foobar".AsStream());
            Flows.If<Char, Char, Char>(Chars.Upper(), Chars.Any(), Prims.Error<Char, Char>())
                .ErrorTest("foobar".AsStream());

            Flows.If<Char, Char, Char>(Chars.Any(), Chars.Any(), Prims.Error<Char, Char>())
                .SuccessTest("foobar".AsStream(), "foobar".First());

            Flows.If<Char, Char, Char>(Chars.Lower(), Chars.Any(), Prims.Error<Char, Char>())
                .SuccessTest("foobar".AsStream(), "foobar".First());
            Flows.If<Char, Char, Char>(Chars.Upper(), Prims.Error<Char, Char>(), Chars.Any())
                .SuccessTest("foobar".AsStream(), "foobar".First());
        }

        [TestMethod]
        public void Flows_UnlessTest()
        {
            Flows.Unless<Char, Char, Char>(Prims.Error<Char, Char>(), Chars.Any(), Chars.Any())
                .ErrorTest("foobar".AsStream());
            Flows.Unless<Char, Char, Char>(Chars.Lower(), Chars.Any(), Prims.Error<Char, Char>())
                .ErrorTest("foobar".AsStream());
            Flows.Unless<Char, Char, Char>(Chars.Upper(), Prims.Error<Char, Char>(), Chars.Any())
                .ErrorTest("foobar".AsStream());

            Flows.Unless<Char, Char, Char>(Chars.Any(), Prims.Error<Char, Char>(), Chars.Any())
                .SuccessTest("foobar".AsStream(), "foobar".First());

            Flows.Unless<Char, Char, Char>(Chars.Lower(), Prims.Error<Char, Char>(), Chars.Any())
                .SuccessTest("foobar".AsStream(), "foobar".First());
            Flows.Unless<Char, Char, Char>(Chars.Upper(), Chars.Any(), Prims.Error<Char, Char>())
                .SuccessTest("foobar".AsStream(), "foobar".First());
        }
    }
}
