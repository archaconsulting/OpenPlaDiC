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
    }

    public class Response<T> : Response
    {
        public T? Data { get; set; }
    }

}
