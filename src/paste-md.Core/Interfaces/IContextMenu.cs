using System;
using System.Runtime.InteropServices;
using System.Text;

namespace PasteMd.Core.Interfaces
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214e4-0000-0000-c000-000000000046")]
    public interface IContextMenu
    {
        [PreserveSig]
        int QueryContextMenu(
            IntPtr hmenu,
            uint indexMenu,
            uint idCmdFirst,
            uint idCmdLast,
            uint uFlags);

        [PreserveSig]
        int InvokeCommand(IntPtr pici);

        [PreserveSig]
        int GetCommandString(
            uint idCmd,
            uint uType,
            IntPtr pReserved,
            StringBuilder pszName,
            uint cchMax);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214e8-0000-0000-c000-000000000046")]
    public interface IShellExtInit
    {
        [PreserveSig]
        int Initialize(
            IntPtr pidlFolder,
            IntPtr pDataObj,
            IntPtr hKeyProgID);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct CMINVOKECOMMANDINFO
    {
        public uint cbSize;
        public uint fMask;
        public IntPtr hwnd;
        public IntPtr lpVerb;
        public string lpParameters;
        public string lpDirectory;
        public int nShow;
        public uint dwHotKey;
        public IntPtr hIcon;
    }

    public static class MenuFlags
    {
        public const uint MF_STRING = 0x00000000;
        public const uint MF_SEPARATOR = 0x00000800;
        public const uint MF_BYPOSITION = 0x00000400;
    }

    public static class GCS
    {
        public const uint GCS_VERBA = 0x00000000;
        public const uint GCS_HELPTEXTA = 0x00000001;
        public const uint GCS_VALIDATEA = 0x00000002;
        public const uint GCS_VERBW = 0x00000004;
        public const uint GCS_HELPTEXTW = 0x00000005;
        public const uint GCS_VALIDATEW = 0x00000006;
    }
}