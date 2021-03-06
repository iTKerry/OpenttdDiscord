﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenttdDiscord.Database.Servers
{
    public interface IServerRepository
    {
        Task<Server> GetServer(ulong guildId, string ip, int port);
        Task<Server> GetServer(ulong serverId);
        Task<List<Server>> GetServers(string ip, int port);
        Task<Server> AddServer(ulong guildId, string ip, int port, string name);

        Task UpdatePassword(ulong serverId, string password);

        Task<Server> GetServer(ulong guildId, string serverName);
        Task<List<Server>> GetAll(ulong guildId);
        
    }
}
