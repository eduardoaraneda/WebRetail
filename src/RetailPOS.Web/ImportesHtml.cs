using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
namespace RetailPOS.Web;

// Los input type=number envían punto decimal, independientemente del idioma del equipo.
public sealed class ImporteHtmlBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext context)
    {
        var valor=context.ValueProvider.GetValue(context.ModelName);
        if(valor==ValueProviderResult.None)return Task.CompletedTask;
        context.ModelState.SetModelValue(context.ModelName,valor);
        if(string.IsNullOrWhiteSpace(valor.FirstValue)&&context.ModelType==typeof(decimal?)){context.Result=ModelBindingResult.Success(null);return Task.CompletedTask;}
        if(decimal.TryParse(valor.FirstValue,NumberStyles.AllowLeadingSign|NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out var numero))context.Result=ModelBindingResult.Success(numero);
        else context.ModelState.TryAddModelError(context.ModelName,"Indica un importe válido con punto decimal y sin separadores de miles.");
        return Task.CompletedTask;
    }
}
public sealed class ImporteHtmlProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)=>context.Metadata.ModelType==typeof(decimal)||context.Metadata.ModelType==typeof(decimal?)?new ImporteHtmlBinder():null;
}

