namespace FlightManagement.NewFolder;

using FlightManagement.Models.Management_Flight;
using System;
using System.ComponentModel;
using System.Reflection;
using static FlightManagement.NewFolder.EnumExtensions;

public static class EnumExtensions
{
    public enum Permission
    {
        [Description("No Permission")]
        NoPermission = 0,

        [Description("Readonly")]
        Readonly = 1,

        [Description("Read and Modify")]
        ReadAndModify = 2
    }
}




