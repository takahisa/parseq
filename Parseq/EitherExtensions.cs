using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq
{
    public static class EitherExtensions {

        public static T Merge<TLeft, TRight, T>(
            this Either<TLeft, TRight> either,
            Func<TLeft, T> lselector,
            Func<TRight, T> rselector)
        {
            if (either == null)
                throw new ArgumentNullException("either");
            if (lselector == null)
                throw new ArgumentNullException("lselector");
            if (rselector == null)
                throw new ArgumentNullException("rselector");
            
            TLeft left;TRight right;
            return either.TryGetValue(out left, out right) == Hand.Left ?
                lselector(left) :
                rselector(right);
        }

        public static TLeft MergeLeft<TLeft, TRight>(
            this Either<TLeft, TRight> either,
            Func<TRight, TLeft> selector)
        {
            if (either == null)
                throw new ArgumentNullException("either");
            if (selector == null)
                throw new ArgumentNullException("selector");

            TLeft left; TRight right;
            return either.TryGetValue(out left, out right) == Hand.Left ?
                left :
                selector(right);
        }

        public static TRight MergeRight<TLeft, TRight>(
            this Either<TLeft, TRight> either,
            Func<TLeft,TRight> selector)
        {
            if (either == null)
                throw new ArgumentNullException("either");
            if (selector == null)
                throw new ArgumentNullException("selector");

            TLeft left; TRight right;
            return either.TryGetValue(out left, out right) == Hand.Left ?
                selector(left) :
                right;
        }



    }
}
