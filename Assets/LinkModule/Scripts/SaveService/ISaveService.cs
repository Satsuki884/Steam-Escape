using System.Collections.Generic;
using LinkModule.Scripts.SaveService;

namespace TitanModulePackage.Scripts.SaveService
{
    public interface ISaveService
    {
        #region Full Save/Load Operations
        void Save(SaveData data);
        SaveData Load();
        #endregion

        #region Field-Specific Operations
        /// <summary>
        /// Save a specific field value
        /// </summary>
        void SaveField<T>(string fieldName, T value);
        
        /// <summary>
        /// Load a specific field value with optional default
        /// </summary>
        T LoadField<T>(string fieldName, T defaultValue = default);
        
        /// <summary>
        /// Save multiple fields at once
        /// </summary>
        void SaveFields(Dictionary<string, object> fields);
        
        /// <summary>
        /// Check if a field exists
        /// </summary>
        bool HasField(string fieldName);
        
        /// <summary>
        /// Delete a specific field
        /// </summary>
        void DeleteField(string fieldName);
        
        /// <summary>
        /// Get all saved field names
        /// </summary>
        string[] GetFieldNames();
        #endregion

        #region Convenience Methods for SaveData Fields
        void SaveUrl(string url);
        string LoadUrl(string defaultUrl = "");
        
        void SaveOpenGame(bool openGame);
        bool LoadOpenGame(bool defaultValue = false);
        
        void SaveIsOpenFirstTime(bool isOpenFirstTime);
        bool LoadIsOpenFirstTime(bool defaultValue = true);
        #endregion

        #region Cleanup Operations
        /// <summary>
        /// Clear in-memory cache
        /// </summary>
        void ClearCache();
        
        /// <summary>
        /// Delete all save files
        /// </summary>
        void DeleteAllSaves();
        #endregion
    }
}