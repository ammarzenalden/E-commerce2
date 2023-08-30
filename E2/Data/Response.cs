namespace E2.Data;

public abstract class Response<T>
{
    public abstract bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}
public class ErrorResponse : Response<object>
{
    public override bool IsSuccess { get; set; } = false;
    public ErrorResponse(string message)
    {
        this.Message = message;
    }
}
public class DataResponse<T> : Response<T>
{
    public override bool IsSuccess { get; set; } = true;
    public DataResponse( T data,string? message = null)
    {
        this.Data = data;
        this.Message = message;
    }
}
