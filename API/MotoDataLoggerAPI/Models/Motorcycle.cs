using System.ComponentModel.DataAnnotations;

namespace MotoDataLoggerAPI.Models
{
    public class Motorcycle
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Manufacturer is required.")]
        [StringLength(100, ErrorMessage = "Manufacturer cannot be longer than 100 characters.")]
        public string Manufacturer { get; set; } = string.Empty;

        [Required(ErrorMessage = "Model is required.")]
        [StringLength(100, ErrorMessage = "Model cannot be longer than 100 characters.")]
        public string Model { get; set; } = string.Empty;

        [Required(ErrorMessage = "Year is required.")]
        [Range(1900, 2100, ErrorMessage = "Year must be between 1900 and 2100.")]
        public int Year { get; set; }

        [StringLength(200, ErrorMessage = "Description cannot be longer than 200 characters.")]
        public string? Description { get; set; }

        [StringLength(200, ErrorMessage = "Vin cannot be longer than 80 characters.")]
        public string? Vin { get; set; }
        
        [StringLength(200, ErrorMessage = "License Plate cannot be longer than 20 characters.")]
        public string? LicensePlate { get; set; }
        
        [Range(10, 2000, ErrorMessage = "Engine Displacement must be between 10 and 2000 m3.")]
        public int? EngineDisplacement {get; set;}
    }
}
