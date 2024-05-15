using System;

namespace vcrossing2.Code.Dependencies;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class RequireAttribute : Attribute {}
