using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using System;

namespace SOD.Common.Shadows.Implementations
{
    public sealed class Il2CppConverter
    {
        /// <summary>
        /// Convert a delegate into its Il2Cpp counter part.
        /// <br>Example: Convert&lt;Il2CppSystem.Action&lt;int&gt;&gt;((intValue) => { })</br>
        /// </summary>
        /// <typeparam name="TOutput">The IL2CPP counterpart</typeparam>
        /// <param name="del">The delegate</param>
        /// <returns></returns>
        public TOutput ConvertDelegate<TOutput>(Delegate del)
            where TOutput : Il2CppObjectBase
        {
            return DelegateSupport.ConvertDelegate<TOutput>(del);
        }
    }
}
