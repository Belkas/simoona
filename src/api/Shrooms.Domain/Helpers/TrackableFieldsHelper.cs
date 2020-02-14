﻿using System;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Helpers
{
    public static class TrackableFieldsHelper
    {
        public static void UpdateMetadata(this ITrackable trackableEntity, string userId, DateTime? timestamp = null)
        {
            if (!timestamp.HasValue)
            {
                timestamp = DateTime.UtcNow;
            }

            trackableEntity.Modified = timestamp.Value;
            trackableEntity.ModifiedBy = userId;
        }
    }
}
