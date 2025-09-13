using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Runtime.Core.Data;

namespace Game.Runtime.Core.Interfaces
{
    /// <summary>
    /// Modern save service interface - JSON based, async, type-safe
    /// </summary>
    public interface ISaveService
    {
        // Generic save/load methods
        Task SaveDataAsync<T>(string key, T data) where T : class;
        Task<T> LoadDataAsync<T>(string key) where T : class;
        bool HasData(string key);
        Task DeleteDataAsync(string key);
        Task ClearAllDataAsync();

        // BaseDataModel specific methods (using ID)
        Task SaveModelAsync<T>(T model) where T : BaseDataModel;
        Task<T> LoadModelAsync<T>(string id) where T : BaseDataModel;
        Task<List<T>> LoadAllModelsAsync<T>() where T : BaseDataModel;
        Task DeleteModelAsync<T>(string id) where T : BaseDataModel;

        // Batch operations for performance
        Task SaveMultipleAsync<T>(Dictionary<string, T> dataDict) where T : class;
        Task<Dictionary<string, T>> LoadMultipleAsync<T>(List<string> keys) where T : class;

        // Events
        event Action<string> OnDataSaved;
        event Action<string> OnDataLoaded;
        event Action<string> OnDataDeleted;
    }

}