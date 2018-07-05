using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IGSLControlPanel.Models
{
    public class Address
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public int AddressType { get; set; }

        public string ZipCode { get; set; }

        [Required]
        public string Country { get; set; }

        public string Region { get; set; }

        public string District { get; set; }

        public string City { get; set; }

        public string Locality { get; set; }

        [Required]
        public string Street { get; set; }

        [Required]
        public string House { get; set; }

        public string Building { get; set; }

        public string Flat { get; set; }
    }
}
