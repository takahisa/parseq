using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq
{
    public static class ReplyExtensions {

        public static Reply<TToken, T> Where<TToken, T>(this Reply<TToken, T> reply,
            Func<T, bool> predicate)
        {
            if (reply == null)
                throw new ArgumentNullException("reply");
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            Option<T> result; T value; ErrorMessage message;
            switch (reply.TryGetValue(out result,out message)){
                case Hand.Left:
                    return result.TryGetValue(out value) && predicate(value) ?
                        Reply.Success<TToken, T>(reply.Stream, value) :
                        Reply.Failure<TToken, T>(reply.Stream);
                default:
                    return Reply.Error<TToken, T>(reply.Stream, message);
            }
        }

        public static Reply<TToken, U> Select<TToken, T, U>(this Reply<TToken, T> reply,
            Func<T, U> selector)
        {
            if (reply == null)
                throw new ArgumentNullException("reply");
            if (selector == null)
                throw new ArgumentNullException("selector");

            T result; ErrorMessage message;
            switch (reply.TryGetValue(out result, out message)){
                case ReplyStatus.Success:
                    return Reply.Success<TToken, U>(reply.Stream, selector(result));
                case ReplyStatus.Failure:
                    return Reply.Failure<TToken, U>(reply.Stream);
                default:
                    return Reply.Error<TToken, U>(reply.Stream, message);
            }
        }

        public static Reply<TToken, U> SelectMany<TToken, T, U>(this Reply<TToken, T> reply,
            Func<T, Reply<TToken,U>> selector)
        {
            if (reply == null)
                throw new ArgumentNullException("reply");
            if (selector == null)
                throw new ArgumentNullException("selector");

            T result; ErrorMessage message;
            switch (reply.TryGetValue(out result, out message)){
                case ReplyStatus.Success:
                    return selector(result);
                case ReplyStatus.Failure:
                    return Reply.Failure<TToken, U>(reply.Stream);
                default:
                    return Reply.Error<TToken, U>(reply.Stream, message);
            }
        }

        public static Reply<TToken, V> SelectMany<TToken, T, U, V>(this Reply<TToken, T> reply,
            Func<T, Reply<TToken, U>> selector,Func<T, U, V> projector)
        {
            if (reply == null)
                throw new ArgumentNullException("reply");
            if (selector == null)
                throw new ArgumentNullException("selector");
            if (projector == null)
                throw new ArgumentNullException("projector");

            return reply.SelectMany(x => selector(x).Select(y => projector(x, y)));
        }
    }
}
