using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SettingsProfileDemo.BusinessLayer.Extensions
{
    public static class GenericValidator
    {
        public static bool TryValidate(object obj, out ICollection<ValidationResult> results)  
        {  
            var context = new ValidationContext(obj);  
            results = new List<ValidationResult>();  
            return Validator.TryValidateObject(obj, context, results, true);  
        } 
    }
}