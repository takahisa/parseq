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
    public class PrimsTest
    {
        [TestMethod]
        public void Prims_EofTest()
        {
            Prims.Eof<Char>().SuccessTest("".AsStream(), Unit.Instance);
            Prims.Eof<Char>().FailureTest("foobar".AsStream());
        }

        [TestMethod]
        public void Prims_ReturnTest()
        {
            Prims.Return<Char, Char>('a').SuccessTest("".AsStream(), 'a');

            Prims.Return<Char, Char>('b').SuccessTest("foobar".AsStream(), 'b');
            Prims.Return<Char, Char>('c').SuccessTest("foobar".AsStream(), 'c');

            Prims.Return<Char, Char>('d').Right(Chars.Satisfy("foobar".First()))
                .SuccessTest("foobar".AsStream(), "foobar".First());
        }

        [TestMethod]
        public void Prims_FailTest()
        {
            Prims.Fail<Char, Char>().FailureTest("foobar".AsStream());
        }

        [TestMethod]
        public void Prims_ErrorTest()
        {
            Prims.Error<Char, Char>().ErrorTest("foobar".AsStream());
            Prims.Error<Char, Char>("here is an error-message").ErrorTest("foobar".AsStream());
        }

        [TestMethod]
        public void Prims_SatisfyTest()
        {
            Prims.Satisfy<Char>(_ => _ == "foobar".First()).SuccessTest("foobar".AsStream(), 'f');
            Prims.Satisfy<Char>(_ => _ != "foobar".First()).FailureTest("foobar".AsStream());
        }

        [TestMethod]
        public void Prims_AnyTest()
        {
            Prims.Any<Char>().FailureTest("".AsStream());

            Prims.Any<Char>().SuccessTest("foobar".AsStream(), "foobar".First());
            Prims.Any<Char>().SuccessTest("xyzzy".AsStream(), "xyzzy".First());   
        }

        [TestMethod]
        public void Prims_OneOfTest()
        {
            Prims.OneOf<Char>().FailureTest("foobar".AsStream());

            Prims.OneOf('x', 'y', 'z').FailureTest("foobar".AsStream());

            Prims.OneOf("foobar".First(), 'y', 'z').SuccessTest("foobar".AsStream(), "foobar".First());
            Prims.OneOf('x', "foobar".First(), 'z').SuccessTest("foobar".AsStream(), "foobar".First());
            Prims.OneOf('x', 'y', "foobar".First()).SuccessTest("foobar".AsStream(), "foobar".First());
        }

        [TestMethod]
        public void Prims_NoneOfTest()
        {
            Prims.NoneOf<Char>().SuccessTest("foobar".AsStream(), 'f');

            Prims.NoneOf('x', 'y', 'z').SuccessTest("foobar".AsStream(), 'f');

            Prims.NoneOf("foobar".First(), 'y', 'z').FailureTest("foobar".AsStream());
            Prims.NoneOf('x', "foobar".First(), 'z').FailureTest("foobar".AsStream());
            Prims.NoneOf('x', 'y', "foobar".First()).FailureTest("foobar".AsStream());
        }

        [TestMethod]
        public void Prims_PipeTest()
        {
            Prims.Pipe(Prims.Fail<Char, Char>(), Chars.Any(), (x, y) => x.ToString() + y.ToString())
                .FailureTest("foobar".AsStream());
            Prims.Pipe(Chars.Any(), Prims.Fail<Char, Char>(), (x, y) => x.ToString() + y.ToString())
                .FailureTest("foobar".AsStream());
            Prims.Pipe(Chars.Any(), Prims.Fail<Char, Char>(), Chars.Any(), (x, y, z) => x.ToString() + y.ToString() + z.ToString())
                .FailureTest("foobar".AsStream());

            Prims.Pipe('f'.Satisfy(), 'o'.Satisfy(), (a, b) => a.ToString() + b.ToString())
                .SuccessTest("foobar".AsStream(), "fo");
            Prims.Pipe('f'.Satisfy(), 'o'.Satisfy(), 'o'.Satisfy(), (a, b, c) => a.ToString() + b.ToString() + c.ToString())
                .SuccessTest("foobar".AsStream(), "foo");
            Prims.Pipe('f'.Satisfy(), 'o'.Satisfy(), 'o'.Satisfy(), 'b'.Satisfy(), (a, b, c, d) => a.ToString() + b.ToString() + c.ToString() + d.ToString())
                .SuccessTest("foobar".AsStream(), "foob");
            Prims.Pipe('f'.Satisfy(), 'o'.Satisfy(), 'o'.Satisfy(), 'b'.Satisfy(), 'a'.Satisfy(), (a, b, c, d, e) => a.ToString() + b.ToString() + c.ToString() + d.ToString() + e.ToString())
                .SuccessTest("foobar".AsStream(), "fooba");



        }

        [TestMethod]
        public void Prims_LeftTest()
        {
            Prims.Left(Prims.Fail<Char, Char>(), Prims.Fail<Char, Char>())
                .FailureTest("foobar".AsStream());
            Prims.Left(Chars.Any(), Prims.Fail<Char, Char>())
                .FailureTest("foobar".AsStream());
            Prims.Left(Prims.Fail<Char, Char>(), Chars.Any())
                .FailureTest("foobar".AsStream());

            Prims.Left(Prims.Error<Char,Char>(), Prims.Fail<Char, Char>())
                .ErrorTest("foobar".AsStream());

            Prims.Left(Prims.Fail<Char, Char>(), Prims.Error<Char, Char>())
                .FailureTest("foobar".AsStream());

            Prims.Left(Chars.Any(), Chars.Any())
                .SuccessTest("foobar".AsStream(), "foobar".First());
        }

        [TestMethod]
        public void Prims_RightTest()
        {
            Prims.Right(Prims.Fail<Char, Char>(), Prims.Fail<Char, Char>())
                .FailureTest("foobar".AsStream());
            Prims.Right(Chars.Any(), Prims.Fail<Char, Char>())
                .FailureTest("foobar".AsStream());
            Prims.Right(Prims.Fail<Char, Char>(), Chars.Any())
                .FailureTest("foobar".AsStream());

            Prims.Right(Prims.Error<Char, Char>(), Prims.Fail<Char, Char>())
                .ErrorTest("foobar".AsStream());

            Prims.Right(Prims.Fail<Char, Char>(), Prims.Error<Char, Char>())
                .FailureTest("foobar".AsStream());

            Prims.Right(Chars.Any(), Chars.Any())
                .SuccessTest("foobar".AsStream(), "foobar".Skip(1).First());
        }

        [TestMethod]
        public void Prims_BothTest()
        {
            Prims.Both(Prims.Fail<Char, Char>(), Prims.Fail<Char, Char>())
                .FailureTest("foobar".AsStream());
            Prims.Both(Chars.Any(), Prims.Fail<Char, Char>())
                .FailureTest("foobar".AsStream());
            Prims.Both(Prims.Fail<Char, Char>(), Chars.Any())
                .FailureTest("foobar".AsStream());

            Prims.Both(Prims.Error<Char, Char>(), Prims.Fail<Char, Char>())
                .ErrorTest("foobar".AsStream());

            Prims.Both(Prims.Fail<Char, Char>(), Prims.Error<Char, Char>())
                .FailureTest("foobar".AsStream());

            Prims.Both(Chars.Any(), Chars.Any())
                .SuccessTest("foobar".AsStream(), Tuple.Create("foobar".First(), "foobar".Skip(1).First()));
        }

        [TestMethod]
        public void Prims_BetweenTest()
        {
            Prims.Between(Chars.Any(), Chars.Any(), Prims.Fail<Char, Char>())
                .FailureTest("foobar".AsStream());
            Prims.Between(Chars.Any(), Prims.Fail<Char, Char>(), Chars.Any())
                .FailureTest("foobar".AsStream());
            Prims.Between(Prims.Fail<Char, Char>(), Chars.Any(), Chars.Any())
                .FailureTest("foobar".AsStream());

            Prims.Between(Chars.Any(), Chars.Any(), Prims.Error<Char, Char>())
                .ErrorTest("foobar".AsStream());
            Prims.Between(Chars.Any(), Prims.Error<Char, Char>(), Chars.Any())
                .ErrorTest("foobar".AsStream());
            Prims.Between(Prims.Error<Char, Char>(), Chars.Any(), Chars.Any())
                .ErrorTest("foobar".AsStream());
            

            Prims.Between(Chars.Sequence("foobar"), Chars.Satisfy('('), Chars.Satisfy(')'))
                .SuccessTest("(foobar)".AsStream(), "foobar".AsEnumerable(), Enumerable.SequenceEqual);
        }

        [TestMethod]
        public void Prims_SepByTest()
        {
            Prims.SepBy(Chars.Any(), Chars.Satisfy(','))
                .FailureTest("".AsStream());

            Prims.SepBy(Chars.Any(), 1, Chars.Satisfy(','))
                .FailureTest("a".AsStream());
            Prims.SepBy(Chars.Any(), 2, Chars.Satisfy(','))
                .FailureTest("a,b".AsStream());

            Prims.SepBy(Chars.Any(), Chars.Satisfy(','))
                .SuccessTest("a".AsStream(), new[] { 'a' }, Enumerable.SequenceEqual);
            Prims.SepBy(Chars.Any(), Chars.Satisfy(','))
                .SuccessTest("a,b,c".AsStream(), new[] { 'a', 'b', 'c' }, Enumerable.SequenceEqual);
        }

        [TestMethod]
        public void Prims_SepEndByTest()
        {
            Prims.SepEndBy(Chars.Any(), Chars.Satisfy(','))
                .FailureTest("".AsStream());

            Prims.SepEndBy(Chars.Any(), 1, Chars.Satisfy(','))
                .FailureTest("a".AsStream());
            Prims.SepEndBy(Chars.Any(), 1, Chars.Satisfy(','))
                .FailureTest("a,".AsStream());
            Prims.SepEndBy(Chars.Any(), 2, Chars.Satisfy(','))
                .FailureTest("a,b".AsStream());
            Prims.SepEndBy(Chars.Any(), 2, Chars.Satisfy(','))
                .FailureTest("a,b,".AsStream());

            Prims.SepEndBy(Chars.Any(), Chars.Satisfy(','))
                .SuccessTest("a".AsStream(), new[] { 'a' }, Enumerable.SequenceEqual);
            Prims.SepEndBy(Chars.Any(), Chars.Satisfy(','))
                .SuccessTest("a,".AsStream(), new[] { 'a' }, Enumerable.SequenceEqual);
            Prims.SepEndBy(Chars.Any(), Chars.Satisfy(','))
                .SuccessTest("a,b,c".AsStream(), new[] { 'a', 'b', 'c' }, Enumerable.SequenceEqual);
            Prims.SepEndBy(Chars.Any(), Chars.Satisfy(','))
                .SuccessTest("a,b,c,".AsStream(), new[] { 'a', 'b', 'c' }, Enumerable.SequenceEqual);
        }

        [TestMethod]
        public void Prims_WhenSuccessTest()
        {
            Prims.WhenSuccess(Prims.Fail<Char, Char>(), Chars.Any())
                .FailureTest("foobar".AsStream());
            Prims.WhenSuccess(Chars.Any(), Prims.Fail<Char, Char>())
                .FailureTest("foobar".AsStream());

            Prims.WhenSuccess(Prims.Error<Char, Char>(), Chars.Any())
                .ErrorTest("foobar".AsStream());
            Prims.WhenSuccess(Chars.Any(), Prims.Error<Char, Char>())
                .ErrorTest("foobar".AsStream());

            Prims.WhenSuccess(Chars.Any(), Chars.Any())
                .SuccessTest("foobar".AsStream(), "foobar".Skip(1).First());
        }

        [TestMethod]
        public void Prims_WhenFailureTest()
        {
            Prims.WhenFailure(Prims.Fail<Char, Char>(), Prims.Fail<Char, Char>())
                .FailureTest("foobar".AsStream());

            Prims.WhenFailure(Prims.Error<Char, Char>(), Chars.Any())
                .ErrorTest("foobar".AsStream());

            Prims.WhenFailure(Chars.Any(), Chars.Any())
                .SuccessTest("foobar".AsStream(), "foobar".First());
            Prims.WhenFailure(Prims.Fail<Char, Char>(), Chars.Any())
                .SuccessTest("foobar".AsStream(), "foobar".First());
        }

        [TestMethod]
        public void Prims_WhenErrorTest()
        {
            Prims.WhenError(Chars.Any(), Chars.Any())
                .SuccessTest("foobar".AsStream(), "foobar".First());
            Prims.WhenError(Prims.Fail<Char, Char>(), Chars.Any())
                .FailureTest("foobar".AsStream());
            Prims.WhenError(Prims.Error<Char, Char>(), Chars.Any())
                .SuccessTest("foobar".AsStream(), "foobar".First());

            Prims.WhenError(Prims.Error<Char, Char>(), Prims.Error<Char, Char>())
                .ErrorTest("foobar".AsStream());
        }
    }
}
