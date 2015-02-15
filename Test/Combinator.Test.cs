/*
 * Copyright (C) 2012 - 2015 Takahisa Watanabe <linerlock@outlook.com> All rights reserved.
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
using NUnit.Framework;
using Parseq;
using Parseq.Combinators;

namespace Test
{
    [TestFixture]
    public class CombinatorTest
    {
        [TestCase]
        public void SequenceTest()
        {
            Combinator.Sequence<Char, Char>(Enumerable.Empty<Parser<Char, Char>>())
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, _) => { /* OK */ });

            Combinator.Sequence<Char, Char>(
                new[] {
                    Chars.Satisfy('i'),
                    Chars.Satisfy('n'),
                    Chars.Satisfy('p'),
                    Chars.Satisfy('u'),
                    Chars.Satisfy('t')
                })
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                        {
                            Assert.True(Enumerable.SequenceEqual(value, "input"));
                            Assert.True(restStream.Current.HasValue);
                            Assert.AreEqual('S', restStream.Current.Value.Item0);
                            Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                            Assert.AreEqual(6, restStream.Current.Value.Item1.Column);
                        });

            Combinator.Sequence<Char, Char>(
                new[] {
                    Chars.Satisfy('i'),
                    Chars.Satisfy('n'),
                    Chars.Satisfy('X'),
                    Chars.Satisfy('u'),
                    Chars.Satisfy('t')
                })
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) =>
                    {
                        Assert.True(restStream.Current.HasValue);
                        Assert.AreEqual('p', restStream.Current.Value.Item0);
                        Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                        Assert.AreEqual(3, restStream.Current.Value.Item1.Column);
                    },
                    success: (restStream, value) =>
                        Assert.Fail());
        }

        [TestCase]
        public void ChoiceTest()
        {
            Combinator.Choice<Char, Char>(Enumerable.Empty<Parser<Char, Char>>())
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => { /* OK */ },
                    success: (restStream, _) => Assert.Fail());

            Combinator.Choice<Char, Char>(new[] { 
                Parser.Fail<Char, Char>("Failure"),
                Parser.Fail<Char, Char>("Failure")
            })
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => { /* OK */ },
                    success: (restStream, _) => Assert.Fail());

            Combinator.Choice<Char, Char>(new[] { 
                Chars.Satisfy('i'),
                Parser.Fail<Char, Char>("Failure")
            })
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                    {
                        Assert.AreEqual('i', value);
                        Assert.True(restStream.Current.HasValue);
                        Assert.AreEqual('n', restStream.Current.Value.Item0);
                        Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                        Assert.AreEqual(2, restStream.Current.Value.Item1.Column);
                    });

            Combinator.Choice<Char, Char>(new[] { 
                Parser.Fail<Char, Char>("Failure"),
                Chars.Satisfy('i')
            })
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                    {
                        Assert.AreEqual('i', value);
                        Assert.True(restStream.Current.HasValue);
                        Assert.AreEqual('n', restStream.Current.Value.Item0);
                        Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                        Assert.AreEqual(2, restStream.Current.Value.Item1.Column);
                    });

            Combinator.Choice<Char, Char>(new[] { 
                Parser.Return<Char, Char>('a'),
                Parser.Return<Char, Char>('b'),
            })
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                    {
                        Assert.AreEqual('a', value);
                    });
        }

        [TestCase]
        public void RepeatTest()
        {
            Combinator.Repeat(Parser.Fail<Char, Char>("Failure"), 0)
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, _) => { /* OK */});

            Combinator.Repeat(Chars.Any(), 5)
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                        {
                            Assert.True(Enumerable.SequenceEqual(value, "input"));
                            Assert.True(restStream.Current.HasValue);
                            Assert.AreEqual('S', restStream.Current.Value.Item0);
                            Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                            Assert.AreEqual(6, restStream.Current.Value.Item1.Column);
                        });

            Combinator.Repeat(Chars.Any(), 12)
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) =>
                        {
                            Assert.False(restStream.Current.HasValue);
                        },
                    success: (restStream, value) => Assert.Fail());
        }

        [TestCase]
        public void Many0Test()
        {
            Combinator.Many0(Parser.Fail<Char, Char>("Failure"))
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                        {
                            Assert.False(value.Any());
                        });

            Combinator.Many0(Chars.Any())
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                    {
                        Assert.True(Enumerable.SequenceEqual(value, "inputString"));
                    });
        }

        [TestCase]
        public void Many1Test()
        {
            Combinator.Many1(Parser.Fail<Char, Char>("Failure"))
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => { /* OK */ },
                    success: (restStream, _) => Assert.Fail());

            Combinator.Many1(Chars.Any())
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                    {
                        Assert.True(Enumerable.SequenceEqual(value, "inputString"));
                    });
        }

        [TestCase]
        public void ManyTillTest()
        {
            Combinator.ManyTill(
                Parser.Fail<Char, Char>("Failure"),
                Parser.Fail<Char, Unit>("Failure"))
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => { /* OK */ },
                    success: (restStream, _) => Assert.Fail());

            Combinator.ManyTill(
                    Parser.Fail<Char, Char>("Failure"),
                    Parser.Return<Char, Unit>(Unit.Instance))
                    .Run("inputString".AsStream())
                    .Case(
                        failure: (restStream, _) => Assert.Fail(),
                        success: (restStream, value) =>
                            {
                                Assert.False(value.Any());
                            });

            /*
             * line comment parser example:
             */
            var lineCommentBegin = Chars.Sequence("//");
            var lineCommentEnd = Chars.EndOfInput().Or(Chars.Char('\n').Ignore());
            var lineComment = lineCommentBegin
                .Bindr(Combinator.ManyTill(Chars.Any(), lineCommentEnd))
                .Ignore();

            var foobar = Chars.Sequence("foobar");

            lineComment.Bindr(foobar)
                .Run("//comment comment comment\nfoobar".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                        {
                            Assert.True(Enumerable.SequenceEqual(value, "foobar"));
                        });
        }

        [TestCase]
        public void SepBy0Test()
        {
            Combinator.SepBy0(
                Parser.Fail<Char, Char>("Failure"),
                Parser.Fail<Char, Unit>("Failure"))
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                        {
                            Assert.False(value.Any());
                        });

            Combinator.SepBy0(
                Chars.Any(),
                Chars.Char(',').Ignore())
                .Run("i,n,p,u,t,S,t,r,i,n,g".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                    {
                        Assert.True(Enumerable.SequenceEqual(value, "inputString"));
                        Assert.False(restStream.Current.HasValue);
                    });

            Combinator.SepBy0(
                Chars.Any(),
                Chars.Char(',').Ignore())
                .Run("i,n,p,u,t,S,t,r,i,n,g,".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                    {
                        Assert.True(Enumerable.SequenceEqual(value, "inputString"));
                        Assert.True(restStream.Current.HasValue);
                        Assert.AreEqual(',', restStream.Current.Value.Item0);
                        Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                        Assert.AreEqual(22, restStream.Current.Value.Item1.Column);
                    });
        }

        [TestCase]
        public void SepBy1Test()
        {
            Combinator.SepBy1(
                Parser.Fail<Char, Char>("Failure"),
                Parser.Fail<Char, Unit>("Failure"))
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => { /* OK */ },
                    success: (restStream, _) => Assert.Fail());

            Combinator.SepBy1(
                Chars.Any(),
                Chars.Char(',').Ignore())
                .Run("i,n,p,u,t,S,t,r,i,n,g".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                    {
                        Assert.True(Enumerable.SequenceEqual(value, "inputString"));
                        Assert.False(restStream.Current.HasValue);
                    });

            Combinator.SepBy1(
                Chars.Any(),
                Chars.Char(',').Ignore())
                .Run("i,n,p,u,t,S,t,r,i,n,g,".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                    {
                        Assert.True(Enumerable.SequenceEqual(value, "inputString"));
                        Assert.True(restStream.Current.HasValue);
                        Assert.AreEqual(',', restStream.Current.Value.Item0);
                        Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                        Assert.AreEqual(22, restStream.Current.Value.Item1.Column);
                    });
        }

        [TestCase]
        public void EndBy0Test()
        {
            Combinator.EndBy0(
                Parser.Fail<Char, Char>("Failure"),
                Parser.Fail<Char, Unit>("Failure"))
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                    {
                        Assert.False(value.Any());
                    });

            Combinator.EndBy0(
                    Chars.Any(),
                    Chars.Char(',').Ignore())
                    .Run("i,n,p,u,t,S,t,r,i,n,g,".AsStream())
                    .Case(
                        failure: (restStream, _) => Assert.Fail(),
                        success: (restStream, value) =>
                        {
                            Assert.True(Enumerable.SequenceEqual(value, "inputString"));
                            Assert.False(restStream.Current.HasValue);
                        });

            Combinator.EndBy0(
                    Chars.Any(),
                    Chars.Char(',').Ignore())
                    .Run("i,n,p,u,t,S,t,r,i,n,g".AsStream())
                    .Case(
                        failure: (restStream, _) => Assert.Fail(),
                        success: (restStream, value) =>
                        {
                            Assert.True(Enumerable.SequenceEqual(value, "inputStrin"));
                            Assert.True(restStream.Current.HasValue);
                            Assert.AreEqual('g', restStream.Current.Value.Item0);
                            Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                            Assert.AreEqual(21, restStream.Current.Value.Item1.Column);
                        });
        }

        [TestCase]
        public void EndBy1Test()
        {
            Combinator.EndBy1(
                Parser.Fail<Char, Char>("Failure"),
                Parser.Fail<Char, Unit>("Failure"))
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => { /* OK */ },
                    success: (restStream, _) => Assert.Fail());

            Combinator.EndBy1(
                    Chars.Any(),
                    Chars.Char(',').Ignore())
                    .Run("i,n,p,u,t,S,t,r,i,n,g,".AsStream())
                    .Case(
                        failure: (restStream, _) => Assert.Fail(),
                        success: (restStream, value) =>
                        {
                            Assert.True(Enumerable.SequenceEqual(value, "inputString"));
                            Assert.False(restStream.Current.HasValue);
                        });

            Combinator.EndBy1(
                    Chars.Any(),
                    Chars.Char(',').Ignore())
                    .Run("i,n,p,u,t,S,t,r,i,n,g".AsStream())
                    .Case(
                        failure: (restStream, _) => Assert.Fail(),
                        success: (restStream, value) =>
                        {
                            Assert.True(Enumerable.SequenceEqual(value, "inputStrin"));
                            Assert.True(restStream.Current.HasValue);
                            Assert.AreEqual('g', restStream.Current.Value.Item0);
                            Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                            Assert.AreEqual(21, restStream.Current.Value.Item1.Column);
                        });
        }

        [TestCase]
        public void SepEndBy0Test()
        {
            Combinator.SepEndBy0(
                Parser.Fail<Char, Char>("Failure"),
                Parser.Fail<Char, Unit>("Failure"))
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                    {
                        Assert.False(value.Any());
                    });

            Combinator.SepEndBy0(
                Chars.Any(),
                Chars.Char(',').Ignore())
                .Run("i,n,p,u,t,S,t,r,i,n,g".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                    {
                        Assert.True(Enumerable.SequenceEqual(value, "inputString"));
                        Assert.False(restStream.Current.HasValue);
                    });

            Combinator.SepEndBy0(
                Chars.Any(),
                Chars.Char(',').Ignore())
                .Run("i,n,p,u,t,S,t,r,i,n,g,".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                    {
                        Assert.True(Enumerable.SequenceEqual(value, "inputString"));
                        Assert.False(restStream.Current.HasValue);
                    });
        }

        [TestCase]
        public void SepEndBy1Test()
        {
            Combinator.SepEndBy1(
                Parser.Fail<Char, Char>("Failure"),
                Parser.Fail<Char, Unit>("Failure"))
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => { /* OK */ },
                    success: (restStream, _) => Assert.Fail());

            Combinator.SepEndBy1(
                Chars.Any(),
                Chars.Char(',').Ignore())
                .Run("i,n,p,u,t,S,t,r,i,n,g".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                    {
                        Assert.True(Enumerable.SequenceEqual(value, "inputString"));
                        Assert.False(restStream.Current.HasValue);
                    });

            Combinator.SepEndBy1(
                Chars.Any(),
                Chars.Char(',').Ignore())
                .Run("i,n,p,u,t,S,t,r,i,n,g,".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                    {
                        Assert.True(Enumerable.SequenceEqual(value, "inputString"));
                        Assert.False(restStream.Current.HasValue);
                    });
        }

        [TestCase]
        public void NotFollowedByTest()
        {
            Combinator.NotFollowedBy(Chars.EndOfInput())
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, _) => { /* OK */ });

            Combinator.NotFollowedBy(Chars.EndOfInput())
                .Run("".AsStream())
                .Case(
                    failure: (restStream, _) => { /* OK */ },
                    success: (restStream, _) => Assert.Fail());
        }

        [TestCase]
        public void ChainlTest()
        {
            var number = Chars.Digit().Many1()
                .Map(value => new String(value.ToArray()));
            var opAdd = Chars.Char('+')
                .Map(_ => new Func<String, String, String>((lhs, rhs) => String.Format("({0}+{1})", lhs, rhs)));
            var opSub = Chars.Char('-')
                .Map(_ => new Func<String, String, String>((lhs, rhs) => String.Format("({0}-{1})", lhs, rhs)));


            var term = number;
            var expr = Combinator.Chainl(term, opAdd.Or(opSub)).Or(term);

            expr.Run("1".AsStream()).Case(
                failure: (restStream, _) => Assert.Fail(),
                success: (restStream, value) =>
                    {
                        Assert.True(Enumerable.SequenceEqual(value, "1"));
                    });

            expr.Run("1+2-3".AsStream()).Case(
                failure: (restStream, _) => Assert.Fail(),
                success: (restStream, value) =>
                    {
                        Assert.True(Enumerable.SequenceEqual(value, "((1+2)-3)"));
                    });
        }

        [TestCase]
        public void ChainrTest()
        {
            var number = Chars.Digit().Many1()
                .Map(value => new String(value.ToArray()));
            var opAdd = Chars.Char('+')
                .Map(_ => new Func<String, String, String>((lhs, rhs) => String.Format("({0}+{1})", lhs, rhs)));
            var opSub = Chars.Char('-')
                .Map(_ => new Func<String, String, String>((lhs, rhs) => String.Format("({0}-{1})", lhs, rhs)));

            var term = number;
            var expr = Combinator.Chainr(term, opAdd.Or(opSub)).Or(term);

            expr.Run("1".AsStream()).Case(
                failure: (restStream, _) => Assert.Fail(),
                success: (restStream, value) =>
                {
                    Assert.True(Enumerable.SequenceEqual(value, "1"));
                });

            expr.Run("1+2-3".AsStream()).Case(
                failure: (restStream, _) => Assert.Fail(),
                success: (restStream, value) =>
                {
                    Assert.True(Enumerable.SequenceEqual(value, "(1+(2-3))"));
                });
        }

        [TestCase]
        public void BetweenTest()
        {
            var lp = Chars.Char('(').Ignore();
            var rp = Chars.Char(')').Ignore();

            Combinator.Between(Chars.Any(), lp, rp)
                .Run("(a)".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) => Assert.AreEqual('a', value));
        }

        [TestCase]
        public void OrTest()
        {
            Combinator.Or(
                Parser.Fail<Char, Char>("Failure"),
                Parser.Fail<Char, Char>("Failure"))
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => { /* OK */ },
                    success: (restStream, _) => Assert.Fail());

            Combinator.Or(
                Parser.Return<Char, Char>('a'),
                Parser.Fail<Char, Char>("Failure"))
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                        {
                            Assert.AreEqual('a', value);
                        });

            Combinator.Or(
                Parser.Fail<Char, Char>("Failure"),
                Parser.Return<Char, Char>('a'))
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                    {
                        Assert.AreEqual('a', value);
                    });

            Combinator.Or(
                Parser.Return<Char, Char>('a'),
                Parser.Return<Char, Char>('b'))
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                    {
                        Assert.AreEqual('a', value);
                    });
        }

        [TestCase]
        public void OptionalTest()
        {
            Combinator.Optional(Chars.Any())
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, optionalValue) =>
                        {
                            Assert.True(optionalValue.HasValue);
                            Assert.AreEqual('i', optionalValue.Value);
                        });

            Combinator.Optional(Chars.Any())
                .Run("".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, optionalValue) =>
                    {
                        Assert.False(optionalValue.HasValue);
                    });

            Combinator.Optional(Combinator.Sequence("inpXtString".Select(Chars.Char)))
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, optionalValue) =>
                    {
                        Assert.False(optionalValue.HasValue);
                        Assert.True(restStream.Current.HasValue);
                        Assert.AreEqual('i', restStream.Current.Value.Item0);
                        Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                        Assert.AreEqual(1, restStream.Current.Value.Item1.Column);
                    });
        }

        [TestCase]
        public void LookaheadTest()
        {
            Combinator.Lookahead(Combinator.Sequence("inpXtString".Select(Chars.Char)))
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) =>
                        {
                            Assert.True(restStream.Current.HasValue);
                            Assert.AreEqual('i', restStream.Current.Value.Item0);
                            Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                            Assert.AreEqual(1, restStream.Current.Value.Item1.Column);
                        },
                    success: (restStream, value) => Assert.Fail());

            Combinator.Lookahead(Combinator.Sequence("inputString".Select(Chars.Char)))
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                    {
                        Assert.True(Enumerable.SequenceEqual(value, "inputString"));
                        Assert.True(restStream.Current.HasValue);
                        Assert.AreEqual('i', restStream.Current.Value.Item0);
                        Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                        Assert.AreEqual(1, restStream.Current.Value.Item1.Column);
                    });
        }

        [TestCase]
        public void AttemptTest()
        {
            Combinator.Attempt(Combinator.Sequence("inpXtString".Select(Chars.Char)))
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) =>
                        {
                            Assert.True(restStream.Current.HasValue);
                            Assert.AreEqual('i', restStream.Current.Value.Item0);
                            Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                            Assert.AreEqual(1, restStream.Current.Value.Item1.Column);
                        },
                    success: (restStream, value) => Assert.Fail());

            Combinator.Attempt(Combinator.Sequence("inputString".Select(Chars.Char)))
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                        {
                            Assert.True(Enumerable.SequenceEqual(value, "inputString"));
                            Assert.False(restStream.Current.HasValue);
                        });
        }

        [TestCase]
        public void AndTest()
        {
            Combinator.And(Combinator.Sequence("inpXtString".Select(Chars.Char)))
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) =>
                    {
                        Assert.True(restStream.Current.HasValue);
                        Assert.AreEqual('i', restStream.Current.Value.Item0);
                        Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                        Assert.AreEqual(1, restStream.Current.Value.Item1.Column);
                    },
                    success: (restStream, value) => Assert.Fail());

            Combinator.And(Combinator.Sequence("inputString".Select(Chars.Char)))
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                    {
                        Assert.True(restStream.Current.HasValue);
                        Assert.AreEqual('i', restStream.Current.Value.Item0);
                        Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                        Assert.AreEqual(1, restStream.Current.Value.Item1.Column);
                    });
        }

        [TestCase]
        public void NotTest()
        {
            Combinator.Not(Combinator.Sequence("inpXtString".Select(Chars.Char)))
                .Run("inputString".AsStream())
                .Case(
                    success: (restStream, _) =>
                    {
                        Assert.True(restStream.Current.HasValue);
                        Assert.AreEqual('i', restStream.Current.Value.Item0);
                        Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                        Assert.AreEqual(1, restStream.Current.Value.Item1.Column);
                    },
                    failure: (restStream, value) => Assert.Fail());

            Combinator.Not(Combinator.Sequence("inputString".Select(Chars.Char)))
                .Run("inputString".AsStream())
                .Case(
                    success: (restStream, _) => Assert.Fail(),
                    failure: (restStream, value) =>
                    {
                        Assert.True(restStream.Current.HasValue);
                        Assert.AreEqual('i', restStream.Current.Value.Item0);
                        Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                        Assert.AreEqual(1, restStream.Current.Value.Item1.Column);
                    });
        }

        [TestCase]
        public void LazyTest()
        {
            var throwException = new Func<Parser<Char, Char>>(() =>
                {
                    throw new InvalidOperationException();
                });

            Assert.Throws(typeof(InvalidOperationException), () =>
                Combinator.Or(
                    Chars.Any(),
                    throwException())
                    .Run("inputString".AsStream()));

            Combinator.Or(
                Chars.Any(),
                Combinator.Lazy(throwException))
                .Run("inputString".AsStream())
                .Case(
                failure: (restStream, _) => Assert.Fail(),
                success: (restStream, value) =>
                    {
                        Assert.AreEqual('i', value);
                        Assert.True(restStream.Current.HasValue);
                        Assert.AreEqual('n', restStream.Current.Value.Item0);
                        Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                        Assert.AreEqual(2, restStream.Current.Value.Item1.Column);
                    });
        }

        [TestCase]
        public void Pipe1Test()
        {
            Combinator.Pipe(
                Chars.Char('i'),
                Chars.Char('n'),
                (value0, value1) => new[] { value0, value1 })
                .Run("inputString".AsStream())
                .Case(
                failure: (restStream, _) => Assert.Fail(),
                success: (restStream, value) =>
                    {
                        Assert.True(Enumerable.SequenceEqual(value, "in"));
                        Assert.AreEqual('p', restStream.Current.Value.Item0);
                        Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                        Assert.AreEqual(3, restStream.Current.Value.Item1.Column);
                    });

            Combinator.Pipe(
                Chars.Char('i'),
                Parser.Fail<Char, Char>("Failure"),
                (value0, value1) => new[] { value0, value1 })
                .Run("inputString".AsStream())
                .Case(
                failure: (restStream, _) =>
                    {
                        Assert.True(restStream.Current.HasValue);
                        Assert.AreEqual('n', restStream.Current.Value.Item0);
                        Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                        Assert.AreEqual(2, restStream.Current.Value.Item1.Column);
                    },
                success: (restStream, value) => Assert.Fail());
        }

        [TestCase]
        public void Pipe2Test()
        {
            Combinator.Pipe(
                Chars.Char('i'),
                Chars.Char('n'),
                Chars.Char('p'),
                (value0, value1, value2) => new[] { value0, value1, value2 })
                .Run("inputString".AsStream())
                .Case(
                failure: (restStream, _) => Assert.Fail(),
                success: (restStream, value) =>
                {
                    Assert.True(Enumerable.SequenceEqual(value, "inp"));
                    Assert.AreEqual('u', restStream.Current.Value.Item0);
                    Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                    Assert.AreEqual(4, restStream.Current.Value.Item1.Column);
                });

            Combinator.Pipe(
                Chars.Char('i'),
                Chars.Char('n'),
                Parser.Fail<Char, Char>("Failure"),
                (value0, value1, value2) => new[] { value0, value1, value2 })
                .Run("inputString".AsStream())
                .Case(
                failure: (restStream, _) =>
                {
                    Assert.True(restStream.Current.HasValue);
                    Assert.AreEqual('p', restStream.Current.Value.Item0);
                    Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                    Assert.AreEqual(3, restStream.Current.Value.Item1.Column);
                },
                success: (restStream, value) => Assert.Fail());
        }

        [TestCase]
        public void Pipe3Test()
        {
            Combinator.Pipe(
                Chars.Char('i'),
                Chars.Char('n'),
                Chars.Char('p'),
                Chars.Char('u'),
                (value0, value1, value2, value3) => new[] { value0, value1, value2, value3 })
                .Run("inputString".AsStream())
                .Case(
                failure: (restStream, _) => Assert.Fail(),
                success: (restStream, value) =>
                {
                    Assert.True(Enumerable.SequenceEqual(value, "inpu"));
                    Assert.AreEqual('t', restStream.Current.Value.Item0);
                    Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                    Assert.AreEqual(5, restStream.Current.Value.Item1.Column);
                });

            Combinator.Pipe(
                Chars.Char('i'),
                Chars.Char('n'),
                Chars.Char('p'),
                Parser.Fail<Char, Char>("Failure"),
                (value0, value1, value2, value3) => new[] { value0, value1, value2, value3 })
                .Run("inputString".AsStream())
                .Case(
                failure: (restStream, _) =>
                {
                    Assert.True(restStream.Current.HasValue);
                    Assert.AreEqual('u', restStream.Current.Value.Item0);
                    Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                    Assert.AreEqual(4, restStream.Current.Value.Item1.Column);
                },
                success: (restStream, value) => Assert.Fail());
        }

        [TestCase]
        public void Pipe4Test()
        {
            Combinator.Pipe(
                Chars.Char('i'),
                Chars.Char('n'),
                Chars.Char('p'),
                Chars.Char('u'),
                Chars.Char('t'),
                (value0, value1, value2, value3, value4) => new[] { value0, value1, value2, value3, value4 })
                .Run("inputString".AsStream())
                .Case(
                failure: (restStream, _) => Assert.Fail(),
                success: (restStream, value) =>
                {
                    Assert.True(Enumerable.SequenceEqual(value, "input"));
                    Assert.AreEqual('S', restStream.Current.Value.Item0);
                    Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                    Assert.AreEqual(6, restStream.Current.Value.Item1.Column);
                });

            Combinator.Pipe(
                Chars.Char('i'),
                Chars.Char('n'),
                Chars.Char('p'),
                Chars.Char('u'),
                Parser.Fail<Char, Char>("Failure"),
                (value0, value1, value2, value3, value4) => new[] { value0, value1, value2, value3, value4 })
                .Run("inputString".AsStream())
                .Case(
                failure: (restStream, _) =>
                {
                    Assert.True(restStream.Current.HasValue);
                    Assert.AreEqual('t', restStream.Current.Value.Item0);
                    Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                    Assert.AreEqual(5, restStream.Current.Value.Item1.Column);
                },
                success: (restStream, value) => Assert.Fail());
        }

        [TestCase]
        public void Pipe5Test()
        {
            Combinator.Pipe(
                Chars.Char('i'),
                Chars.Char('n'),
                Chars.Char('p'),
                Chars.Char('u'),
                Chars.Char('t'),
                Chars.Char('S'),
                (value0, value1, value2, value3, value4, value5) => new[] { value0, value1, value2, value3, value4, value5 })
                .Run("inputString".AsStream())
                .Case(
                failure: (restStream, _) => Assert.Fail(),
                success: (restStream, value) =>
                {
                    Assert.True(Enumerable.SequenceEqual(value, "inputS"));
                    Assert.AreEqual('t', restStream.Current.Value.Item0);
                    Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                    Assert.AreEqual(7, restStream.Current.Value.Item1.Column);
                });

            Combinator.Pipe(
                Chars.Char('i'),
                Chars.Char('n'),
                Chars.Char('p'),
                Chars.Char('u'),
                Chars.Char('t'),
                Parser.Fail<Char, Char>("Failure"),
                (value0, value1, value2, value3, value4, value5) => new[] { value0, value1, value2, value3, value4, value5 })
                .Run("inputString".AsStream())
                .Case(
                failure: (restStream, _) =>
                {
                    Assert.True(restStream.Current.HasValue);
                    Assert.AreEqual('S', restStream.Current.Value.Item0);
                    Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                    Assert.AreEqual(6, restStream.Current.Value.Item1.Column);
                },
                success: (restStream, value) => Assert.Fail());
        }

        [TestCase]
        public void Pipe6Test()
        {
            Combinator.Pipe(
                Chars.Char('i'),
                Chars.Char('n'),
                Chars.Char('p'),
                Chars.Char('u'),
                Chars.Char('t'),
                Chars.Char('S'),
                Chars.Char('t'),
                (value0, value1, value2, value3, value4, value5, value6) => new[] { value0, value1, value2, value3, value4, value5, value6 })
                .Run("inputString".AsStream())
                .Case(
                failure: (restStream, _) => Assert.Fail(),
                success: (restStream, value) =>
                {
                    Assert.True(Enumerable.SequenceEqual(value, "inputSt"));
                    Assert.AreEqual('r', restStream.Current.Value.Item0);
                    Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                    Assert.AreEqual(8, restStream.Current.Value.Item1.Column);
                });

            Combinator.Pipe(
                Chars.Char('i'),
                Chars.Char('n'),
                Chars.Char('p'),
                Chars.Char('u'),
                Chars.Char('t'),
                Chars.Char('S'),
                Parser.Fail<Char, Char>("Failure"),
                (value0, value1, value2, value3, value4, value5, value6) => new[] { value0, value1, value2, value3, value4, value5, value6 })
                .Run("inputString".AsStream())
                .Case(
                failure: (restStream, _) =>
                {
                    Assert.True(restStream.Current.HasValue);
                    Assert.AreEqual('t', restStream.Current.Value.Item0);
                    Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                    Assert.AreEqual(7, restStream.Current.Value.Item1.Column);
                },
                success: (restStream, value) => Assert.Fail());
        }

        [TestCase]
        public void Pipe7Test()
        {
            Combinator.Pipe(
                Chars.Char('i'),
                Chars.Char('n'),
                Chars.Char('p'),
                Chars.Char('u'),
                Chars.Char('t'),
                Chars.Char('S'),
                Chars.Char('t'),
                Chars.Char('r'),
                (value0, value1, value2, value3, value4, value5, value6, value7) => new[] { value0, value1, value2, value3, value4, value5, value6, value7 })
                .Run("inputString".AsStream())
                .Case(
                failure: (restStream, _) => Assert.Fail(),
                success: (restStream, value) =>
                {
                    Assert.True(Enumerable.SequenceEqual(value, "inputStr"));
                    Assert.AreEqual('i', restStream.Current.Value.Item0);
                    Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                    Assert.AreEqual(9, restStream.Current.Value.Item1.Column);
                });

            Combinator.Pipe(
                Chars.Char('i'),
                Chars.Char('n'),
                Chars.Char('p'),
                Chars.Char('u'),
                Chars.Char('t'),
                Chars.Char('S'),
                Chars.Char('t'),
                Parser.Fail<Char, Char>("Failure"),
                (value0, value1, value2, value3, value4, value5, value6, value7) => new[] { value0, value1, value2, value3, value4, value5, value6, value7 })
                .Run("inputString".AsStream())
                .Case(
                failure: (restStream, _) =>
                {
                    Assert.True(restStream.Current.HasValue);
                    Assert.AreEqual('r', restStream.Current.Value.Item0);
                    Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                    Assert.AreEqual(8, restStream.Current.Value.Item1.Column);
                },
                success: (restStream, value) => Assert.Fail());
        }

        [TestCase]
        public void IgnoreTest()
        {
            Combinator.Ignore(Combinator.Sequence("inpXtString".Select(Chars.Char)))
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) =>
                    {
                        Assert.True(restStream.Current.HasValue);
                        Assert.AreEqual('u', restStream.Current.Value.Item0);
                        Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                        Assert.AreEqual(4, restStream.Current.Value.Item1.Column);
                    },
                    success: (restStream, value) => Assert.Fail());

            Combinator.Ignore(Combinator.Sequence("inputString".Select(Chars.Char)))
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                    {
                        Assert.False(restStream.Current.HasValue);
                    });
        }
    }
}
