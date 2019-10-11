using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Media;

namespace StreetChat
{
    [Serializable]
    class User : INotifyPropertyChanged
    {
        private IPEndPoint _ipendpoint;
        private string _username = "";
        private IPAddress _ipaddress;
        private Version _chatversion;
        private bool _isadmin = false;
        private string _status = "";
        private TcpClient _tcpclient;
        private bool _islocaluser = false;
        private DateTime _started = DateTime.Now;
        private Guid _uniqueID;

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public User(TcpClient tcpclient)
        {
            this._tcpclient = tcpclient;
            this._ipendpoint = ((IPEndPoint)tcpClient.Client.RemoteEndPoint);
            this._ipaddress = _ipendpoint.Address;
        }
        public User(UserInfo info)
        {
            this._username = info.Username;
            this._chatversion = info.ChatVersion;
            this._isadmin = info.IsAdmin;
            this._status = info.Status;
            this._uniqueID = info.UniqueID;
            this._started = info.Started;
        }
        public User(string username)
        {
            this._username = username;
        }
        public User(TcpClient tcpclient, string username)
        {
            this._username = username;
            this._tcpclient = tcpclient;
            this._ipendpoint = ((IPEndPoint)tcpClient.Client.RemoteEndPoint);
            this._ipaddress = _ipendpoint.Address;
        }
        public User(TcpClient tcpclient, string username, Version version)
        {
            this._username = username;
            this._chatversion = version;
            this._tcpclient = tcpclient;
            this._ipendpoint = ((IPEndPoint)tcpClient.Client.RemoteEndPoint);
            this._ipaddress = _ipendpoint.Address;
        }
        public User(TcpClient tcpclient, string username, Version version, Guid uniqueid)
        {
            this._username = username;
            this._chatversion = version;
            this._tcpclient = tcpclient;
            this._ipendpoint = ((IPEndPoint)tcpClient.Client.RemoteEndPoint);
            this._ipaddress = _ipendpoint.Address;
            this._uniqueID = uniqueid;
        }
        public User()
        {

        }
        public string Username
        {
            get { return _username; }
            set { _username = value; OnPropertyChanged("Username"); }
        }
        public IPEndPoint IPEndPoint
        {
            get { return _ipendpoint; }
            set
            {
                _ipendpoint = value;
                _ipaddress = _ipendpoint.Address;
                OnPropertyChanged("IPEndPoint");
            }
        }
        public IPAddress IPAddress
        {
            get { return _ipaddress; }
            set { _ipaddress = value; OnPropertyChanged("IPAddress"); }
        }
        public bool IsConnected
        {
            get { return _tcpclient.Connected; }
        }
        public Version ChatVersion
        {
            get { return _chatversion; }
            set { _chatversion = value; OnPropertyChanged("ChatVersion"); }
        }
        public bool IsAdmin
        {
            get { return _isadmin; }
            set { _isadmin = value; OnPropertyChanged("IsAdmin"); }
        }
        public string Status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged("Status"); }
        }
        public TcpClient tcpClient
        {
            get { return _tcpclient; }
            set
            {
                _tcpclient = value;
                _ipendpoint = (IPEndPoint)_tcpclient.Client.RemoteEndPoint;
                _ipaddress = _ipendpoint.Address;
                OnPropertyChanged("tcpClient");
            }
        }
        public bool IsLocalUser
        {
            get { return _islocaluser; }
            set { _islocaluser = value; OnPropertyChanged("IsLocalUser"); }
        }
        public Guid UniqueID
        {
            get { return _uniqueID; }
            set { _uniqueID = value; OnPropertyChanged("UniqueID"); }
        }
        public DateTime Started
        {
            get { return _started; }
            set { _started = value; OnPropertyChanged("Started"); }
        }
        public string DisplayText
        {
            get
            {
                string _displaytext = _username;
                if (_status != "")
                {
                    _displaytext += " [" + _status + "]";
                }
                return _displaytext;
            }
        }
    }
}