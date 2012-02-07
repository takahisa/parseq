using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq
{
    public delegate Reply<TToken,TResult> Parser<TToken,TResult>(Stream<TToken> stream);
}
