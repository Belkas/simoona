﻿using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects.Models;
using Shrooms.Contracts.DataTransferObjects.Models.Permissions;

namespace Shrooms.Domain.Services.Permissions
{
    public interface IPermissionService
    {
        bool UserHasPermission(UserAndOrganizationDTO userAndOrg, string permissionName);
        IEnumerable<PermissionGroupDTO> GetGroupNames(int organizationId);
        IEnumerable<string> GetUserPermissions(string userId, int organizationId);
        IEnumerable<PermissionDTO> GetRolePermissions(string roleId, int organizationId);
        void RemoveCache(string userId);
    }
}
