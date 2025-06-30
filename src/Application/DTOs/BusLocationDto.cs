using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Application.DTOs;

public class BusLocationDto
{
    [JsonProperty("id")]
    [Required(ErrorMessage = "ID alanı zorunludur")]
    [StringLength(50, ErrorMessage = "Lokasyon ID 50 karakterden uzun olamaz")]
    public string? Id { get; set; }
    
    [JsonProperty("name")]
    [Required(ErrorMessage = "İsim alanı zorunludur")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Lokasyon adı 2-100 karakter arasında olmalıdır")]
    public string? Name { get; set; }
    
    [JsonProperty("country")]
    [Required(ErrorMessage = "Ülke alanı zorunludur")]
    [StringLength(50, ErrorMessage = "Ülke adı en fazla 50 karakter olabilir")]
    public string? Country { get; set; }
    
    [JsonProperty("city")]
    [Required(ErrorMessage = "Şehir alanı zorunludur")]
    [StringLength(50, ErrorMessage = "Şehir adı en fazla 50 karakter olabilir")]
    public string? City { get; set; }
}