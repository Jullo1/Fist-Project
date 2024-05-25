using UnityEngine;
using System;

public class ISAdQualityInitCallbackWrapper : MonoBehaviour
{
    private ISAdQualityInitCallback mCallback;

#if UNITY_ANDROID
    private ISAdQualityAndroidInitHandler adQualityAndroidInitHandler;
#endif

#if UNITY_IPHONE || UNITY_IOS
    private ISAdQualityiOSInitHandler adQualityiOSInitHandler;
#endif

    public ISAdQualityInitCallback AdQualityInitCallback 
    {
        set 
        {
            mCallback = value;  
        }
        get 
        {
            return mCallback;
        }
    }

    void Awake ()
    {
#if UNITY_ANDROID
        adQualityAndroidInitHandler = new ISAdQualityAndroidInitHandler();  //sets this.adQualityAndroidInitHandler as listener for init events on the bridge
        registerAdQualityAndroidInitEvents();
#endif

#if UNITY_IPHONE || UNITY_IOS        
        registerAdQualityiOSInitEvents();
        adQualityiOSInitHandler = new ISAdQualityiOSInitHandler();  //sets this.adQualityiOSInit as listener for init events on the bridge
#endif
        DontDestroyOnLoad(gameObject);  //Makes the object not be destroyed automatically when loading a new scene.
    }

    private void adQualitySdkInitSuccess() 
    {
        if (mCallback != null) 
        {
            mCallback.adQualitySdkInitSuccess();
        }
    }

    private void onAdQualitySdkInitFailed(ISAdQualityInitError sdkInitError, string errorMsg) 
    {
        if (mCallback != null) 
        {
            mCallback.adQualitySdkInitFailed(sdkInitError, errorMsg);
        }
    }

#if UNITY_ANDROID
    //subscribe to ISAdQualityAndroidInitHandler events
    private void registerAdQualityAndroidInitEvents() {
        adQualityAndroidInitHandler.OnAdQualitySdkInitSuccess += () =>
        {
            adQualitySdkInitSuccess();
        };

        adQualityAndroidInitHandler.OnAdQualitySdkInitFailed += (sdkInitError, errorMsg) => 
        {
            onAdQualitySdkInitFailed(sdkInitError, errorMsg);
        };
    }
#endif

#if UNITY_IPHONE || UNITY_IOS
    //subscribe to ISAdQualityiOSInitHandler events
    private void registerAdQualityiOSInitEvents() {
        ISAdQualityiOSInitHandler.OnAdQualitySdkInitSuccess += () =>
        {
            adQualitySdkInitSuccess();
        };

        ISAdQualityiOSInitHandler.OnAdQualitySdkInitFailed += (sdkInitError, errorMsg) => 
        {
            onAdQualitySdkInitFailed(sdkInitError, errorMsg);
        };
    }
#endif

}