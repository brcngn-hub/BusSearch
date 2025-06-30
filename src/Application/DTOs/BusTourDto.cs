using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class BusTourDto
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Tur adı zorunludur")]
        [StringLength(100, ErrorMessage = "Tur adı en fazla 100 karakter olabilir")]
        public string? Name { get; set; }
        
        [Required(ErrorMessage = "Açıklama zorunludur")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Fiyat zorunludur")]
        [Range(0, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır")]
        public decimal Price { get; set; }
        
        [Required(ErrorMessage = "Geçerli kapasite zorunludur")]
        [Range(1, 50, ErrorMessage = "Kapasite 1-100 arasında olmalıdır")]
        public int AvailableSeats { get; set; }
        
        [Required(ErrorMessage = "Kalkış tarihi zorunludur")]
        public DateTime DepartureDate { get; set; }
        
        [Required(ErrorMessage = "Dönüş tarihi zorunludur")]
        public DateTime ReturnDate { get; set; }
        
        [Required(ErrorMessage = "Kalkış lokasyonu zorunludur")]
        public string? DepartureLocation { get; set; }
        
        [Required(ErrorMessage = "Varış lokasyonu zorunludur")]
        public string? Destination { get; set; }
        
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<BusStopDto>? Stops { get; set; }
        public List<string>? Features { get; set; }
    }

    public class BusStopDto
    {
        public int Id { get; set; }
        
        [StringLength(100, ErrorMessage = "Durak adı 100 karakterden uzun olamaz")]
        public string? Name { get; set; }
        
        [StringLength(100, ErrorMessage = "İstasyon adı 100 karakterden uzun olamaz")]
        public string? Station { get; set; }
        
        [DataType(DataType.DateTime)]
        public DateTime? Time { get; set; }
        
        public bool IsOrigin { get; set; }
        public bool IsDestination { get; set; }
        
        [Range(0, 100, ErrorMessage = "İndeks 0-100 arasında olmalıdır")]
        public int Index { get; set; }
    }
} 