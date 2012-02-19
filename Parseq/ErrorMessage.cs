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
        private readonly string _message;
        private readonly Position _beginning;
        private readonly Position _end;

        public ErrorMessage(ErrorMessageType messageType, string message,Exception cause,
            Position beginning, Position end)
            :base(message,cause)
        {
            if (message == null)
                throw new ArgumentNullException("message");
            _messageType = messageType;
            _message = message;
            _beginning = beginning;
            _end = end;
        }

        public ErrorMessage(ErrorMessageType messageType, string message,
            Position beginning, Position end)
            : this(messageType, message,null, beginning, end)
        {

        }

        public ErrorMessageType MessageType {
            get { return _messageType; }
        }

        public override string Message {
            get { return _message; }
        }

        public virtual string MessageDetails {
            get {
                return string.Format("{0}\n({1},{2})",
                    this.Message, this.Beginning, this.End);
            }
        }

        public Position Beginning {
            get { return _beginning; }
        }

        public Position End {
            get { return _end; }
        }

        public override string ToString(){
            return this.MessageDetails;
        }
    }
}
