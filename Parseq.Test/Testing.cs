using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Parseq;
using Parseq.Combinators;

namespace Parseq.Test
{
    public static class Testing
    {
        internal static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var i in enumerable)
                action(i);
        }

        internal static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, Int32> action)
        {
            var index = 0;
            foreach (var i in enumerable)
                action(i, index++);
        }

        public static Parser<TToken, IEnumerable<TResult>> ExpectSuccess<TToken, TResult>(this Parser<TToken, IEnumerable<TResult>> parser, IEnumerable<TResult> expected)
            where TToken : IEquatable<TToken>
        {
            if (parser == null)
                throw new ArgumentNullException("parser");

            return parser.DoWhenSuccess(actual => Assert.IsTrue(expected.SequenceEqual(actual)))
                .DoWhenFailure(() => Assert.Fail())
                .DoWhenError(error => Assert.Fail(error.MessageDetails));
        }

        public static Parser<TToken, TResult> ExpectSuccess<TToken, TResult>(this Parser<TToken, TResult> parser, TResult expected)
            where TToken : IEquatable<TToken>
        {
            if(parser == null)
                throw new ArgumentNullException("parser");

            return parser.DoWhenSuccess(actual => Assert.AreEqual(expected, actual))
                .DoWhenFailure(() => Assert.Fail())
                .DoWhenError(error => Assert.Fail(error.MessageDetails));                
        }

        public static Parser<TToken, TResult> ExpectSuccess<TToken, TResult>(this Parser<TToken, TResult> parser)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");

            return parser.DoWhenFailure(() => Assert.Fail())
                .DoWhenError(error => Assert.Fail(error.MessageDetails)); 
        }

        public static Parser<TToken, TResult> ExpectFailure<TToken, TResult>(this Parser<TToken, TResult> parser)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");

            return parser.DoWhenSuccess(actual => Assert.Fail())
                .DoWhenError(error => Assert.Fail(error.MessageDetails));
        }

        public static Parser<TToken, TResult> ExpectError<TToken, TResult>(this Parser<TToken, TResult> parser)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");

            return parser.DoWhenSuccess(actual => Assert.Fail())
                .DoWhenFailure(() => Assert.Fail());
        }

        public static Parser<TToken, TResult> ExpectTrue<TToken, TResult>(this Parser<TToken, TResult> parser, Func<TResult, Boolean> predicate)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");

            return parser.DoWhenSuccess(actual => Assert.IsTrue(predicate(actual)))
                .DoWhenFailure(() => Assert.Fail())
                .DoWhenError(error => Assert.Fail(error.MessageDetails));
        }

        public static Parser<TToken, TResult> ExpectFalse<TToken, TResult>(this Parser<TToken, TResult> parser, Func<TResult, Boolean> predicate)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");

            return parser.DoWhenSuccess(actual => Assert.IsFalse(predicate(actual)))
                .DoWhenFailure(() => Assert.Fail())
                .DoWhenError(error => Assert.Fail(error.MessageDetails));
        }
    }
}
