using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Utils
{
    public static class API
    {
        public const uint MAX_PATH = 255;

        /// https://www.pinvoke.net/
        /// https://docs.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-getshortpathnamew
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetShortPathName(
           [MarshalAs(UnmanagedType.LPTStr)]
           string lpszLongPath,
           [MarshalAs(UnmanagedType.LPTStr)]
           StringBuilder lpszShortPath,
           uint cchBuffer);
    }
}
