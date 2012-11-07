/*
 * Parseq - a monadic parser combinator library for C#
 *
 * Copyright (c) 2012 WATANABE TAKAHISA <x.linerlock@gmail.com> All rights reserved.
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

        public Position(Int32 line, Int32 column, Int32 index)
        {
            if (line <= 0)
                throw new ArgumentOutOfRangeException("line must be 1 or more");
            if (column <= 0)
                throw new ArgumentOutOfRangeException("column must be 1 or more");
            if (index < 0)
                throw new ArgumentOutOfRangeException("position must be bigger than zero");

            _column = column;
            _line = line;
            _index = index;
        }

        public Int32 Line
        {
            get { return _line; }
        }

        public Int32 Column
        {
            get { return _column; }
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
