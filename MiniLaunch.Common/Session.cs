using System.Collections.Generic;
using System.Linq;

namespace MiniLaunch.Common
{
    public class Session
    {
        private List<Subscription> _subscriptions;

        public IReadOnlyList<Subscription> Subscriptions
        {
            get => _subscriptions.AsReadOnly();
            init => _subscriptions = value.ToList();
        }

        public string Ticket { get; init; }

        public string Username { get; init; }
    }
}