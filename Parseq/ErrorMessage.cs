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
