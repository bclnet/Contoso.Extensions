using Microsoft.Win32.SafeHandles;
using System.Net;
using System.Runtime.InteropServices;

namespace System.Security
{
    /// <summary>
    /// CredentialManager
    /// </summary>
    public static class CredentialManager
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

        /// <summary>
        /// CredentialFlags
        /// </summary>
        [Flags]
        public enum CredentialFlags : uint
        {
            /// <summary>
            /// The none
            /// </summary>
            NONE = 0x0,
            /// <summary>
            /// The prompt now
            /// </summary>
            PROMPT_NOW = 0x2,
            /// <summary>
            /// The username target
            /// </summary>
            USERNAME_TARGET = 0x4
        }

        /// <summary>
        /// CredentialErrors
        /// </summary>
        public enum CredentialErrors : uint
        {
            /// <summary>
            /// The error success
            /// </summary>
            ERROR_SUCCESS = 0x0,
            /// <summary>
            /// The error invali d_ parameter
            /// </summary>
            ERROR_INVALID_PARAMETER = 0x80070057,
            /// <summary>
            /// The error invali d_ flags
            /// </summary>
            ERROR_INVALID_FLAGS = 0x800703EC,
            /// <summary>
            /// The error not found
            /// </summary>
            ERROR_NOT_FOUND = 0x80070490,
            /// <summary>
            /// The error no such logon session
            /// </summary>
            ERROR_NO_SUCH_LOGON_SESSION = 0x80070520,
            /// <summary>
            /// The error bad username
            /// </summary>
            ERROR_BAD_USERNAME = 0x8007089A
        }

        /// <summary>
        /// CredentialPersist
        /// </summary>
        public enum CredentialPersist : uint
        {
            /// <summary>
            /// The session
            /// </summary>
            SESSION = 1,
            /// <summary>
            /// The local machine
            /// </summary>
            LOCAL_MACHINE = 2,
            /// <summary>
            /// The enterprise
            /// </summary>
            ENTERPRISE = 3
        }

        /// <summary>
        /// CredentialType
        /// </summary>
        public enum CredentialType : uint
        {
            /// <summary>
            /// The generic
            /// </summary>
            GENERIC = 1,
            /// <summary>
            /// The domain password
            /// </summary>
            DOMAIN_PASSWORD = 2,
            /// <summary>
            /// The domain certificate
            /// </summary>
            DOMAIN_CERTIFICATE = 3,
            /// <summary>
            /// The domain visible password
            /// </summary>
            DOMAIN_VISIBLE_PASSWORD = 4,
            /// <summary>
            /// The generic certificate
            /// </summary>
            GENERIC_CERTIFICATE = 5,
            /// <summary>
            /// The domain extended
            /// </summary>
            DOMAIN_EXTENDED = 6,
            /// <summary>
            /// The maximum
            /// </summary>
            MAXIMUM = 7, // Maximum supported cred type
            /// <summary>
            /// The maximu m_ ex
            /// </summary>
            MAXIMUM_EX = MAXIMUM + 1000, // Allow new applications to run on old OSes
        }

        /// <summary>
        /// Credential
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct Credential
        {
            /// <summary>
            /// The flags
            /// </summary>
            public CredentialFlags Flags;
            /// <summary>
            /// The type
            /// </summary>
            public CredentialType Type;
            /// <summary>
            /// The target name
            /// </summary>
            public string TargetName;
            /// <summary>
            /// The comment
            /// </summary>
            public string Comment;
            /// <summary>
            /// The last written
            /// </summary>
            public DateTime LastWritten;
            /// <summary>
            /// The credential BLOB size
            /// </summary>
            public uint CredentialBlobSize;
            /// <summary>
            /// The credential BLOB
            /// </summary>
            public string CredentialBlob;
            /// <summary>
            /// The persist
            /// </summary>
            public CredentialPersist Persist;
            /// <summary>
            /// The attribute count
            /// </summary>
            public uint AttributeCount;
            /// <summary>
            /// The attributes
            /// </summary>
            public IntPtr Attributes;
            /// <summary>
            /// The target alias
            /// </summary>
            public string TargetAlias;
            /// <summary>
            /// The user name
            /// </summary>
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

        /// <summary>
        /// Deletes the specified target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static int Delete(string target, CredentialType type) => !CredDeleteW(target, type, 0) ? Marshal.GetHRForLastWin32Error() : 0;

        /// <summary>
        /// Queries the specified filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="credentials">The credentials.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Tries to read the specified target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="type">The type.</param>
        /// <param name="credential">The credential.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Writes the specified user credential.
        /// </summary>
        /// <param name="credential">The user credential.</param>
        /// <returns></returns>
        public static int Write(Credential credential) => !CredWriteW(ref credential, 0) ? Marshal.GetHRForLastWin32Error() : 0;

        /// <summary>
        /// Reads a generic credential (default).
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>Credential.</returns>
        /// <exception cref="System.InvalidOperationException">Unable to read credential store</exception>
        public static NetworkCredential ReadGeneric(string target)
        {
            if (TryRead(target, CredentialType.GENERIC, out var credential) != 0)
                throw new InvalidOperationException("Unable to read credential store");
            return new NetworkCredential(credential.UserName, credential.CredentialBlob);
        }
    }
}
