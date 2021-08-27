using System;
using System.IO;
using System.Reflection;

namespace ModuleCore.Tools
{
    internal class FileTools
    {
        private static bool CanCreateFileHere()
        {
            string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            string file = string.Format("{0}.txt", Guid.NewGuid());
            string filepath = Path.Combine(path, file);
            bool result;
            try
            {
                File.WriteAllText(filepath, "dummy");
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            finally
            {
                try
                {
                    if (File.Exists(filepath))
                    {
                        File.Delete(filepath);
                    }
                }
                catch (Exception)
                {
                }
            }
            return result;
        }
    }
}