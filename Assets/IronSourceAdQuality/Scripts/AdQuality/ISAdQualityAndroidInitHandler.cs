#if UNITY_ANDROID
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ISAdQualityAndroidInitHandler : AndroidJavaProxy
{
    private const string IRON_SOURCE_AD_QUALITY_CLASS = "com.ironsource.adqualitysdk.sdk.unity.IronSourceAdQuality";
    private const string UNITY_IS_AD_QUALITY_INIT_LISTENER = "com.ironsource.adqualitysdk.sdk.unity.UnityISAdQualityInitListener";
    
    public event Action OnAdQualitySdkInitSuccess = delegate { };
    public event Action<ISAdQualityInitError, string> OnAdQualitySdkInitFailed = delegate { };

    //implements UnityISAdQualityInitListener java interface
    public ISAdQualityAndroidInitHandler(): base(UNITY_IS_AD_QUALITY_INIT_LISTENER)
    {
#if !UNITY_EDITOR
        try
        {
            using (var jniAdQualityClass = new AndroidJavaClass(IRON_SOURCE_AD_QUALITY_CLASS))
            {
                jniAdQualityClass.CallStatic("setUnityISAdQualityInitListener", this);
            }
        }
        catch(Exception e)
        {
            Debug.LogError("setUnityISAdQualityInitListener method doesn't exist, error: " + e.Message);
        }
#endif
    }
  
    [Preserve]
    public void adQualitySdkInitSuccess()
    {
        if (this.OnAdQualitySdkInitSuccess != null)
        {
            this.OnAdQualitySdkInitSuccess();
        }

    }

    [Preserve]
    public void adQualitySdkInitFailed(int adQualitySdkInitError, string errorMessage)
    {
        if (this.OnAdQualitySdkInitFailed != null)
        {
            this.OnAdQualitySdkInitFailed((ISAdQualityInitError)adQualitySdkInitError, errorMessage);
        }
    }
}
#endif