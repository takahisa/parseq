/*
 * Copyright (C) 2012 - 2015 Takahisa Watanabe <linerlock@outlook.com> All rights reserved.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */
using System;

namespace Parseq
{
    public struct Position
        : IComparable<Position>
        , IEquatable<Position>
    {
        public readonly static Position Zero = new Position(1, 1);    

        public Int32 Line
        {
            get;
            private set;
        }

        public Int32 Column
        {
            get;
            private set;
        }

        public Position(Int32 line, Int32 column)
            : this()
        {
            if (line < 1)
                throw new ArgumentOutOfRangeException("line");
            if (column < 1)
                throw new ArgumentOutOfRangeException("column");

            this.Line = line;
            this.Column = column;
        }

        public Int32 CompareTo(Position other)
        {
            var distance = (this.Line == other.Line)
                ? this.Column - other.Column
                : this.Line - other.Line;
            /*
            return
                (distance > 0) ? +1 :
                (distance < 0) ? -1 :
                0;
             */
            return distance;
        }

        public Boolean Equals(Position other)
        {
            return this.CompareTo(other) == 0;
        }

        public override Int32 GetHashCode()
        {
            const Int32 magic = 117;
            return unchecked((this.Line.GetHashCode() * magic)
                ^ this.Column.GetHashCode());
        }

        public override Boolean Equals(Object obj)
        {
            return obj != null && this.Equals((Position)obj);
        }

        public static Boolean operator ==(Position lhs, Position rhs)
        {
            return lhs.Equals(rhs);
        }

        public static Boolean operator !=(Position lhs, Position rhs)
        {
            return !(lhs == rhs);
        }

        public static Boolean operator >(Position lhs, Position rhs)
        {
            return lhs.CompareTo(rhs) > 0;
        }

        public static Boolean operator <(Position lhs, Position rhs)
        {
            return lhs.CompareTo(rhs) < 0;
        }

        public static Boolean operator >=(Position lhs, Position rhs)
        {
            return !(lhs < rhs);
        }

        public static Boolean operator <=(Position lhs, Position rhs)
        {
            return !(lhs > rhs);
        }
    }
}
