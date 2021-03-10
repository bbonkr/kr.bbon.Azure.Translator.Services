using System;
using System.Net;

namespace kr.bbon.Azure.Translator.Services
{
    public abstract class ApiException : Exception
    {
        public ApiException(HttpStatusCode httpStatusCode, string message)
            : base(message)
        {
            this.StatusCode = httpStatusCode;
        }
        public ApiException(int httpStatusCode, string message)
            : this((HttpStatusCode)httpStatusCode, message) { }

        public HttpStatusCode StatusCode { get; init; }

        public abstract object GetDetails();

        public abstract T GetDetails<T>();
    }

    public class ApiHttpStatusException<TDetails> : ApiException
    {
        public ApiHttpStatusException(HttpStatusCode httpStatusCode, string message, TDetails details)
            : base(httpStatusCode, message)
        {
            this.Details = details;
        }

        public ApiHttpStatusException(int httpStatusCode, string message, TDetails details)
            : this((HttpStatusCode)httpStatusCode, message, details) { }

        public TDetails Details { get; init; }

        public override object GetDetails()
        {
            return Details;
        }

        public override T GetDetails<T>()
        {
            return (T)GetDetails();
        }
    }
}
