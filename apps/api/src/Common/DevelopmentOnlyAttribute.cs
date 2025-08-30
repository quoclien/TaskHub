using Microsoft.AspNetCore.Mvc;

namespace TaskHub.Api.Common;

/// <summary>
/// Specifies that a controller or action is only available in the Development environment.
/// This is a marker attribute used by the DevelopmentOnlyFilter.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class DevelopmentOnlyAttribute : Attribute
{
}
