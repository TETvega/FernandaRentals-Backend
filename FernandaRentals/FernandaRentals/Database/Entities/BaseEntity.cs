﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using InmobiliariaUNAH.Database.Entities;

namespace FernandaRentals.Database.Entities
{
    public class BaseEntity : AuditEntity
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "El {0} de la categoria es requerido.")]
        [Column("name")]
        public string Name { get; set; }

    }
}
