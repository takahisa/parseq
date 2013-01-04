﻿/*
 * Parseq - a monadic parser combinator library for C#
 *
 * Copyright (c) 2012 - 2013 WATANABE TAKAHISA <x.linerlock@gmail.com> All rights reserved.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq
{
    public static class ReplyExtensions
    {
        public static Boolean IsSuccess<TToken, TResult>(this Reply<TToken, TResult> reply)
        {
            if(reply == null)
                throw new ArgumentNullException("reply");
            
            TResult result; ErrorMessage message;
            return ReplyStatus.Success == reply.TryGetValue(out result, out message);
        }

        public static Boolean IsFailure<TToken, TResult>(this Reply<TToken, TResult> reply)
        {
            if (reply == null)
                throw new ArgumentNullException("reply");

            TResult result; ErrorMessage message;
            return ReplyStatus.Failure == reply.TryGetValue(out result, out message);
        }

        public static Boolean IsError<TToken, TResult>(this Reply<TToken, TResult> reply)
        {
            if (reply == null)
                throw new ArgumentNullException("reply");

            TResult result; ErrorMessage message;
            return ReplyStatus.Error == reply.TryGetValue(out result, out message);
        }

        public static Reply<TToken, T> Where<TToken, T>(this Reply<TToken, T> reply,
            Func<T, Boolean> predicate)
        {
            if (reply == null)
                throw new ArgumentNullException("reply");
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            Option<T> result; T value; ErrorMessage message;
            switch (reply.TryGetValue(out result, out message))
            {
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
            switch (reply.TryGetValue(out result, out message))
            {
                case ReplyStatus.Success:
                    return Reply.Success<TToken, U>(reply.Stream, selector(result));
                case ReplyStatus.Failure:
                    return Reply.Failure<TToken, U>(reply.Stream);
                default:
                    return Reply.Error<TToken, U>(reply.Stream, message);
            }
        }

        public static Reply<TToken, U> SelectMany<TToken, T, U>(this Reply<TToken, T> reply,
            Func<T, Reply<TToken, U>> selector)
        {
            if (reply == null)
                throw new ArgumentNullException("reply");
            if (selector == null)
                throw new ArgumentNullException("selector");

            T result; ErrorMessage message;
            switch (reply.TryGetValue(out result, out message))
            {
                case ReplyStatus.Success:
                    return selector(result);
                case ReplyStatus.Failure:
                    return Reply.Failure<TToken, U>(reply.Stream);
                default:
                    return Reply.Error<TToken, U>(reply.Stream, message);
            }
        }

        public static Reply<TToken, V> SelectMany<TToken, T, U, V>(this Reply<TToken, T> reply,
            Func<T, Reply<TToken, U>> selector, Func<T, U, V> projector)
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
