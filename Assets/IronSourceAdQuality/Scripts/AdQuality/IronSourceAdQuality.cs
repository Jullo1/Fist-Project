using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using ISAdQualityJSON;

public class IronSourceAdQuality : CodeGeneratedSingleton
{
    private static GameObject adQualityGameObject = new GameObject("IronSourceAdQuality");

#if UNITY_IOS && !UNITY_EDITOR

    [DllImport ("__Internal")]
    private static extern int ironSourceAdQuality_initialize(string appKey, string userId, bool userIdSet, bool testMode, 
                                                        bool debug, int logLevel, string initializationSource, bool coppa,
                                                        int deviceIdType, bool isInitCallbackSet);
    [DllImport ("__Internal")]
    private static extern int ironSourceAdQuality_changeUserId(string userId);
    [DllImport ("__Internal")]
    private static extern int ironSourceAdQuality_setUserConsent(bool userConsent);
    [DllImport ("__Internal")]
    private static extern int ironSourceAdQuality_sendCustomMediationRevenue(int mediationNetwork, int adType, string placement, double revenue);
    [DllImport ("__Internal")]
    private static extern int ironSourceAdQuality_setSegment(string jsonString);

#endif

    protected override bool DontDestroySingleton { get { return true; }	}

    protected override void InitAfterRegisteringAsSingleInstance() {
        base.InitAfterRegisteringAsSingleInstance();
    }

    public static void Initialize(string appKey) {
        Initialize(appKey, new ISAdQualityConfig());
    }
    
    public static void Initialize(string appKey, ISAdQualityConfig adQualityConfig) {
        if (adQualityConfig == null) {
            adQualityConfig = new ISAdQualityConfig();
        }
        Initialize(appKey, 
            adQualityConfig.UserId,
            adQualityConfig.UserIdSet,
            adQualityConfig.TestMode, 
            adQualityConfig.LogLevel,
            adQualityConfig.InitializationSource,
            adQualityConfig.Coppa,
            adQualityConfig.DeviceIdType,
            adQualityConfig.AdQualityInitCallback);
    }
    
    private static void Initialize(string appKey, 
                                    string userId, 
                                    bool userIdSet, 
                                    bool testMode, 
                                    ISAdQualityLogLevel logLevel, 
                                    string initializationSource,
                                    bool coppa,
                                    ISAdQualityDeviceIdType deviceIdType,
                                    ISAdQualityInitCallback adQualityInitCallback) {

        #if !UNITY_EDITOR
            GetSynchronousCodeGeneratedInstance<IronSourceAdQuality>();
            ISAdQualityInitCallbackWrapper initCallbackWrapper = adQualityGameObject.GetComponent<ISAdQualityInitCallbackWrapper>();
            if (initCallbackWrapper == null) {
                initCallbackWrapper = adQualityGameObject.AddComponent<ISAdQualityInitCallbackWrapper>();
            }
            initCallbackWrapper.AdQualityInitCallback = adQualityInitCallback;
            bool isInitCallbackSet = (adQualityInitCallback != null);

            #if UNITY_ANDROID
                AndroidJNI.PushLocalFrame(100);

                AndroidJNI.PopLocalFrame(IntPtr.Zero);
            #endif

            #if UNITY_ANDROID
                AndroidJNI.PushLocalFrame(100);
                using(AndroidJavaClass jniAdQualityClass = new AndroidJavaClass("com.ironsource.adqualitysdk.sdk.unity.IronSourceAdQuality")) {
                    AndroidJavaClass jLogLevelEnum = new AndroidJavaClass("com.ironsource.adqualitysdk.sdk.ISAdQualityLogLevel");
                    AndroidJavaObject jLogLevel = jLogLevelEnum.CallStatic<AndroidJavaObject>("fromInt", (int)logLevel);
                    AndroidJavaClass jDeviceIdTypeEnum = new AndroidJavaClass("com.ironsource.adqualitysdk.sdk.ISAdQualityDeviceIdType");
                    AndroidJavaObject jDeviceIdType = jDeviceIdTypeEnum.CallStatic<AndroidJavaObject>("fromInt", (int)deviceIdType);
                    jniAdQualityClass.CallStatic("initialize", appKey, userId, userIdSet, testMode, jLogLevel, initializationSource, coppa, jDeviceIdType, isInitCallbackSet);
                }
                AndroidJNI.PopLocalFrame(IntPtr.Zero);
            #elif UNITY_IOS
                ironSourceAdQuality_initialize(appKey, userId, userIdSet ,testMode, DEBUG, (int)logLevel, initializationSource, coppa, (int)deviceIdType, isInitCallbackSet);
            #endif
        #else
            ISAdQualityUtils.LogWarning(TAG, "Ad Quality SDK works only on Android or iOS devices.");
        #endif
    }

    public static void ChangeUserId(String userId) {
        #if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJNI.PushLocalFrame(100);
            using(AndroidJavaClass jniAdQualityClass = new AndroidJavaClass("com.ironsource.adqualitysdk.sdk.unity.IronSourceAdQuality")) {
                jniAdQualityClass.CallStatic("changeUserId", userId);
            }
            AndroidJNI.PopLocalFrame(IntPtr.Zero);
        #elif UNITY_IOS && !UNITY_EDITOR
            ironSourceAdQuality_changeUserId(userId);
        #endif
    }

    [Obsolete("This method has been deprecated and will be removed soon")]
    public static void SetUserConsent(bool userConsent) {
        #if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJNI.PushLocalFrame(100);
            using(AndroidJavaClass jniAdQualityClass = new AndroidJavaClass("com.ironsource.adqualitysdk.sdk.unity.IronSourceAdQuality")) {
                jniAdQualityClass.CallStatic("setUserConsent", userConsent);
            }
            AndroidJNI.PopLocalFrame(IntPtr.Zero);
        #elif UNITY_IOS && !UNITY_EDITOR
            ironSourceAdQuality_setUserConsent(userConsent);
        #endif
    }

    public static void SendCustomMediationRevenue(ISAdQualityCustomMediationRevenue customMediationRevenue) {
        #if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJNI.PushLocalFrame(100);
            using(AndroidJavaClass jniAdQualityClass = new AndroidJavaClass("com.ironsource.adqualitysdk.sdk.unity.IronSourceAdQuality")) {
                jniAdQualityClass.CallStatic("sendCustomMediationRevenue", 
                                            (int) customMediationRevenue.MediationNetwork,
                                            (int) customMediationRevenue.AdType,
                                            customMediationRevenue.Placement,
                                            customMediationRevenue.Revenue);
            }
            AndroidJNI.PopLocalFrame(IntPtr.Zero);
        #elif UNITY_IOS && !UNITY_EDITOR
            ironSourceAdQuality_sendCustomMediationRevenue((int) customMediationRevenue.MediationNetwork,
                                                            (int) customMediationRevenue.AdType,
                                                            customMediationRevenue.Placement,
                                                            customMediationRevenue.Revenue);
        #endif
    }

    public static void setSegment(ISAdQualitySegment segment) {
        Dictionary <string,string> dict = segment.getSegmentAsDict();
        string jsonString = ISAdQualityJSON.Json.Serialize(dict);
        #if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJNI.PushLocalFrame(100);
            using(AndroidJavaClass jniAdQualityClass = new AndroidJavaClass("com.ironsource.adqualitysdk.sdk.unity.IronSourceAdQuality")) {
                jniAdQualityClass.CallStatic("setSegment", jsonString);
            }
            AndroidJNI.PopLocalFrame(IntPtr.Zero);
        #elif UNITY_IOS && !UNITY_EDITOR
            ironSourceAdQuality_setSegment(jsonString);
        #endif
    }

    private const string TAG = "IronSource AdQuality";
    private const bool DEBUG = false;

}
