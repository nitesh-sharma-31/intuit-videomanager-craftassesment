namespace VideoManager.Model
{
    /// <summary>
    /// Represents the result of an operation with success/failure status
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();

        public static Result Success(string message = "")
        {
            return new Result { IsSuccess = true, Message = message };
        }

        public static Result Failure(string message, List<string>? errors = null)
        {
            return new Result 
            { 
                IsSuccess = false, 
                Message = message, 
                Errors = errors ?? new List<string>() 
            };
        }
    }

    /// <summary>
    /// Represents the result of an operation with a return value
    /// </summary>
    public class Result<T> : Result
    {
        public T? Data { get; set; }

        public static Result<T> Success(T data, string message = "")
        {
            return new Result<T> 
            { 
                IsSuccess = true, 
                Message = message, 
                Data = data 
            };
        }

        public static new Result<T> Failure(string message, List<string>? errors = null)
        {
            return new Result<T> 
            { 
                IsSuccess = false, 
                Message = message, 
                Errors = errors ?? new List<string>() 
            };
        }
    }
}
