using System;
using UnityEngine;

namespace LinkModule.Scripts.Helper
{
    public static class ServerDataParser
    {
        public static ReferrerData Parse(string rawData)
        {
            try
            {
                var wrapper = JsonUtility.FromJson<ServerResponse>(rawData);
                if (wrapper == null || wrapper.data == null)
                {
                    Debug.LogWarning("[ReferrerDataParser] No data found in server response.");
                    return null;
                }

                return wrapper.data;
            }
            catch (Exception e)
            {
                Debug.LogError("[ReferrerDataParser] Failed to parse: " + e.Message);
                return null;
            }
        }
        
        public static FbData ParseSimple(string rawData)
        {
            try
            {
                var data = JsonUtility.FromJson<FbData>(rawData);
                if (data == null || (string.IsNullOrEmpty(data.fid) && string.IsNullOrEmpty(data.ftok)))
                {
                    Debug.LogWarning("[FbDataParser] No fields found in server response.");
                    return null;
                }
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError("[FbDataParser] Failed to parse simple: " + e.Message);
                return null;
            }
        }
    }
}