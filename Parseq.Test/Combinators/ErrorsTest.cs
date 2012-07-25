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
    public class ErrorsTest
    {
        [TestMethod]
        public void Errors_ResuceTest()
        {
            Errors.Rescue(Chars.Any())
                .SuccessTest("foobar".AsStream(), Option.Just<Char>("foobar".First()));
            Errors.Rescue(Prims.Fail<Char, Char>())
                .FailureTest("foobar".AsStream());

            Errors.Rescue(Prims.Error<Char, Char>())
                .SuccessTest("foobar".AsStream(), Option.None<Char>());
        }

        [TestMethod]
        public void Errors_ErrorTest()
        {
            Errors.Error<Char, Char>("Error Message")
                .ErrorTest("foobar".AsStream());
        }

        [TestMethod]
        public void Errors_WarnTest()
        {
            Errors.Warn<Char, Char>("Error Message")
                .ErrorTest("foobar".AsStream());
        }

        [TestMethod]
        public void Errors_MessageTest()
        {
            Errors.Message<Char, Char>("Error Message")
                .ErrorTest("foobar".AsStream());
        }

        [TestMethod]
        public void Errors_ErrorWhenSuccessTest()
        {
            Errors.ErrorWhenSuccess(Chars.Any(), "Error")
                .ErrorTest("foobar".AsStream());

            Errors.ErrorWhenSuccess(Prims.Fail<Char,Char>(), "Error")
                .FailureTest("foobar".AsStream());

            Errors.ErrorWhenSuccess(Prims.Error<Char, Char>(), "Error")
                .ErrorTest("foobar".AsStream());
        }

        [TestMethod]
        public void Errors_ErrorWhenFailureTest()
        {
            Errors.ErrorWhenFailure(Chars.Any(), "Error")
                .SuccessTest("foobar".AsStream(), "foobar".First());

            Errors.ErrorWhenFailure(Prims.Fail<Char, Char>(), "Error")
                .ErrorTest("foobar".AsStream());

            Errors.ErrorWhenFailure(Prims.Error<Char, Char>(), "Error")
                .ErrorTest("foobar".AsStream());
        }

        [TestMethod]
        public void Errors_ErrorWhenErrorTest()
        {
            Errors.ErrorWhenError(Chars.Any(), "Error")
                .SuccessTest("foobar".AsStream(), "foobar".First());

            Errors.ErrorWhenError(Prims.Fail<Char, Char>(), "Error")
                .FailureTest("foobar".AsStream());

            Errors.ErrorWhenError(Prims.Error<Char, Char>(), "Error")
                .ErrorTest("foobar".AsStream());
        }

        [TestMethod]
        public void Errors_WarnWhenSuccessTest()
        {
            Errors.WarnWhenSuccess(Chars.Any(), "Error")
                .ErrorTest("foobar".AsStream());

            Errors.WarnWhenSuccess(Prims.Fail<Char, Char>(), "Error")
                .FailureTest("foobar".AsStream());

            Errors.WarnWhenSuccess(Prims.Error<Char, Char>(), "Error")
                .ErrorTest("foobar".AsStream());
        }

        [TestMethod]
        public void Errors_WarnWhenFailureTest()
        {
            Errors.WarnWhenFailure(Chars.Any(), "Error")
                .SuccessTest("foobar".AsStream(), "foobar".First());

            Errors.WarnWhenFailure(Prims.Fail<Char, Char>(), "Error")
                .ErrorTest("foobar".AsStream());

            Errors.WarnWhenFailure(Prims.Error<Char, Char>(), "Error")
                .ErrorTest("foobar".AsStream());
        }

        [TestMethod]
        public void Errors_WarnWhenErrorTest()
        {
            Errors.WarnWhenError(Chars.Any(), "Error")
                .SuccessTest("foobar".AsStream(), "foobar".First());

            Errors.WarnWhenError(Prims.Fail<Char, Char>(), "Error")
                .FailureTest("foobar".AsStream());

            Errors.WarnWhenError(Prims.Error<Char, Char>(), "Error")
                .ErrorTest("foobar".AsStream());
        }

        [TestMethod]
        public void Errors_MessageWhenSuccessTest()
        {
            Errors.MessageWhenSuccess(Chars.Any(), "Error")
                .ErrorTest("foobar".AsStream());

            Errors.MessageWhenSuccess(Prims.Fail<Char, Char>(), "Error")
                .FailureTest("foobar".AsStream());

            Errors.MessageWhenSuccess(Prims.Error<Char, Char>(), "Error")
                .ErrorTest("foobar".AsStream());
        }

        [TestMethod]
        public void Errors_MessageWhenFailureTest()
        {
            Errors.MessageWhenFailure(Chars.Any(), "Error")
                .SuccessTest("foobar".AsStream(), "foobar".First());

            Errors.MessageWhenFailure(Prims.Fail<Char, Char>(), "Error")
                .ErrorTest("foobar".AsStream());

            Errors.MessageWhenFailure(Prims.Error<Char, Char>(), "Error")
                .ErrorTest("foobar".AsStream());
        }

        [TestMethod]
        public void Errors_MessageWhenErrorTest()
        {
            Errors.MessageWhenError(Chars.Any(), "Error")
                .SuccessTest("foobar".AsStream(), "foobar".First());

            Errors.MessageWhenError(Prims.Fail<Char, Char>(), "Error")
                .FailureTest("foobar".AsStream());

            Errors.MessageWhenError(Prims.Error<Char, Char>(), "Error")
                .ErrorTest("foobar".AsStream());
        }

        [TestMethod]
        public void Errors_FollowedByTest()
        {
            Errors.FollowedBy(Chars.Any())
                .SuccessTest("foobar".AsStream(), Unit.Instance);

            Errors.FollowedBy(Prims.Fail<Char, Char>())
                .ErrorTest("foobar".AsStream());
            Errors.FollowedBy(Prims.Error<Char, Char>())
                .ErrorTest("foobar".AsStream());
        }

        [TestMethod]
        public void Errors_NotFollowedByTest()
        {
            Errors.NotFollowedBy(Chars.Any())
                .ErrorTest("foobar".AsStream());

            Errors.NotFollowedBy(Prims.Fail<Char, Char>())
                .SuccessTest("foobar".AsStream(), Unit.Instance);
            Errors.NotFollowedBy(Prims.Error<Char, Char>())
                .ErrorTest("foobar".AsStream());
        }
    }
}
