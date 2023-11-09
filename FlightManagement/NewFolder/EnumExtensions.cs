namespace FlightManagement.NewFolder;
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
    public static List<string> GetPermissionDescriptions()
    {
        var permissions = Enum.GetValues(typeof(Permission));
        var descriptions = new List<string>();

        foreach (Permission permission in permissions)
        {
            FieldInfo fieldInfo = permission.GetType().GetField(permission.ToString());

            if (fieldInfo != null)
            {
                DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attributes.Length > 0)
                {
                    descriptions.Add(attributes[0].Description);
                }
            }
        }

        return descriptions;
    }

    public static class PermissionExtensions
    {
        public static Permission GetPermissionFromDescription(string description)
        {
            foreach (Permission permission in Enum.GetValues(typeof(Permission)))
            {
                if (EnumExtensions.GetPermissionDescriptions().Contains(description))
                {
                    return permission;
                }
            }
            throw new ArgumentException("No matching enum value found.", nameof(description));
        }
    }
}




