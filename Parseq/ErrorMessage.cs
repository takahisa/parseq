/*
 * Parseq - a monadic parser combinator library for C#
 *
 * Copyright (c) 2012 - 2013 WATANABE TAKAHISA <x.linerlock@gmail.com> All rights reserved.
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
    public class ErrorMessage
        : Exception
    {
        private readonly ErrorMessageType _messageType;
        private readonly String _message;
        private readonly Position _beginning;
        private readonly Position _end;

        public ErrorMessage(ErrorMessageType messageType, String message, Exception cause,
            Position beginning, Position end)
            : base(message, cause)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            _messageType = messageType;
            _message = message;
            _beginning = beginning;
            _end = end;
        }

        public ErrorMessage(ErrorMessageType messageType, String message,
            Position beginning, Position end)
            : this(messageType, message, null, beginning, end)
        {

        }

        public ErrorMessageType MessageType
        {
            get { return _messageType; }
        }

        public override String Message
        {
            get { return _message; }
        }

        public virtual String MessageDetails
        {
            get
            {
                return String.Format("{0}\n({1},{2})",
                    this.Message, this.Beginning, this.End);
            }
        }

        public Position Beginning
        {
            get { return _beginning; }
        }

        public Position End
        {
            get { return _end; }
        }

        public override String ToString()
        {
            return this.MessageDetails;
        }
    }
}
