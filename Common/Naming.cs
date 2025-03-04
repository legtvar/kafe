using System;
using System.Collections.Generic;
using System.Text;

namespace Kafe;

public static class Naming
{
    public static string ToDashCase(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Cannot convert an empty or white-space string to dash-case.", nameof(name));
        }

        var sb = new StringBuilder(name.Length);
        sb.Append(char.ToLower(name[0]));
        for (int i = 1; i < name.Length; ++i)
        {
            if (char.IsUpper(name[i]) != char.IsUpper(name[i - 1]))
            {
                sb.Append('-');
            }
        }
        return sb.ToString();
    }

    public static string WithoutSuffix(string name, string suffix)
    {
        if (name.EndsWith(suffix))
        {
            return name[0..(name.Length - suffix.Length)];
        }

        return name;
    }
}
