using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UWP_Crypto.Lang;
public class KeyDeletedException : ArgumentNullException
{
    public KeyDeletedException()
    {
    }

    public KeyDeletedException(string paramName) : base(paramName)
    {
    }

    public KeyDeletedException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public KeyDeletedException(string paramName, string message) : base(paramName, message)
    {
    }
}
