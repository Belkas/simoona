﻿using System;

namespace Shrooms.Contracts.DataTransferObjects
{
    public interface ITrackable
    {
        DateTime Created { get; set; }

        string CreatedBy { get; set; }

        DateTime Modified { get; set; }

        string ModifiedBy { get; set; }
    }
}
