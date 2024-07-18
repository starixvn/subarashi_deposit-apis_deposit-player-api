namespace Fanex.DepositPlayerAPI.Application.Extensions;

using Microsoft.AspNetCore.Http;

public static class HttpContextExtension
{
    private const string SiteKey = "Site";
    private const string SiteDbServerKey = "SiteDbServerKey";
    private const string SiteDbGroupIdKey = "SiteDbGroupIdKey";
    private const string CustDbServerKey = "CustDbServerKey";
    private const string CustDbGroupIdKey = "CustDbGroupIdKey";

    private const string HttpContextNullExceptionMsg = "HttpContext should not be null!";

    public static string GetSite(this HttpContext context)
        => GetValueByKey(SiteKey, context);

    public static string GetSiteDbServer(this HttpContext context)
        => GetValueByKey(SiteDbServerKey, context);

    public static string GetCustDbServer(this HttpContext context)
        => GetValueByKey(CustDbServerKey, context);

    public static int GetSiteDbGroupId(this HttpContext context)
        => int.TryParse(GetValueByKey(SiteDbGroupIdKey, context), out var result) ? result : default;

    public static int GetCustDbGroupId(this HttpContext context)
        => int.TryParse(GetValueByKey(CustDbGroupIdKey, context), out var result) ? result : default;

    public static void SetSite(this HttpContext context, string siteName)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context), HttpContextNullExceptionMsg);
        }

        context.Items[SiteKey] = siteName;
    }

    public static void SetSiteDbServer(this HttpContext context, string server)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context), HttpContextNullExceptionMsg);
        }

        context.Items[SiteDbServerKey] = server;
    }

    public static void SetCustDbServer(this HttpContext context, string server)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context), HttpContextNullExceptionMsg);
        }

        context.Items[CustDbServerKey] = server;
    }

    public static void SetSiteDbGroupId(this HttpContext context, int groupId)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context), HttpContextNullExceptionMsg);
        }

        context.Items[SiteDbGroupIdKey] = groupId;
    }

    public static void SetCustDbGroupId(this HttpContext context, int groupId)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context), HttpContextNullExceptionMsg);
        }

        context.Items[CustDbGroupIdKey] = groupId;
    }

    private static string GetValueByKey(string key, HttpContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context), HttpContextNullExceptionMsg);
        }

        if (!context.Items.TryGetValue(key, out var value))
        {
            throw new KeyNotFoundException($"{key} not found in HttpContext");
        }

        return value?.ToString();
    }
}