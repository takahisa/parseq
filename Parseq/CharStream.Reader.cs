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
using System.IO;

namespace Parseq
{
    public partial class CharStream
    {
        class Reader
            : IDisposable
        {
            public const Int32 EOF = -1;

            private Position currentPosition;
            private TextReader baseReader;

            public Reader(TextReader baseReader)
            {
                if (baseReader == null)
                    throw new ArgumentNullException("baseReader");

                this.currentPosition = Position.Zero;
                this.baseReader = baseReader;
            }

            public Position CurrentPosition
            {
                get { return this.currentPosition; }
            }

            public Int32 Read()
            {
                if (this.baseReader == null)
                    throw new ObjectDisposedException("baseReader");

                var c = this.baseReader.Read();

                this.currentPosition = (c == '\n' || c == '\r' && this.baseReader.Peek() == '\r')
                    ? new Position(this.currentPosition.Line + 1, 1)
                    : new Position(this.currentPosition.Line, this.currentPosition.Column + 1);
                return c;
            }

            public Int32 Peek()
            {
                if (this.baseReader == null)
                    throw new ObjectDisposedException("baseReader");

                return this.baseReader.Peek();
            }

            public void Dispose()
            {
                if (this.baseReader != null)
                {
                    this.baseReader.Dispose();
                    this.baseReader = null;
                }
            }
        }
    }
}
