using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq
{
    public struct Position
        : IComparable<Position>
        , IEquatable<Position>
    {
        private readonly Int32 _column;
        private readonly Int32 _line;
        private readonly Int32 _index;

        public Position(Int32 column, Int32 line, Int32 index)
        {
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

        public Int32 Column
        {
            get { return _column; }
        }

        public Int32 Line
        {
            get { return _line; }
        }

        public Int32 Index
        {
            get { return _index; }
        }

        public Int32 CompareTo(Position other)
        {
            return this.Index.CompareTo(other.Index);
        }

        public Boolean Equals(Position other)
        {
            return other.Column == this.Column
                && other.Line == this.Line
                && other.Index == this.Index;
        }

        public override Boolean Equals(object obj)
        {
            return obj is Position && this.Equals((Position)obj);
        }

        public override Int32 GetHashCode()
        {
            return this.Column.GetHashCode()
                ^ this.Line.GetHashCode()
                ^ this.Index.GetHashCode();
        }

        public override String ToString()
        {
            return String.Format("{0}:{1}", this.Line, this.Column);
        }

        public static Boolean operator >(Position x, Position y)
        {
            return x.Index > y.Index;
        }

        public static Boolean operator >=(Position x, Position y)
        {
            return x.Index >= y.Index;
        }

        public static Boolean operator <(Position x, Position y)
        {
            return x.Index < y.Index;
        }

        public static Boolean operator <=(Position x, Position y)
        {
            return x.Index <= y.Index;
        }

        public static Boolean operator ==(Position x, Position y)
        {
            return x.Equals(y);
        }

        public static Boolean operator !=(Position x, Position y)
        {
             return !(x.Equals(y));
        }
    }
}
