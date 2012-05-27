using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parseq
{
    public struct Location : IEquatable<Location>
    {
        private readonly Int32 _column;
        private readonly Int32 _line;
        private readonly Int32 _position;

        public Location(Int32 column, Int32 line, Int32 position)
        {
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

        public Int32 Column
        {
            get { return _column; }
        }

        public Int32 Line
        {
            get { return _line; }
        }

        public Int32 Position
        {
            get { return _position; }
        }

        public Boolean Equals(Location other)
        {
            return other.Column == this.Column
                && other.Line == this.Line
                && other.Position == this.Position;
        }

        public override Boolean Equals(object obj)
        {
            return obj is Location && this.Equals((Location)obj);
        }

        public override Int32 GetHashCode()
        {
            return this.Column.GetHashCode()
                ^ this.Line.GetHashCode()
                ^ this.Position.GetHashCode();
        }

        public override String ToString()
        {
            return String.Format("{0}:{1}", this.Column, this.Line);
        }

        public static Boolean operator >(Location x, Location y)
        {
            return x.Position > y.Position;
        }

        public static Boolean operator >=(Location x, Location y)
        {
            return x.Position >= y.Position;
        }

        public static Boolean operator <(Location x, Location y)
        {
            return x.Position < y.Position;
        }

        public static Boolean operator <=(Location x, Location y)
        {
            return x.Position <= y.Position;
        }

        public static Boolean operator ==(Location x, Location y)
        {
            return x.Equals(y);
        }

        public static Boolean operator !=(Location x, Location y)
        {
            return !(x.Equals(y));
        }
    }
}
