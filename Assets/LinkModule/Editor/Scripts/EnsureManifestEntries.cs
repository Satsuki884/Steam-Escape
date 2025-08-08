#if UNITY_ANDROID

using System.IO;
using System.Xml;
using UnityEditor.Android;
using UnityEngine;
#if USE_FACEBOOK_SDK
using Facebook.Unity.Settings;
#endif

namespace LinkModule.Editor.Scripts
{
    public class EnsureManifestEntries : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 200;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            string manifestPath = Path.Combine(path, "src", "main", "AndroidManifest.xml");
            if (!File.Exists(manifestPath))
            {
                Debug.LogError("AndroidManifest.xml not found");
                return;
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(manifestPath);

            XmlNode manifest = doc.SelectSingleNode("/manifest");
            XmlNode application = manifest.SelectSingleNode("application");

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
            nsMgr.AddNamespace("android", "http://schemas.android.com/apk/res/android");
            
            // --- Add uses-permission ---
            void AddPermission(string name)
            {
                if (doc.SelectSingleNode($"/manifest/uses-permission[@android:name='{name}']", nsMgr) == null)
                {
                    XmlElement perm = doc.CreateElement("uses-permission");
                    perm.SetAttribute("name", "http://schemas.android.com/apk/res/android", name);
                    manifest.InsertBefore(perm, application);
                    Debug.Log($"Added permission: {name}");
                }
            }
            
            // --- Add meta-data ---
            void AddMeta(string name, string value, bool isResource = false)
            {
                foreach (XmlNode node in application.ChildNodes)
                {
                    if (node.Name == "meta-data" && node.Attributes?["android:name"]?.Value == name)
                        return;
                }

                XmlElement meta = doc.CreateElement("meta-data");
                meta.SetAttribute("name", "http://schemas.android.com/apk/res/android", name);
                string attr = isResource ? "resource" : "value";
                meta.SetAttribute(attr, "http://schemas.android.com/apk/res/android", value);
                application.AppendChild(meta);
                Debug.Log($"Added meta-data: {name}");
            }

            // --- Add <queries> ---
            void EnsureQueries(XmlDocument docRef, XmlNode manifestNode)
            {
                XmlNode queriesNode = manifestNode.SelectSingleNode("queries");
                if (queriesNode == null)
                {
                    queriesNode = docRef.CreateElement("queries");
                    manifestNode.PrependChild(queriesNode);
                }

                string[] packages = {
                    "org.telegram.messenger",
                    //"org.telegram.messenger.web",
                    "com.whatsapp",
                    "com.viber.voip"
                };

                foreach (string pkg in packages)
                {
                    bool exists = false;
                    foreach (XmlNode node in queriesNode.ChildNodes)
                    {
                        if (node.Name == "package" && node.Attributes?["android:name"]?.Value == pkg)
                        {
                            exists = true;
                            break;
                        }
                    }

                    if (!exists)
                    {
                        XmlElement pkgElem = docRef.CreateElement("package");
                        pkgElem.SetAttribute("name", "http://schemas.android.com/apk/res/android", pkg);
                        queriesNode.AppendChild(pkgElem);
                        Debug.Log($"Added <package android:name=\"{pkg}\" /> в <queries>");
                    }
                }
            }
            
            // --- Modify Unity activity ---
            XmlNodeList activities = application.SelectNodes("activity");
            foreach (XmlNode activity in activities)
            {
                var nameAttr = activity.Attributes?["android:name", nsMgr.LookupNamespace("android")];
                if (nameAttr != null && nameAttr.Value == "com.unity3d.player.UnityPlayerActivity")
                {
                    SetAttribute(activity, "launchMode", "singleTask");
                    SetAttribute(activity, "alwaysRetainTaskState", "true");
                    SetAttribute(activity, "configChanges", "orientation|screenSize|keyboardHidden|keyboard|navigation|locale|fontScale|screenLayout|density|uiMode");
                    SetAttribute(activity, "windowSoftInputMode", "adjustResize");
                    
                    // --- Helper method to set or update attribute ---
                    void SetAttribute(XmlNode node, string attributeName, string value)
                    {
                        XmlAttribute attr = node.Attributes["android:" + attributeName];
                        if (attr == null)
                        {
                            attr = doc.CreateAttribute("android", attributeName, nsMgr.LookupNamespace("android"));
                            node.Attributes.Append(attr);
                        }
                        attr.Value = value;
                    }

                    Debug.Log("Updated UnityPlayerActivity attributes to prevent app restart");
                    break;
                }
            }
            
            AddPermission("android.permission.INTERNET");
            AddPermission("android.permission.CAMERA");
            
#if USE_FACEBOOK_SDK
            // === FACEBOOK SDK ===
            string fbAppId = FacebookSettings.AppId;
            string fbClientToken = FacebookSettings.ClientToken;

            if (!string.IsNullOrEmpty(fbAppId))
                AddMeta("com.facebook.sdk.ApplicationId", "fb" + fbAppId);

            if (!string.IsNullOrEmpty(fbClientToken))
                AddMeta("com.facebook.sdk.ClientToken", fbClientToken);
#endif

#if USE_APPSFLYER_SDK
            // === APPSFLYER SDK ===
            AddPermission("android.permission.ACCESS_NETWORK_STATE");
            // AddMeta("com.appsflyer.devkey", "YOUR_REAL_DEV_KEY_HERE");
            AddMeta("com.appsflyer.disableBroadcastReceiver", "false");
#endif

#if USE_FIREBASE_SDK
            // === FIREBASE MESSAGING ===
            AddPermission("android.permission.WAKE_LOCK");
            AddPermission("com.google.android.c2dm.permission.RECEIVE");
#endif

            // === MESSENGERS ===
            EnsureQueries(doc, manifest);

            doc.Save(manifestPath);
            Debug.Log("AndroidManifest.xml updated successfully");
        }
    }
}
#endif