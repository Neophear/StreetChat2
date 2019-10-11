using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace StreetChat
{
    class IPSub
    {
        private IPAddress _ipaddress;
        private IPAddress _subnet;

        public IPSub(IPAddress ipaddress)
        {
            this._ipaddress = ipaddress;
        }
        public IPSub(IPAddress ipaddress, IPAddress submask)
        {
            this._ipaddress = ipaddress;
            this._subnet = submask;
        }
        public IPAddress ipaddress
        {
            get { return _ipaddress; }
            set { _ipaddress = value; }
        }
        public IPAddress submask
        {
            get { return _subnet; }
            set { _subnet = value; }
        }
    }
}
