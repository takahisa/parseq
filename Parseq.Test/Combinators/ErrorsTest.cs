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
        public void RescueTest()
        {
            var alwaysSuccess = Prims.Return<Char, Unit>(Unit.Instance);
            var alwaysFailure = Prims.Fail<Char, Unit>();
            var alwaysError = Prims.Error<Char, Unit>();

            alwaysSuccess.Rescue().ExpectSuccess().Run("foobar".AsStream());
            alwaysFailure.Rescue().ExpectFailure().Run("foobar".AsStream());
            alwaysError.Rescue().ExpectFailure().Run("foobar".AsStream());

            var message = Errors.Message<Char, Unit>("message");
            var warn = Errors.Warn<Char, Unit>("warning");
            var error = Errors.Error<Char, Unit>("error");

            message.Rescue(ErrorMessageType.Message).ExpectFailure().Run("foobar".AsStream());
            message.Rescue(ErrorMessageType.Error).ExpectError().Run("foobar".AsStream());
            message.Rescue(ErrorMessageType.Warn).ExpectError().Run("foobar".AsStream());
            message.Rescue(ErrorMessageType.Error | ErrorMessageType.Warn).ExpectError().Run("foobar".AsStream());

            warn.Rescue(ErrorMessageType.Warn).ExpectFailure().Run("foobar".AsStream());
            warn.Rescue(ErrorMessageType.Message).ExpectError().Run("foobar".AsStream());
            warn.Rescue(ErrorMessageType.Error).ExpectError().Run("foobar".AsStream());
            warn.Rescue(ErrorMessageType.Error | ErrorMessageType.Message).ExpectError().Run("foobar".AsStream());

            error.Rescue(ErrorMessageType.Error).ExpectFailure().Run("foobar".AsStream());
            error.Rescue(ErrorMessageType.Message).ExpectError().Run("foobar".AsStream());
            error.Rescue(ErrorMessageType.Warn).ExpectError().Run("foobar".AsStream());
            error.Rescue(ErrorMessageType.Message | ErrorMessageType.Warn).ExpectError().Run("foobar".AsStream());
        }

    }
}
