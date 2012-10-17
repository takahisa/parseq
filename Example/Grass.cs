using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Parseq;
using Parseq.Combinators;

namespace Example
{
    /*
     * About Grass  
     *   ja: http://www.blue.sky.or.jp/grass/doc_ja.html
     *   en: http://www.blue.sky.or.jp/grass/
     * 
     * Grammer ( via http://www.blue.sky.or.jp/grass/ )
     *   app ::= W+ w+
     *   abs ::= w+ app*
     *   prog ::= abs | prog v abs | prog v app*
     * 
     */
    public static class Grass
    {
        public abstract class Exp { }

        public class App : Exp
        {
            public Int32 Fun { get; set; }
            public Int32 Arg { get; set; }

            public override String ToString()
            {
                return String.Format("App({0},{1})", this.Fun,this.Arg); 
            }
        }

        public class Abs : Exp
        {
            public Int32 Arg { get; set; }
            public IEnumerable<Exp> Body { get; set; }

            public override String ToString() 
            { 
                return String.Format("Abs({0}, {1})", this.Arg, String.Join(",", this.Body.Select(_ => _.ToString())));
            }
        }

        static Grass()
        {
            var comments = Chars.NoneOf('w', 'W', 'ｗ', 'Ｗ', 'v', 'V').Many();

            var lcw =
                Chars.OneOf('w', 'ｗ').Between(comments, comments);
            var ucw =
                Chars.OneOf('W', 'Ｗ').Between(comments, comments);
            var v =
                Chars.OneOf('v', 'V').Between(comments, comments);

            var app =
                from x in ucw.Many(1).Select(t => t.Count())
                from y in lcw.Many(1).Select(t => t.Count())
                select (Exp)new App { Fun = x, Arg = y };

            var abs =
                from x in lcw.Many(1).Select(t => t.Count())
                from y in app.Many()
                select (Exp)new Abs { Arg = x, Body = y };

            var prog = abs.Pipe(v.Right(abs.Select(_ => _.Enumerate()).Or(app.Many())).Many(),
                (x, y) => x.Concat(y.SelectMany(_ => _)));

            Grass.Parser = prog;

        }

        public static Parser<Char, IEnumerable<Exp>> Parser
        {
            get;
            private set;
        }

        public static void Test(String source)
        {
            IEnumerable<Exp> e; ErrorMessage m;
            switch (Grass.Parser.Run(source.AsStream()).TryGetValue(out e, out m))
            {
                case ReplyStatus.Success:
                    e.ForEach(_ => Console.WriteLine(_.ToString()));
                    break;
                case ReplyStatus.Failure:
                    Console.WriteLine("Failure");
                    break;
                default:
                    Console.WriteLine(m.MessageDetails);
                    break;
            }
        }
    }
}
