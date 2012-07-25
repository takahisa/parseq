using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Parseq;
using Parseq.Combinators;

namespace Parseq.Test
{
    [TestClass]
    public class CombinatorTest
    {
        [TestMethod]
        public void Combinator_SequenceTest()
        {
            Combinator.Choice(Enumerable.Empty<Parser<Char, Char>>()).FailureTest("xyzzy".AsStream());

            Combinator.Sequence("foobar".Select(t => t.Satisfy())).SuccessTest("foobar".AsStream(), "foobar".AsEnumerable(), Enumerable.SequenceEqual);

            Combinator.Sequence("foobar".Select(t => t.Satisfy())).FailureTest("woobar".AsStream());
            Combinator.Sequence("foobar".Select(t => t.Satisfy())).FailureTest("fwobar".AsStream());
            Combinator.Sequence("foobar".Select(t => t.Satisfy())).FailureTest("fowbar".AsStream());
            Combinator.Sequence("foobar".Select(t => t.Satisfy())).FailureTest("foowar".AsStream());
            Combinator.Sequence("foobar".Select(t => t.Satisfy())).FailureTest("foobwr".AsStream());
            Combinator.Sequence("foobar".Select(t => t.Satisfy())).FailureTest("foobaw".AsStream());

            Combinator.Sequence(
                'w'.Satisfy(),
                Errors.Error<Char, Char>("short circuit evaluation failure"))
                .FailureTest("foobar".AsStream());

            Combinator.Sequence(Errors.Error<Char, Char>("This combinator's result will be Error if input parsers contains parser what returns Error."))
                .ErrorTest("foobar".AsStream());

            var counter = 0;
            Combinator.Sequence(
                'f'.Satisfy().DoWhenSuccess(e => Assert.IsTrue(counter++ == 0)),
                'o'.Satisfy().DoWhenSuccess(e => Assert.IsTrue(counter++ == 1)),
                'o'.Satisfy().DoWhenSuccess(e => Assert.IsTrue(counter++ == 2)),
                'b'.Satisfy().DoWhenSuccess(e => Assert.IsTrue(counter++ == 3)),
                'a'.Satisfy().DoWhenSuccess(e => Assert.IsTrue(counter++ == 4)),
                Chars.Satisfy(_ => counter == 5))
                .SuccessTest("foobar".AsStream(), "foobar".AsEnumerable(), Enumerable.SequenceEqual);
        }

        [TestMethod]
        public void Combinator_ChoiceTest()
        {
            Combinator.Choice(Enumerable.Empty<Parser<Char, Char>>()).FailureTest("foobar".AsStream());

            Combinator.Choice("foobar".Select(t => t.Satisfy())).FailureTest("xyzzy".AsStream());

            Combinator.Choice("foobar".Select(t => t.Satisfy())).SuccessTest("f".AsStream(), 'f');
            Combinator.Choice("foobar".Select(t => t.Satisfy())).SuccessTest("o".AsStream(), 'o');
            Combinator.Choice("foobar".Select(t => t.Satisfy())).SuccessTest("b".AsStream(), 'b');
            Combinator.Choice("foobar".Select(t => t.Satisfy())).SuccessTest("a".AsStream(), 'a');
            Combinator.Choice("foobar".Select(t => t.Satisfy())).SuccessTest("r".AsStream(), 'r');

            Combinator.Choice('f'.Satisfy(), Errors.Error<Char, Char>("short circuit evaluation failure"))
                .SuccessTest("foobar".AsStream(), "foobar".First());

            Combinator.Choice(Errors.Error<Char, Char>("This combinator's result will be Error if input parsers contains parser what returns Error."))
                .ErrorTest("foobar".AsStream());

            var counter = 0;
            Combinator.Choice(
                Prims.Fail<Char, Char>().DoWhenFailure(() => Assert.IsTrue(counter++ == 0)),
                Prims.Fail<Char, Char>().DoWhenFailure(() => Assert.IsTrue(counter++ == 1)),
                Prims.Fail<Char, Char>().DoWhenFailure(() => Assert.IsTrue(counter++ == 2)),
                Chars.Satisfy(_ => counter == 3))
                .SuccessTest("foobar".AsStream(), "foobar".First());
        }

        [TestMethod]
        public void Combinator_OrTest()
        {
            Combinator.Or(Prims.Fail<Char, Char>(), Prims.Fail<Char, Char>())
                .FailureTest("foobar".AsStream());

            Combinator.Or(Chars.Any(), Prims.Fail<Char, Char>())
                .SuccessTest("foobar".AsStream(), "foobar".First());
            Combinator.Or(Prims.Fail<Char, Char>(), Chars.Any())
                .SuccessTest("foobar".AsStream(), "foobar".First());
            Combinator.Or(Chars.Any(), Chars.Any())
                .SuccessTest("foobar".AsStream(), "foobar".First());

            Combinator.Or(Chars.Any(), Errors.Error<Char, Char>("short circuit evaluation failure"))
                .SuccessTest("foobar".AsStream(), "foobar".First());

            Combinator.Or(Errors.Error<Char, Char>("This combinator's result will be Error if parser0 returns Error"), Chars.Any())
                .ErrorTest("foobar".AsStream());

            Combinator.Or(Prims.Fail<Char, Char>(), Errors.Error<Char, Char>("This combinator's result will be Error if parser0 returns Failure and parser1 returns Error"))
                .ErrorTest("foobar".AsStream());
        }

        [TestMethod]
        public void Combinator_AndTest()
        {
            Combinator.And(Chars.Any())
                .SuccessTest("foobar".AsStream(), Unit.Instance);
            Combinator.And(Prims.Fail<Char, Char>())
                .FailureTest("foobar".AsStream());
            Combinator.And(Prims.Error<Char, Char>())
                .ErrorTest("foobar".AsStream());

            Parser<Char, Position> position = stream => Reply.Success(stream, stream.Position);

            Prims.Pipe(position, Combinator.And(Chars.Any()), position,
                (x, y, z) => { Assert.IsTrue(x == z); return Unit.Instance; })
                .SuccessTest("foobar".AsStream(), Unit.Instance);
        }

        [TestMethod]
        public void Combinator_NotTest()
        {
            Combinator.Not(Chars.Any())
                .FailureTest("foobar".AsStream());
            Combinator.Not(Prims.Fail<Char, Char>())
                .SuccessTest("foobar".AsStream(), Unit.Instance);
            Combinator.Not(Prims.Error<Char, Char>())
                .ErrorTest("foobar".AsStream());

            Parser<Char, Position> position = stream => Reply.Success(stream, stream.Position);

            Prims.Pipe(position, Combinator.Not(Prims.Fail<Char,Char>()), position,
                (x, y, z) => { Assert.IsTrue(x == z); return Unit.Instance; })
                .SuccessTest("foobar".AsStream(), Unit.Instance);
        }

        [TestMethod]
        public void Combinator_RepeatTest()
        {
            Combinator.Repeat(Prims.Fail<Char, Char>(), 0)
                .SuccessTest("foobar".AsStream(), Enumerable.Empty<Char>(), Enumerable.SequenceEqual);

            Combinator.Repeat(Prims.Error<Char, Char>(), 0)
                .SuccessTest("foobar".AsStream(), Enumerable.Empty<Char>(), Enumerable.SequenceEqual);
            Combinator.Repeat(Prims.Error<Char, Char>(), 1)
                .ErrorTest("foobar".AsStream());

            Combinator.Repeat(Prims.Fail<Char, Char>(), 1)
                .FailureTest("foobar".AsStream());

            Combinator.Repeat(Chars.Any(), 1)
                .SuccessTest("foobar".AsStream(), "f".AsEnumerable(), Enumerable.SequenceEqual);

            Combinator.Repeat(Chars.Any(), 2)
                .SuccessTest("foobar".AsStream(), "fo".AsEnumerable(), Enumerable.SequenceEqual);

            Combinator.Repeat(Chars.Any(), "foobar".Length)
                .SuccessTest("foobar".AsStream(), "foobar".AsEnumerable(), Enumerable.SequenceEqual);

            Combinator.Repeat(Chars.Any(), "foobar".Length + 1)
                .FailureTest("foobar".AsStream());
        }

        [TestMethod]
        public void Combinator_ManyTest()
        {
            Combinator.Many(Prims.Fail<Char, Char>())
                .SuccessTest("foobar".AsStream(), Enumerable.Empty<Char>(), Enumerable.SequenceEqual);

            Combinator.Many(Prims.Error<Char, Char>())
                .ErrorTest("foobar".AsStream());

            Combinator.Many(Chars.Any())
                .SuccessTest("foobar".AsStream(), "foobar".AsEnumerable(), Enumerable.SequenceEqual);


            Combinator.Many(Chars.Any(), 0)
                .SuccessTest("foobar".AsStream(), "foobar".AsEnumerable(), Enumerable.SequenceEqual);
            Combinator.Many(Chars.Any(), 1)
                .SuccessTest("foobar".AsStream(), "foobar".AsEnumerable(), Enumerable.SequenceEqual);
            Combinator.Many(Chars.Any(), "foobar".Length)
                .SuccessTest("foobar".AsStream(), "foobar".AsEnumerable(), Enumerable.SequenceEqual);
            Combinator.Many(Chars.Any(), "foobar".Length + 1)
                .FailureTest("foobar".AsStream());

            Combinator.Many(Chars.Any(), 0, 0)
                .SuccessTest("foobar".AsStream(), Enumerable.Empty<Char>(), Enumerable.SequenceEqual);
            Combinator.Many(Chars.Any(), 0, 1)
                .SuccessTest("foobar".AsStream(), "f".AsEnumerable(), Enumerable.SequenceEqual);
            Combinator.Many(Chars.Any(), 0, 2)
                .SuccessTest("foobar".AsStream(), "fo".AsEnumerable(), Enumerable.SequenceEqual);

            Combinator.Many(Chars.Any(), 0, "foobar".Length)
                .SuccessTest("foobar".AsStream(), "foobar".AsEnumerable(), Enumerable.SequenceEqual);
            Combinator.Many(Chars.Any(), 0, "foobar".Length + 1)
                .SuccessTest("foobar".AsStream(), "foobar".AsEnumerable(), Enumerable.SequenceEqual);
        }

        [TestMethod]
        public void Combinator_MaybeTest()
        {
            Combinator.Maybe(Chars.Any())
                .SuccessTest("".AsStream(), Option.None<Char>());

            Combinator.Maybe(Chars.Any())
                .SuccessTest("foobar".AsStream(), Option.Just('f'));
            Combinator.Maybe(Prims.Fail<Char,Char>())
                .SuccessTest("foobar".AsStream(), Option.None<Char>());
            Combinator.Maybe(Prims.Error<Char, Char>())
                .ErrorTest("xyzzy".AsStream());

        }
    }
}
