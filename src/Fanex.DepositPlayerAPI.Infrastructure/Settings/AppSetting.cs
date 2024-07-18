namespace Fanex.DepositPlayerAPI.Infrastructure.Settings;

public class AppSetting
{
    public const string SettingName = "AppSetting";

    public bool UseErrorHandler { get; set; }

    public bool UseSwagger { get; set; }

    public string Version { get; set; }

    public string Environment { get; set; }
}