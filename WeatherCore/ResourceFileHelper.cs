using System;
using System.IO;

namespace WeatherCore
{
    public static class ResourcePathHelper
    {
        public static string GetPath(string relativePath)
        {
            // Получает путь до исполняемого файла
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // Чистим путь от лишних / и \, и нормализуем
            string fullPath = Path.Combine(baseDir, relativePath.Replace("/", "\\"));

            return Path.GetFullPath(fullPath); // на всякий случай делаем абсолютным
        }
    }
}
