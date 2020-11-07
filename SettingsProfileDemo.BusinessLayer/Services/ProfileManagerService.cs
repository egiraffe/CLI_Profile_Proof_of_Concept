using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using SettingsProfileDemo.BusinessLayer.Extensions;
using SettingsProfileDemo.BusinessLayer.Interfaces;
using SettingsProfileDemo.BusinessLayer.Models;

namespace SettingsProfileDemo.BusinessLayer.Services
{
    public class ProfileManagerService : IProfileManagerService
    {
        private static readonly string _profileManagerSettingsPath;
        private static readonly string _profileManagerFileName;
        private static readonly IMemoryCache _profiles;
        private static readonly JsonSerializerOptions _jsonSerializerOptions;
        private static DateTime? _cacheLastUpdated;

        static ProfileManagerService()
        {
            // create a new cache if not exists
            _profiles ??= new MemoryCache(new MemoryCacheOptions());
            
            // set the path of the profile settings
            _profileManagerSettingsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) +
                                          Path.DirectorySeparatorChar +
                                          ".valid-identity-solutions" +
                                          Path.DirectorySeparatorChar;

            // the filename for profile settings
            _profileManagerFileName = "settings-profile-demo.json";
            
            // setup json serialization options
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.Default,
                IgnoreNullValues = true,
                IgnoreReadOnlyProperties = false,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReadCommentHandling = JsonCommentHandling.Skip,
                WriteIndented = true
            };
        }

        public ProfileManagerService()
        {
            // create the settings directory if it does not exist
            if (!Directory.Exists(_profileManagerSettingsPath))
            {
                Directory.CreateDirectory(_profileManagerSettingsPath);
            }

            CreateSettingsFileIfNotExists().Wait();

            if (NoProfilesConfigured().Result)
            {
                OnNoProfilesFound?.Invoke(this, EventArgs.Empty);
            }
        }

        public ProfileManagerService(EventHandler onNoProfilesFound) : this()
        {
            OnNoProfilesFound += onNoProfilesFound;
            
            // because base constructors are called first, gotta invoke this now if we need to
            if (NoProfilesConfigured().Result)
            {
                onNoProfilesFound?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, ProfileModel>> GetProfilesAsync()
        {
            // check for existing model
            // -- if the cache has never been updated or does not exist, or the file last updated date is less than
            //    the file last updated date, we need to pull in fresh data from the file, otherwise, continue on
            //    with the profileDictionary fetched from the cache
            if (!_profiles.TryGetValue("profiles", out Dictionary<string, ProfileModel> profileDictionary) ||
                _cacheLastUpdated == null ||
                _cacheLastUpdated < GetSettingsFileLastUpdated())
            {
                profileDictionary = await ReadSettingsFileAsync();
                _cacheLastUpdated = DateTime.UtcNow;
            }

            return profileDictionary;
        }

        /// <inheritdoc/>
        public async Task<ProfileModel> GetProfileAsync(string name)
        {
            // ensure case insensitive dictionary of profiles are returned
            var profiles = new Dictionary<string, ProfileModel>(
                await GetProfilesAsync(), 
                StringComparer.InvariantCultureIgnoreCase);

            if (profiles.ContainsKey(name))
            {
                return profiles[name];
            }
            
            throw new KeyNotFoundException($"a profile with the name '{name}' could not be found.");
        }

        /// <inheritdoc/>
        public async Task DeleteProfileAsync(string name)
        {
            // ensure case insensitive dictionary of profiles are returned
            var profiles = new Dictionary<string, ProfileModel>(
                await GetProfilesAsync(), 
                StringComparer.InvariantCultureIgnoreCase);

            // if the profile doesn't actually exist, bail
            if (!profiles.ContainsKey(name))
            {
                return;
            }

            // remove it from the collection
            profiles.Remove(name);
            
            // update the file
            await SaveProfiles(profiles.Values.ToArray());
            
            // update the cache
            _profiles.Set("profile", profiles);
        }
        
        /// <inheritdoc/>
        public async Task CreateOrUpdateProfile(string name, Guid clientId, string clientSecret)
        {
            var profileModel = new ProfileModel(name, clientId, clientSecret);
            await CreateOrUpdateProfile(profileModel);
        }
        
        /// <inheritdoc/>
        public async Task CreateOrUpdateProfile(ProfileModel profileModel)
        {
            // validate the model
            var isValidModel = GenericValidator.TryValidate(
                profileModel, 
                out var validationResults);

            if (!isValidModel)
            {
                throw new ValidationException(validationResults.First().ErrorMessage);
            }

            // get profiles in a case insensitive dictionary and add or replace this profile
            var profiles = new Dictionary<string, ProfileModel>(
                await GetProfilesAsync(), 
                StringComparer.InvariantCultureIgnoreCase)
            {
                [profileModel.Name] = profileModel
            };
            
            // save to file
            await SaveProfiles(profiles.Values.ToArray());
            
            // update the cache
            _profiles.Set("profiles", profiles);
        }

        // EventHandlers are really cool because you can add handlers for when events are raised
        // (so that an event can be handled differently based on app type web vs console vs whatever)
        // in this app in program.cs I'm going to have a method to handle this and ask for input on the first run
        // add handlers by doing profileManagerService.OnNoProfilesFound += YourMethodName
        public event EventHandler OnNoProfilesFound = (sender, args) => Console.WriteLine("No Profiles Found");

        #region helperMethods

        private static string GetSettingsFilePath()
        {
            // join the file directory and name to create a file path
            var filePath = _profileManagerSettingsPath + _profileManagerFileName;
            return filePath;
        }

        private static DateTime? GetSettingsFileLastUpdated()
        {
            try
            {
                return File.GetLastWriteTimeUtc(GetSettingsFilePath());
            }
            catch
            {
                return null;
            }
        }
        
        private static async Task<Dictionary<string, ProfileModel>> ReadSettingsFileAsync()
        {
            var filePath = GetSettingsFilePath();
            
            // ensure the settings file exists
            await CreateSettingsFileIfNotExists();
            
            // read the file
            var fileJson = await File.ReadAllTextAsync(filePath);
            
            // deserialize the file from JSON to c# object
            var profileModels = JsonSerializer.Deserialize<ProfileModel[]>(fileJson, _jsonSerializerOptions);
            
            // convert to a Dictionary for our result (with case insensitive keys)
            var result = new Dictionary<string, ProfileModel>(
                profileModels.ToDictionary(k => k.Name, v=>v),
                StringComparer.InvariantCultureIgnoreCase
                );

            return result;
        }

        private static async Task CreateSettingsFileIfNotExists()
        {
            const int maxAttempts = 3;
            var attempts = 1;
            while (attempts <= maxAttempts)
            {
                attempts++;
                var filePath = GetSettingsFilePath();

                // if the file doesn't exist, create it, and populate it with a default so that it may be modified
                // outside of the app (if need be).  Creating the file as a JsonArray
                if (!File.Exists(filePath))
                {
                    // create the file
                    await using var sw = File.CreateText(filePath);

                    // create the default model inside an array (sans validation, because we're going to pass an empty guid)
                    var documentBodyModel = new[] {new ProfileModel("default", Guid.Empty, "top-secret-client-secret")};

                    // save the file
                    await SaveProfiles(sw, documentBodyModel);
                }

                // verify file integrity
                var contents = await File.ReadAllTextAsync(filePath);
                var validArray = !string.IsNullOrWhiteSpace(contents) && 
                                 contents.FirstOrDefault(f => !char.IsWhiteSpace(f)) == '[' && 
                                 contents.Reverse().FirstOrDefault(f => !char.IsWhiteSpace(f)) == ']';
                
                if (validArray) return;
                // if not valid, backup file, and replace
                var fileName = Path.GetFileNameWithoutExtension(_profileManagerFileName);
                await File.WriteAllTextAsync(_profileManagerSettingsPath + $"/{fileName}_{Guid.NewGuid():N}.json", contents);
                File.Delete(filePath);
            }

            if (attempts == maxAttempts)
            {
                throw new ApplicationException($"unable to manage profile settings file, check settings in '{_profileManagerSettingsPath}'");
            }
        }

        private static async Task SaveProfiles(params ProfileModel[] profileModels)
        {
            // get file path
            var filePath = GetSettingsFilePath();
            
            // open file for writing replacement
            await using var fs = File.Open(filePath, FileMode.Create, FileAccess.Write);
            await using var sw = new StreamWriter(fs);
            
            // save
            await SaveProfiles(sw, profileModels);
        }

        private static async Task SaveProfiles(StreamWriter sw, params ProfileModel[] profileModels)
        {
            // serialize the model (take it from C# code to JSON
            var profileModelsJson = JsonSerializer.Serialize(profileModels, _jsonSerializerOptions);
            
            // write file contents
            await sw.WriteAsync(profileModelsJson);
        }

        private async Task<bool> NoProfilesConfigured()
        {
            // if the settings file does not exist / default is our blank value using a case insensitive method,
            // raise the no profiles found
            var currentProfiles = new Dictionary<string, ProfileModel>(
                await GetProfilesAsync(),
                StringComparer.InvariantCultureIgnoreCase);

            return currentProfiles.Count == 0 ||
                   currentProfiles.Count == 1 &&
                   currentProfiles.ContainsKey("default") &&
                   currentProfiles["default"].ClientId == Guid.Empty;
        }

        #endregion
    }
}