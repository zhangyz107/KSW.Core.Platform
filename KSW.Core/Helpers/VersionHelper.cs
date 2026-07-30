using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSW.Helpers
{
    public static class VersionHelper
    {
        // 基础格式化
        public static string ToFormattedString(this Version version)
        {
            if (version == null)
                return null;

            if (version.Revision > 0)
                return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            else
                return $"{version.Major}.{version.Minor}.{version.Build}";
        }

        // 带自定义格式
        public static string ToFormattedString(this Version version, string format)
        {
            if (version == null)
                return null;

            if (format == null)
                return null;

            return format
                .Replace("{M}", version.Major.ToString())
                .Replace("{m}", version.Minor.ToString())
                .Replace("{b}", version.Build.ToString())
                .Replace("{r}", version.Revision.ToString());
        }
    }
}
