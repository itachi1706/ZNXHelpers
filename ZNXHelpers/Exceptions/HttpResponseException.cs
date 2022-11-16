using System.Runtime.Serialization;
namespace ZNXHelpers.Exceptions
{
    [Serializable]
    public class HttpResponseException : Exception
    {
        public int StatusCode { get; }
        public string? CustomMessage { get; }

        public HttpResponseException(int statusCode)
        {
            StatusCode = statusCode;
        }

        public HttpResponseException(int statusCode, string customMessage)
        {
            StatusCode = statusCode;
            CustomMessage = customMessage;
        }

        protected HttpResponseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override string Message => string.IsNullOrWhiteSpace(CustomMessage) ? $"HTTP Response Status Code {StatusCode}." : $"{CustomMessage} : HTTP Response Status Code {StatusCode}.";
    }
}
