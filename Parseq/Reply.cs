
using System;

namespace Parseq
{
    public interface IReply<out TToken, out T>
        : IEither<String, T>
    {
        TResult Case<TResult>(
            Func<ITokenStream<TToken>, String, TResult> failure,
            Func<ITokenStream<TToken>, T, TResult> success);
    }

    public partial class Reply
    {
        public static IReply<TToken, T> Success<TToken, T>(ITokenStream<TToken> restStream, T value)
        {
            return new Reply.SuccessImpl<TToken, T>(restStream, value);
        }

        public static IReply<TToken, T> Failure<TToken, T>(ITokenStream<TToken> restStream, String errorMessage)
        {
            return new Reply.FailureImpl<TToken, T>(restStream, errorMessage);
        }
    }

    public partial class Reply
    {
        class SuccessImpl<TToken, T>
            : IReply<TToken, T>
        {
            private readonly ITokenStream<TToken> restStream;
            private readonly T value;

            public SuccessImpl(ITokenStream<TToken> restStream, T value)
            {
                this.restStream = restStream;
                this.value = value;
            }

            public TResult Case<TResult>(
                Func<ITokenStream<TToken>, String, TResult> failure,
                Func<ITokenStream<TToken>, T, TResult> success)
            {
                return success(this.restStream, this.value);
            }

            TResult IEither<String, T>.Case<TResult>(
                Func<String, TResult> left,
                Func<T, TResult> right)
            {
                return right(this.value);
            }
        }

        class FailureImpl<TToken, T>
            : IReply<TToken, T>
        {
            private readonly ITokenStream<TToken> restStream;
            private readonly String errorMessage;

            public FailureImpl(ITokenStream<TToken> restStream, String errorMessage)
            {
                this.restStream = restStream;
                this.errorMessage = errorMessage;
            }

            public TResult Case<TResult>(
                Func<ITokenStream<TToken>, String, TResult> failure,
                Func<ITokenStream<TToken>, T, TResult> success)
            {
                return failure(this.restStream, this.errorMessage);
            }

            TResult IEither<String, T>.Case<TResult>(
                Func<String, TResult> left,
                Func<T, TResult> right)
            {
                return left(this.errorMessage);
            }
        }
    }
}