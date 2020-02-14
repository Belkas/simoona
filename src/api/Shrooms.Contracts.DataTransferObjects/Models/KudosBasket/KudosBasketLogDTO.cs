﻿using System;

namespace Shrooms.Contracts.DataTransferObjects.Models.KudosBasket
{
    public class KudosBasketLogDTO
    {
        public decimal DonationAmount { get; set; }
        public KudosBasketLogUserDTO Donator { get; set; }
        public DateTime DonationDate { get; set; }
    }
}
