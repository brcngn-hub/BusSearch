using FluentValidation;
using Application.DTOs;

namespace Application.Validators
{
    public class BusTourDtoValidator : AbstractValidator<BusTourDto>
    {
        public BusTourDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tur adı boş olamaz")
                .MaximumLength(100).WithMessage("Tur adı en fazla 100 karakter olabilir");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Açıklama boş olamaz")
                .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır");

            RuleFor(x => x.DepartureDate)
                .NotEmpty().WithMessage("Kalkış tarihi boş olamaz")
                .GreaterThan(DateTime.Today).WithMessage("Kalkış tarihi bugünden sonra olmalıdır");

            RuleFor(x => x.ReturnDate)
                .NotEmpty().WithMessage("Dönüş tarihi boş olamaz")
                .GreaterThan(x => x.DepartureDate).WithMessage("Dönüş tarihi kalkış tarihinden sonra olmalıdır");

            RuleFor(x => x.DepartureLocation)
                .NotEmpty().WithMessage("Kalkış lokasyonu boş olamaz")
                .MaximumLength(50).WithMessage("Kalkış lokasyonu en fazla 50 karakter olabilir");

            RuleFor(x => x.Destination)
                .NotEmpty().WithMessage("Varış lokasyonu boş olamaz")
                .MaximumLength(50).WithMessage("Varış lokasyonu en fazla 50 karakter olabilir");

            RuleForEach(x => x.Stops)
                .SetValidator(new BusStopDtoValidator())
                .When(x => x.Stops != null);

            RuleForEach(x => x.Features)
                .MaximumLength(50).WithMessage("Özellik adı 50 karakterden uzun olamaz")
                .When(x => x.Features != null);
        }
    }

    public class BusStopDtoValidator : AbstractValidator<BusStopDto>
    {
        public BusStopDtoValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(100).WithMessage("Durak adı 100 karakterden uzun olamaz")
                .When(x => !string.IsNullOrEmpty(x.Name));

            RuleFor(x => x.Station)
                .MaximumLength(100).WithMessage("İstasyon adı 100 karakterden uzun olamaz")
                .When(x => !string.IsNullOrEmpty(x.Station));

            RuleFor(x => x.Time)
                .GreaterThan(DateTime.Now).WithMessage("Durak zamanı gelecekte olmalıdır")
                .When(x => x.Time.HasValue);

            RuleFor(x => x.Index)
                .GreaterThanOrEqualTo(0).WithMessage("İndeks negatif olamaz")
                .LessThanOrEqualTo(100).WithMessage("İndeks 100'den büyük olamaz");
        }
    }
} 