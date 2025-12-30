namespace OpenPlaDiC.Framework
{
    public class Response
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public bool IsException { get; set; }
        public int Code { get; set; }
    }

    public class Response<T> : Response
    {
        public T? Data { get; set; }
    }
}
