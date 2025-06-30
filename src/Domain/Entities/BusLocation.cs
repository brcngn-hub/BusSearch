using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class BusLocation
    {
        [Required]
        [StringLength(50)]
        public string Id { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Country { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string City { get; set; } = string.Empty;
        
    }
} 