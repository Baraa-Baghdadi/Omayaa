using Concord.Domain.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Concord.Application.Services.Hub
{
    public class BroadcastHub : Hub<IHubClient>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<BroadcastHub> _context;
        private static readonly ConcurrentDictionary<Guid, List<string>> _userSessions
            = new ConcurrentDictionary<Guid, List<string>>();

        public BroadcastHub(IHubContext<BroadcastHub> context)
        {
            _context = context;
        }


        public override async Task OnConnectedAsync()
        {
            // The connection ID provided by SignalR
            var connectionId = Context.ConnectionId;

            // Access the authenticated user's claims
            var user = Context.User;

            // Check if the user is authenticated
            if (user?.Identity?.IsAuthenticated ?? false)
            {
                var tenantIdClaim = user.Claims.FirstOrDefault(c => c.Type == "TenantId");

                // If TenantId is present in the claims, extract its value
                if (tenantIdClaim?.Value != null)
                {
                    var tenantId = new Guid(tenantIdClaim?.Value!);
                    // add new connection Id:
                    RegiserUserSessions(connectionId!, tenantId);
                }

            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {

            // get Connection Id from token:
            var connectionId = Context?.ConnectionId;

            // Access the authenticated user's claims
            var user = Context?.User;

            // Check if the user is authenticated
            if (user?.Identity?.IsAuthenticated ?? false)
            {
                var tenantIdClaim = user.Claims.FirstOrDefault(c => c.Type == "TenantId");

                // If TenantId is present in the claims, extract its value
                var tenantId = new Guid(tenantIdClaim?.Value!);

                // Remove connection Id:
                RemoveUserSessions(connectionId!, tenantId);
            }

            await base.OnDisconnectedAsync(exception);
        }


        private void RegiserUserSessions(string connectionId, Guid tenantId)
        {
            // add new Tenant & connection Id:
            if (!_userSessions.ContainsKey(tenantId))
            {
                _userSessions.TryAdd(tenantId, new List<string>() { connectionId });
            }
            // add new connection Id:
            else if (!_userSessions[tenantId].Contains(connectionId))
            {
                _userSessions[tenantId].Add(connectionId);
            }
        }

        private void RemoveUserSessions(string connectionId, Guid tenantId)
        {
            if (_userSessions.ContainsKey(tenantId))
            {
                var removeIndex = _userSessions[tenantId].FindIndex(x => !x.Contains(connectionId));

                if (removeIndex != -1)
                {
                    _userSessions[tenantId].RemoveAt(removeIndex);
                }
            }
        }

        public ConcurrentDictionary<Guid, List<string>> getAllConnectionsId()
        {
            return _userSessions;
        }

    }
}
