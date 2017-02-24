using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq
{
	public class FixedPoint<TToken,T>
	{
		private Parser<TToken, T> _fixedParser;
		public Parser<TToken, T> FixedParser
		{
			set
			{
				if (value == null) throw new NullReferenceException();
				if (_fixedParser != null) throw new InvalidOperationException("Parser is already registered.");
				_fixedParser = value;

			}
		}

		public IReply<TToken, T> Parse(ITokenStream<TToken> stream)
		{
			if (_fixedParser == null) throw new InvalidOperationException("Parser isn't registered.");
			return _fixedParser(stream);
		}
	}
}
