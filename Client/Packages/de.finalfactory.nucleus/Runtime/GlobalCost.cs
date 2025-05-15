using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FinalFactory.Preferences")]

namespace FinalFactory
{
    public static class GlobalConst
    {
        public const MethodImplOptions ImplOptions = MethodImplOptions.AggressiveInlining
#if NETCOREAPP2_0
        | MethodImplOptions.AggressiveOptimization
#endif
            ;
    }
}