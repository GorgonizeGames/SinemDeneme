// File: Assets/_Game/Scripts/Runtime/Core/Services/SaveService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Game.Runtime.Core.Data;
using System.Linq;
using Game.Runtime.Core.Interfaces;

namespace Game.Runtime.Core.Services
{
    /// <summary>
    /// Modern save service implementation using Unity's persistent data path + JSON
    /// </summary>
    public class SaveService : ISaveService
    {
        private readonly string _saveDirectory;
        private readonly string _fileExtension = ".json";
        
        // Cache to avoid repeated file reads
        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();
        private readonly Dictionary<string, DateTime> _cacheTimestamps = new Dictionary<string, DateTime>();
        private readonly float _cacheExpireTime = 30f; // 30 seconds cache

        public event Action<string> OnDataSaved;
        public event Action<string> OnDataLoaded;
        public event Action<string> OnDataDeleted;

        public SaveService()
        {
            _saveDirectory = System.IO.Path.Combine(Application.persistentDataPath, "SaveData");
            
            // Create directory if it doesn't exist
            if (!System.IO.Directory.Exists(_saveDirectory))
            {
                System.IO.Directory.CreateDirectory(_saveDirectory);
            }
        }

        // ==================== Generic Save/Load ====================

        public async Task SaveDataAsync<T>(string key, T data) where T : class
        {
            if (string.IsNullOrEmpty(key) || data == null) return;

            try
            {
                string json = JsonUtility.ToJson(data, true);
                string filePath = GetFilePath(key);

                await WriteFileAsync(filePath, json);

                // Update cache
                _cache[key] = data;
                _cacheTimestamps[key] = DateTime.Now;

                OnDataSaved?.Invoke(key);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save data for key '{key}': {e.Message}");
            }
        }

        public async Task<T> LoadDataAsync<T>(string key) where T : class
        {
            if (string.IsNullOrEmpty(key)) return null;

            try
            {
                // Check cache first
                if (IsCacheValid(key) && _cache.TryGetValue(key, out var cachedData))
                {
                    return cachedData as T;
                }

                string filePath = GetFilePath(key);
                
                if (!System.IO.File.Exists(filePath))
                    return null;

                string json = await ReadFileAsync(filePath);
                
                if (string.IsNullOrEmpty(json))
                    return null;

                T data = JsonUtility.FromJson<T>(json);

                // Update cache
                _cache[key] = data;
                _cacheTimestamps[key] = DateTime.Now;

                OnDataLoaded?.Invoke(key);
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load data for key '{key}': {e.Message}");
                return null;
            }
        }

        public bool HasData(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            
            // Check cache first
            if (IsCacheValid(key) && _cache.ContainsKey(key))
                return true;

            string filePath = GetFilePath(key);
            return System.IO.File.Exists(filePath);
        }

        public async Task DeleteDataAsync(string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            try
            {
                string filePath = GetFilePath(key);
                
                if (System.IO.File.Exists(filePath))
                {
                    await Task.Run(() => System.IO.File.Delete(filePath));
                }

                // Remove from cache
                _cache.Remove(key);
                _cacheTimestamps.Remove(key);

                OnDataDeleted?.Invoke(key);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete data for key '{key}': {e.Message}");
            }
        }

        public async Task ClearAllDataAsync()
        {
            try
            {
                if (System.IO.Directory.Exists(_saveDirectory))
                {
                    await Task.Run(() => System.IO.Directory.Delete(_saveDirectory, true));
                    System.IO.Directory.CreateDirectory(_saveDirectory);
                }

                _cache.Clear();
                _cacheTimestamps.Clear();

                Debug.Log("All save data cleared");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to clear all data: {e.Message}");
            }
        }

        // ==================== BaseDataModel Specific Methods ====================

        public async Task SaveModelAsync<T>(T model) where T : BaseDataModel
        {
            if (model == null || string.IsNullOrEmpty(model.id))
            {
                Debug.LogError("Cannot save model: null model or empty ID");
                return;
            }

            string key = GetModelKey<T>(model.id);
            await SaveDataAsync(key, model);
        }

        public async Task<T> LoadModelAsync<T>(string id) where T : BaseDataModel
        {
            if (string.IsNullOrEmpty(id)) return null;

            string key = GetModelKey<T>(id);
            return await LoadDataAsync<T>(key);
        }

        public async Task<List<T>> LoadAllModelsAsync<T>() where T : BaseDataModel
        {
            var results = new List<T>();
            string modelPrefix = GetModelPrefix<T>();

            try
            {
                var files = System.IO.Directory.GetFiles(_saveDirectory, $"{modelPrefix}*{_fileExtension}");
                
                foreach (string file in files)
                {
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(file);
                    string id = fileName.Substring(modelPrefix.Length);
                    
                    var model = await LoadModelAsync<T>(id);
                    if (model != null)
                    {
                        results.Add(model);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load all models of type {typeof(T).Name}: {e.Message}");
            }

            return results;
        }

        public async Task DeleteModelAsync<T>(string id) where T : BaseDataModel
        {
            if (string.IsNullOrEmpty(id)) return;

            string key = GetModelKey<T>(id);
            await DeleteDataAsync(key);
        }

        // ==================== Batch Operations ====================

        public async Task SaveMultipleAsync<T>(Dictionary<string, T> dataDict) where T : class
        {
            if (dataDict == null || dataDict.Count == 0) return;

            var tasks = new List<Task>();
            
            foreach (var kvp in dataDict)
            {
                tasks.Add(SaveDataAsync(kvp.Key, kvp.Value));
            }

            await Task.WhenAll(tasks);
        }

        public async Task<Dictionary<string, T>> LoadMultipleAsync<T>(List<string> keys) where T : class
        {
            var result = new Dictionary<string, T>();
            
            if (keys == null || keys.Count == 0) return result;

            var tasks = keys.Select(async key =>
            {
                var data = await LoadDataAsync<T>(key);
                return new { Key = key, Data = data };
            });

            var results = await Task.WhenAll(tasks);

            foreach (var item in results)
            {
                if (item.Data != null)
                {
                    result[item.Key] = item.Data;
                }
            }

            return result;
        }

        // ==================== Helper Methods ====================

        private string GetFilePath(string key)
        {
            return System.IO.Path.Combine(_saveDirectory, key + _fileExtension);
        }

        private string GetModelKey<T>(string id) where T : BaseDataModel
        {
            return $"{typeof(T).Name}_{id}";
        }

        private string GetModelPrefix<T>() where T : BaseDataModel
        {
            return $"{typeof(T).Name}_";
        }

        private bool IsCacheValid(string key)
        {
            if (!_cacheTimestamps.TryGetValue(key, out var timestamp))
                return false;

            return (DateTime.Now - timestamp).TotalSeconds < _cacheExpireTime;
        }

        private async Task WriteFileAsync(string filePath, string content)
        {
            await Task.Run(() =>
            {
                System.IO.File.WriteAllText(filePath, content, System.Text.Encoding.UTF8);
            });
        }

        private async Task<string> ReadFileAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                return System.IO.File.ReadAllText(filePath, System.Text.Encoding.UTF8);
            });
        }
    }
}