using System;
using PriSecDBAPI_SC_SDK.Helper;

namespace PriSecDBAPI_SC_SDK
{
    public static class ApplicationPathInitializer
    {
        public static void InitializeApplicationPath(String Path,Boolean IsWindows=false)
        {
            if (Path != null && Path.CompareTo("") != 0 )
            {
                ApplicationPath.Path = Path;
                ApplicationPath.IsWindows = IsWindows;
            }
            else
            {
                throw new ArgumentException("Error: Path for Application must not be null/empty");
            }
        }

        public static String ShowApplicationPath()
        {
            return ApplicationPath.Path;
        }

        public static Boolean ShowIsWindows()
        {
            return ApplicationPath.IsWindows;
        }
    }
}
