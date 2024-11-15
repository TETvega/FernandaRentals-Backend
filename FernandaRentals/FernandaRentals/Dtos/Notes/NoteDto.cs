﻿using FernandaRentals.Database.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FernandaRentals.Dtos.Notes
{
    public class NoteDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public Guid EventId { get; set; }
        public string Description { get; set; }
    }
}
