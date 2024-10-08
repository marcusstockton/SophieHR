﻿using System.ComponentModel.DataAnnotations;

namespace SophieHR.Api.Models.DTOs.Address
{
    public class AddressCreateDto
    {
        public string Line1 { get; set; }
        public string? Line2 { get; set; }
        public string? Line3 { get; set; }
        public string? Line4 { get; set; }

        [DataType(DataType.PostalCode)]
        public string Postcode { get; set; }

        public string County { get; set; }
        public string? MapImage { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
    }
}