using System.Runtime.Serialization;

[Serializable]
internal class PdfParsingException : Exception
{
    public PdfParsingException()
    {
    }

    public PdfParsingException(string? message) : base(message)
    {
    }

    public PdfParsingException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected PdfParsingException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}