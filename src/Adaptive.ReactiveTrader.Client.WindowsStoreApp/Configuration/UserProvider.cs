using System;

namespace Adaptive.ReactiveTrader.Client.Configuration
{
    class UserProvider : IUserProvider
    {
        private string _username = "Win8-" + new Random().Next(1000);

        public string Username
        {
            get { return _username; }
        }
    }
}
