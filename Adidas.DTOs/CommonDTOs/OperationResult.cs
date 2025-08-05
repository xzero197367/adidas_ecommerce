namespace Adidas.DTOs.Common_DTOs;

public class OperationResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }

    public static OperationResult<T> Success(T data) => new() { IsSuccess = true, Data = data };
    public static OperationResult<T> Fail(string error) => new() { IsSuccess = false, ErrorMessage = error };
}
