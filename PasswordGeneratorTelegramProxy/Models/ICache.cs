﻿using Microsoft.AspNet.SignalR.Client;

namespace PasswordGeneratorTelegramProxy.Models
{
    public interface ICache
    {
        void Add(long key, HubConnection connection, IHubProxy proxy);

        void Delete(long key);

        bool TryGet(long key, out HubConnection connection, out IHubProxy proxy);
    }
}