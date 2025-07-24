#if UNITY_ANDROID
using System;
using Unity.Services.LevelPlay;
using UnityEngine;

[Obsolete("This class will be made private in version 9.0.0.")]
public class IronSourceRewardedVideoLevelPlayManualAndroid : AndroidJavaProxy, IUnityLevelPlayRewardedVideoManual
{
    public IronSourceRewardedVideoLevelPlayManualAndroid() : base(IronSourceConstants.LevelPlayRewardedVideoManualBridgeListenerClass)
    {
        try
        {
            using (var pluginClass = new AndroidJavaClass(IronSourceConstants.bridgeClass))
            {
                var bridgeInstance = pluginClass.CallStatic<AndroidJavaObject>("getInstance");
                bridgeInstance.Call("setUnityRewardedVideoManualLevelPlayListener", this);
            }
        }
        catch (Exception e)
        {
            LevelPlayLogger.LogError("setUnityRewardedVideoManualLevelPlayListener method doesn't exist, error: " + e.Message);
        }
    }

    public event Action<IronSourceError> OnAdLoadFailed = delegate {};
    public event Action<IronSourceAdInfo> OnAdReady = delegate {};


    void onAdReady(string data)
    {
        if (this.OnAdReady != null)
        {
            IronSourceAdInfo adInfo = new IronSourceAdInfo(data);
            this.OnAdReady(adInfo);
        }
    }

    void onAdLoadFailed(string args)
    {
        if (this.OnAdLoadFailed != null)
        {
            IronSourceError err = IronSourceUtils.getErrorFromErrorObject(args);
            this.OnAdLoadFailed(err);
        }
    }
}

#endif
