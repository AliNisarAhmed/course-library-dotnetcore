using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CourseLibrary.API.Helpers
{
    public class ArrayModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            // our binder should work only on Enumerable types, so we check that first.
            if (!bindingContext.ModelMetadata.IsEnumerableType) 
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            // if it is Enumerable type, get the values from the route
            var value =
                bindingContext
                .ValueProvider
                .GetValue(bindingContext.ModelName).ToString();

            // if the string, which should now be like "1, 2, 3, 4", is empty
            if (string.IsNullOrWhiteSpace(value))
            {
                bindingContext.Result = ModelBindingResult.Success(null); // model binding succeeds, but with null.
                return Task.CompletedTask;
            }

            // at this point, the value should be a string of comma separated values. convert it to IEnumerable and return

            // use reflection to get the type specified for the output Model and put it in elementType

            // Get the first Type argument, which is Guid specified in the authorcollections GET request.
            Type elementType = bindingContext.ModelType.GetTypeInfo().GenericTypeArguments[0];
            var converter = TypeDescriptor.GetConverter(elementType);

            var values =
                value
                    .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => converter.ConvertFromString(x.Trim()))
                    .ToArray();

            // Create an array of the output type i.e. elementType
            var typedValues = Array.CreateInstance(elementType, values.Length);
            values.CopyTo(typedValues, 0);
            bindingContext.Model = typedValues;

            // return a successful result, passing in the Model.
            bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);

            return Task.CompletedTask;

        }
    }
}
