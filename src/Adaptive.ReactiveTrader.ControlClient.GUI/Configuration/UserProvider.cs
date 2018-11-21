using System;
using Adaptive.ReactiveTrader.Client.Configuration;

namespace Adaptive.ReactiveTrader.ControlClient.GUI.Configuration
{
    internal class UserProvider : IUserProvider
    {
        private readonly string _username;

        public UserProvider(string username)
        {
            _username = username;
        }

        public string Username
        {
            get { return _username; }
        }
    }

}