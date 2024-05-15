using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;

namespace Template.Core.Dependencies;

internal partial class DependencyManager : Node
{
    private static bool _targetMembersInitialized;
    private static readonly Dictionary<Type, IEnumerable<MemberInfo>> TargetMemberInfos = new();

    public override void _EnterTree()
    {
        if (!_targetMembersInitialized)
        {
            ScanAssemblyForTargetMembers();
            _targetMembersInitialized = true;
        }

        GetTree().Connect("node_added", Callable.From((Node node) => CheckNodeRequirementsExist(node)));
    }

    private void CheckNodeRequirementsExist(Node node)
    {
        if (node.GetScript().VariantType == Variant.Type.Nil)
            return;

        if (!TargetMemberInfos.TryGetValue(node.GetType(), out var targetMembers))
            return;

        foreach (var member in targetMembers)
        {
            var requiredChild = GetMemberValue(node, member);
            var memberType = GetMemberType(member)!;

            try
            {
                if (requiredChild is null)
                    throw new ArgumentException(
                        $"{node.GetType()} requires dependency of type {memberType} for {member.Name}");

                if (!memberType.IsInstanceOfType(requiredChild))
                    throw new InvalidCastException(
                        $"{node.GetType()} requires dependency {member.Name} to be an instance of type {memberType}");
            }
            catch (Exception ex)
            {
                GD.PushError(ex.Message);
                GetTree().Quit();
            }
        }
    }

    private static void ScanAssemblyForTargetMembers()
    {
        var validTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract)
            .Where(type => type.Assembly.GetName().Name != "GodotSharp" &&
                           !type.IsDefined(typeof(CompilerGeneratedAttribute)))
            .Where(type => type.IsSubclassOf(typeof(Node)));

        foreach (var validType in validTypes)
            RegisterTargetMembers(validType);
    }

    private static void RegisterTargetMembers(Type validType)
    {
        var members = validType.FindMembers(
            MemberTypes.Field | MemberTypes.Property,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            (member, _) => IsTargetMember(member),
            null).Where(m => m.GetCustomAttribute<RequireAttribute>() != null);

        if (members.Any())
            TargetMemberInfos[validType] = members;
    }

    private static bool IsTargetMember(MemberInfo member)
    {
        if (member.IsDefined(typeof(CompilerGeneratedAttribute)))
            return false;

        if (!member.IsDefined(typeof(RequireAttribute)) || member is not PropertyInfo property)
            return GetMemberType(member)?.IsSubclassOf(typeof(Node)) ?? false;
        
        if (!(property is { CanRead: true, CanWrite: true } && property.GetGetMethod(nonPublic: true)!
                .IsDefined(typeof(CompilerGeneratedAttribute), inherit: true)))
            throw new InvalidOperationException(
                $"{nameof(RequireAttribute)} can be used only on auto properties or fields ({member.DeclaringType}.{member.Name})");

        return GetMemberType(member)?.IsSubclassOf(typeof(Node)) ?? false;
    }
    
    private static object? GetMemberValue(Node node, MemberInfo member) =>
        member switch
        {
            FieldInfo field => field.GetValue(node),
            PropertyInfo property => property.GetValue(node),
            _ => null
        };

    private static Type? GetMemberType(MemberInfo member) =>
        member switch
        {
            FieldInfo field => field.FieldType,
            PropertyInfo property => property.PropertyType,
            _ => null,
        };
}