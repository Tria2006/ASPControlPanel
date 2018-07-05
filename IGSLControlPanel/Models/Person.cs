using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IGSLControlPanel.Models
{
    public class Person
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string MiddleName { get; set; }

        [Required]
        public int Gender { get; set; }

        [Required]
        public string BirthPlace { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        public int DocumentType { get; set; }

        [Required]
        public string DocumentSeries { get; set; }

        [Required]
        public string DocumentNumber { get; set; }

        [Required]
        public string DocumentIssuer { get; set; }

        [Required]
        public string DocumentIssuerCode { get; set; }

        [Required]
        public DateTime DocumentIssueDate { get; set; }

        public bool IsDeleted { get; set; }
    }
}
