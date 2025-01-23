using System;

namespace Unity.Services.LevelPlay.Editor
{
    internal class EditorServices : IEditorServices
    {
        private static readonly EditorServices k_Instance = new EditorServices();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static EditorServices()
        {
        }

        internal EditorServices()
        {
        }

        internal static EditorServices Instance
        {
            get
            {
                return k_Instance;
            }
        }

        private readonly object m_PropertyLock = new object();

        private IFileService m_FileService;
        public IFileService FileService
        {
            get
            {
                lock (m_PropertyLock) {
                    if (m_FileService == null)
                    {
                        m_FileService = new FileService();
                    }
                    return m_FileService;
                }
            }
        }

        private IXmlDocumentFactory m_XmlDocumentFactory;
        public IXmlDocumentFactory XmlDocumentFactory
        {
            get
            {
                lock (m_PropertyLock) {
                    if (m_XmlDocumentFactory == null)
                    {
                        m_XmlDocumentFactory = new XmlDocumentFactory();
                    }
                    return m_XmlDocumentFactory;
                }
            }
        }

        private IWebRequestService m_WebRequestService;
        public IWebRequestService WebRequestService
        {
            get
            {
                lock (m_PropertyLock) {
                    if (m_WebRequestService == null)
                    {
                        m_WebRequestService = new WebRequestService();
                    }
                    return m_WebRequestService;
                }
            }
        }

        private ILevelPlayNetworkManager m_LevelPlayNetworkManager;
        public ILevelPlayNetworkManager LevelPlayNetworkManager
        {
            get
            {
                var fileService = this.FileService;
                var xmlDocumentFactory = this.XmlDocumentFactory;
                var webRequestService = this.WebRequestService;
                var levelPlayLogger = this.LevelPlayLogger;
                lock (m_PropertyLock) {
                    if (m_LevelPlayNetworkManager == null)
                    {
                        m_LevelPlayNetworkManager = new LevelPlayNetworkManager(fileService, xmlDocumentFactory, webRequestService, levelPlayLogger);
                    }
                    return m_LevelPlayNetworkManager;
                }
            }
        }

        private IEditorAnalyticsSender m_EditorAnalyticsSender;
        public IEditorAnalyticsSender EditorAnalyticsSender
        {
            get
            {
                lock (m_PropertyLock) {
                    if (m_EditorAnalyticsSender == null)
                    {
                        m_EditorAnalyticsSender = new EditorAnalyticsSender();
                    }
                    return m_EditorAnalyticsSender;
                }
            }
        }

        private IEditorAnalyticsService m_EditorAnalyticsService;
        public IEditorAnalyticsService EditorAnalyticsService
        {
            get
            {
                var editorAnalyticsSender = this.EditorAnalyticsSender;
                lock (m_PropertyLock) {
                    if (m_EditorAnalyticsService == null)
                    {
                        m_EditorAnalyticsService = new EditorAnalyticsService(editorAnalyticsSender);
                    }
                    return m_EditorAnalyticsService;
                }
            }
        }

        private LevelPlayLogger m_LevelPlayLogger;
        public ILevelPlayLogger LevelPlayLogger
        {
            get
            {
                lock (m_PropertyLock) {
                    if (m_LevelPlayLogger == null)
                    {
                        m_LevelPlayLogger = new LevelPlayLogger();
                    }
                    return m_LevelPlayLogger;
                }
            }
        }
    }
}
