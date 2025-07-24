using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Unity.Services.LevelPlay
{
    class AndroidInterstitialAd : IPlatformInterstitialAd, IUnityInterstitialAdListener
    {
        const string k_AndroidInterstitialClass = "com.ironsource.unity.androidbridge.InterstitialAd";
        const string k_AndroidLoadAdFunction = "loadAd";
        const string k_AndroidShowAdFunction = "showAd";
        const string k_IsAdReadyFunction = "isAdReady";
        const string k_IsPlacementCappedStaticFunction = "isPlacementCapped";
        const string k_FuncGetAdId       = "getAdId";

        const string k_ErrorDisposed = "Instance is disposed. Please create a new instance in order to call any method.";

#pragma warning disable 0618
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdLoaded;
        public event Action<com.unity3d.mediation.LevelPlayAdError> OnAdLoadFailed;
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdDisplayed;
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdClosed;
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdClicked;
        public event Action<com.unity3d.mediation.LevelPlayAdDisplayInfoError> OnAdDisplayFailed;
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdInfoChanged;
#pragma warning restore 0618

        AndroidJavaObject m_InterstitialJavaObject;
        IUnityInterstitialAdListener m_InterstitialListener;

        volatile bool m_Disposed;
        volatile bool m_IsReady;


        public string AdUnitId { get; }

        public string AdId => m_InterstitialJavaObject.Call<string>(k_FuncGetAdId);

        internal AndroidInterstitialAd(string adUnitId)
        {
            AdUnitId = adUnitId;
            ThreadUtil.Send(state =>
            {
                try
                {
                    if (m_InterstitialListener == null)
                    {
                        m_InterstitialListener = new UnityInterstitialAdListener(this);
                    }
                    m_InterstitialJavaObject =
                        new AndroidJavaObject(k_AndroidInterstitialClass, adUnitId, m_InterstitialListener);
                }
                catch (Exception e)
                {
                    LevelPlayLogger.LogException(e);
                }
            });
        }

        internal AndroidInterstitialAd(string adUnitId, Config config)
        {
            AdUnitId = adUnitId;
            ThreadUtil.Send(state =>
            {
                try
                {
                    if (m_InterstitialListener == null)
                    {
                        m_InterstitialListener = new UnityInterstitialAdListener(this);
                    }

                    m_InterstitialJavaObject =
                        new AndroidJavaObject(k_AndroidInterstitialClass, adUnitId, config.ConfigJavaObject, m_InterstitialListener);
                }
                catch (Exception e)
                {
                    LevelPlayLogger.LogException(e);
                }
            });
        }

        public void LoadAd()
        {
            if (!CheckDisposedAndLogError())
            {
                ThreadUtil.Post(state =>
                {
                    try
                    {
                        m_InterstitialJavaObject.Call(k_AndroidLoadAdFunction);
                    }
                    catch (Exception e)
                    {
                        LevelPlayLogger.LogException(e);
                    }
                });
            }
        }

        public void ShowAd(string placementName)
        {
            if (!CheckDisposedAndLogError())
            {
                ThreadUtil.Post(state =>
                {
                    try
                    {
                        m_InterstitialJavaObject.Call(k_AndroidShowAdFunction, placementName);
                    }
                    catch (Exception e)
                    {
                        LevelPlayLogger.LogException(e);
                    }
                });
            }
        }

        public bool IsAdReady()
        {
            if (!CheckDisposedAndLogError())
            {
                ThreadUtil.Send(state =>
                {
                    try
                    {
                        m_IsReady = m_InterstitialJavaObject.Call<bool>(k_IsAdReadyFunction);
                    }
                    catch (Exception e)
                    {
                        LevelPlayLogger.LogException(e);
                    }
                });
            }
            return m_IsReady;
        }

        public static bool IsPlacementCapped(string placementName)
        {
            var isPlacementCapped = false;
            try
            {
                using (var interstitialJavaClass = new AndroidJavaClass(k_AndroidInterstitialClass))
                {
                    isPlacementCapped = interstitialJavaClass.CallStatic<bool>(k_IsPlacementCappedStaticFunction, placementName);
                }
            }
            catch (Exception e)
            {
                LevelPlayLogger.LogException(e);
            }
            return isPlacementCapped;
        }

#pragma warning disable 0618
        public void onAdLoaded(string adInfo)
        {
            OnAdLoaded?.Invoke(new com.unity3d.mediation.LevelPlayAdInfo(adInfo));
        }

        public void onAdLoadFailed(string error)
        {
            OnAdLoadFailed?.Invoke(new com.unity3d.mediation.LevelPlayAdError(error));
        }

        public void onAdDisplayed(string adInfo)
        {
            OnAdDisplayed?.Invoke(new com.unity3d.mediation.LevelPlayAdInfo(adInfo));
        }

        public void onAdClosed(string adInfo)
        {
            OnAdClosed?.Invoke(new com.unity3d.mediation.LevelPlayAdInfo(adInfo));
        }

        public void onAdDisplayFailed(string error, string adInfo)
        {
            OnAdDisplayFailed?.Invoke(new com.unity3d.mediation.LevelPlayAdDisplayInfoError(new LevelPlayAdInfo(adInfo), new LevelPlayAdError(error)));
        }

        public void onAdClicked(string adInfo)
        {
            OnAdClicked?.Invoke(new com.unity3d.mediation.LevelPlayAdInfo(adInfo));
        }

        public void onAdInfoChanged(string adInfo)
        {
            OnAdInfoChanged?.Invoke(new com.unity3d.mediation.LevelPlayAdInfo(adInfo));
        }

#pragma warning restore 0618

        void Dispose(bool disposing)
        {
            if (m_Disposed) return;
            m_Disposed = true;
            if (disposing)
            {
                ThreadUtil.Post(state =>
                {
                    m_InterstitialJavaObject?.Dispose();
                    m_IsReady = false;
                    m_InterstitialListener = null;
                    m_InterstitialJavaObject = null;
                });
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AndroidInterstitialAd()
        {
            Dispose(false);
        }

        bool CheckDisposedAndLogError()
        {
            if (m_Disposed)
            {
                LevelPlayLogger.LogError(k_ErrorDisposed);
            }
            return m_Disposed;
        }

        internal class Config : IPlatformInterstitialAd.IConfig
        {
            internal AndroidJavaObject ConfigJavaObject { get; }

            private Config(AndroidJavaObject config)
            {
                ConfigJavaObject = config;
            }

            internal class Builder : IPlatformInterstitialAd.IConfigBuilder
            {
                private const string KBuilderClass = "com.ironsource.unity.androidbridge.InterstitialAd$ConfigBuilder";
                private readonly AndroidJavaObject m_BuilderJavaObject;

                internal Builder()
                {
                    m_BuilderJavaObject = new AndroidJavaObject(KBuilderClass);
                }

                public void SetBidFloor(double bidFloor)
                {
                    m_BuilderJavaObject.Call("setBidFloor", bidFloor);
                }

                public IPlatformInterstitialAd.IConfig Build()
                {
                    var androidConfig = m_BuilderJavaObject.Call<AndroidJavaObject>("build");
                    return new Config(androidConfig);
                }
            }
        }
    }
}
