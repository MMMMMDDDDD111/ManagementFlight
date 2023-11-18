using AutoMapper;
using FlightManagement.Models.Management_Flight;

namespace FlightManagement.AllProfile
{
    public class AllProfile : Profile
    {
        public AllProfile() 
        {
            CreateMap<AddFlightDTO, AddFlight>();
        }
    }
}
