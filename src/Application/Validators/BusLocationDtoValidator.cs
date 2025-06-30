using FluentValidation;
using Application.DTOs;

namespace Application.Validators
{
    public class BusLocationDtoValidator : AbstractValidator<BusLocationDto>
    {
        public BusLocationDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Lokasyon ID boş olamaz")
                .Length(1, 50).WithMessage("Lokasyon ID 1-50 karakter arasında olmalıdır")
                .Matches(@"^[a-zA-Z0-9\-_]+$").WithMessage("Lokasyon ID sadece harf, rakam, tire ve alt çizgi içerebilir");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Lokasyon adı boş olamaz")
                .Length(2, 100).WithMessage("Lokasyon adı 2-100 karakter arasında olmalıdır")
                .Matches(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ\s\-\.]+$").WithMessage("Lokasyon adı sadece harf, boşluk, tire ve nokta içerebilir");

            RuleFor(x => x.Country)
                .NotEmpty().WithMessage("Ülke adı boş olamaz")
                .Length(2, 50).WithMessage("Ülke adı 2-50 karakter arasında olmalıdır")
                .Matches(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ\s\-\.]+$").WithMessage("Ülke adı sadece harf, boşluk, tire ve nokta içerebilir");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("Şehir adı boş olamaz")
                .Length(2, 50).WithMessage("Şehir adı 2-50 karakter arasında olmalıdır")
                .Matches(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ\s\-\.]+$").WithMessage("Şehir adı sadece harf, boşluk, tire ve nokta içerebilir");
        }
    }
} 