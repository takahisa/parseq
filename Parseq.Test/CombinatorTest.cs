using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Parseq;
using Parseq.Combinators;

namespace Parseq.Test
{
    [TestClass]
    public class CombinatorTest
    {
        [TestMethod]
        public void SequenceTest()
        {
            Combinator.Sequence(Enumerable.Empty<Parser<Char, Char>>())
                .ExpectSuccess(Enumerable.Empty<Char>())
                .Run("foobar".AsStream());

            var p1 = Prims.Return<Char, Char>('a');
            var p2 = Prims.Return<Char, Char>('b');
            var p3 = Prims.Return<Char, Char>('c');
            /* ... */
            var pn = Prims.Return<Char, Char>('n');

            Combinator.Sequence(p1, p2, p3, /* ... */ pn)
                .ExpectSuccess(new[] { 'a', 'b', 'c', /* ... */ 'n' })
                .Run("foobar".AsStream());

            var success = Prims.Return<Char, Char>(default(Char));
            var failure = Prims.Fail<Char, Char>();

            Combinator.Sequence(failure, success, success, /* ... */ success).ExpectFailure().Run("foobar".AsStream());
            Combinator.Sequence(success, failure, success, /* ... */ success).ExpectFailure().Run("foobar".AsStream());
            Combinator.Sequence(success, success, failure, /* ... */ success).ExpectFailure().Run("foobar".AsStream());
            Combinator.Sequence(success, success, success, /* ... */ failure).ExpectFailure().Run("foobar".AsStream());

            var error = Errors.Error<Char, Char>("Error!");

            Combinator.Sequence(error, success, success, /* ... */ success).ExpectError().Run("foobar".AsStream());
            Combinator.Sequence(success, error, success, /* ... */ success).ExpectError().Run("foobar".AsStream());
            Combinator.Sequence(success, success, error, /* ... */ success).ExpectError().Run("foobar".AsStream());
            Combinator.Sequence(success, success, success, /* ... */ error).ExpectError().Run("foobar".AsStream());

            Combinator.Sequence(failure, error).ExpectFailure().Run("foobar".AsStream());
            Combinator.Sequence(error, failure).ExpectError().Run("foobar".AsStream());
        }

        [TestMethod]
        public void ChoiceTest()
        {
            Combinator.Choice(Enumerable.Empty<Parser<Char, Char>>())
                .ExpectFailure()
                .Run("foobar".AsStream());

            var failure = Prims.Fail<Char, Char>();
            var success = Prims.Return<Char, Char>(default(Char));

            Combinator.Choice(failure, failure, failure, /* ... */ failure).ExpectFailure().Run("foobar".AsStream());
            Combinator.Choice(success, failure, failure, /* ... */ failure).ExpectSuccess().Run("foobar".AsStream());
            Combinator.Choice(failure, success, failure, /* ... */ failure).ExpectSuccess().Run("foobar".AsStream());
            Combinator.Choice(failure, failure, success, /* ... */ failure).ExpectSuccess().Run("foobar".AsStream());
            Combinator.Choice(failure, failure, failure, /* ... */ success).ExpectSuccess().Run("foobar".AsStream());

            var error = Errors.Error<Char, Char>("Error!");

            Combinator.Choice(failure, failure, failure, /* ... */ failure).ExpectFailure().Run("foobar".AsStream());
            Combinator.Choice(error,   failure, failure, /* ... */ failure).ExpectError().Run("foobar".AsStream());
            Combinator.Choice(failure,   error, failure, /* ... */ failure).ExpectError().Run("foobar".AsStream());
            Combinator.Choice(failure, failure,   error, /* ... */ failure).ExpectError().Run("foobar".AsStream());
            Combinator.Choice(failure, failure, failure, /* ... */   error).ExpectError().Run("foobar".AsStream());

            Combinator.Choice(failure, success).ExpectSuccess().Run("foobar".AsStream());
            Combinator.Choice(failure, error).ExpectError().Run("foobar".AsStream());
            Combinator.Choice(success, error).ExpectSuccess().Run("foobar".AsStream());
            Combinator.Choice(error, success).ExpectError().Run("foobar".AsStream());

            // Issue #4 
            // Combinator.Choice does not work since second time parsing in Parseq 
            var parser = Combinator.Choice(new[] { 'a', 'b', 'c' }.Select(_ => _.Satisfy()));
            parser.ExpectSuccess('c').Run("c".AsStream());
            parser.ExpectSuccess('c').Run("c".AsStream());        
        }

        [TestMethod]
        public void ForkTest()
        {
            var p1 = Chars.Satisfy('a');
            var p2 = Chars.Satisfy('b');
            Combinator.Fork(p1, p2).ExpectTrue(Either.Left<Char, Char>('a').Equals).Run("a".AsStream());
            Combinator.Fork(p1, p2).ExpectTrue(Either.Right<Char, Char>('b').Equals).Run("b".AsStream());
            Combinator.Fork(p1, p2).ExpectFailure().Run("c".AsStream());

            var p3 = Prims.Return<Char, Char>('c');
            var p4 = Prims.Return<Char, Char>('d');
            Combinator.Fork(p3, p4).ExpectTrue(Either.Left<Char, Char>('c').Equals).Run("foobar".AsStream());
            Combinator.Fork(p4, p3).ExpectTrue(Either.Left<Char, Char>('d').Equals).Run("foobar".AsStream());

            var success = Prims.Return<Char, Char>(default(Char));
            var failure = Prims.Fail<Char, Char>();
            var error = Errors.Error<Char, Char>("Error!");

            Combinator.Fork(success, failure).ExpectSuccess().Run("foobar".AsStream());
            Combinator.Fork(failure, success).ExpectSuccess().Run("foobar".AsStream());
            Combinator.Fork(failure, failure).ExpectFailure().Run("foobar".AsStream());
            Combinator.Fork(success, error).ExpectSuccess().Run("foobar".AsStream());
            Combinator.Fork(failure, error).ExpectError().Run("foobar".AsStream());
            Combinator.Fork(error, success).ExpectError().Run("foobar".AsStream());
            Combinator.Fork(error, failure).ExpectError().Run("foobar".AsStream());
        }

        [TestMethod]
        public void OrTest()
        {
            var p1 = Chars.Satisfy('a');
            var p2 = Chars.Satisfy('b');
            Combinator.Or(p1, p2).ExpectSuccess('a').Run("a".AsStream());
            Combinator.Or(p1, p2).ExpectSuccess('b').Run("b".AsStream());
            Combinator.Or(p1, p2).ExpectFailure().Run("c".AsStream());

            var p3 = Prims.Return<Char, Char>('c');
            var p4 = Prims.Return<Char, Char>('d');
            Combinator.Or(p3, p4).ExpectSuccess('c').Run("foobar".AsStream());
            Combinator.Or(p4, p3).ExpectSuccess('d').Run("foobar".AsStream());

            var success = Prims.Return<Char, Char>(default(Char));
            var failure = Prims.Fail<Char, Char>();
            var error = Errors.Error<Char, Char>("Error!");

            Combinator.Or(success, failure).ExpectSuccess().Run("foobar".AsStream());
            Combinator.Or(failure, success).ExpectSuccess().Run("foobar".AsStream());
            Combinator.Or(failure, failure).ExpectFailure().Run("foobar".AsStream());
            Combinator.Or(success, error).ExpectSuccess().Run("foobar".AsStream());
            Combinator.Or(failure, error).ExpectError().Run("foobar".AsStream());
            Combinator.Or(error, success).ExpectError().Run("foobar".AsStream());
            Combinator.Or(error, failure).ExpectError().Run("foobar".AsStream());
        }

        [TestMethod]
        public void AndTest()
        {
            var success = Prims.Return<Char, Unit>(Unit.Instance);
            var failure = Prims.Fail<Char, Unit>();
            var error = Prims.Error<Char, Unit>("Error!");

            Combinator.And(success).ExpectSuccess().Run("foobar".AsStream());
            Combinator.And(failure).ExpectFailure().Run("foobar".AsStream());
            Combinator.And(error).ExpectError().Run("foobar".AsStream());

            var p1 = Chars.Satisfy('a');
            var p2 = Chars.Satisfy('b');
            Combinator.Sequence(Combinator.And(p1), p1).ExpectSuccess().Run("a".AsStream());
            Combinator.Sequence(p1, p1).ExpectFailure().Run("a".AsStream());
            Combinator.Sequence(Combinator.And(p1), p2).ExpectFailure().Run("ab".AsStream());
            Combinator.Sequence(p1, p2).ExpectSuccess().Run("ab".AsStream());
        }

        [TestMethod]
        public void NotTest()
        {
            var success = Prims.Return<Char, Unit>(Unit.Instance);
            var failure = Prims.Fail<Char, Unit>();
            var error = Prims.Error<Char, Unit>("Error!");

            Combinator.Not(success).ExpectFailure().Run("foobar".AsStream());
            Combinator.Not(failure).ExpectSuccess().Run("foobar".AsStream());
            Combinator.Not(error).ExpectError().Run("foobar".AsStream());

            var p1 = Chars.Satisfy('a');
            var p2 = Chars.Satisfy('b');
            Testing.ExpectFailure(from r1 in Combinator.Not(p1) from r2 in p2 select r2)
                .Run("cb".AsStream());
            Testing.ExpectSuccess(from r1 in Combinator.Not(p1) from r2 in p2 select r2, 'b')
                .Run("b".AsStream());
        }

        [TestMethod]
        public void GreedTest()
        {
            var p1 = Chars.Satisfy('a');

            Combinator.Greed(Enumerable.Repeat(p1, 10)).ExpectSuccess(Enumerable.Repeat('a', 10)).Run(Enumerable.Repeat('a', 10).AsStream());
            Combinator.Greed(Enumerable.Repeat(p1, 10)).ExpectSuccess(Enumerable.Repeat('a', 9)).Run(Enumerable.Repeat('a', 9).AsStream());
            Combinator.Greed(Enumerable.Repeat(p1, 10)).ExpectSuccess(Enumerable.Repeat('a', 8)).Run(Enumerable.Repeat('a', 8).AsStream());
            Combinator.Greed(Enumerable.Repeat(p1, 10)).ExpectSuccess(Enumerable.Repeat('a', 1)).Run(Enumerable.Repeat('a', 1).AsStream());

            Combinator.Greed(Enumerable.Repeat(p1, 10)).ExpectSuccess(Enumerable.Empty<Char>()).Run("foobar".AsStream());

            var success = Prims.Return<Char, Unit>(Unit.Instance);
            var failure = Prims.Fail<Char, Unit>();
            var error = Errors.Error<Char, Unit>("Error!");

            Combinator.Greed(new[] { success, success, /* ... */ failure }.AsEnumerable()).ExpectSuccess().Run("foobar".AsStream());
            Combinator.Greed(new[] { success, failure, /* ... */ success }.AsEnumerable()).ExpectSuccess().Run("foobar".AsStream());
            Combinator.Greed(new[] { failure, success, /* ... */ failure }.AsEnumerable()).ExpectSuccess().Run("foobar".AsStream());
            Combinator.Greed(new[] { success, success, /* ... */ error }.AsEnumerable()).ExpectError().Run("foobar".AsStream());
            Combinator.Greed(new[] { success, error,   /* ... */ success }.AsEnumerable()).ExpectError().Run("foobar".AsStream());
            Combinator.Greed(new[] { error, success,   /* ... */ success }.AsEnumerable()).ExpectError().Run("foobar".AsStream());
        }

        [TestMethod]
        public void RepeatTest()
        {
            var p1 = Chars.Satisfy('a');
            
            Combinator.Repeat(p1, 10).ExpectSuccess(Enumerable.Repeat('a', 10)).Run(Enumerable.Repeat('a', 10).AsStream());
            Combinator.Repeat(p1, 10).ExpectFailure().Run(Enumerable.Repeat('a', 9).AsStream());

            var success = Prims.Return<Char, Unit>(Unit.Instance);
            var failure = Prims.Fail<Char, Unit>();
            var error = Errors.Error<Char, Unit>("Error!");
            
            Combinator.Repeat(success, 10).ExpectSuccess(Enumerable.Repeat(Unit.Instance, 10)).Run("foobar".AsStream());
            Combinator.Repeat(failure, 0).ExpectSuccess(Enumerable.Empty<Unit>()).Run("foobar".AsStream());
            Combinator.Repeat(error, 0).ExpectSuccess(Enumerable.Empty<Unit>()).Run("foobar".AsStream());
        }

        [TestMethod]
        public void ManyTest()
        {
            var any = Chars.Any();

            var success = Prims.Return<Char, Unit>(Unit.Instance);
            var failure = Prims.Fail<Char, Unit>();
            var error = Prims.Error<Char, Unit>();

            Combinator.Many(any)
                .ExpectSuccess("foobar").Run("foobar".AsStream());
            Combinator.Many(failure)
                .ExpectSuccess(Enumerable.Empty<Unit>()).Run("foobar".AsStream());
            Combinator.Many(error)
                .ExpectError().Run("foobar".AsStream());

            Combinator.Many(failure, 0).ExpectSuccess().Run("foobar".AsStream());
            Combinator.Many(failure, 1).ExpectFailure().Run("foobar".AsStream());
            Combinator.Many(any, "foobar".Length + 1)
                .ExpectFailure()
                .Run("foobar".AsStream());

            Combinator.Many(success, 0, 10)
                .ExpectSuccess(Enumerable.Repeat(Unit.Instance, 10))
                .Run("foobar".AsStream());
            Combinator.Many(failure, 0, 0)
                .ExpectSuccess(Enumerable.Empty<Unit>())
                .Run("foobar".AsStream());
        }

        [TestMethod]
        public void MaybeTest()
        {
            var success = Prims.Return<Char, Unit>(Unit.Instance);
            var failure = Prims.Fail<Char, Unit>();

            Combinator.Maybe(success).ExpectSuccess(Option.Just<Unit>(Unit.Instance)).Run("foobar".AsStream());
            Combinator.Maybe(failure).ExpectSuccess(Option.None<Unit>()).Run("foobar".AsStream());
        }
    }
}
