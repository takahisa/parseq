using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq
{
    public static class Extensions
    {
        static T With<T>(this T value, Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            action(value);
            return value;
        }
    }
}
