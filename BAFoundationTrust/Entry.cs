using System.Reflection;
using System.Runtime.InteropServices;

namespace BAFoundationTrust
{
    public static class Entry
    {
        [DllImport("User32.dll")]
        static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);
        public static int Main(string[] Arguments)
        {
            int result = Worker.Run(Arguments);
            if (result != 0)
            {
                _ = MessageBox(
                    IntPtr.Zero,
                    $"{Strings.Prefix}\r\n\r\n{result}: {Strings.ResourceManager.GetString($"Error_{result}")!}\r\n\r\n{Strings.URL}",
                    Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>()!.Product,
                    0x30
                );
            }
            return result;
        }
    }
}