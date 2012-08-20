using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq
{
    public delegate TResult Cps<TResult,T>(Func<T,TResult> func);
}
