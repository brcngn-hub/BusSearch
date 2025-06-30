using Domain.Entities;
using Application.DTOs;

namespace Application.Mappings
{
    public static class EntityMappings
    {
        public static BusTourDto ToDto(this BusTour entity)
        {
            return new BusTourDto
            {
                Name = entity.Name,
                Description = entity.Description,
                Price = entity.Price,
                AvailableSeats = entity.AvailableSeats,
                DepartureDate = entity.DepartureDate,
                ReturnDate = entity.ReturnDate,
                DepartureLocation = entity.DepartureLocation,
                Destination = entity.Destination,
                IsActive = entity.IsActive
            };
        }

        public static BusTour ToEntity(this BusTourDto dto)
        {
            return new BusTour
            {
                Name = dto.Name ?? string.Empty,
                Description = dto.Description ?? string.Empty,
                Price = dto.Price,
                AvailableSeats = dto.AvailableSeats,
                DepartureDate = dto.DepartureDate,
                ReturnDate = dto.ReturnDate,
                DepartureLocation = dto.DepartureLocation ?? string.Empty,
                Destination = dto.Destination ?? string.Empty,
                IsActive = dto.IsActive
            };
        }

        public static BusLocationDto ToDto(this BusLocation entity)
        {
            return new BusLocationDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Country = entity.Country,
                City = entity.City
            };
        }

        public static BusLocation ToEntity(this BusLocationDto dto)
        {
            return new BusLocation
            {
                Id = dto.Id ?? string.Empty,
                Name = dto.Name ?? string.Empty,
                Country = dto.Country ?? string.Empty,
                City = dto.City ?? string.Empty
            };
        }

        public static BusStopDto ToDto(this BusStop entity)
        {
            return new BusStopDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Station = entity.Station,
                Time = entity.Time,
                IsOrigin = entity.IsOrigin,
                IsDestination = entity.IsDestination,
                Index = entity.Index
            };
        }

        public static BusStop ToEntity(this BusStopDto dto)
        {
            return new BusStop
            {
                Id = dto.Id,
                Name = dto.Name,
                Station = dto.Station,
                Time = dto.Time,
                IsOrigin = dto.IsOrigin,
                IsDestination = dto.IsDestination,
                Index = dto.Index
            };
        }
    }
} 