#if NETSTANDARD2_0

using System;
using System.Runtime.CompilerServices;

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Polyfill of the attribute that is not available before .NET 6; the compiler binds to it by name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class CallerArgumentExpressionAttribute : Attribute
    {
        public CallerArgumentExpressionAttribute(string parameterName)
        {
            ParameterName = parameterName;
        }

        public string ParameterName { get; }
    }
}
#endif
