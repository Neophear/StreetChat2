using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StreetChat
{
    [Serializable]
    class Message
    {
        private Guid _fromID;
        private StreetChat.MainWindow.MessageType _type;
        private object _attachment;

        public Message()
        {

        }
        public Message(Guid fromID, StreetChat.MainWindow.MessageType type, object attachment)
        {
            this._fromID = fromID;
            this._type = type;
            this._attachment = attachment;
        }
        public Guid fromID
        {
            get { return _fromID; }
            set { _fromID = value; }
        }
        public StreetChat.MainWindow.MessageType type
        {
            get { return _type; }
            set { _type = value; }
        }
        public object attachment
        {
            get { return _attachment; }
            set { _attachment = value; }
        }
    }
}
