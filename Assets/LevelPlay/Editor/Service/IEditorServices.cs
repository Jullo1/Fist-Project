using System;

namespace Unity.Services.LevelPlay.Editor
{
    internal interface IEditorServices
    {
        IFileService FileService { get; }
        IXmlDocumentFactory XmlDocumentFactory { get; }
        IWebRequestService WebRequestService { get; }
        ILevelPlayNetworkManager LevelPlayNetworkManager { get; }
        IEditorAnalyticsSender EditorAnalyticsSender { get; }
        IEditorAnalyticsService EditorAnalyticsService { get; }
        ILevelPlayLogger LevelPlayLogger { get; }
    }
}
