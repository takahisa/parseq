using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq
{
    [Serializable]
    public struct Location : IEquatable<Location>
    {
        private readonly int _column;
        private readonly int _line;
        private readonly int _position;

        public Location(int column, int line, int position){
            if (column <= 0)
                throw new ArgumentOutOfRangeException("column must be 1 or more");
            if (line <= 0)
                throw new ArgumentOutOfRangeException("line must be 1 or more");
            if (position < 0)
                throw new ArgumentOutOfRangeException("position must be bigger than zero");
            this._column = column;
            this._line = line;
            this._position = position;
        }

        public int Column {
            get { return _column; }
        }

        public int Line {
            get { return _line; }
        }

        public int Position {
            get { return _position; }
        }

        public bool Equals(Location other) {
            return other.Column == this.Column &&
                other.Line == this.Line &&
                other.Position == this.Position;
        }

        public override bool Equals(object obj) {
            return obj is Location && this.Equals((Location)obj);
        }

        public override int GetHashCode(){
            return this.Column.GetHashCode() 
                ^ this.Line.GetHashCode() 
                ^ this.Position.GetHashCode();
        }

        public override string ToString(){
            return string.Format("{0}:{1}",this.Column, this.Line);
        }

        public static bool operator >(Location x, Location y){
            return x.Position > y.Position;
        }

        public static bool operator >=(Location x, Location y){
            return x.Position >= y.Position;
        }

        public static bool operator <(Location x, Location y){
            return x.Position < y.Position;
        }

        public static bool operator <=(Location x, Location y){
            return x.Position <= y.Position;
        }

        public static bool operator ==(Location x, Location y){
            return x.Equals(y);
        }

        public static bool operator !=(Location x, Location y){
            return !(x.Equals(y));
        }
    }
}
