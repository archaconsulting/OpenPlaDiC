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

        public static Response Success(string message)
        {
            return new Response
            {
                
                IsSuccess = true,
                IsException = false,
                Message = message,
                Code = 0

            };         
         
        }
        

    }

    public class Response<T> : Response
    {
        public T? Data { get; set; }

        public static Response<T> Exception(Exception ex, string v)
        {

            return new Response<T>
            {
                
                IsSuccess = false,
                IsException = true,
                Message = v,
                InnerException = ex.Message,
                Code = 500

            };

        }

        public static Response<T> Fail(string v)
        {
            return new Response<T>
            {
                
                IsSuccess = false,
                IsException = false,
                Message = v,
                Code = 500

            };
        }

        public static Response<T> Success(T data, string message)
        {
            return new Response<T>
            {
                
                IsSuccess = true,
                IsException = false,
                Message = message,
                Data = data,
                Code = 0

            };         
         
        }
    }

}
