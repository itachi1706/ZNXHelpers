using System;
using System.Runtime.Serialization;
namespace ZNXHelpers.Exceptions
{
    [Serializable]
    public class HttpResponseException : Exception
    {
        public int StatusCode { get; }
        public string CustomMessage { get; }

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

        public override string Message
        {
            get
            {
                if (string.IsNullOrWhiteSpace(CustomMessage))
                {
                    return $"HTTP Response Status Code {StatusCode}.";
                }
                return $"{CustomMessage} : HTTP Response Status Code {StatusCode}.";
            }
        }
    }
}
