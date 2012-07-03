using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq
{
    public static class Continuations
    {
        public static Continuation<TResult, T> Return<TResult, T>(T value)
        {
            return k => k(value);
        }

        public static Continuation<TResult, T> CallCC<TResult, T, U>(
            Func<Func<T, Continuation<TResult, U>>,Continuation<TResult, T>> selector)
        {
            if (selector == null)
                throw new ArgumentNullException("selector");

            return k => selector(t => _ => k(t))(k);
        }

        public static Continuation<Unit, Unit> For<TResult, T>(
            this IEnumerable<T> enumerable,
            Func<T, Continuation<Unit, Unit>> func)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");
            if (func == null)
                throw new ArgumentNullException("func");

            return enumerable.Match(
                () => Unit.Instance.ToContinuation<Unit,Unit>(),
                (head, tail) => tail.Aggregate(func(head),
                    (x, y) => x.SelectMany(_ => func(y))));
        }

        public static Continuation<Unit,Unit> ForEach<TResult,T>(
            this IEnumerable<T> enumerable,
            Func<
                T,
                Continuation<Unit,Unit>,
                Continuation<Unit,Unit>,
                Continuation<Unit,Unit>> selector)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return Continuations.CallCC<Unit, Unit, Unit>(
                @break => Continuations.CallCC<Unit, Unit, Unit>(
                    @continue => enumerable.For<Unit,T>(
                        i => selector(i, @break(Unit.Instance), @continue(Unit.Instance)))));
        }

    }

    public static class ContinuationExtensions
    {
        public static Continuation<TResult, U> Select<TResult,T,U>(
            this Continuation<TResult, T> cont,
            Func<T, U> selector)
        {
            if (cont == null)
                throw new ArgumentNullException("cont");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return k => cont(t => k(selector(t)));
        }

        public static Continuation<TResult, U> SelectMany<TResult, T, U>(
            this Continuation<TResult, T> cont,
            Func<T, Continuation<TResult, U>> selector)
        {
            if (cont == null)
                throw new ArgumentNullException("cont");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return k => cont(t => selector(t)(k));
        }

        public static Continuation<TResult, V> SelectMany<TResult, T, U,V>(
            this Continuation<TResult, T> cont,
            Func<T, Continuation<TResult, U>> selector,
            Func<T,U,V> projector)
        {
            if (cont == null)
                throw new ArgumentNullException("cont");
            if (selector == null)
                throw new ArgumentNullException("selector");
            if (projector == null)
                throw new ArgumentNullException("projector");

            return cont.SelectMany(x => selector(x).Select(y => projector(x, y)));
        }

        public static Continuation<TResult, T> ToContinuation<TResult, T>(this T value)
        {
            return Continuations.Return<TResult, T>(value);
        }
    }
}
