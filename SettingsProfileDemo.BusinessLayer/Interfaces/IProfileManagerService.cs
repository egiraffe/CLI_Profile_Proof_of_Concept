using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SettingsProfileDemo.BusinessLayer.Models;

namespace SettingsProfileDemo.BusinessLayer.Interfaces
{
    public interface IProfileManagerService
    {
        /// <summary>
        /// returns profiles
        /// </summary>
        /// <returns>Dictionary of Profiles</returns>
        Task<Dictionary<string, ProfileModel>> GetProfilesAsync();

        /// <summary>
        /// returns profile by name
        /// </summary>
        /// <param name="name">Profile Name</param>
        Task<ProfileModel> GetProfileAsync(string name);
        
        /// <summary>
        /// delete profile by name
        /// </summary>
        /// <param name="name">Profile name</param>
        Task DeleteProfileAsync(string name);

        /// <summary>
        /// Create or update a profile model
        /// </summary>
        /// <param name="name">Profile name - this will be used when calling commands that require the profile parameter</param>
        /// <param name="clientId">CAP Client Id</param>
        /// <param name="clientSecret">CAP Client Secret</param>
        Task CreateOrUpdateProfile(string name, Guid clientId, string clientSecret);

        /// <summary>
        /// Create or update a profile model
        /// </summary>
        /// <param name="profileModel">Model holding the profile name, CAP Client Id, and CAP Client Secret</param>
        Task CreateOrUpdateProfile(ProfileModel profileModel);

        event EventHandler OnNoProfilesFound;

    }
}