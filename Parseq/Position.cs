using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq
{
    public struct Position 
        : IEquatable<Position>
    {
        private readonly int _column;
        private readonly int _line;
        private readonly int _index;

        public Position(int column, int line, int index){
            if (column <= 0)
                throw new ArgumentOutOfRangeException("column must be 1 or more");
            if (line <= 0)
                throw new ArgumentOutOfRangeException("line must be 1 or more");
            if (index < 0)
                throw new ArgumentOutOfRangeException("position must be bigger than zero");
            this._column = column;
            this._line = line;
            this._index = index;
        }

        public int Column {
            get { return _column; }
        }

        public int Line {
            get { return _line; }
        }

        public int Index {
            get { return _index; }
        }

        public bool Equals(Position other) {
            return other.Column == this.Column &&
                other.Line == this.Line &&
                other.Index == this.Index;
        }

        public override bool Equals(object obj) {
            return obj is Position && this.Equals((Position)obj);
        }

        public override int GetHashCode(){
            return this.Column.GetHashCode() 
                ^ this.Line.GetHashCode() 
                ^ this.Index.GetHashCode();
        }

        public override string ToString(){
            return string.Format("{0}:{1}",this.Column, this.Line);
        }

        public static bool operator >(Position x, Position y){
            return x.Index > y.Index;
        }

        public static bool operator >=(Position x, Position y){
            return x.Index >= y.Index;
        }

        public static bool operator <(Position x, Position y){
            return x.Index < y.Index;
        }

        public static bool operator <=(Position x, Position y){
            return x.Index <= y.Index;
        }

        public static bool operator ==(Position x, Position y){
            return x.Equals(y);
        }

        public static bool operator !=(Position x, Position y){
            return !(x.Equals(y));
        }
    }
}
