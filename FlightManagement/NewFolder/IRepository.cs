using FlightManagement.Models.Management_Flight;
using Microsoft.AspNetCore.Mvc;
using static FlightManagement.NewFolder.EnumExtensions;

namespace FlightManagement.NewFolder
{
    public interface IRepository
    {
        Task<IEnumerable<Groups>> SearchGroup(string name, Permission permission);
    }
}
