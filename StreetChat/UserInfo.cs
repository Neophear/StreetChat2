using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StreetChat
{
    [Serializable]
    class UserInfo
    {
        private string _username;
        private Version _chatversion;
        private bool _isadmin;
        private string _status;
        private Guid _uniqueid;
        private DateTime _started;

        public UserInfo()
        {

        }
        public UserInfo(string Username, Version ChatVersion, bool IsAdmin, string Status, Guid UniqueID, DateTime Started)
        {
            this._username = Username;
            this._chatversion = ChatVersion;
            this._isadmin = IsAdmin;
            this._status = Status;
            this._uniqueid = UniqueID;
            this._started = Started;
        }
        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }
        public Version ChatVersion
        {
            get { return _chatversion; }
            set { _chatversion = value; }
        }
        public bool IsAdmin
        {
            get { return _isadmin; }
            set { _isadmin = value; }
        }
        public string Status
        {
            get { return _status; }
            set { _status = value; }
        }
        public Guid UniqueID
        {
            get { return _uniqueid; }
            set { _uniqueid = value; }
        }
        public DateTime Started
        {
            get { return _started; }
            set { _started = value; }
        }
    }
}
