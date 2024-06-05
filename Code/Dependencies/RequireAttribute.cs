using System;

namespace vcrossing.Code.Dependencies;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class RequireAttribute : Attribute {}
