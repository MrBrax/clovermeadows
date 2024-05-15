using System;
using Godot;

namespace Template.Core.Dependencies;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class RequireAttribute : Attribute {}