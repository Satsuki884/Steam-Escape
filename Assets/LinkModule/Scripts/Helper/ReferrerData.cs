using System;

namespace LinkModule.Scripts.Helper
{
    [Serializable]
    public class ReferrerData
    {
        public string ad_id;
        public string ad_objective_name;
        public string adgroup_id;
        public string adgroup_name;
        public string campaign_id;
        public string campaign_name;
        public string campaign_group_id;
        public string campaign_group_name;
        public string account_id;
        public string is_instagram;
        public string is_an;
        public string publisher_platform;
        public string platform_position;
    }
    
    [Serializable]
    public class FbData
    {
        public string fid;
        public string ftok;
    }
    
    [Serializable]
    public class ServerResponse
    {
        public string status;
        public string message;
        public ReferrerData data;
    }
}