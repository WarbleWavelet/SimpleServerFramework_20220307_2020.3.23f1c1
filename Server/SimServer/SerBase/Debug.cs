using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;




/// <summary>
/// Debug（工具NuGet包管理器/管理解决方案的NuGet程序/搜索log4Net）<para />
/// 它不会检测到已安装的log4Net，安装选项列表也没有208<para />
/// 所以给SerBase升级到最先的610<para />
/// 以上发生过，后来又正常了，所以最重是208<para />
/// 视频是到SerBaser.csproj中找，编辑器没有，需要到资源管理器看，用其他文本编辑器打开<para />
/// 配置看log4net.config<para />
/// </summary>
public static class Debug
{
    private static ILog m_Log;


    #region 构造

    #endregion
    static Debug()
    {
        XmlConfigurator.ConfigureAndWatch(new FileInfo(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile));
        m_Log = LogManager.GetLogger(typeof(Debug));
    }


    #region 辅助1，ILog
    public static void Log(object message)
    {
        m_Log.Debug(message);
    }

    public static void Log(string format, params object[] args)
    {
        m_Log.DebugFormat(format, args);
    }

    public static void LogInfo(object message)
    {
        m_Log.Info(message);
    }

    public static void LogInfo(string format, params object[] args)
    {
        m_Log.InfoFormat(format, args);
    }

    public static void LogWarn(object message)
    {
        m_Log.Warn(message);
    }

    public static void LogWarn(string format, params object[] args)
    {
        m_Log.WarnFormat(format, args);
    }

    public static void LogError(object message)
    {
        m_Log.Error(message);
    }

    public static void LogError(string format, params object[] args)
    {
        m_Log.ErrorFormat(format, args);
    }

    public static void LogFatle(object message)
    {
        m_Log.Fatal(message);
    }

    public static void LogFatle(string format, params object[] args)
    {
        m_Log.FatalFormat(format, args);
    }
    #endregion

}
