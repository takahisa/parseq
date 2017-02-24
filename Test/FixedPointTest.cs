using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Parseq;

using static Parseq.Combinators.Chars;
using static Parseq.Combinator; 

namespace Test.Parseq
{
	[TestFixture]
	public class FixedPointTest
	{
		[TestCase]
		public void NullSetTest()
		{
			var fix = new FixedPoint<char, char>();
			Assert.Throws<NullReferenceException>(() => { fix.FixedParser = null; });
		}

		[TestCase]
		public void DuplicateSetTest()
		{
			var fix = new FixedPoint<char, char> {FixedParser = Char('n')};

			Assert.Throws<InvalidOperationException>(() => { fix.FixedParser = Char('o'); });


			Assert.IsTrue(fix.Parse("n".AsStream()).Case(_ => false, _ => true));
			Assert.IsFalse(fix.Parse("o".AsStream()).Case(_ => false, _ => true));
		}

		[TestCase]
		public void UnsettedCallTest()
		{
			var fix = new FixedPoint<char, char>();

			Assert.Throws<InvalidOperationException>(() => { fix.Parse("n".AsStream()); });
		}

		[TestCase]
		public void TypicalUseCaseTest()
		{
			var fix = new FixedPoint<char, IEnumerable<char>>();

			//digit<-[0-9]
			var digit = Digit().Select(c => new[] {c});

			//field<-digit/collection
			var field = Choice(digit, fix.Parse);


			var tmp = (from f in field
				from _ in Char(',')
				select f).Many0();

			//fields<-(field ",")* field
			var fields = from f in tmp
				from n in field
				select f.SelectMany(c => c).Concat(n);

			//collection<-"{" fields? "}"
			var collection = from _ in Char('{')
				from f in fields.Optional()
				from __ in Char('}')
				select new[] {'{'}.Concat(f.HasValue ? f.Value : new char[0]).Concat(new[] {'}'});

			fix.FixedParser = collection;

			Assert.IsTrue(collection.Run("{}".AsStream()).Case(_ => false, act => act.SequenceEqual("{}")));
			Assert.IsTrue(collection.Run("{1,2,3}".AsStream()).Case(_ => false, act => act.SequenceEqual("{123}")));
			Assert.IsTrue(collection.Run("{1,2,{3,4},5}".AsStream()).Case(_ => false, act => act.SequenceEqual("{12{34}5}")));
			Assert.IsTrue(collection.Run("{{}}".AsStream()).Case(_ => false, act => act.SequenceEqual("{{}}")));



		}
	}
}
