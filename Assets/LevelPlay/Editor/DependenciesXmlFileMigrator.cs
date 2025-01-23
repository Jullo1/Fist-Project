using System;
using System.IO;
using UnityEditor;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

namespace Unity.Services.LevelPlay.Editor
{
    class DependenciesXmlFileMigrator
    {
        private readonly ILevelPlayLogger m_Logger;
        private readonly ILevelPlayNetworkManager m_LevelPlayNetworkManager;
        internal DependenciesXmlFileMigrator() : this(EditorServices.Instance.LevelPlayLogger, EditorServices.Instance.LevelPlayNetworkManager)
        {
        }

        internal DependenciesXmlFileMigrator(ILevelPlayLogger logger, ILevelPlayNetworkManager levelPlayNetworkManager)
        {
            m_Logger = logger;
            m_LevelPlayNetworkManager = levelPlayNetworkManager;
        }

        [InitializeOnLoadMethod]
        static async Task InstallIronSourceSdkIfNotInstalled()
        {
            var migrator = new DependenciesXmlFileMigrator();
            await migrator.InstallIronSourceSdkIfNotInstalledInternal();
        }

        internal async Task InstallIronSourceSdkIfNotInstalledInternal()
        {
            try
            {
                m_LevelPlayNetworkManager.LoadVersionsFromJson();
            }
            catch (Exception e)
            {
                m_Logger.LogError($"Failed to load versions json : {e.ToString()}");
            }
            try
            {
                await m_LevelPlayNetworkManager.GetVersionsWebRequest();
            }
            catch (Exception e)
            {
                m_Logger.LogError($"Failed to fetch versions json : {e.ToString()}");
            }
            try
            {
                m_LevelPlayNetworkManager.LoadVersionsFromJson();
            }
            catch (Exception e)
            {
                m_Logger.LogError($"Failed to load versions json after fetching from remote : {e.ToString()}");
            }
            try
            {
                var currentIronSourceSdkVersion = m_LevelPlayNetworkManager.InstalledSdkVersion();
                // Install the latest compatible IronSource SDK if it is not installed
                if (currentIronSourceSdkVersion == null)
                {
                    var latestIronSourceSdkVersion = m_LevelPlayNetworkManager.CompatibleIronSourceSdkVersions().FirstOrDefault();
                    if (latestIronSourceSdkVersion != null)
                    {
                        await m_LevelPlayNetworkManager.Install(latestIronSourceSdkVersion);
                        AssetDatabase.Refresh();
                        m_LevelPlayNetworkManager.UiUpdate();
                    }
                }
            }
            catch (Exception e)
            {
                m_Logger.LogError($"Failed to install IronSource SDK with exception : {e.ToString()}");
            }
        }
    }
}
