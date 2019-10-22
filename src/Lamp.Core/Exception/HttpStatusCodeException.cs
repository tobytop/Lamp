using System;

namespace Lamp.Core
{
    public class HttpStatusCodeException : Exception
    {
        public int StatusCode { get; set; }
        public string ContentType { get; set; } = @"application/json";
        public string Path { get; set; }
        public HttpStatusCodeException(int statusCode)
        {
            StatusCode = statusCode;
        }
        public HttpStatusCodeException(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCodeException(int statusCode, string message, string path) : base(message)
        {
            StatusCode = statusCode;
            Path = path;
        }
    }
}
