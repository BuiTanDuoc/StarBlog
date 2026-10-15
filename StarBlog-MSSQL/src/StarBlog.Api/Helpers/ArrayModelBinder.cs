using System.ComponentModel;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StarBlog.Api.Helpers;

public class ArrayModelBinder : IModelBinder {
    public Task BindModelAsync(ModelBindingContext bindingContext) {
        if (!bindingContext.ModelMetadata.IsEnumerableType) {
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).ToString();
        if (string.IsNullOrWhiteSpace(value)) {
            bindingContext.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        // Dùng reflection để lấy kiểu tham số
        var elementType = bindingContext.ModelType.GetTypeInfo().GenericTypeParameters[0];
        
        // Dựa vào kiểu tham số, tạo một type converter mới
        var converter = TypeDescriptor.GetConverter(elementType);

        // Tách chuỗi theo dấu phẩy (,),
        // Chỉ định tham số `StringSplitOptions.RemoveEmptyEntries` để loại bỏ giá trị rỗng, ví dụ `1,,3,4` -> `["1", "3", "4"]`
        var values = value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => converter.ConvertFromString(x.Trim()))
            .ToArray();

        var typedValues = Array.CreateInstance(elementType, values.Length);
        values.CopyTo(typedValues, 0);
        bindingContext.Model = typedValues;

        bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);

        return Task.CompletedTask;
    }
}