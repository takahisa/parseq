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

namespace Test.Parseq.Combinators
{
    [TestFixture]
    public class PrimsTest
    {
        [TestCase]
        public void EndOfInputTest()
        {
            Prims.EndOfInput<Char>()
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, errorMessage) => { /* OK */ },
                    success: (restStream, value) => Assert.Fail());

            Prims.EndOfInput<Char>()
                .Run("".AsStream())
                .Case(
                    failure: (restStream, errorMessage) => Assert.Fail(),
                    success: (restStream, value) => { /* OK */ });
        }

        [TestCase]
        public void AnyTest()
        {
            Prims.Any<Char>()
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, errorMessage) => Assert.Fail(),
                    success: (restStream, value) =>
                        {
                            Assert.AreEqual('i', value);
                            Assert.True(restStream.Current.HasValue);
                            Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                            Assert.AreEqual(2, restStream.Current.Value.Item1.Column);
                        });

            Prims.Any<Char>()
                .Run("".AsStream())
                .Case(
                    failure: (restStream, errorMessage) => { /* OK */ },
                    success: (restStream, value) => Assert.Fail());
        }

        [TestCase]
        public void SatisfyTest()
        {
            Prims.Satisfy<Char>('i')
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, errorMessage) => Assert.Fail(),
                    success: (restStream, value) =>
                    {
                        Assert.AreEqual('i', value);
                        Assert.True(restStream.Current.HasValue);
                        Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                        Assert.AreEqual(2, restStream.Current.Value.Item1.Column);
                    });

            Prims.Satisfy<Char>('i')
                .Run("".AsStream())
                .Case(
                    failure: (restStream, errorMessage) => { /* OK */ },
                    success: (restStream, value) => Assert.Fail());

            Prims.Satisfy<Char>(c => c == 'i')
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, errorMessage) => Assert.Fail(),
                    success: (restStream, value) =>
                    {
                        Assert.AreEqual('i', value);
                        Assert.True(restStream.Current.HasValue);
                        Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                        Assert.AreEqual(2, restStream.Current.Value.Item1.Column);
                    });

            Prims.Satisfy<Char>(c => c == 'i')
                .Run("".AsStream())
                .Case(
                    failure: (restStream, errorMessage) => { /* OK */ },
                    success: (restStream, value) => Assert.Fail());
        }

        [TestCase]
        public void OneOfTest()
        {
            Chars.OneOf()
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => { /* OK */ },
                    success: (restStream, _) => Assert.Fail());

            Chars.OneOf('a', 'b', 'c')
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => { /* OK */ },
                    success: (restStream, _) => Assert.Fail());

            Chars.OneOf('a', 'b', 'i', 'c')
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                        {
                            Assert.AreEqual('i', value);
                            Assert.True(restStream.Current.HasValue);
                            Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                            Assert.AreEqual(2, restStream.Current.Value.Item1.Column);
                        });

            Chars.OneOf(new [] { 'a', 'b', 'i', 'c' })
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                    {
                        Assert.AreEqual('i', value);
                        Assert.True(restStream.Current.HasValue);
                        Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                        Assert.AreEqual(2, restStream.Current.Value.Item1.Column);
                    });
        }

        [TestCase]
        public void NoneOfTest()
        {
            Chars.NoneOf()
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) => 
                        {
                            Assert.AreEqual('i', value);
                            Assert.True(restStream.Current.HasValue);
                            Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                            Assert.AreEqual(2, restStream.Current.Value.Item1.Column);
                        });

            Chars.NoneOf('a', 'b', 'c')
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) => 
                        {
                            Assert.AreEqual('i', value);
                            Assert.True(restStream.Current.HasValue);
                            Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                            Assert.AreEqual(2, restStream.Current.Value.Item1.Column);
                        });

            Chars.NoneOf('a', 'b', 'i', 'c')
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => { /* OK */ },
                    success: (restStream, value) => Assert.Fail());

            Chars.NoneOf(new[] { 'a', 'b', 'i', 'c' })
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => { /* OK */ },
                    success: (restStream, value) => Assert.Fail());
        }

        [TestCase]
        public void UnitTest()
        {
            Prims.Unit<Char>()
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, errorMessage) =>
                        Assert.Fail(),
                    success: (restStream, value) =>
                        {
                            Assert.AreEqual(Unit.Instance, value);
                            Assert.True(restStream.Current.HasValue);
                            Assert.AreEqual('i', restStream.Current.Value.Item0);
                            Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                            Assert.AreEqual(1, restStream.Current.Value.Item1.Column);
                        });
        }
        
        [TestCase]
        public void EmptyTest()
        {
            Prims.Empty<Char, Char>()
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, errorMessage) =>
                        Assert.Fail(),
                    success: (restStream, value) =>
                        {
                            Assert.True(Enumerable.Equals(value, Enumerable.Empty<Char>()));
                            Assert.True(restStream.Current.HasValue);
                            Assert.AreEqual('i', restStream.Current.Value.Item0);
                            Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                            Assert.AreEqual(1, restStream.Current.Value.Item1.Column);
                        });
        }

        [TestCase]
        public void AppendTest()
        {
            Prims.Empty<Char, Char>()
                .Append(Chars.Char('f'))
                .Append(Chars.Char('o'))
                .Append(Chars.Char('o'))
                .Run("foo".AsStream())
                .Case(
                    failure: (restStream, errorMessage) =>
                        Assert.Fail(),
                    success: (restStream, value) =>
                        {
                            Assert.True(Enumerable.SequenceEqual(value, "foo"));
                            Assert.False(restStream.Current.HasValue);
                        });

            Prims.Empty<Char, Char>()
                .Append(Chars.Char('f'))
                .Append(Chars.Char('o').Optional())
                .Append(Chars.Char('o'))
                .Run("foo".AsStream())
                .Case(
                    failure: (restStream, errorMessage) =>
                        Assert.Fail(),
                    success: (restStream, value) =>
                    {
                        Assert.True(Enumerable.SequenceEqual(value, "foo"));
                        Assert.False(restStream.Current.HasValue);
                    });

            Prims.Empty<Char, Char>()
                .Append(Chars.Char('f'))
                .Append(Chars.Char('X').Optional())
                .Append(Chars.Char('o'))
                .Run("foo".AsStream())
                .Case(
                    failure: (restStream, errorMessage) =>
                        Assert.Fail(),
                    success: (restStream, value) =>
                    {
                        Assert.True(Enumerable.SequenceEqual(value, "fo"));
                        Assert.True(restStream.Current.HasValue);
                        Assert.AreEqual('o', restStream.Current.Value.Item0);
                    });

            Prims.Empty<Char, Char>()
                .Append(Chars.Char('f'))
                .Append(Chars.Char('X'))
                .Append(Chars.Char('o'))
                .Run("foo".AsStream())
                .Case(
                    failure: (restStream, errorMessage) =>
                        {
                            Assert.True(restStream.Current.HasValue);
                            Assert.AreEqual('o', restStream.Current.Value.Item0);
                            Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                            Assert.AreEqual(2, restStream.Current.Value.Item1.Column);
                        },
                    success: (restStream, value) => Assert.Fail());

            Prims.Empty<Char, Char>()
                .Append(Chars.Char('f'))
                .Append(Chars.Char('o').Optional())
                .Append(Chars.Char('o'))
                .Append(Prims.Empty<Char, Char>()
                    .Append(Chars.Char('b'))
                    .Append(Chars.Char('a'))
                    .Append(Chars.Char('r')))
                .Run("foobar".AsStream())
                .Case(
                    failure: (restStream, errorMessage) =>
                        Assert.Fail(),
                    success: (restStream, value) =>
                    {
                        Assert.True(Enumerable.SequenceEqual(value, "foobar"));
                        Assert.False(restStream.Current.HasValue);
                    });

            Prims.Empty<Char, Char>()
                .Append(Chars.Char('f'))
                .Append(Chars.Char('o').Optional())
                .Append(Chars.Char('o'))
                .Append(Prims.Empty<Char, Char>()
                    .Append(Chars.Char('b'))
                    .Append(Chars.Char('X'))
                    .Append(Chars.Char('r'))
                    .Optional())
                .Run("foobar".AsStream())
                .Case(
                    failure: (restStream, errorMessage) =>
                        Assert.Fail(),
                    success: (restStream, value) =>
                    {
                        Assert.True(Enumerable.SequenceEqual(value, "foo"));
                        Assert.True(restStream.Current.HasValue);
                        Assert.AreEqual('b', restStream.Current.Value.Item0);
                        Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                        Assert.AreEqual(4, restStream.Current.Value.Item1.Column);
                    });
        }
    }
}