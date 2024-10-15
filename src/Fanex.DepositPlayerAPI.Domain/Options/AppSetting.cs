namespace Fanex.DepositPlayerAPI.Domain.Options;

public class AppSetting
{
    public const string SettingName = "AppSetting";

    public bool UseErrorHandler { get; set; }

    public bool UseSwagger { get; set; }

    public string Version { get; set; }

    public string Environment { get; set; }

    public bool DebugMode { get; set; }
}