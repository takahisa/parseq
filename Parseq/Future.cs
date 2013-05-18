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
    public static class Future
    {
        public static IFuture<T> Create<T>(Func<T> value)
        {
            return Future<T>.Create(value);
        }

        public static IFuture<T> Create<T>(T value)
        {
            return Future<T>.Create(value);
        }
    }

    public interface IFuture<out T>
        : IObservable<T>
    {
        Boolean IsCompleted { get; }

        T Apply();
        T Apply(Int32 timeout);

        T Perform();
    }

    public abstract partial class Future<T>
        : IFuture<T>
    {
        public abstract Boolean IsCompleted { get; }

        public abstract T Apply();
        public abstract T Apply(Int32 timeout);

        public abstract T Perform();
        public abstract Boolean TryGetValue(out T result);

        public abstract IDisposable Subscribe(IObserver<T> observer);
    }

    public abstract partial class Future<T>
    {
        public static IFuture<T> Create(Func<T> func)
        {
            return new FutureImpl(func);
        }

        public static IFuture<T> Create(T value)
        {
            return new ActualImpl(value);
        }

        private class ActualImpl
            : Future<T>
        {
            private readonly T _value;

            public ActualImpl(T value)
            {
                _value = value;
            }

            public override Boolean IsCompleted
            {
                get { return true; }
            }

            public override T Apply()
            {
                return _value;
            }

            public override T Apply(Int32 timeout)
            {
                return _value;
            }

            public override T Perform()
            {
                return _value;
            }

            public override Boolean TryGetValue(out T result)
            {
                result = _value;
                return true;
            }

            public override IDisposable Subscribe(IObserver<T> observer)
            {
                observer.OnNext(_value);
                return ObservableDisposer.Create(() => { });
            }
        }

        private class FutureImpl
            : Future<T>
        {
            private List<IObserver<T>> _observers;
            private AutoResetEvent _mediator;
            private IOption<T> _value;

            public FutureImpl(Func<T> func)
            {
                if (func == null)
                    throw new ArgumentNullException("func");

                _observers = new List<IObserver<T>>();
                _mediator = new AutoResetEvent(false);
                _value = Option.None<T>();
                ThreadPool.QueueUserWorkItem(_ => this.CalculateProc(func));
            }

            public override Boolean IsCompleted
            {
                get { return _value.Exists(); }
            }

            public override IDisposable Subscribe(IObserver<T> observer)
            {
                if (!_observers.Contains(observer))
                    _observers.Add(observer);
                return ObservableDisposer.Create(() => _observers.RemoveAll(_ => _ == observer));
            }

            public override T Apply()
            {
                T result;
                if (!this.IsCompleted)
                    _mediator.WaitOne();

                if (_value.TryGetValue(out result))
                    return result;
                else
                    throw new InvalidOperationException();
            }

            public override T Apply(Int32 timeout)
            {
                T result;
                if (!this.IsCompleted)
                    _mediator.WaitOne(timeout);
                if (_value.TryGetValue(out result))
                    return result;
                else
                    throw new TimeoutException();
            }

            public override T Perform()
            {
                T result;
                if (this.TryGetValue(out result))
                    return result;
                else
                    throw new InvalidOperationException();
            }

            public override Boolean TryGetValue(out T result)
            {
                if (this.IsCompleted)
                {
                    result = _value.Perform();
                    return true;
                }
                else
                {
                    result = default(T);
                    return false;
                }
            }

            private void CalculateProc(Func<T> f)
            {
                var result = f();
                lock (this)
                {
                    _value = Option.Just<T>(result);
                }
                _mediator.Set();
                _observers.ForEach(observer => observer.OnNext(result));
            }
        }

        public abstract partial class ObservableDisposer
        : IDisposable
        {
            public abstract void Dispose();
        }

        public abstract partial class ObservableDisposer
        {
            public static IDisposable Create(Action action)
            {
                return new ObservableDisposerImpl(action);
            }

            private class ObservableDisposerImpl
                : ObservableDisposer
            {
                private readonly Action _action;

                public ObservableDisposerImpl(Action action)
                {
                    if (action == null)
                        throw new ArgumentNullException("action");

                    _action = action;
                }

                public override void Dispose()
                {
                    _action();
                }
            }
        }
    }
}
