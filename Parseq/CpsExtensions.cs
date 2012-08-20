using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq
{
    public static class Cps
    {
        public static Cps<TResult, T> Return<TResult, T>(T value)
        {
            return k => k(value);
        }

        public static Cps<TResult, U> CallCC<TResult, T, U>(Func<Func<U, Cps<TResult, T>>, Cps<TResult, U>> selector)
        {
            if (selector == null)
                throw new ArgumentNullException("f");

            return k => selector(x => _ => k(x))(k);
        }

        public static Cps<TResult, U> For<TResult, T, U>(this IEnumerable<T> enumerable, Func<T, Cps<TResult, U>> selector)
        {
            return enumerable.Case(
                () => { throw new InvalidOperationException(); },
                (head, tail) => enumerable.Foldl(selector(head), (x, y) => x.SelectMany(_ => selector(y))));
        }

        public static Unit ForEach<T>(this IEnumerable<T> enumerable,
            Func<T, Cps<Unit, T>, Cps<Unit, T>, Cps<Unit, Unit>> f)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");
            if (f == null)
                throw new ArgumentNullException("f");

            Func<Unit, Unit> id = _ => _;

            return Cps.CallCC<Unit, T, Unit>(@break =>
                Cps.For(enumerable, i =>
                    Cps.CallCC<Unit, T, Unit>(@continue =>
                        f(i, @break(Unit.Instance), @continue(Unit.Instance)))))
                .Run(id);
        }
    }

    public static class CpsExtensions
    {
        public static TResult Run<TResult, T>(this Cps<TResult, T> cont, Func<T, TResult> f)
        {
            if (cont == null)
                throw new ArgumentNullException("cont");
            if (f == null)
                throw new ArgumentNullException("f");

            return cont(f);
        }

        public static Cps<TResult, U> Select<TResult, T, U>(
            this Cps<TResult, T> cont,
            Func<T, U> selector)
        {
            if (cont == null)
                throw new ArgumentNullException("cont");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return k => cont(t => k(selector(t)));
        }

        public static Cps<TResult, U> SelectMany<TResult, T, U>(
            this Cps<TResult, T> cont,
            Func<T, Cps<TResult, U>> selector)
        {
            if (cont == null)
                throw new ArgumentNullException("cont");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return k => cont(t => selector(t)(k));
        }

        public static Cps<TResult, V> SelectMany<TResult, T, U, V>(
            this Cps<TResult, T> cont,
            Func<T, Cps<TResult, U>> selector,
            Func<T, U, V> projector)
        {
            if (cont == null)
                throw new ArgumentNullException("cont");
            if (selector == null)
                throw new ArgumentNullException("selector");
            if (projector == null)
                throw new ArgumentNullException("projector");

            return cont.SelectMany(x => selector(x).Select(y => projector(x, y)));
        }
    }
}
