namespace OpenPlaDiC.Framework
{
    public class Response
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public bool IsException { get; set; }
        public string? InnerException { get; set; }
        public int Code { get; set; }
        public string? Value { get; set; }
        public bool Flag { get; set; }
        public string? ExRef { get; set; }

        public void SetErrorResponse(string message)
        {
            
            IsSuccess = false;
            IsException = true;
            Message = message;
            Code = 500;

        }
    }

    public class Response<T> : Response
    {
        public T? Data { get; set; }
    }

}
