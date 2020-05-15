/*
 * Copyright (C) 2012 - 2020 Takahisa Watanabe <takahisa.wt@gmail.com> All rights reserved.
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
    public class CharsTest
    {
        [TestCase]
        public void EndOfInputTest()
        {
            Chars.EndOfInput()
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, errorMessage) => { /* OK */ },
                    success: (restStream, value) => Assert.Fail());

            Chars.EndOfInput()
                .Run("".AsStream())
                .Case(
                    failure: (restStream, errorMessage) => Assert.Fail(),
                    success: (restStream, value) => { /* OK */ });
        }

        [TestCase]
        public void AnyTest()
        {
            Chars.Any()
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

            Chars.Any()
                .Run("".AsStream())
                .Case(
                    failure: (restStream, errorMessage) => { /* OK */ },
                    success: (restStream, value) => Assert.Fail());
        }

        [TestCase]
        public void CharTest()
        {
            Chars.Char('i')
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

            Chars.Char('i')
                .Run("".AsStream())
                .Case(
                    failure: (restStream, errorMessage) => { /* OK */ },
                    success: (restStream, value) => Assert.Fail());

            Chars.Char(c => c == 'i')
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

            Chars.Char(c => c == 'i')
                .Run("".AsStream())
                .Case(
                    failure: (restStream, errorMessage) => { /* OK */ },
                    success: (restStream, value) => Assert.Fail());
        }

        [TestCase]
        public void SatisfyTest()
        {
            Chars.Satisfy('i')
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

            Chars.Satisfy('i')
                .Run("".AsStream())
                .Case(
                    failure: (restStream, errorMessage) => { /* OK */ },
                    success: (restStream, value) => Assert.Fail());

            Chars.Satisfy(c => c == 'i')
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

            Chars.Satisfy(c => c == 'i')
                .Run("".AsStream())
                .Case(
                    failure: (restStream, errorMessage) => { /* OK */ },
                    success: (restStream, value) => Assert.Fail());
        }

        [TestCase]
        public void SequenceTest()
        {
            Chars.Sequence()
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, _) => { /* OK */ });

            Chars.Sequence("inputString")
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, _) => Assert.Fail(),
                    success: (restStream, value) =>
                        Assert.True(Enumerable.SequenceEqual(value, "inputString")));

            Chars.Sequence("inpXtString")
                .Run("inputString".AsStream())
                .Case(
                    failure: (restStream, errorMessage) => 
                        {
                            Assert.True(restStream.Current.HasValue);
                            Assert.AreEqual('i', restStream.Current.Value.Item0);
                            Assert.AreEqual(1, restStream.Current.Value.Item1.Line);
                            Assert.AreEqual(1, restStream.Current.Value.Item1.Column);
                        },
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

            Chars.OneOf(new[] { 'a', 'b', 'i', 'c' })
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
    }
}