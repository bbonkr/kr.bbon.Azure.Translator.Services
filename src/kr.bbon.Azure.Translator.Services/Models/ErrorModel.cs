namespace kr.bbon.Azure.Translator.Services.Models
{
    public class ErrorModel<TCode>
    {
        public TCode Code { get; init; }

        public string Message { get; init; }

        public ErrorModel<TCode> InnerError { get; init; } = null;
    }
}
