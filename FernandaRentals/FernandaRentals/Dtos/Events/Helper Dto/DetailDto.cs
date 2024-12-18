﻿using FernandaRentals.Database.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FernandaRentals.Dtos.Events.Helper_Dto
{
    public class DetailDto
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public Guid ProductId { get; set; }
        public virtual ProductEntity Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; } 
    }
}
