using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unity.Services.LevelPlay.Editor
{
    internal class FileService : IFileService
    {
        private readonly string m_LevelPlayPackagePath;
        internal FileService()
        {
            m_LevelPlayPackagePath = GetLevelPlayPackagePath();
        }

        private string GetLevelPlayPackagePath()
        {
            var upmPath = FilePaths.UpmPackageDirectoryPath;
            var unityPackagePath = FilePaths.UnityPackageDirectoryPath;

            if (Directory.Exists(upmPath))
            {
                return upmPath;
            }

            if (Directory.Exists(unityPackagePath))
            {
                return unityPackagePath;
            }

            return String.Empty;
        }

        public void Delete(string path)
        {
            File.Delete(path);
        }

        #nullable enable
        public bool Exists(string? path)
        {
            return File.Exists(path);
        }

        #nullable disable

        public void WriteAllBytes(string path, byte[] bytes)
        {
            File.WriteAllBytes(path, bytes);
        }

        public void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            File.Copy(sourceFileName, destFileName, overwrite);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        #nullable enable
        public void WriteAllText(string path, string? contents)
        {
            File.WriteAllText(path, contents);
        }

        #nullable disable

        public void ImportPackage(string packagePath, bool interactive)
        {
            AssetDatabase.ImportPackage(packagePath, interactive);
        }

        public string GetNewTempFilePath()
        {
            return FileUtil.GetUniqueTempPathInProject();
        }

        public string GetPathRelativeToLevelPlayPackage(string path)
        {
            return Path.Combine(m_LevelPlayPackagePath, path);
        }

        public void CreateDirectoryForFilePath(string path)
        {
            var fileInfo = new FileInfo(path);
            fileInfo.Directory.Create(); // Create the directory if it doesn't exist
        }
    }
}
