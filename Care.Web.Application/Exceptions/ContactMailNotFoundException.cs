using System.Runtime.Serialization;

namespace Care.Web.Application.Common.Exceptions;
[Serializable]
public class ContactMailNotFoundException : Exception
{
    public ContactMailNotFoundException()
    {
    }

    public ContactMailNotFoundException(string? message) : base(message)
    {
    }

    public ContactMailNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected ContactMailNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}