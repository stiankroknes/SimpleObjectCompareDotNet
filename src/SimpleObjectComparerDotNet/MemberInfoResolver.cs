using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleObjectComparerDotNet;

internal static class MemberInfoResolver
{
    internal static IReadOnlyList<MemberInfo> ResolveByFields(Type type, SimpleObjectComparerOptions config)
    {
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);

        if (fields.Length == 0)
        {
            return Array.Empty<MemberInfo>();
        }

        var memberInfos = new List<MemberInfo>();

        foreach (var field in fields)
        {
            if (config.ExcludeMembersWithName.Contains(field.Name))
            {
                continue;
            }
            else if (config.FilterMembersByAttributes && !config.IncludeMembersWithAttribute.Any(atributeType => Attribute.IsDefined(field, atributeType)))
            {
                continue;
            }
            else if (config.RespectIgnoreDataMember && Attribute.IsDefined(field, typeof(System.Runtime.Serialization.IgnoreDataMemberAttribute)))
            {
                continue;
            }

            memberInfos.Add(field);
        }

        return memberInfos;
    }

    internal static IReadOnlyList<MemberInfo> ResolveByProperties(Type type, SimpleObjectComparerOptions config)
    {
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

        if (properties.Length == 0)
        {
            return Array.Empty<MemberInfo>();
        }

        var memberInfos = new List<MemberInfo>();

        foreach (var property in properties)
        {
            if (property.GetIndexParameters().Any())
            {
                continue;
            }
            else if (config.ExcludeMembersWithName.Contains(property.Name))
            {
                continue;
            }
            else if (config.FilterMembersByAttributes && !config.IncludeMembersWithAttribute.Any(atributeType => Attribute.IsDefined(property, atributeType)))
            {
                continue;
            }
            else if (config.RespectIgnoreDataMember && Attribute.IsDefined(property, typeof(System.Runtime.Serialization.IgnoreDataMemberAttribute)))
            {
                continue;
            }

            memberInfos.Add(property);
        }

        return memberInfos;
    }
}