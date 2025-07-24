#if UNITY_IOS && !UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using com.unity3d.mediation;

#pragma warning disable 0618
namespace com.unity3d.mediation
{
    [Obsolete("This class will be made private in version 9.0.0.")]
    public class iOSBannerAd : Unity.Services.LevelPlay.iOSBannerAd
    {
        public iOSBannerAd(string adUnitId, com.unity3d.mediation.LevelPlayAdSize size, com.unity3d.mediation.LevelPlayBannerPosition bannerPosition, string placementName, bool displayOnLoad) : base(adUnitId, size, bannerPosition, placementName, displayOnLoad) {}
    }
}

namespace Unity.Services.LevelPlay
{
    [Obsolete("This class will be made private in version 9.0.0.")]
    public class iOSBannerAd : IosNativeObject, IPlatformBannerAd
    {
        public event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdLoaded;
        public event EventHandler<com.unity3d.mediation.LevelPlayAdError> OnAdLoadFailed;
        public event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdClicked;
        public event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdDisplayed;
        public event EventHandler<com.unity3d.mediation.LevelPlayAdDisplayInfoError> OnAdDisplayFailed;
        public event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdExpanded;
        public event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdCollapsed;
        public event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdLeftApplication;

        public string AdUnitId { get; }
        public com.unity3d.mediation.LevelPlayAdSize AdSize { get; }
        public string PlacementName { get; }
        public com.unity3d.mediation.LevelPlayBannerPosition Position { get; }
        private bool DisplayOnLoad { get; }

        public string AdId => GetAdId();

        IosBannerAdListener _mBannerAdListener;

        public iOSBannerAd(string adUnitId, com.unity3d.mediation.LevelPlayAdSize size,  com.unity3d.mediation.LevelPlayBannerPosition bannerPosition, string placementName,  bool displayOnLoad) : base(true)
        {
            AdUnitId = adUnitId;
            AdSize = size;
            Position = bannerPosition;
            PlacementName = placementName;
            DisplayOnLoad = displayOnLoad;

            IosLevelPlayAdSize iosAdSize = (IosLevelPlayAdSize)AdSize.GetPlatformLevelPlayAdSize();
            NativePtr = BannerAdCreate(adUnitId, placementName, iosAdSize.NativePtr);
            if (_mBannerAdListener == null)
            {
                _mBannerAdListener = new IosBannerAdListener(this);
            }

            BannerAdSetDelegate(NativePtr, _mBannerAdListener.NativePtr);
        }

        internal iOSBannerAd(string adUnitId, Config config) : base(true)
        {
            AdUnitId = adUnitId;
            AdSize = config.AdSize;
            Position = config.Position;
            PlacementName = config.PlacementName;
            DisplayOnLoad = config.DisplayOnLoad;

            IosLevelPlayAdSize iosAdSize = (IosLevelPlayAdSize)AdSize.GetPlatformLevelPlayAdSize();
            NativePtr = BannerAdCreate(adUnitId, config.IosConfig, iosAdSize.NativePtr);
            if (_mBannerAdListener == null)
            {
                _mBannerAdListener = new IosBannerAdListener(this);
            }

            BannerAdSetDelegate(NativePtr, _mBannerAdListener.NativePtr);
        }

        public void PauseAutoRefresh()
        {
            if (CheckDisposedAndLogError("Cannot pause auto-refresh")) return;
            BannerAdPauseAutoRefresh(NativePtr);
        }

        public void ResumeAutoRefresh()
        {
            if (CheckDisposedAndLogError("Cannot resume auto-refresh")) return;
            BannerAdResumeAutoRefresh(NativePtr);
        }

        public void Load()
        {
            if (CheckDisposedAndLogError("Cannot call Load()")) return;
            BannerAdLoad(NativePtr);
            SetPosition();
            if (DisplayOnLoad)
            {
                ShowAd();
            }
            else
            {
                HideAd();
            }
        }

        public void DestroyAd()
        {
            if (NativePtr != IntPtr.Zero)
            {
                BannerAdDestroy(NativePtr);
                NativePtr = IntPtr.Zero;
            }

            base.Dispose();
        }

        public void SetPosition()
        {
            if (CheckDisposedAndLogError("Cannot set Banner Position")) return;
            BannerAdSetPosition(NativePtr, Position.Description, Position.Position.x, Position.Position.y);
        }

        public void ShowAd()
        {
            BannerAdViewShow(NativePtr);
        }

        public void HideAd()
        {
            BannerAdViewHide(NativePtr);
        }

        //Invoke events defined in iOSBannerAdListener.cs
        internal void InvokeLoadedEvent(com.unity3d.mediation.LevelPlayAdInfo adInfo)
        {
            ThreadUtil.Post(state => OnAdLoaded?.Invoke(this, adInfo));
        }

        internal void InvokeFailedLoadEvent(com.unity3d.mediation.LevelPlayAdError error)
        {
            ThreadUtil.Post(state => OnAdLoadFailed?.Invoke(this, error));
        }

        internal void InvokeClickedEvent(com.unity3d.mediation.LevelPlayAdInfo adInfo)
        {
            ThreadUtil.Post(state => OnAdClicked?.Invoke(this, adInfo));
        }

        internal void InvokeDisplayedEvent(com.unity3d.mediation.LevelPlayAdInfo adInfo)
        {
            ThreadUtil.Post(state => OnAdDisplayed?.Invoke(this, adInfo));
        }

        internal void InvokeFailedDisplayEvent(com.unity3d.mediation.LevelPlayAdInfo adInfo, LevelPlayAdError error)
        {
            com.unity3d.mediation.LevelPlayAdDisplayInfoError errorInfo =
                new com.unity3d.mediation.LevelPlayAdDisplayInfoError(adInfo, error);
            ThreadUtil.Post(state => OnAdDisplayFailed?.Invoke(this, errorInfo));
        }

        internal void InvokeExpandedEvent(com.unity3d.mediation.LevelPlayAdInfo adInfo)
        {
            ThreadUtil.Post(state => OnAdExpanded?.Invoke(this, adInfo));
        }

        internal void InvokeCollapsedEvent(com.unity3d.mediation.LevelPlayAdInfo adInfo)
        {
            ThreadUtil.Post(state => OnAdCollapsed?.Invoke(this, adInfo));
        }

        internal void InvokeLeftApplicationEvent(com.unity3d.mediation.LevelPlayAdInfo adInfo)
        {
            ThreadUtil.Post(state => OnAdLeftApplication?.Invoke(this, adInfo));
        }

        private string GetAdId()
        {
            if (CheckDisposedAndLogError("Cannot get Banner ad Id")) return "";
            return BannerAdId(NativePtr);
        }

        [DllImport("__Internal", EntryPoint = "LPMBannerAdViewCreate")]
        static extern IntPtr BannerAdCreate(string adUnitId, string placementName, IntPtr adSize);

        [DllImport("__Internal", EntryPoint = "LPMBannerAdViewCreateWithConfig")]
        static extern IntPtr BannerAdCreate(string adUnitId, IntPtr config, IntPtr adSize);

        [DllImport("__Internal", EntryPoint = "LPMBannerAdViewSetDelegate")]
        static extern void BannerAdSetDelegate(IntPtr bannerAdView, IntPtr bannerAdListener);

        [DllImport("__Internal", EntryPoint = "LPMBannerAdViewLoadAd")]
        static extern void BannerAdLoad(IntPtr bannerAdView);

        [DllImport("__Internal", EntryPoint = "LPMBannerAdViewDestroy")]
        static extern void BannerAdDestroy(IntPtr bannerAdView);

        [DllImport("__Internal", EntryPoint = "LPMBannerAdViewSetPosition")]
        private static extern void BannerAdSetPosition(IntPtr bannerAdView, string position, float x, float y);

        [DllImport("__Internal", EntryPoint = "LPMBannerAdViewShow")]
        private static extern void BannerAdViewShow(IntPtr bannerAdView);

        [DllImport("__Internal", EntryPoint = "LPMBannerAdViewHide")]
        private static extern void BannerAdViewHide(IntPtr bannerAdView);

        [DllImport("__Internal", EntryPoint = "LPMBannerAdViewPauseAutoRefresh")]
        static extern void BannerAdPauseAutoRefresh(IntPtr bannerAdView);

        [DllImport("__Internal", EntryPoint = "LPMBannerAdViewResumeAutoRefresh")]
        static extern void BannerAdResumeAutoRefresh(IntPtr bannerAdView);

        [DllImport("__Internal", EntryPoint = "LPMBannerAdViewAdId")]
        static extern string BannerAdId(IntPtr bannerAdView);

        internal class Config : IPlatformBannerAd.IConfig
        {
            internal com.unity3d.mediation.LevelPlayAdSize AdSize { get; }
            internal com.unity3d.mediation.LevelPlayBannerPosition Position { get; }
            internal string PlacementName { get; }
            internal bool DisplayOnLoad { get; }
            internal IntPtr IosConfig { get; }

            private Config(
                com.unity3d.mediation.LevelPlayAdSize adSize,
                com.unity3d.mediation.LevelPlayBannerPosition position,
                string placementName,
                bool displayOnLoad,
                IntPtr iosConfig)
            {
                AdSize = adSize;
                Position = position;
                PlacementName = placementName;
                DisplayOnLoad = displayOnLoad;
                IosConfig = iosConfig;
            }

            internal class Builder : IPlatformBannerAd.IConfigBuilder
            {
                private com.unity3d.mediation.LevelPlayAdSize _adSize;
                private com.unity3d.mediation.LevelPlayBannerPosition _position;
                private string _placementName;
                private bool _displayOnLoad;
                private readonly IntPtr m_Builder = BannerAdCreateConfigBuilder();

                public void SetBidFloor(double bidFloor)
                {
                    BannerAdConfigBuilderSetBidFloor(m_Builder, bidFloor);
                }

                public void SetSize(com.unity3d.mediation.LevelPlayAdSize size)
                {
                    _adSize = size;
                    IosLevelPlayAdSize iosAdSize = (IosLevelPlayAdSize)size.GetPlatformLevelPlayAdSize();
                    BannerAdConfigBuilderSetSize(m_Builder, iosAdSize.NativePtr);
                }

                public void SetPosition(com.unity3d.mediation.LevelPlayBannerPosition position)
                {
                    _position = position;
                }

                public void SetPlacementName(string placementName)
                {
                    _placementName = placementName;
                    BannerAdConfigBuilderSetPlacementName(m_Builder, placementName);
                }

                public void SetDisplayOnLoad(bool displayOnLoad)
                {
                    _displayOnLoad = displayOnLoad;
                }

                public void SetRespectSafeArea(bool respectSafeArea)
                {
                    // unused
                }

                public IPlatformBannerAd.IConfig Build()
                {
                    var iosConfig = BannerAdConfigBuilderBuild(m_Builder);
                    return new Config(_adSize, _position, _placementName, _displayOnLoad, iosConfig);
                }

                [DllImport("__Internal", EntryPoint = "LPMBannerAdAdCreateConfigBuilder")]
                static extern IntPtr BannerAdCreateConfigBuilder();

                [DllImport("__Internal", EntryPoint = "LPMBannerAdAdConfigBuilderSetBidFloor")]
                static extern IntPtr BannerAdConfigBuilderSetBidFloor(IntPtr builder, double bidFloor);

                [DllImport("__Internal", EntryPoint = "LPMBannerAdConfigBuilderSetSize")]
                static extern IntPtr BannerAdConfigBuilderSetSize(IntPtr builder, IntPtr size);

                [DllImport("__Internal", EntryPoint = "LPMBannerAdConfigBuilderSetPlacementName")]
                static extern IntPtr BannerAdConfigBuilderSetPlacementName(IntPtr builder, string placementName);

                [DllImport("__Internal", EntryPoint = "LPMBannerAdAdConfigBuilderBuild")]
                static extern IntPtr BannerAdConfigBuilderBuild(IntPtr builder);
            }
        }
    }
}
#pragma warning restore 0618
#endif
