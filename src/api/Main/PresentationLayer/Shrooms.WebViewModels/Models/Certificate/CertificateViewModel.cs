﻿using System.Collections.Generic;
using Shrooms.WebViewModels.Models.User;

namespace Shrooms.WebViewModels.Models.Certificate
{
    public class CertificateViewModel : AbstractClassifierAbstractViewModel
    {
        public IEnumerable<ApplicationUserViewModel> ApplicationUsers { get; set; }
    }
}