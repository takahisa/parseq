using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Parseq;
using Parseq.Combinators;

namespace Parseq.Test
{
    public static class Testing
    {
        public static void SuccessTest<TToken, TResult>(
            this Parser<TToken, TResult> parser,
            Stream<TToken> stream,
            TResult expected,
            Func<TResult, TResult, Boolean> predicate)
        {
            Assert.IsTrue(Flows.IsSuccess(parser.DoWhenSuccess(actual => Assert.IsTrue(predicate(actual, expected))), stream));
        }

        public static void SuccessTest<TToken, TResult>(
            this Parser<TToken, TResult> parser,
            Stream<TToken> stream,
            TResult expected)
        {
            Testing.SuccessTest(parser, stream, expected, EqualityComparer<TResult>.Default.Equals);
        }

        public static void FailureTest<TToken, TResult>(
            this Parser<TToken, TResult> parser,
            Stream<TToken> stream)
        {
            Assert.IsTrue(Flows.IsFailure(parser, stream));
        }

        public static void ErrorTest<TToken, TResult>(
            this Parser<TToken, TResult> parser,
            Stream<TToken> stream)
        {
            Assert.IsTrue(Flows.IsError(parser, stream));
        }
    }
}
