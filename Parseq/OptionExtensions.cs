using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq
{
    public static class OptionExtensions
    {
        public static Boolean Exists<T>(this Option<T> option)
        {
            T value;
            return option.TryGetValue(out value);
        }

        public static T Otherwise<T>(this Option<T> option, Func<T> selector)
        {
            if (option == null)
                throw new ArgumentNullException("option");
            if (selector == null)
                throw new ArgumentNullException("selector");

            T value;
            return option.TryGetValue(out value) ?
                value :
                selector();
        }

        public static Option<T> Where<T>(this Option<T> option, Func<T, Boolean> predicate)
        {
            if (option == null)
                throw new ArgumentNullException("option");
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            T value;
            return option.TryGetValue(out value) && predicate(value) ?
                Option.Just<T>(value) :
                Option.None<T>();
        }

        public static Option<U> Select<T, U>(this Option<T> option, Func<T, U> selector)
        {
            if (option == null)
                throw new ArgumentNullException("option");
            if (selector == null)
                throw new ArgumentNullException("selector");

            T value;
            return option.TryGetValue(out value) ?
                Option.Just(selector(value)) :
                Option.None<U>();
        }

        public static Option<U> SelectMany<T, U>(this Option<T> option, Func<T, Option<U>> selector)
        {
            if (option == null)
                throw new ArgumentNullException("option");
            if (selector == null)
                throw new ArgumentNullException("selector");

            T value;
            return option.TryGetValue(out value) ?
                selector(value) :
                Option.None<U>();
        }

        public static Option<V> SelectMany<T, U, V>(this Option<T> option, Func<T, Option<U>> selector, Func<T, U, V> projector)
        {
            if (option == null)
                throw new ArgumentNullException("option");
            if (selector == null)
                throw new ArgumentNullException("selector");
            if (projector == null)
                throw new ArgumentNullException("projector");

            return option.SelectMany(x => selector(x).Select(y => projector(x, y)));
        }
    }
}
