using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq.Combinators
{
    public static class Errors {

        public static Parser<TToken, Unit> Fail<TToken>(string message){
            return stream => Reply.Error<TToken, Unit>(stream,
                new ErrorMessage(ErrorMessageType.Error,message,stream.Location,stream.Location));
        }

        public static Parser<TToken, Unit> Warn<TToken>(string message){
            return stream => Reply.Error<TToken, Unit>(stream,
                new ErrorMessage(ErrorMessageType.Warn, message, stream.Location, stream.Location));
        }

        public static Parser<TToken, Unit> Message<TToken>(string message){
            return stream => Reply.Error<TToken, Unit>(stream,
                new ErrorMessage(ErrorMessageType.Message, message, stream.Location, stream.Location));
        }

        public static Parser<TToken, TResult> Try<TToken, TResult>(
            this Parser<TToken, TResult> parser)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            return stream => {
                Reply<TToken,TResult> reply;
                ErrorMessage message; TResult result;
                switch ((reply = parser(stream)).TryGetValue(out result,out message)){
                    case ReplyStatus.Success:
                    case ReplyStatus.Failure:
                        return reply;
                    default:
                        return Reply.Failure<TToken, TResult>(stream);
                }
            };
        }

        public static Parser<TToken, TResult> Try<TToken, TResult>(
            this Parser<TToken, TResult> parser,Parser<TToken,TResult> catcher)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (catcher == null)
                throw new ArgumentNullException("catcher");
            return stream => {
                Reply<TToken, TResult> reply;
                ErrorMessage message; TResult result;
                switch ((reply = parser(stream)).TryGetValue(out result, out message)){
                    case ReplyStatus.Success:
                    case ReplyStatus.Failure:
                        return reply;
                    default:
                        return catcher(stream);
                }
            };
        }

        public static Parser<TToken, TResult> Assert<TToken,TResult>(
            this Parser<TToken, TResult> parser,string message)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");

            return stream => {
                Reply<TToken, TResult> reply;
                ErrorMessage error; TResult result;
                switch ((reply = parser(stream)).TryGetValue(out result,out error)){
                    case ReplyStatus.Success:
                    case ReplyStatus.Error:
                        return reply;
                    default:
                        return Reply.Error<TToken,TResult>(stream,
                            new ErrorMessage(ErrorMessageType.Error,message,reply.Stream.Location,reply.Stream.Location));
                }
            };
        }

        public static Parser<TToken, TResult> Assert<TToken, TResult>(
            this Parser<TToken, TResult> parser)
        {
            return Errors.Assert(parser, "assertion failed");
        }
    }
}
