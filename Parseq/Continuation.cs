﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq
{
    public delegate TResult Continuation<TResult,T>(Func<T,TResult> func);
}
