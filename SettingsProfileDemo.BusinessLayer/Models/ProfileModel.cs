using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SettingsProfileDemo.BusinessLayer.Models
{
    public class ProfileModel : IValidatableObject
    {
        public string Name { get; set; }
        public Guid ClientId { get; set; }
        public string ClientSecret { get; set; }

        public ProfileModel()
        {
            
        }

        public ProfileModel(string name, Guid clientId, string clientSecret)
        {
            Name = name;
            ClientId = clientId;
            ClientSecret = clientSecret;
        }
        
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            
            if (string.IsNullOrWhiteSpace(Name))
            {
                results.Add(new ValidationResult(
                    "profile name is required", 
                    new[] {nameof(Name)}
                    )
                );
            }

            if (ClientId == Guid.Empty)
            {
                results.Add(new ValidationResult(
                    "client id may not be empty",
                    new [] {nameof(ClientId)}
                    )
                );
            }

            if (string.IsNullOrWhiteSpace(ClientSecret))
            {
                results.Add(new ValidationResult(
                    "client secret is required",
                    new [] {nameof(ClientSecret)}
                    )
                );
            }

            return results;
        }
    }
}