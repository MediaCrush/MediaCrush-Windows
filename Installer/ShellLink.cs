﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace vbAccelerator.Components.Shell
{
   #region ShellLink Object
   /// <summary>
   /// Summary description for ShellLink.
   /// </summary>
   public class ShellLink : IDisposable
   {
      #region ComInterop for IShellLink

      #region IPersist Interface
      [ComImportAttribute()]
      [GuidAttribute("0000010C-0000-0000-C000-000000000046")]
      [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
      private interface IPersist
      {
         [PreserveSig]
         void GetClassID(out Guid pClassID);
      }
      #endregion

      #region IPersistFile Interface
      [ComImportAttribute()]
      [GuidAttribute("0000010B-0000-0000-C000-000000000046")]
      [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
      private interface IPersistFile
      {
         // can't get this to go if I extend IPersist, so put it here:
         [PreserveSig]
         void GetClassID(out Guid pClassID);

         void IsDirty();

         void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);

         void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fRemember);

         void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);

         void GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
      }
      #endregion

      #region IShellLink Interface
      [ComImportAttribute()]
      [GuidAttribute("000214EE-0000-0000-C000-000000000046")]
      [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
      private interface IShellLinkA
      {
         void GetPath([Out(), MarshalAs(UnmanagedType.LPStr)] StringBuilder pszFile, int cchMaxPath, ref _WIN32_FIND_DATAA pfd, uint fFlags);

         void GetIDList(out IntPtr ppidl);

         void SetIDList(IntPtr pidl);

         void GetDescription([Out(), MarshalAs(UnmanagedType.LPStr)] StringBuilder pszFile, int cchMaxName);
      
         void SetDescription([MarshalAs(UnmanagedType.LPStr)] string pszName);

         void GetWorkingDirectory([Out(), MarshalAs(UnmanagedType.LPStr)] StringBuilder pszDir, int cchMaxPath);

         void SetWorkingDirectory([MarshalAs(UnmanagedType.LPStr)] string pszDir);

         void GetArguments([Out(), MarshalAs(UnmanagedType.LPStr)] StringBuilder pszArgs, int cchMaxPath);

         void SetArguments([MarshalAs(UnmanagedType.LPStr)] string pszArgs);

         void GetHotkey(out short pwHotkey);

         void SetHotkey(short pwHotkey);

         void GetShowCmd(out uint piShowCmd);

         void SetShowCmd(uint piShowCmd);

         void GetIconLocation([Out(), MarshalAs(UnmanagedType.LPStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
      
         void SetIconLocation([MarshalAs(UnmanagedType.LPStr)] string pszIconPath, int iIcon);

         void SetRelativePath([MarshalAs(UnmanagedType.LPStr)] string pszPathRel, uint dwReserved);

         void Resolve(IntPtr hWnd, uint fFlags);

         void SetPath([MarshalAs(UnmanagedType.LPStr)] string pszFile);
      }


      [ComImportAttribute()]
      [GuidAttribute("000214F9-0000-0000-C000-000000000046")]
      [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
      private interface IShellLinkW
      {
         void GetPath([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, ref _WIN32_FIND_DATAW pfd, uint fFlags);

         void GetIDList(out IntPtr ppidl);

         void SetIDList(IntPtr pidl);

         void GetDescription([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile,int cchMaxName);
      
         void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);

         void GetWorkingDirectory([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir,int cchMaxPath);

         void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);

         void GetArguments([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);

         void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

         void GetHotkey(out short pwHotkey);
         void SetHotkey(short pwHotkey);

         void GetShowCmd(out uint piShowCmd);
         void SetShowCmd(uint piShowCmd);

         void GetIconLocation([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
      
         void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);

         void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);

         void Resolve(IntPtr hWnd, uint fFlags);

         void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
      }
      #endregion

      #region ShellLinkCoClass
      [GuidAttribute("00021401-0000-0000-C000-000000000046")]
      [ClassInterfaceAttribute(ClassInterfaceType.None)]
      [ComImportAttribute()]
      private class CShellLink{}

      #endregion
   
      #region Private IShellLink enumerations
      private enum EShellLinkGP : uint
      {
         SLGP_SHORTPATH = 1,
         SLGP_UNCPRIORITY = 2
      }

      [Flags]
      private enum EShowWindowFlags : uint
      {
         SW_HIDE = 0,
         SW_SHOWNORMAL = 1,
         SW_NORMAL = 1,
         SW_SHOWMINIMIZED = 2,
         SW_SHOWMAXIMIZED = 3,
         SW_MAXIMIZE = 3,
         SW_SHOWNOACTIVATE = 4,
         SW_SHOW = 5,
         SW_MINIMIZE = 6,
         SW_SHOWMINNOACTIVE = 7,
         SW_SHOWNA = 8,
         SW_RESTORE = 9,
         SW_SHOWDEFAULT = 10,
         SW_MAX = 10
      }
      #endregion

      #region IShellLink Private structs

      [StructLayoutAttribute(LayoutKind.Sequential, Pack=4, Size=0,
       CharSet=CharSet.Unicode)]
      private struct _WIN32_FIND_DATAW
      {
         public uint dwFileAttributes;
         public _FILETIME ftCreationTime;
         public _FILETIME ftLastAccessTime;
         public _FILETIME ftLastWriteTime;
         public uint nFileSizeHigh;
         public uint nFileSizeLow;
         public uint dwReserved0;
         public uint dwReserved1;
         [MarshalAs(UnmanagedType.ByValTStr , SizeConst = 260)] // MAX_PATH
         public string cFileName;
         [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
         public string cAlternateFileName;
      }

      [StructLayoutAttribute(LayoutKind.Sequential, Pack=4, Size=0,
       CharSet=CharSet.Ansi)]
      private struct _WIN32_FIND_DATAA
      {
         public uint dwFileAttributes;
         public _FILETIME ftCreationTime;
         public _FILETIME ftLastAccessTime;
         public _FILETIME ftLastWriteTime;
         public uint nFileSizeHigh;
         public uint nFileSizeLow;
         public uint dwReserved0;
         public uint dwReserved1;
         [MarshalAs(UnmanagedType.ByValTStr , SizeConst = 260)] // MAX_PATH
         public string cFileName;
         [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
         public string cAlternateFileName;
      }

      [StructLayoutAttribute(LayoutKind.Sequential, Pack=4, Size=0)]
      private struct _FILETIME 
      {
         public uint dwLowDateTime;
         public uint dwHighDateTime;
      }  
      #endregion   

      #region UnManaged Methods
      private class UnManagedMethods
      {
         [DllImport("Shell32", CharSet=CharSet.Auto)]
         internal extern static int ExtractIconEx (
            [MarshalAs(UnmanagedType.LPTStr)] 
            string lpszFile,
            int nIconIndex,
            IntPtr[] phIconLarge, 
            IntPtr[] phIconSmall,
            int nIcons);

         [DllImport("user32")]
         internal static extern int DestroyIcon(IntPtr hIcon);
      }
      #endregion

      #endregion

      #region Enumerations
      /// <summary>
      /// Flags determining how the links with missing
      /// targets are resolved.
      /// </summary>
      [Flags]
      public enum EShellLinkResolveFlags : uint
      {
         /// <summary>
         /// Allow any match during resolution.  Has no effect
         /// on ME/2000 or above, use the other flags instead.
         /// </summary>
         SLR_ANY_MATCH = 0x2,
         /// <summary>
         /// Call the Microsoft Windows Installer. 
         /// </summary>
         SLR_INVOKE_MSI = 0x80,
         /// <summary>
         /// Disable distributed link tracking. By default, 
         /// distributed link tracking tracks removable media 
         /// across multiple devices based on the volume name. 
         /// It also uses the UNC path to track remote file 
         /// systems whose drive letter has changed. Setting 
         /// SLR_NOLINKINFO disables both types of tracking.
         /// </summary>
         SLR_NOLINKINFO = 0x40,
         /// <summary>
         /// Do not display a dialog box if the link cannot be resolved. 
         /// When SLR_NO_UI is set, a time-out value that specifies the 
         /// maximum amount of time to be spent resolving the link can 
         /// be specified in milliseconds. The function returns if the 
         /// link cannot be resolved within the time-out duration. 
         /// If the timeout is not set, the time-out duration will be 
         /// set to the default value of 3,000 milliseconds (3 seconds). 
         /// </summary>                                  
         SLR_NO_UI = 0x1,
         /// <summary>
         /// Not documented in SDK.  Assume same as SLR_NO_UI but 
         /// intended for applications without a hWnd.
         /// </summary>
         SLR_NO_UI_WITH_MSG_PUMP = 0x101,
         /// <summary>
         /// Do not update the link information. 
         /// </summary>
         SLR_NOUPDATE = 0x8,
         /// <summary>
         /// Do not execute the search heuristics. 
         /// </summary>                                                        
                                                                               
                                                                               
                                                                               
                                                                               
                                                                               
                                                                               
                                                                               
                                                                               
           
         SLR_NOSEARCH = 0x10,
         /// <summary>
         /// Do not use distributed link tracking. 
         /// </summary>
         SLR_NOTRACK = 0x20,
         /// <summary>
         /// If the link object has changed, update its path and list 
         /// of identifiers. If SLR_UPDATE is set, you do not need to 
         /// call IPersistFile::IsDirty to determine whether or not 
         /// the link object has changed. 
         /// </summary>
         SLR_UPDATE  = 0x4
      }

      public enum LinkDisplayMode : uint
      {
         edmNormal = EShowWindowFlags.SW_NORMAL,
         edmMinimized = EShowWindowFlags.SW_SHOWMINNOACTIVE,
         edmMaximized = EShowWindowFlags.SW_MAXIMIZE
      }
      #endregion

      #region Member Variables
      // Use Unicode (W) under NT, otherwise use ANSI      
      private IShellLinkW linkW;
      private IShellLinkA linkA;
      private string shortcutFile = "";
      #endregion

      #region Constructor
      /// <summary>
      /// Creates an instance of the Shell Link object.
      /// </summary>
      public ShellLink()
      {
         if (System.Environment.OSVersion.Platform == PlatformID.Win32NT)
         {
            linkW = (IShellLinkW)new CShellLink();
         }
         else
         {
            linkA = (IShellLinkA)new CShellLink();
         }
      }

      /// <summary>
      /// Creates an instance of a Shell Link object
      /// from the specified link file
      /// </summary>
      /// <param name="linkFile">The Shortcut file to open</param>
      public ShellLink(string linkFile) : this()
      {
         Open(linkFile);
      }
      #endregion

      #region Destructor and Dispose
      /// <summary>
      /// Call dispose just in case it hasn't happened yet
      /// </summary>
      ~ShellLink()
      {
         Dispose();
      }

      /// <summary>
      /// Dispose the object, releasing the COM ShellLink object
      /// </summary>
      public void Dispose()
      {
         if (linkW != null ) 
         {
            Marshal.ReleaseComObject(linkW);
            linkW = null;
         }
         if (linkA != null)
         {
            Marshal.ReleaseComObject(linkA);
            linkA = null;
         }
      }
      #endregion

      #region Implementation
      public string ShortCutFile
      {
         get
         {
            return this.shortcutFile;
         }
         set
         {
            this.shortcutFile = value;
         }
      }

      /// <summary>
      /// Gets a System.Drawing.Icon containing the icon for this
      /// ShellLink object.
      /// </summary>
      public Icon LargeIcon
      {
         get
         {
            return getIcon(true);
         }
      }

      public Icon SmallIcon
      {
         get
         {
            return getIcon(false);
         }
      }

      private Icon getIcon(bool large)
      {
         // Get icon index and path:
         int iconIndex = 0;
         StringBuilder iconPath = new StringBuilder(260, 260);
         if (linkA == null)
         {
            linkW.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
         }
         else
         {
            linkA.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
         }
         string iconFile = iconPath.ToString();

         // If there are no details set for the icon, then we must use
         // the shell to get the icon for the target:
         if (iconFile.Length == 0)
         {
            // Use the FileIcon object to get the icon:
            FileIcon.SHGetFileInfoConstants flags =
             FileIcon.SHGetFileInfoConstants.SHGFI_ICON |
               FileIcon.SHGetFileInfoConstants.SHGFI_ATTRIBUTES;
            if (large)
            {
               flags = flags | FileIcon.SHGetFileInfoConstants.SHGFI_LARGEICON;
            }
            else
            {
               flags = flags | FileIcon.SHGetFileInfoConstants.SHGFI_SMALLICON;
            }
            FileIcon fileIcon = new FileIcon(Target, flags);
            return fileIcon.ShellIcon;
         }
         else
         {
            // Use ExtractIconEx to get the icon:
            IntPtr[] hIconEx = new IntPtr[1] {IntPtr.Zero};         
            int iconCount = 0;
            if (large)
            {
               iconCount = UnManagedMethods.ExtractIconEx(
                  iconFile,
                  iconIndex,
                  hIconEx,
                  null,
                  1);
            }
            else
            {
               iconCount = UnManagedMethods.ExtractIconEx(
                  iconFile,
                  iconIndex,
                  null,
                  hIconEx,
                  1);
            }
            // If success then return as a GDI+ object
            Icon icon = null;
            if (hIconEx[0] != IntPtr.Zero)
            {
               icon = Icon.FromHandle(hIconEx[0]);
               //UnManagedMethods.DestroyIcon(hIconEx[0]);
            }
            return icon;
         }            
      }

      /// <summary>
      /// Gets the path to the file containing the icon for this shortcut.
      /// </summary>
      public string IconPath
      {
         get
         {
            StringBuilder iconPath = new StringBuilder(260, 260);
            int iconIndex = 0;
            if (linkA == null)
            {
               linkW.GetIconLocation(iconPath, iconPath.Capacity, out
                iconIndex);
            }
            else
            {
               linkA.GetIconLocation(iconPath, iconPath.Capacity, out
                iconIndex);
            }
            return iconPath.ToString();
         }
         set
         {
            StringBuilder iconPath = new StringBuilder(260, 260);
            int iconIndex = 0;
            if (linkA == null)
            {
               linkW.GetIconLocation(iconPath, iconPath.Capacity, out
                iconIndex);
            }
            else
            {
               linkA.GetIconLocation(iconPath, iconPath.Capacity, out
                iconIndex);
            }
            if (linkA == null)
            {
               linkW.SetIconLocation(value, iconIndex);
            }
            else
            {
               linkA.SetIconLocation(value, iconIndex);
            }
         }
      }

      /// <summary>
      /// Gets the index of this icon within the icon path's resources
      /// </summary>
      public int IconIndex
      {
         get
         {
            StringBuilder iconPath = new StringBuilder(260, 260);
            int iconIndex = 0;
            if (linkA == null)
            {
               linkW.GetIconLocation(iconPath, iconPath.Capacity, out
                iconIndex);
            }
            else
            {
               linkA.GetIconLocation(iconPath, iconPath.Capacity, out
                iconIndex);
            }
            return iconIndex;
         }
         set
         {
            StringBuilder iconPath = new StringBuilder(260, 260);
            int iconIndex = 0;
            if (linkA == null)
            {
               linkW.GetIconLocation(iconPath, iconPath.Capacity, out
                iconIndex);
            }
            else
            {
               linkA.GetIconLocation(iconPath, iconPath.Capacity, out
                iconIndex);
            }
            if (linkA == null)
            {
               linkW.SetIconLocation(iconPath.ToString(), value);
            }
            else
            {
               linkA.SetIconLocation(iconPath.ToString(), value);
            }
         }
      }

      /// <summary>
      /// Gets/sets the fully qualified path to the link's target
      /// </summary>
      public string Target
      {
         get
         {      
            StringBuilder target = new StringBuilder(260, 260);
            if (linkA == null)
            {
               _WIN32_FIND_DATAW fd = new _WIN32_FIND_DATAW();
               linkW.GetPath(target, target.Capacity, ref fd,
                (uint)EShellLinkGP.SLGP_UNCPRIORITY);
            }
            else
            {
               _WIN32_FIND_DATAA fd = new _WIN32_FIND_DATAA();
               linkA.GetPath(target, target.Capacity, ref fd,
                (uint)EShellLinkGP.SLGP_UNCPRIORITY);
            }
            return target.ToString();
         }
         set
         {
            if (linkA == null)
            {
               linkW.SetPath(value);
            }
            else
            {
               linkA.SetPath(value);
            }
         }
      }

      /// <summary>
      /// Gets/sets the Working Directory for the Link
      /// </summary>
      public string WorkingDirectory
      {
         get
         {
            StringBuilder path = new StringBuilder(260, 260);
            if (linkA == null)
            {
               linkW.GetWorkingDirectory(path, path.Capacity);
            }
            else
            {
               linkA.GetWorkingDirectory(path, path.Capacity);
            }
            return path.ToString();
         }
         set
         {
            if (linkA == null)
            {
               linkW.SetWorkingDirectory(value);   
            }
            else
            {
               linkA.SetWorkingDirectory(value);
            }
         }
      }

      /// <summary>
      /// Gets/sets the description of the link
      /// </summary>
      public string Description
      {
         get
         {
            StringBuilder description = new StringBuilder(1024, 1024);
            if (linkA == null)
            {
               linkW.GetDescription(description, description.Capacity);
            }
            else
            {
               linkA.GetDescription(description, description.Capacity);
            }
            return description.ToString();
         }
         set
         {
            if (linkA == null)
            {
               linkW.SetDescription(value);
            }
            else
            {
               linkA.SetDescription(value);
            }
         }
      }

      /// <summary>
      /// Gets/sets any command line arguments associated with the link
      /// </summary>
      public string Arguments
      {
         get
         {            
            StringBuilder arguments = new StringBuilder(260, 260);
            if (linkA == null)
            {
               linkW.GetArguments(arguments, arguments.Capacity);
            }
            else
            {
               linkA.GetArguments(arguments, arguments.Capacity);
            }
            return arguments.ToString();
         }
         set
         {
            if (linkA == null)
            {
               linkW.SetArguments(value);
            }
            else
            {
               linkA.SetArguments(value);
            }
         }
      }

      /// <summary>
      /// Gets/sets the initial display mode when the shortcut is
      /// run
      /// </summary>
      public LinkDisplayMode DisplayMode
      {
         get
         {
            uint cmd = 0;
            if (linkA == null)
            {
               linkW.GetShowCmd(out cmd);
            }
            else
            {
               linkA.GetShowCmd(out cmd);
            }
            return (LinkDisplayMode)cmd;
         }
         set
         {
            if (linkA == null)
            {
               linkW.SetShowCmd((uint)value);
            }
            else
            {
               linkA.SetShowCmd((uint)value);
            }
         }
      }

      /// <summary>
      /// Gets/sets the HotKey to start the shortcut (if any)
      /// </summary>
      public Keys HotKey
      {
         get
         {
            short key = 0;
            if (linkA == null)
            {
               linkW.GetHotkey(out key);
            }
            else
            {
               linkA.GetHotkey(out key);
            }
            return (Keys)key;
         }
         set
         {
            if (linkA == null)
            {
               linkW.SetHotkey((short)value);
            }
            else
            {
               linkA.SetHotkey((short)value);
            }
         }
      }

      /// <summary>
      /// Saves the shortcut to ShortCutFile.
      /// </summary>
      public void Save()
      {
         Save(shortcutFile);
      }

      /// <summary>
      /// Saves the shortcut to the specified file
      /// </summary>
      /// <param name="linkFile">The shortcut file (.lnk)</param>
      public void Save(
         string linkFile
         )
      {   
         // Save the object to disk
         if (linkA == null)
         {
            ((IPersistFile)linkW).Save(linkFile, true);
            shortcutFile = linkFile;
         }
         else
         {
            ((IPersistFile)linkA).Save(linkFile, true);
            shortcutFile = linkFile;
         }
      }

      /// <summary>
      /// Loads a shortcut from the specified file
      /// </summary>
      /// <param name="linkFile">The shortcut file (.lnk) to load</param>
      public void Open(
         string linkFile         
         )
      {
         Open(linkFile, 
            IntPtr.Zero, 
            (EShellLinkResolveFlags.SLR_ANY_MATCH |
             EShellLinkResolveFlags.SLR_NO_UI),
            1);
      }
      
      /// <summary>
      /// Loads a shortcut from the specified file, and allows flags controlling
      /// the UI behaviour if the shortcut's target isn't found to be set.
      /// </summary>
      /// <param name="linkFile">The shortcut file (.lnk) to load</param>
      /// <param name="hWnd">The window handle of the application's UI, if any</param>
      /// <param name="resolveFlags">Flags controlling resolution behaviour</param>
      public void Open(
         string linkFile, 
         IntPtr hWnd, 
         EShellLinkResolveFlags resolveFlags
         )
      {
         Open(linkFile, 
            hWnd, 
            resolveFlags, 
            1);
      }

      /// <summary>
      /// Loads a shortcut from the specified file, and allows flags controlling
      /// the UI behaviour if the shortcut's target isn't found to be set.  If
      /// no SLR_NO_UI is specified, you can also specify a timeout.
      /// </summary>
      /// <param name="linkFile">The shortcut file (.lnk) to load</param>
      /// <param name="hWnd">The window handle of the application's UI, if any</param>
      /// <param name="resolveFlags">Flags controlling resolution behaviour</param>
      /// <param name="timeOut">Timeout if SLR_NO_UI is specified, in ms.</param>
      public void Open(
         string linkFile,
         IntPtr hWnd, 
         EShellLinkResolveFlags resolveFlags,
         ushort timeOut
         )
      {
         uint flags;

         if ((resolveFlags & EShellLinkResolveFlags.SLR_NO_UI) 
            == EShellLinkResolveFlags.SLR_NO_UI)
         {
            flags = (uint)((int)resolveFlags | (timeOut << 16));
         }
         else
         {
            flags = (uint)resolveFlags;
         }

         if (linkA == null)
         {
            ((IPersistFile)linkW).Load(linkFile, 0); //STGM_DIRECT)
            linkW.Resolve(hWnd, flags);
            this.shortcutFile = linkFile;
         }
         else
         {
            ((IPersistFile)linkA).Load(linkFile, 0); //STGM_DIRECT)
            linkA.Resolve(hWnd, flags);
            this.shortcutFile = linkFile;
         }
      }
      #endregion
   }
   #endregion

   /// <summary>
   /// Enables extraction of icons for any file type from
   /// the Shell.
   /// </summary>
   public class FileIcon
   {

       #region UnmanagedCode
       private const int MAX_PATH = 260;

       [StructLayout(LayoutKind.Sequential)]
       private struct SHFILEINFO
       {
           public IntPtr hIcon;
           public int iIcon;
           public int dwAttributes;
           [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
           public string szDisplayName;
           [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
           public string szTypeName;
       }

       [DllImport("shell32")]
       private static extern int SHGetFileInfo(
          string pszPath,
          int dwFileAttributes,
          ref SHFILEINFO psfi,
          uint cbFileInfo,
          uint uFlags);

       [DllImport("user32.dll")]
       private static extern int DestroyIcon(IntPtr hIcon);

       private const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;
       private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;
       private const int FORMAT_MESSAGE_FROM_HMODULE = 0x800;
       private const int FORMAT_MESSAGE_FROM_STRING = 0x400;
       private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
       private const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
       private const int FORMAT_MESSAGE_MAX_WIDTH_MASK = 0xFF;
       [DllImport("kernel32")]
       private extern static int FormatMessage(
          int dwFlags,
          IntPtr lpSource,
          int dwMessageId,
          int dwLanguageId,
          string lpBuffer,
          uint nSize,
          int argumentsLong);

       [DllImport("kernel32")]
       private extern static int GetLastError();
       #endregion

       #region Member Variables
       private string fileName;
       private string displayName;
       private string typeName;
       private SHGetFileInfoConstants flags;
       private Icon fileIcon;
       #endregion

       #region Enumerations
       [Flags]
       public enum SHGetFileInfoConstants : int
       {
           SHGFI_ICON = 0x100,                // get icon 
           SHGFI_DISPLAYNAME = 0x200,         // get display name 
           SHGFI_TYPENAME = 0x400,            // get type name 
           SHGFI_ATTRIBUTES = 0x800,          // get attributes 
           SHGFI_ICONLOCATION = 0x1000,       // get icon location 
           SHGFI_EXETYPE = 0x2000,            // return exe type 
           SHGFI_SYSICONINDEX = 0x4000,       // get system icon index 
           SHGFI_LINKOVERLAY = 0x8000,        // put a link overlay on icon 
           SHGFI_SELECTED = 0x10000,          // show icon in selected state 
           SHGFI_ATTR_SPECIFIED = 0x20000,    // get only specified attributes 
           SHGFI_LARGEICON = 0x0,             // get large icon 
           SHGFI_SMALLICON = 0x1,             // get small icon 
           SHGFI_OPENICON = 0x2,              // get open icon 
           SHGFI_SHELLICONSIZE = 0x4,         // get shell size icon 
           //SHGFI_PIDL = 0x8,                  // pszPath is a pidl 
           SHGFI_USEFILEATTRIBUTES = 0x10,     // use passed dwFileAttribute 
           SHGFI_ADDOVERLAYS = 0x000000020,     // apply the appropriate overlays
           SHGFI_OVERLAYINDEX = 0x000000040     // Get the index of the overlay
       }
       #endregion

       #region Implementation
       /// <summary>
       /// Gets/sets the flags used to extract the icon
       /// </summary>
       public FileIcon.SHGetFileInfoConstants Flags
       {
           get
           {
               return flags;
           }
           set
           {
               flags = value;
           }
       }

       /// <summary>
       /// Gets/sets the filename to get the icon for
       /// </summary>
       public string FileName
       {
           get
           {
               return fileName;
           }
           set
           {
               fileName = value;
           }
       }

       /// <summary>
       /// Gets the icon for the chosen file
       /// </summary>
       public Icon ShellIcon
       {
           get
           {
               return fileIcon;
           }
       }

       /// <summary>
       /// Gets the display name for the selected file
       /// if the SHGFI_DISPLAYNAME flag was set.
       /// </summary>
       public string DisplayName
       {
           get
           {
               return displayName;
           }
       }

       /// <summary>
       /// Gets the type name for the selected file
       /// if the SHGFI_TYPENAME flag was set.
       /// </summary>
       public string TypeName
       {
           get
           {
               return typeName;
           }
       }

       /// <summary>
       ///  Gets the information for the specified 
       ///  file name and flags.
       /// </summary>
       public void GetInfo()
       {
           fileIcon = null;
           typeName = "";
           displayName = "";

           SHFILEINFO shfi = new SHFILEINFO();
           uint shfiSize = (uint)Marshal.SizeOf(shfi.GetType());

           int ret = SHGetFileInfo(
              fileName, 0, ref shfi, shfiSize, (uint)(flags));
           if (ret != 0)
           {
               if (shfi.hIcon != IntPtr.Zero)
               {
                   fileIcon = System.Drawing.Icon.FromHandle(shfi.hIcon);
                   // Now owned by the GDI+ object
                   //DestroyIcon(shfi.hIcon);
               }
               typeName = shfi.szTypeName;
               displayName = shfi.szDisplayName;
           }
           else
           {

               int err = GetLastError();
               Console.WriteLine("Error {0}", err);
               string txtS = new string('\0', 256);
               int len = FormatMessage(
                  FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
                  IntPtr.Zero, err, 0, txtS, 256, 0);
               Console.WriteLine("Len {0} text {1}", len, txtS);

               // throw exception

           }
       }

       /// <summary>
       /// Constructs a new, default instance of the FileIcon
       /// class.  Specify the filename and call GetInfo()
       /// to retrieve an icon.
       /// </summary>
       public FileIcon()
       {
           flags = SHGetFileInfoConstants.SHGFI_ICON |
              SHGetFileInfoConstants.SHGFI_DISPLAYNAME |
              SHGetFileInfoConstants.SHGFI_TYPENAME |
              SHGetFileInfoConstants.SHGFI_ATTRIBUTES |
              SHGetFileInfoConstants.SHGFI_EXETYPE;
       }
       /// <summary>
       /// Constructs a new instance of the FileIcon class
       /// and retrieves the icon, display name and type name
       /// for the specified file.      
       /// </summary>
       /// <param name="fileName">The filename to get the icon, 
       /// display name and type name for</param>
       public FileIcon(string fileName)
           : this()
       {
           this.fileName = fileName;
           GetInfo();
       }
       /// <summary>
       /// Constructs a new instance of the FileIcon class
       /// and retrieves the information specified in the 
       /// flags.
       /// </summary>
       /// <param name="fileName">The filename to get information
       /// for</param>
       /// <param name="flags">The flags to use when extracting the
       /// icon and other shell information.</param>
       public FileIcon(string fileName, FileIcon.SHGetFileInfoConstants flags)
       {
           this.fileName = fileName;
           this.flags = flags;
           GetInfo();
       }

       #endregion
   }

   class ShellNotification
   {
       /// <summary>
       /// Notifies the system of an event that an application has performed. An application should use this function if it performs an action that may affect the Shell. 
       /// </summary>
       /// <param name="wEventId">Describes the event that has occurred. The ShellChangeNotificationEvents enum contains a list of options.</param>
       /// <param name="uFlags">Flags that indicate the meaning of the dwItem1 and dwItem2 parameters.</param>
       /// <param name="dwItem1">First event-dependent value.</param>
       /// <param name="dwItem2">Second event-dependent value.</param>
       [DllImport("shell32.dll")]
       private static extern void SHChangeNotify(
           UInt32 wEventId,
           UInt32 uFlags,
           IntPtr dwItem1,
           IntPtr dwItem2);

       /// <summary>
       /// Notify shell of change of file associations.
       /// </summary>
       public static void NotifyOfChange()
       {
           SHChangeNotify(
               (uint)ShellChangeNotificationEvents.SHCNE_ASSOCCHANGED,
               (uint)(ShellChangeNotificationFlags.SHCNF_IDLIST | ShellChangeNotificationFlags.SHCNF_FLUSHNOWAIT),
               IntPtr.Zero,
               IntPtr.Zero);
       }


       [Flags]
       private enum ShellChangeNotificationEvents : uint
       {
           /// <summary>
           /// The name of a nonfolder item has changed. SHCNF_IDLIST or  SHCNF_PATH must be specified in uFlags. dwItem1 contains the  previous PIDL or name of the item. dwItem2 contains the new PIDL or name of the item. 
           /// </summary>
           SHCNE_RENAMEITEM = 0x00000001,
           /// <summary>
           /// A nonfolder item has been created. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the item that was created. dwItem2 is not used and should be NULL.
           /// </summary>
           SHCNE_CREATE = 0x00000002,
           /// <summary>
           /// A nonfolder item has been deleted. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the item that was deleted. dwItem2 is not used and should be NULL.
           /// </summary>
           SHCNE_DELETE = 0x00000004,
           /// <summary>
           /// A folder has been created. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the folder that was created. dwItem2 is not used and should be NULL.
           /// </summary>
           SHCNE_MKDIR = 0x00000008,
           /// <summary>
           /// A folder has been removed. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the folder that was removed. dwItem2 is not used and should be NULL.
           /// </summary>
           SHCNE_RMDIR = 0x00000010,
           /// <summary>
           /// Storage media has been inserted into a drive. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the root of the drive that contains the new media. dwItem2 is not used and should be NULL.
           /// </summary>
           SHCNE_MEDIAINSERTED = 0x00000020,
           /// <summary>
           /// Storage media has been removed from a drive. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the root of the drive from which the media was removed. dwItem2 is not used and should be NULL.
           /// </summary>
           SHCNE_MEDIAREMOVED = 0x00000040,
           /// <summary>
           /// A drive has been removed. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the root of the drive that was removed. dwItem2 is not used and should be NULL.
           /// </summary>
           SHCNE_DRIVEREMOVED = 0x00000080,
           /// <summary>
           /// A drive has been added. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the root of the drive that was added. dwItem2 is not used and should be NULL.
           /// </summary>
           SHCNE_DRIVEADD = 0x00000100,
           /// <summary>
           /// A folder on the local computer is being shared via the network. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the folder that is being shared. dwItem2 is not used and should be NULL.
           /// </summary>
           SHCNE_NETSHARE = 0x00000200,
           /// <summary>
           /// A folder on the local computer is no longer being shared via the network. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the folder that is no longer being shared. dwItem2 is not used and should be NULL.
           /// </summary>
           SHCNE_NETUNSHARE = 0x00000400,
           /// <summary>
           /// The attributes of an item or folder have changed. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the item or folder that has changed. dwItem2 is not used and should be NULL.
           /// </summary>
           SHCNE_ATTRIBUTES = 0x00000800,
           /// <summary>
           /// The contents of an existing folder have changed, but the folder still exists and has not been renamed. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the folder that has changed. dwItem2 is not used and should be NULL. If a folder has been created, deleted, or renamed, use SHCNE_MKDIR, SHCNE_RMDIR, or SHCNE_RENAMEFOLDER, respectively, instead.
           /// </summary>
           SHCNE_UPDATEDIR = 0x00001000,
           /// <summary>
           /// An existing nonfolder item has changed, but the item still exists and has not been renamed. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the item that has changed. dwItem2 is not used and should be NULL. If a nonfolder item has been created, deleted, or renamed, use SHCNE_CREATE, SHCNE_DELETE, or SHCNE_RENAMEITEM, respectively, instead.
           /// </summary>
           SHCNE_UPDATEITEM = 0x00002000,
           /// <summary>
           /// The computer has disconnected from a server. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the server from which the computer was disconnected. dwItem2 is not used and should be NULL.
           /// </summary>
           SHCNE_SERVERDISCONNECT = 0x00004000,
           /// <summary>
           /// An image in the system image list has changed. SHCNF_DWORD must be specified in uFlags. dwItem1 contains the index in the system image list that has changed. dwItem2 is not used and should be NULL.
           /// </summary>
           SHCNE_UPDATEIMAGE = 0x00008000,
           /// <summary>
           /// A drive has been added and the Shell should create a new window for the drive. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the root of the drive that was added. dwItem2 is not used and should be NULL.
           /// </summary>
           SHCNE_DRIVEADDGUI = 0x00010000,
           /// <summary>
           /// The name of a folder has changed. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the previous pointer to an item identifier list (PIDL) or name of the folder. dwItem2 contains the new PIDL or name of the folder.
           /// </summary>
           SHCNE_RENAMEFOLDER = 0x00020000,
           /// <summary>
           /// The amount of free space on a drive has changed. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the root of the drive on which the free space changed. dwItem2 is not used and should be NULL.
           /// </summary>
           SHCNE_FREESPACE = 0x00040000,
           /// <summary>
           /// Not currently used.
           /// </summary>
           SHCNE_EXTENDED_EVENT = 0x04000000,
           /// <summary>
           /// A file type association has changed. SHCNF_IDLIST must be specified in the uFlags parameter. dwItem1 and dwItem2 are not used and must be NULL.
           /// </summary>
           SHCNE_ASSOCCHANGED = 0x08000000,
           /// <summary>
           /// Specifies a combination of all of the disk event identifiers.
           /// </summary>
           SHCNE_DISKEVENTS = 0x0002381F,
           /// <summary>
           /// Specifies a combination of all of the global event identifiers. 
           /// </summary>
           SHCNE_GLOBALEVENTS = 0x0C0581E0,
           /// <summary>
           /// All events have occurred.
           /// </summary>
           SHCNE_ALLEVENTS = 0x7FFFFFFF,
           /// <summary>
           /// The specified event occurred as a result of a system interrupt. As this value modifies other event values, it cannot be used alone.
           /// </summary>
           SHCNE_INTERRUPT = 0x80000000
       }

       private enum ShellChangeNotificationFlags
       {
           /// <summary>
           /// dwItem1 and dwItem2 are the addresses of ITEMIDLIST structures that represent the item(s) affected by the change. Each ITEMIDLIST must be relative to the desktop folder. 
           /// </summary>
           SHCNF_IDLIST = 0x0000,
           /// <summary>
           /// dwItem1 and dwItem2 are the addresses of null-terminated strings of maximum length MAX_PATH that contain the full path names of the items affected by the change.
           /// </summary>
           SHCNF_PATHA = 0x0001,
           /// <summary>
           /// dwItem1 and dwItem2 are the addresses of null-terminated strings that represent the friendly names of the printer(s) affected by the change.
           /// </summary>
           SHCNF_PRINTERA = 0x0002,
           /// <summary>
           /// The dwItem1 and dwItem2 parameters are DWORD values.
           /// </summary>
           SHCNF_DWORD = 0x0003,
           /// <summary>
           /// like SHCNF_PATHA but unicode string
           /// </summary>
           SHCNF_PATHW = 0x0005,
           /// <summary>
           /// like SHCNF_PRINTERA but unicode string
           /// </summary>
           SHCNF_PRINTERW = 0x0006,
           /// <summary>
           /// 
           /// </summary>
           SHCNF_TYPE = 0x00FF,
           /// <summary>
           /// The function should not return until the notification has been delivered to all affected components. As this flag modifies other data-type flags, it cannot by used by itself.
           /// </summary>
           SHCNF_FLUSH = 0x1000,
           /// <summary>
           /// The function should begin delivering notifications to all affected components but should return as soon as the notification process has begun. As this flag modifies other data-type flags, it cannot by used  by itself.
           /// </summary>
           SHCNF_FLUSHNOWAIT = 0x2000
       }
   }
}