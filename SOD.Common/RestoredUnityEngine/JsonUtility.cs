using UnityEngine;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;

namespace SOD.Common
{
    public class RestoredUnityEngine
    {
        public static class JsonUtility
        {
            private delegate System.IntPtr Delegate_FromJsonInternal(System.IntPtr json, System.IntPtr scriptableObject, System.IntPtr type);
            private static Delegate_FromJsonInternal _iCallFromJsonInternal;

            public static T FromJsonInternal<T>(string json, T scriptableObject) where T : UnityEngine.Object
            {
                _iCallFromJsonInternal ??= IL2CPP.ResolveICall<Delegate_FromJsonInternal>("UnityEngine.JsonUtility::FromJsonInternal");
                System.IntPtr jsonPtr = IL2CPP.ManagedStringToIl2Cpp(json);
                System.IntPtr scriptableObjectPtr = (System.IntPtr)typeof(T).GetMethod("get_Pointer").Invoke(scriptableObject, null);
                System.IntPtr typePtr = Il2CppType.Of<T>().Pointer;
                var newPointer = _iCallFromJsonInternal.Invoke(jsonPtr, scriptableObjectPtr, typePtr);
                var newSO = new Il2CppObjectBase(newPointer).TryCast<T>();

                return newSO;
            }

            private delegate System.IntPtr Delegate_ToJsonInternal(System.IntPtr json, bool prettyPrint);
            private static Delegate_ToJsonInternal _iCallToJsonInternal;

            public static string ToJsonInternal(Il2CppObjectBase obj, bool prettyPrint)
            {
                _iCallToJsonInternal ??= IL2CPP.ResolveICall<Delegate_ToJsonInternal>("UnityEngine.JsonUtility::ToJsonInternal");
                System.IntPtr scriptableObjectPtr = (System.IntPtr)typeof(ScriptableObject).GetMethod("get_Pointer").Invoke(obj, null);

                var newPointer = _iCallToJsonInternal.Invoke(scriptableObjectPtr, prettyPrint);
                var newStr = IL2CPP.Il2CppStringToManaged(newPointer);

                return newStr;
            }
        }
    }
}
