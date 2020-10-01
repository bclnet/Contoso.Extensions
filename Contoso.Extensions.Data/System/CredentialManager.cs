using Microsoft.Win32.SafeHandles;
using System.Net;
using System.Runtime.InteropServices;

namespace System.Security
{
    internal static class CredentialManager
    {
        #region Preamble

        [DllImport("Advapi32.dll", SetLastError = true, EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode)]
        static extern bool CredDeleteW([In] string target, [In] CredentialType type, [In] int reservedFlag);

        [DllImport("Advapi32.dll", SetLastError = true, EntryPoint = "CredEnumerateW", CharSet = CharSet.Unicode)]
        static extern bool CredEnumerateW([In] string filter, [In] int flags, out int count, out IntPtr credentialPtr);

        [DllImport("Advapi32.dll", SetLastError = true, EntryPoint = "CredFree")]
        static extern void CredFree([In] IntPtr cred);

        [DllImport("Advapi32.dll", SetLastError = true, EntryPoint = "CredReadW", CharSet = CharSet.Unicode)]
        static extern bool CredReadW([In] string target, [In] CredentialType type, [In] int reservedFlag, out IntPtr credentialPtr);

        [DllImport("Advapi32.dll", SetLastError = true, EntryPoint = "CredWriteW", CharSet = CharSet.Unicode)]
        static extern bool CredWriteW([In] ref Credential userCredential, [In] UInt32 flags);

        #endregion

        #region Fields

        [Flags]
        public enum CredentialFlags : uint
        {
            NONE = 0x0,
            PROMPT_NOW = 0x2,
            USERNAME_TARGET = 0x4
        }

        public enum CredentialErrors : uint
        {
            ERROR_SUCCESS = 0x0,
            ERROR_INVALID_PARAMETER = 0x80070057,
            ERROR_INVALID_FLAGS = 0x800703EC,
            ERROR_NOT_FOUND = 0x80070490,
            ERROR_NO_SUCH_LOGON_SESSION = 0x80070520,
            ERROR_BAD_USERNAME = 0x8007089A
        }

        public enum CredentialPersist : uint
        {
            SESSION = 1,
            LOCAL_MACHINE = 2,
            ENTERPRISE = 3
        }

        public enum CredentialType : uint
        {
            GENERIC = 1,
            DOMAIN_PASSWORD = 2,
            DOMAIN_CERTIFICATE = 3,
            DOMAIN_VISIBLE_PASSWORD = 4,
            GENERIC_CERTIFICATE = 5,
            DOMAIN_EXTENDED = 6,
            MAXIMUM = 7,
            MAXIMUM_EX = MAXIMUM + 1000,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct Credential
        {
            public CredentialFlags Flags;
            public CredentialType Type;
            public string TargetName;
            public string Comment;
            public DateTime LastWritten;
            public uint CredentialBlobSize;
            public string CredentialBlob;
            public CredentialPersist Persist;
            public uint AttributeCount;
            public IntPtr Attributes;
            public string TargetAlias;
            public string UserName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct NativeCredential
        {
            public CredentialFlags Flags;
            public CredentialType Type;
            public IntPtr TargetName;
            public IntPtr Comment;
            public Runtime.InteropServices.ComTypes.FILETIME LastWritten;
            public uint CredentialBlobSize;
            public IntPtr CredentialBlob;
            public uint Persist;
            public uint AttributeCount;
            public IntPtr Attributes;
            public IntPtr TargetAlias;
            public IntPtr UserName;
        }

        class CriticalCredentialHandle : CriticalHandleZeroOrMinusOneIsInvalid
        {
            public CriticalCredentialHandle(IntPtr handle) => SetHandle(handle);

            Credential XlateNativeCred(IntPtr credentialPtr)
            {
                var native = (NativeCredential)Marshal.PtrToStructure(credentialPtr, typeof(NativeCredential));
                //var lastWritten = native.LastWritten.dwHighDateTime;
                //lastWritten = (lastWritten << 32) + native.LastWritten.dwLowDateTime;
                return new Credential
                {
                    Type = native.Type,
                    Flags = native.Flags,
                    Persist = (CredentialPersist)native.Persist,
                    //LastWritten = DateTime.FromFileTime(lastWritten),
                    UserName = Marshal.PtrToStringUni(native.UserName),
                    TargetName = Marshal.PtrToStringUni(native.TargetName),
                    TargetAlias = Marshal.PtrToStringUni(native.TargetAlias),
                    Comment = Marshal.PtrToStringUni(native.Comment),
                    CredentialBlobSize = native.CredentialBlobSize,
                    CredentialBlob = native.CredentialBlobSize > 0 ? Marshal.PtrToStringUni(native.CredentialBlob, (int)native.CredentialBlobSize / 2) : null,
                };
            }

            public Credential GetCredential()
            {
                if (IsInvalid)
                    throw new InvalidOperationException("Invalid CriticalHandle!");
                return XlateNativeCred(handle);
            }

            public Credential[] GetCredentials(int count)
            {
                if (IsInvalid)
                    throw new InvalidOperationException("Invalid CriticalHandle!");
                var credentials = new Credential[count];
                for (var i = 0; i < count; i++)
                    credentials[i] = XlateNativeCred(Marshal.ReadIntPtr(handle, i * IntPtr.Size));
                return credentials;
            }

            protected override bool ReleaseHandle()
            {
                if (IsInvalid)
                    return false;
                CredFree(handle);
                SetHandleAsInvalid();
                return true;
            }
        }

        #endregion

        public static int Delete(string target, CredentialType type) => !CredDeleteW(target, type, 0) ? Marshal.GetHRForLastWin32Error() : 0;

        public static int Query(string filter, out Credential[] credentials)
        {
            var flags = 0x0;
            if (string.IsNullOrEmpty(filter) || filter == "*")
            {
                filter = null;
                if (Environment.OSVersion.Version.Major >= 6) // CRED_ENUMERATE_ALL_CREDENTIALS; only valid is OS >= Vista
                    flags = 0x1;
            }
            if (!CredEnumerateW(filter, flags, out var count, out var credentialPtr))
            {
                credentials = null;
                return Marshal.GetHRForLastWin32Error();
            }
            var credentialHandle = new CriticalCredentialHandle(credentialPtr);
            credentials = credentialHandle.GetCredentials(count);
            return 0;
        }

        public static int TryRead(string target, CredentialType type, out Credential credential)
        {
            if (!CredReadW(target, type, 0, out var credentialPtr))
            {
                credential = new Credential();
                return Marshal.GetHRForLastWin32Error();
            }
            var credentialHandle = new CriticalCredentialHandle(credentialPtr);
            credential = credentialHandle.GetCredential();
            return 0;
        }

        public static int Write(Credential credential) => !CredWriteW(ref credential, 0) ? Marshal.GetHRForLastWin32Error() : 0;

        public static NetworkCredential ReadGeneric(string target)
        {
            if (TryRead(target, CredentialType.GENERIC, out var credential) != 0)
                throw new InvalidOperationException("Unable to read credential store");
            return new NetworkCredential(credential.UserName, credential.CredentialBlob);
        }
    }
}
