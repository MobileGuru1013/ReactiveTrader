using System;

namespace Adaptive.ReactiveTrader.Client.Configuration
{
    internal class UserProvider : IUserProvider
    {
        public string Username
        {
            get { return "WPF-" + new Random().Next(1000); }
        }
    }
}