using MiniLaunch.Common.Responses;
using System;

namespace MiniLaunch.Common
{
    public class Subscription
    {
        public GameId Game { get; init; }

        public string Name { get; init; }

        public string Description { get; init; }

        public string Status { get; init; }

        public static explicit operator Subscription(LoginAccountResponseLoginAccountResultGameSubscription sub)
        {
            return !Enum.TryParse(sub.Game, out GameId gameId)
                ? null
                : new Subscription
                {
                    Name = sub.Name,
                    Game = gameId,
                    Description = sub.Description,
                    Status = sub.Status
                };
        }
    }
}