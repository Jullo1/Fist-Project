#if UNITY_IPHONE || UNITY_IOS
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ISAdQualityiOSInitHandler : MonoBehaviour
{
    public static event Action OnAdQualitySdkInitSuccess = delegate { };
    public static event Action<ISAdQualityInitError, string> OnAdQualitySdkInitFailed = delegate { };


#if UNITY_IOS && !UNITY_EDITOR
    delegate void ISAdQualityUnityInitSuccessCallback(string args);
    [DllImport("__Internal")]
    private static extern int ironSourceAdQuality_registerInitSuccessCallback(ISAdQualityUnityInitSuccessCallback func);

    delegate void ISAdQualityUnityInitFailedCallback(string args);
    [DllImport("__Internal")]
    private static extern void ironSourceAdQuality_registerInitFailedCallback(ISAdQualityUnityInitFailedCallback func);

    public ISAdQualityiOSInitHandler()
    {
        ironSourceAdQuality_registerInitSuccessCallback(fireInitSuccessCallback);
        ironSourceAdQuality_registerInitFailedCallback(fireInitFailedCallback);
    }

    [AOT.MonoPInvokeCallback(typeof(ISAdQualityUnityInitSuccessCallback))]
    public static void fireInitSuccessCallback(string message)
    {
        if (OnAdQualitySdkInitSuccess != null)
        {
            OnAdQualitySdkInitSuccess();
        }
    }

    [AOT.MonoPInvokeCallback(typeof(ISAdQualityUnityInitFailedCallback))]
    public static void fireInitFailedCallback(string message)
    {
        if (OnAdQualitySdkInitFailed != null)
        {
            ISAdQualityInitError sdkInitError = ISAdQualityInitError.EXCEPTION_ON_INIT;
            string errorMsg = String.Empty;
            try
            {
                if (!String.IsNullOrEmpty(message)) 
                {
                    string[] separators = { "Unity:" };
                    string[] splitArray = message.Split(separators, System.StringSplitOptions.RemoveEmptyEntries);
                    if (splitArray.Length > 1) 
                    {
                        sdkInitError = (ISAdQualityInitError)Enum.Parse(typeof(ISAdQualityInitError), splitArray[0]);
                        errorMsg = splitArray[1];
                    }
                }
            }
            catch (Exception e) 
            {
                errorMsg = e.Message;
            }
            OnAdQualitySdkInitFailed(sdkInitError, errorMsg);
        }
    }
    #endif
}

#endif