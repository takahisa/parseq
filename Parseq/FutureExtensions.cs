/*
 * Parseq - a monadic parser combinator library for C#
 *
 * Copyright (c) 2012 WATANABE TAKAHISA <x.linerlock@gmail.com> All rights reserved.
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
using System.Threading;

using Parseq;

namespace Parseq
{
    public static class FutureExtensions
    {
        public static Future<U> Select<T, U>(this Future<T> future, Func<T, U> selector)
        {
            if (future == null)
                throw new ArgumentNullException("future");

            return Future.Create(() => selector(future.Apply()));
        }

        public static Future<U> SelectMany<T, U>(this Future<T> future, Func<T, Future<U>> selector)
        {
            if (future == null)
                throw new ArgumentNullException("future");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return new FuturePipe<U>(() => selector(future.Apply()));
        }

        public static Future<V> SelectMany<T, U, V>(this Future<T> future, Func<T, Future<U>> selector, Func<T, U, V> projector)
        {
            if (future == null)
                throw new ArgumentNullException("future");
            if (selector == null)
                throw new ArgumentNullException("selector");
            if (projector == null)
                throw new ArgumentNullException("projector");

            return future.SelectMany(x => selector(x).Select(y => projector(x, y)));
        }

        private class FuturePipe<T>
            : Future<T>
        {
            private AutoResetEvent _mediator;
            private Option<Future<T>> _value;

            public FuturePipe(Func<Future<T>> func)
            {
                if (func == null)
                    throw new ArgumentNullException("func");

                _mediator = new AutoResetEvent(false);
                _value = Option.None<Future<T>>();
                ThreadPool.QueueUserWorkItem(state => this.CalculateProc(func));
            }

            public override IDisposable Subscribe(IObserver<T> observer)
            {
                if (!this.IsCompleted)
                    _mediator.WaitOne();

                Future<T> future;
                if (_value.TryGetValue(out future))
                    return future.Subscribe(observer);
                else
                    throw new InvalidOperationException();
            }

            public override Boolean IsCompleted
            {
                get { return _value.Exists(); }
            }

            public override T Apply()
            {
                if (!this.IsCompleted)
                    _mediator.WaitOne();

                Future<T> future;
                if (_value.TryGetValue(out future))
                    return future.Apply();
                else
                    throw new InvalidOperationException();
            }

            public override T Apply(Int32 timeout)
            {
                if (!this.IsCompleted)
                    _mediator.WaitOne(timeout);

                Future<T> future;
                if (_value.TryGetValue(out future))
                    return future.Apply();
                else
                    throw new InvalidOperationException();
            }

            public override T Perform()
            {
                if (this.IsCompleted)
                    return _value.Perform().Perform();
                else
                    throw new InvalidOperationException();
            }

            public override Boolean TryGetValue(out T result)
            {
                Future<T> future;
                if (_value.TryGetValue(out future))
                    return future.TryGetValue(out result);
                else
                {
                    result = default(T);
                    return false;
                }
            }

            private void CalculateProc(Func<Future<T>> func)
            {
                var result = func();
                lock (this)
                {
                    _value = result;
                }
                _mediator.Set();
            }
        }
    }
}
