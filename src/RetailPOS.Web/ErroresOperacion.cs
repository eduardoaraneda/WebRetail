using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using RetailPOS.Application;

namespace RetailPOS.Web;

public sealed class ErroresOperacion(ILogger<ErroresOperacion> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not (ReglaNegocioException or DbUpdateException or SqlException)) return;
        logger.LogWarning(context.Exception, "No se pudo completar una operación comercial");
        var mensaje = context.Exception is ReglaNegocioException ? context.Exception.Message : "No se guardó la operación. Puede haber un código duplicado o una operación concurrente. Revisa los datos y vuelve a intentarlo.";
        context.Result = new ViewResult
        {
            ViewName = "OperacionError",
            StatusCode = 409,
            ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(), context.ModelState) { Model = mensaje }
        };
        context.ExceptionHandled = true;
    }
}

