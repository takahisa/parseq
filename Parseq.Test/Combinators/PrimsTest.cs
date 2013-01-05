using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Parseq;
using Parseq.Combinators;

namespace Parseq.Test.Combinators
{
    [TestClass]
    public class PrimsTest
    {
        [TestMethod]
        public void EofTest()
        {
            Prims.Eof<Char>().ExpectFailure().Run("foobar".AsStream());
            Prims.Eof<Char>().ExpectSuccess().Run(String.Empty.AsStream());
        }

        [TestMethod]
        public void ReturnTest()
        {
            Prims.Return<Char, Unit>(Unit.Instance).ExpectSuccess(Unit.Instance).Run(String.Empty.AsStream());
            Prims.Return<Char, Unit>(() => Unit.Instance).ExpectSuccess(Unit.Instance).Run(String.Empty.AsStream());
        }

        [TestMethod]
        public void EmptyTest()
        {
            Prims.Empty<Char, Unit>().ExpectSuccess(Enumerable.Empty<Unit>()).Run(String.Empty.AsStream());
        }

        [TestMethod]
        public void FailTest()
        {
            Prims.Fail<Char, Unit>().ExpectFailure().Run(String.Empty.AsStream());
        }

        [TestMethod]
        public void ErrorTest()
        {
            Prims.Error<Char, Unit>("hoge").ExpectError().Run(String.Empty.AsStream());
            Prims.Error<Char, Unit>().ExpectError().Run(String.Empty.AsStream());
        }

        [TestMethod]
        public void SatisfyTest()
        {
            Prims.Satisfy<Char>('a').ExpectSuccess().Run("a".AsStream());
            Prims.Satisfy<Char>('a').ExpectFailure().Run("b".AsStream());
            Prims.Satisfy<Char>('a').ExpectSuccess().Run("abcdef...".AsStream());

            Prims.Satisfy<Char>(_ => true).ExpectSuccess("foobar".First()).Run("foobar".AsStream());
            Prims.Satisfy<Char>(_ => false).ExpectFailure().Run("foobar".AsStream());
        }

        [TestMethod]
        public void AnyTest()
        {
            Prims.Any<Char>().ExpectSuccess("foobar".First()).Run("foobar".AsStream());
            Prims.Any<Char>().ExpectFailure().Run(String.Empty.AsStream());
        }

        [TestMethod]
        public void OneOfTest()
        {
            Prims.OneOf<Char>('a', 'b', 'c').ExpectSuccess('a').Run("a...".AsStream());
            Prims.OneOf<Char>('a', 'b', 'c').ExpectSuccess('b').Run("b...".AsStream());
            Prims.OneOf<Char>('a', 'b', 'c').ExpectSuccess('c').Run("c...".AsStream());
            Prims.OneOf<Char>('a', 'b', 'c').ExpectFailure().Run("d...".AsStream());
            Prims.OneOf<Char>('a', 'b', 'c').ExpectFailure().Run(String.Empty.AsStream());
        }

        [TestMethod]
        public void NoneOfTest()
        {
            Prims.NoneOf<Char>('a', 'b', 'c').ExpectFailure().Run("a...".AsStream());
            Prims.NoneOf<Char>('a', 'b', 'c').ExpectFailure().Run("b...".AsStream());
            Prims.NoneOf<Char>('a', 'b', 'c').ExpectFailure().Run("c...".AsStream());
            Prims.NoneOf<Char>('a', 'b', 'c').ExpectSuccess('d').Run("d...".AsStream());
            Prims.NoneOf<Char>('a', 'b', 'c').ExpectFailure().Run(String.Empty.AsStream());
        }

        [TestMethod]
        public void PipeTest()
        {
            var any = Chars.Any();
            var success = Prims.Return<Char, Unit>(Unit.Instance);
            var failure = Prims.Fail<Char, Unit>();
            var error = Prims.Error<Char, Unit>();

            Prims.Pipe(any, any, Tuple.Create).ExpectSuccess(Tuple.Create('f', 'o')).Run("foobar".AsStream());
            Prims.Pipe(any, any, any, Tuple.Create).ExpectSuccess(Tuple.Create('f', 'o', 'o')).Run("foobar".AsStream());
            Prims.Pipe(any, any, any, any, Tuple.Create).ExpectSuccess(Tuple.Create('f', 'o', 'o', 'b')).Run("foobar".AsStream());
            Prims.Pipe(any, any, any, any, any, Tuple.Create).ExpectSuccess(Tuple.Create('f', 'o', 'o', 'b', 'a')).Run("foobar".AsStream());

            Prims.Pipe(failure, failure, Tuple.Create).ExpectFailure().Run("foobar".AsStream());
            Prims.Pipe(success, failure, Tuple.Create).ExpectFailure().Run("foobar".AsStream());
            Prims.Pipe(failure, success, Tuple.Create).ExpectFailure().Run("foobar".AsStream());
            Prims.Pipe(failure, error, Tuple.Create).ExpectFailure().Run("foobar".AsStream());
            Prims.Pipe(error, failure, Tuple.Create).ExpectError().Run("foobar".AsStream());
            Prims.Pipe(success, error, Tuple.Create).ExpectError().Run("foobar".AsStream());
        }

        [TestMethod]
        public void LeftTest()
        {
            var any = Chars.Any();
            var success = Prims.Return<Char, Unit>(Unit.Instance);
            var failure = Prims.Fail<Char, Unit>();
            var error = Prims.Error<Char, Unit>();

            Prims.Left(any, any).ExpectSuccess('l').Run("lr".AsStream());
            Prims.Left(success, failure).ExpectFailure().Run("foobar".AsStream());
            Prims.Left(failure, success).ExpectFailure().Run("foobar".AsStream());
            Prims.Left(failure, failure).ExpectFailure().Run("foobar".AsStream());
            Prims.Left(success, error).ExpectError().Run("foobar".AsStream());

            Prims.Left(any, any).ExpectFailure().Run(String.Empty.AsStream());
            Prims.Left(any, any).ExpectFailure().Run("l".AsStream());
        }

        [TestMethod]
        public void RightTest()
        {
            var any = Chars.Any();
            var success = Prims.Return<Char, Unit>(Unit.Instance);
            var failure = Prims.Fail<Char, Unit>();
            var error = Prims.Error<Char, Unit>();

            Prims.Right(any, any).ExpectSuccess('r').Run("lr".AsStream());
            Prims.Right(success, failure).ExpectFailure().Run("foobar".AsStream());
            Prims.Right(failure, success).ExpectFailure().Run("foobar".AsStream());
            Prims.Right(failure, failure).ExpectFailure().Run("foobar".AsStream());
            Prims.Right(success, error).ExpectError().Run("foobar".AsStream());

            Prims.Right(any, any).ExpectFailure().Run(String.Empty.AsStream());
            Prims.Right(any, any).ExpectFailure().Run("l".AsStream());
        }

        [TestMethod]
        public void BothTest()
        {
            var any = Chars.Any();
            var success = Prims.Return<Char, Unit>(Unit.Instance);
            var failure = Prims.Fail<Char, Unit>();
            var error = Prims.Error<Char, Unit>();

            Prims.Both(any, any).ExpectSuccess(Tuple.Create('l', 'r')).Run("lr".AsStream());
            Prims.Both(success, failure).ExpectFailure().Run("foobar".AsStream());
            Prims.Both(failure, success).ExpectFailure().Run("foobar".AsStream());
            Prims.Both(failure, failure).ExpectFailure().Run("foobar".AsStream());
            Prims.Both(success, error).ExpectError().Run("foobar".AsStream());

            Prims.Both(any, any).ExpectFailure().Run(String.Empty.AsStream());
            Prims.Both(any, any).ExpectFailure().Run("l".AsStream());
        }

        [TestMethod]
        public void SepByTest()
        {
            var parser = Chars.Satisfy('a');
            var separator = Chars.Satisfy(',');
            
            Prims.SepBy(parser, separator)
                .ExpectSuccess(new[] { 'a', 'a', 'a' })
                .Run("a,a,a".AsStream());

            Prims.SepBy(parser, separator).Left(Chars.Satisfy(','))
                .ExpectSuccess(new[] { 'a', 'a', 'a' })
                .Run("a,a,a,".AsStream());

            Prims.SepBy(parser, 0, separator)
                .ExpectSuccess(new Char[] { })
                .Run("".AsStream());

            Prims.SepBy(parser, 2, separator)
                .ExpectFailure()
                .Run("a".AsStream());
        }

        [TestMethod]
        public void SepEndBy()
        {
            var parser = Chars.Satisfy('a');
            var separator = Chars.Satisfy(',');

            Prims.SepEndBy(parser, separator)
                .ExpectSuccess(new[] { 'a', 'a', 'a' })
                .Run("a,a,a".AsStream());

            Prims.SepEndBy(parser, separator)
                .ExpectSuccess(new[] { 'a', 'a', 'a' })
                .Run("a,a,a,".AsStream());

            Prims.SepEndBy(parser, 0, separator)
                .ExpectSuccess(new Char[] { })
                .Run("".AsStream());

            Prims.SepEndBy(parser, 1, separator)
                .ExpectSuccess(new[] { 'a', 'a', 'a' })
                .Run("a,a,a,".AsStream());

            Prims.SepEndBy(parser, 1, separator)
                .ExpectFailure()
                .Run("".AsStream());

            Prims.SepEndBy(parser, 2, separator)
                .ExpectFailure()
                .Run("a,".AsStream());            
        }

        [TestMethod]
        public void EndBy()
        {
            var parser = Chars.Satisfy('a');
            var separator = Chars.Satisfy(',');

            Prims.EndBy(parser, separator)
                .ExpectSuccess(new[] { 'a', 'a' })
                .Run("a,a,a".AsStream());

            Prims.EndBy(parser, separator)
                .ExpectSuccess(new[] { 'a', 'a', 'a' })
                .Run("a,a,a,".AsStream());

            Prims.EndBy(parser, 1, separator)
                .ExpectSuccess(new[] { 'a', 'a', 'a' })
                .Run("a,a,a,".AsStream());

            Prims.EndBy(parser, 0, separator)
                .ExpectSuccess(new Char[] { })
                .Run("".AsStream());

            Prims.EndBy(parser, 2, separator)
                .ExpectFailure()
                .Run("a,".AsStream());
        }

        [TestMethod]
        public void BetweenTest()
        {
            var lp = Chars.Satisfy('(');
            var rp = Chars.Satisfy(')');
            var parser = Chars.Any();

            Prims.Between(parser, lp, rp)
                .ExpectSuccess('a')
                .Run("(a)".AsStream());

            Prims.Between(parser, lp, rp)
                .ExpectFailure()
                .Run("(a".AsStream());
            Prims.Between(parser, lp, rp)
                .ExpectFailure()
                .Run("a)".AsStream());

            Prims.Between(parser, lp, rp)
                .ExpectFailure()
                .Run("a".AsStream());
        }

        [TestMethod]
        public void ChainrlTest()
        {
            var parser = Combinator.Not(Chars.Satisfy('0')).Right(Chars.Digit().Many(1))
                .Select(s => Int32.Parse(new String(s.ToArray())));
            var sep = Chars.Satisfy('-').Ignore();

            Prims.Chainl(parser, sep, (x, y) => x - y)
                .ExpectSuccess((((1 - 2) - 3) - 4))
                .Run("1-2-3-4".AsStream());

            Prims.Chainl(parser, sep, 0, (x, y) => x - y)
                .ExpectSuccess(((((0 - 1) - 2) - 3) - 4))
                .Run("1-2-3-4".AsStream());
        }

        [TestMethod]
        public void ChainrTest()
        {
            var parser = Combinator.Not(Chars.Satisfy('0')).Right(Chars.Digit().Many(1))
                .Select(s => Int32.Parse(new String(s.ToArray())));
            var sep = Chars.Satisfy('-').Ignore();

            Prims.Chainr(parser, sep, (x, y) => x - y)
                .ExpectSuccess(1 - ( 2 - ( 3 - 4 )))
                .Run("1-2-3-4".AsStream());

            Prims.Chainr(parser, sep, 0, (x, y) => x - y)
                .ExpectSuccess(1 - (2 - (3 - (4 - 0 ))))
                .Run("1-2-3-4".AsStream());
        }
    }
}
