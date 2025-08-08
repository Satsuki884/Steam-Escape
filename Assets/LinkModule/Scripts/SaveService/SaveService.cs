using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using TitanModulePackage.Scripts.SaveService;
using UnityEngine;

namespace LinkModule.Scripts.SaveService
{
    public class SaveService : MonoBehaviour, ISaveService
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("DS3%l6fq9nx=+R}z<9;E4(+=(lYUF9Jp");
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("&tKWyIH6Bn{|46)9");
        
        private string _saveDirectory;
        private string _mainSaveFile;
        private string _fieldSaveFile;
        
        private SaveData _cachedData;
        private Dictionary<string, object> _fieldCache = new Dictionary<string, object>();
        
        void Awake()
        {
            InitializePaths();
            EnsureSaveDirectoryExists();
            LoadCache();
        }
        
        private void InitializePaths()
        {
            _saveDirectory = Path.Combine(Application.persistentDataPath, "saves");
            _mainSaveFile = Path.Combine(_saveDirectory, "main.dat");
            _fieldSaveFile = Path.Combine(_saveDirectory, "fields.dat");
        }

        #region Full Save/Load Operations
        
        public void Save(SaveData data)
        {
            _cachedData = data;
            SaveToFile(_mainSaveFile, data);
        }

        public SaveData Load()
        {
            if (_cachedData != null)
                return _cachedData;
                
            _cachedData = LoadFromFile<SaveData>(_mainSaveFile) ?? new SaveData();
            return _cachedData;
        }
        
        #endregion

        #region Field-Specific Operations
        
        /// <summary>
        /// Save a specific field value
        /// </summary>
        public void SaveField<T>(string fieldName, T value)
        {
            _fieldCache[fieldName] = value;
            
            var fieldData = new FieldSaveData
            {
                fields = _fieldCache,
                lastModified = DateTime.UtcNow
            };
            
            SaveToFile(_fieldSaveFile, fieldData);
            
            // Also update the main data if it exists
            if (_cachedData != null)
            {
                UpdateMainDataField(fieldName, value);
            }
        }
        
        /// <summary>
        /// Load a specific field value
        /// </summary>
        public T LoadField<T>(string fieldName, T defaultValue = default)
        {
            if (_fieldCache.ContainsKey(fieldName))
            {
                return ConvertValue<T>(_fieldCache[fieldName]);
            }
            
            return defaultValue;
        }
        
        /// <summary>
        /// Save multiple fields at once
        /// </summary>
        public void SaveFields(Dictionary<string, object> fields)
        {
            foreach (var field in fields)
            {
                _fieldCache[field.Key] = field.Value;
            }
            
            var fieldData = new FieldSaveData
            {
                fields = _fieldCache,
                lastModified = DateTime.UtcNow
            };
            
            SaveToFile(_fieldSaveFile, fieldData);
            
            // Update main data if it exists
            if (_cachedData != null)
            {
                foreach (var field in fields)
                {
                    UpdateMainDataField(field.Key, field.Value);
                }
            }
        }
        
        /// <summary>
        /// Check if a field exists
        /// </summary>
        public bool HasField(string fieldName)
        {
            return _fieldCache.ContainsKey(fieldName);
        }
        
        /// <summary>
        /// Delete a specific field
        /// </summary>
        public void DeleteField(string fieldName)
        {
            _fieldCache.Remove(fieldName);
            
            var fieldData = new FieldSaveData
            {
                fields = _fieldCache,
                lastModified = DateTime.UtcNow
            };
            
            SaveToFile(_fieldSaveFile, fieldData);
        }
        
        /// <summary>
        /// Get all saved field names
        /// </summary>
        public string[] GetFieldNames()
        {
            var names = new string[_fieldCache.Count];
            _fieldCache.Keys.CopyTo(names, 0);
            return names;
        }
        
        #endregion

        #region Convenience Methods for SaveData Fields
        
        public void SaveUrl(string url)
        {
            SaveField("url", url);
        }
        
        public string LoadUrl(string defaultUrl = "")
        {
            return LoadField("url", defaultUrl);
        }
        
        public void SaveOpenGame(bool openGame)
        {
            SaveField("openGame", openGame);
        }
        
        public bool LoadOpenGame(bool defaultValue = false)
        {
            return LoadField("openGame", defaultValue);
        }
        
        public void SaveIsOpenFirstTime(bool openFirstTime)
        {
            SaveField("openFirstTime", openFirstTime);
        }
        
        public bool LoadIsOpenFirstTime(bool defaultValue = true)
        {
            return LoadField("openFirstTime", defaultValue);
        }
        
        #endregion

        #region Private Helper Methods
        
        private void EnsureSaveDirectoryExists()
        {
            if (!Directory.Exists(_saveDirectory))
            {
                Directory.CreateDirectory(_saveDirectory);
            }
        }
        
        private void LoadCache()
        {
            var fieldData = LoadFromFile<FieldSaveData>(_fieldSaveFile);
            if (fieldData != null)
            {
                _fieldCache = fieldData.fields ?? new Dictionary<string, object>();
            }
        }
        
        private void UpdateMainDataField(string fieldName, object value)
        {
            switch (fieldName.ToLower())
            {
                case "url":
                    _cachedData.url = value?.ToString() ?? "";
                    break;
                case "opengame":
                    _cachedData.openGame = ConvertValue<bool>(value);
                    break;
                case "openfirsttime":
                    _cachedData.openFirstTime = ConvertValue<bool>(value);
                    break;
            }
        }
        
        private T ConvertValue<T>(object value)
        {
            if (value is T directValue)
                return directValue;
                
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }
        
        private void SaveToFile<T>(string filePath, T data)
        {
            try
            {
                string json;
                if (typeof(T) == typeof(SaveData))
                {
                    json = JsonUtility.ToJson(data);
                }
                else if (typeof(T) == typeof(FieldSaveData))
                {
                    json = SerializeFieldData((FieldSaveData)(object)data);
                }
                else
                {
                    json = JsonUtility.ToJson(data);
                }
                
                byte[] rawData = Encoding.UTF8.GetBytes(json);
                byte[] encrypted = EncryptBytes(rawData);
                
                File.WriteAllBytes(filePath, encrypted);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save to {filePath}: {ex.Message}");
            }
        }
        
        private T LoadFromFile<T>(string filePath) where T : class
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;
                
                byte[] encrypted = File.ReadAllBytes(filePath);
                byte[] decrypted = DecryptBytes(encrypted);
                string json = Encoding.UTF8.GetString(decrypted);
                
                if (typeof(T) == typeof(SaveData))
                {
                    return JsonUtility.FromJson<T>(json);
                }
                else if (typeof(T) == typeof(FieldSaveData))
                {
                    return (T)(object)DeserializeFieldData(json);
                }
                else
                {
                    return JsonUtility.FromJson<T>(json);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load from {filePath}: {ex.Message}");
                return null;
            }
        }
        
        private string SerializeFieldData(FieldSaveData fieldData)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append($"\"lastModified\":\"{fieldData.lastModified:O}\",");
            sb.Append("\"fields\":{");
            
            bool first = true;
            foreach (var kvp in fieldData.fields)
            {
                if (!first) sb.Append(",");
                first = false;
                
                sb.Append($"\"{kvp.Key}\":");
                
                if (kvp.Value == null)
                {
                    sb.Append("null");
                }
                else if (kvp.Value is string)
                {
                    sb.Append($"\"{kvp.Value}\"");
                }
                else if (kvp.Value is bool)
                {
                    sb.Append(kvp.Value.ToString().ToLower());
                }
                else if (IsNumericType(kvp.Value))
                {
                    sb.Append(kvp.Value.ToString());
                }
                else
                {
                    // For complex objects, use JsonUtility
                    sb.Append($"\"{JsonUtility.ToJson(kvp.Value)}\"");
                }
            }
            
            sb.Append("}}");
            return sb.ToString();
        }
        
        private FieldSaveData DeserializeFieldData(string json)
        {
            var fieldData = new FieldSaveData();
            
            // Simple JSON parsing for our specific structure
            json = json.Trim('{', '}');
            var parts = SplitJsonParts(json);
            
            foreach (var part in parts)
            {
                var colonIndex = part.IndexOf(':');
                if (colonIndex == -1) continue;
                
                var key = part.Substring(0, colonIndex).Trim('"');
                var value = part.Substring(colonIndex + 1);
                
                if (key == "lastModified")
                {
                    if (DateTime.TryParse(value.Trim('"'), out DateTime dateTime))
                    {
                        fieldData.lastModified = dateTime;
                    }
                }
                else if (key == "fields")
                {
                    fieldData.fields = ParseFieldsObject(value);
                }
            }
            
            return fieldData;
        }
        
        private Dictionary<string, object> ParseFieldsObject(string fieldsJson)
        {
            var fields = new Dictionary<string, object>();
            fieldsJson = fieldsJson.Trim('{', '}');
            
            if (string.IsNullOrEmpty(fieldsJson))
                return fields;
                
            var parts = SplitJsonParts(fieldsJson);
            
            foreach (var part in parts)
            {
                var colonIndex = part.IndexOf(':');
                if (colonIndex == -1) continue;
                
                var key = part.Substring(0, colonIndex).Trim('"');
                var valueStr = part.Substring(colonIndex + 1);
                
                object value = ParseJsonValue(valueStr);
                fields[key] = value;
            }
            
            return fields;
        }
        
        private string[] SplitJsonParts(string json)
        {
            var parts = new List<string>();
            var current = new StringBuilder();
            int braceLevel = 0;
            bool inQuotes = false;
            
            for (int i = 0; i < json.Length; i++)
            {
                char c = json[i];
                
                if (c == '"' && (i == 0 || json[i - 1] != '\\'))
                {
                    inQuotes = !inQuotes;
                }
                
                if (!inQuotes)
                {
                    if (c == '{') braceLevel++;
                    else if (c == '}') braceLevel--;
                    else if (c == ',' && braceLevel == 0)
                    {
                        parts.Add(current.ToString().Trim());
                        current.Clear();
                        continue;
                    }
                }
                
                current.Append(c);
            }
            
            if (current.Length > 0)
                parts.Add(current.ToString().Trim());
                
            return parts.ToArray();
        }
        
        private object ParseJsonValue(string valueStr)
        {
            valueStr = valueStr.Trim();
            
            if (valueStr == "null")
                return null;
            if (valueStr == "true")
                return true;
            if (valueStr == "false")
                return false;
            if (valueStr.StartsWith("\"") && valueStr.EndsWith("\""))
                return valueStr.Substring(1, valueStr.Length - 2);
            if (int.TryParse(valueStr, out int intVal))
                return intVal;
            if (float.TryParse(valueStr, out float floatVal))
                return floatVal;
            if (double.TryParse(valueStr, out double doubleVal))
                return doubleVal;
                
            return valueStr;
        }
        
        private bool IsNumericType(object value)
        {
            return value is byte || value is sbyte ||
                   value is short || value is ushort ||
                   value is int || value is uint ||
                   value is long || value is ulong ||
                   value is float || value is double ||
                   value is decimal;
        }
        
        private static byte[] EncryptBytes(byte[] plainData)
        {
            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            using MemoryStream ms = new MemoryStream();
            using CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(plainData, 0, plainData.Length);
            cs.FlushFinalBlock();
            return ms.ToArray();
        }

        private static byte[] DecryptBytes(byte[] cipherData)
        {
            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            using MemoryStream ms = new MemoryStream();
            using CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipherData, 0, cipherData.Length);
            cs.FlushFinalBlock();
            return ms.ToArray();
        }
        
        #endregion

        #region Cleanup
        
        public void ClearCache()
        {
            _cachedData = null;
            _fieldCache.Clear();
        }
        
        public void DeleteAllSaves()
        {
            try
            {
                if (File.Exists(_mainSaveFile))
                    File.Delete(_mainSaveFile);
                    
                if (File.Exists(_fieldSaveFile))
                    File.Delete(_fieldSaveFile);
                    
                ClearCache();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete saves: {ex.Message}");
            }
        }
        
        #endregion
    }

    [Serializable]
    public class SaveData
    {
        public string url = "";
        public bool openGame = false;
        public bool openFirstTime = true;
    }
    
    [Serializable]
    internal class FieldSaveData
    {
        public Dictionary<string, object> fields = new Dictionary<string, object>();
        public DateTime lastModified = DateTime.UtcNow;
    }
}