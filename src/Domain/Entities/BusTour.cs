using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class BusTour
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Required]
        public DateTime DepartureDate { get; set; }
        
        [Required]
        public DateTime ReturnDate { get; set; }
        
        [Range(0, 10000)]
        public decimal Price { get; set; }
        
        [Range(1, 50)]
        public int AvailableSeats { get; set; }
        
        [Range(0, 100)]
        public int CurrentBookings { get; set; }
        
        [Required]
        [StringLength(50)]
        public string DepartureLocation { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Destination { get; set; } = string.Empty;
        
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<BusStop>? Stops { get; set; }
        public List<string>? Features { get; set; }
        
        public bool IsFull => AvailableSeats == 0;
    }

    public class BusStop
    {
        public int Id { get; set; }
        
        [StringLength(100)]
        public string? Name { get; set; }
        
        [StringLength(100)]
        public string? Station { get; set; }
        
        public DateTime? Time { get; set; }
        
        public bool IsOrigin { get; set; }
        public bool IsDestination { get; set; }
        
        [Range(0, 100)]
        public int Index { get; set; }
    }
} 