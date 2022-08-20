using System.Runtime.InteropServices;

namespace KarafutoCoreCSharp.Source.Misc;

public static class MarshalUtils
{
    public static void UnmanagedArrayToStructArray<T>(
        IntPtr unmanagedArray,
        int length,
        out T[] managedArray
    )
    {
        var size = Marshal.SizeOf(typeof(T));
        managedArray = new T[length];

        for (var i = 0; i < length; i++)
        {
            var ins = new IntPtr(unmanagedArray.ToInt64() + i * size);
            managedArray[i] = Marshal.PtrToStructure<T>(ins);
        }
    }
}