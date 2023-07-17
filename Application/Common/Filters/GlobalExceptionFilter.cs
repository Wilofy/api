using Microsoft.AspNetCore.Mvc.Filters;

namespace Application.Common.Filters;
public class GlobalExceptionFilter : IAsyncExceptionFilter
{
    Task IAsyncExceptionFilter.OnExceptionAsync(ExceptionContext context)
    {
        throw new NotImplementedException();
    }
}

