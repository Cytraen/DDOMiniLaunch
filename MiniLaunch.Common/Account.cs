using System.Collections.Generic;
using System.Linq;

namespace MiniLaunch.Common
{
    public class Account
    {
        public string Username { get; init; }

        public string Password { get; init; }

        private List<Subscription> _subscriptions;

        public IReadOnlyList<Subscription> Subscriptions
        {
            get => _subscriptions.AsReadOnly();
            init => _subscriptions = value.ToList();
        }
    }
}