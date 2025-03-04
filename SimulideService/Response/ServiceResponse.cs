using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SimulideService.Domain;

namespace SimulideService.Response;

public class ServiceResponse<T>
{
    public T? Data { get; set; }
    public bool IsSuccessful { get; set; }
    public List<ServiceError>? Errors { get; set; }
    public HttpStatusCode StatusCode { get; set; }

    public static ServiceResponse<T> ErrorResult(HttpStatusCode code, List<ServiceError> errors)
    {
        return new ServiceResponse<T>
        {
            IsSuccessful = false,
            Errors = errors,
            Data = default,
            StatusCode = code 
        };
    }

    public static ServiceResponse<T> ErrorResult(HttpStatusCode code, ServiceError error)
    {
        return new ServiceResponse<T>
        {
            IsSuccessful = false,
            Errors = [error],
            Data = default,
            StatusCode = code 
        };
    }
    
    public static ServiceResponse<T> ErrorResult(HttpStatusCode code, List<Exception> exceptions)
    {
        return new ServiceResponse<T>
        {
            IsSuccessful = false,
            Errors = exceptions.Select(x => new ServiceError { Message = x.Message }).ToList(), 
            Data = default,
            StatusCode = code 
        };
    }
    
    public static ServiceResponse<T> ErrorResult(HttpStatusCode code, Exception ex)
    {
        return new ServiceResponse<T>
        {
            IsSuccessful = false,
            Errors = [new ServiceError { Message = ex.Message }], 
            Data = default,
            StatusCode = code 
        };
    }

    public static ServiceResponse<T> SuccessResult(HttpStatusCode code, T data)
    {
        return new ServiceResponse<T>
        {
            IsSuccessful = true,
            Errors = null, 
            Data = data,
            StatusCode = code 
        };
    }
    
    
    public IActionResult ToActionResult()
    {
        return new ObjectResult(StatusCode)
        {
            StatusCode = (int)StatusCode,
            Value = this
        };
    } 
}