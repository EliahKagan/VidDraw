<Query Kind="Statements">
  <NuGetReference Version="7.0.0">Microsoft.Toolkit.Uwp.Notifications</NuGetReference>
</Query>

// Hmm, ToastContentBuilder doesn't have a Show() method, even though
// https://docs.microsoft.com/en-us/windows/uwp/design/shell/tiles-and-notifications/send-local-toast?tabs=desktop
// says to use that method. The package I'm using is
// https://www.nuget.org/packages/Microsoft.Toolkit.Uwp.Notifications/
// and when the ToastContentBuilder type from that package is viewed on FuGet,
// https://www.fuget.org/packages/Microsoft.Toolkit.Uwp.Notifications/7.0.0/lib/netstandard1.4/Microsoft.Toolkit.Uwp.Notifications.dll/Microsoft.Toolkit.Uwp.Notifications/ToastContentBuilder
// no Show() method is listed. (The ToastContent type also has no Show()
// method; I checked that in case the method were moved.)
//
// This suggests the package that is retrieved was built without the
// WINDOWS_UWP symbol defined:
// https://github.com/windows-toolkit/WindowsCommunityToolkit/blob/v7.0.0/Microsoft.Toolkit.Uwp.Notifications/Toasts/Builder/ToastContentBuilder.cs#L399
//
// This isn't a UWP app, so I guess that makes sense, but there is supposed to
// be a way to use this from desktop (including Windows Forms) apps.

using Microsoft.Toolkit.Uwp.Notifications;

new ToastContentBuilder()
    .AddArgument("action", "viewConversation")
    .AddArgument("conversationId", 9813)
    .AddText("Gadzooks!")
    .AddText("Please do not buy whalebone skis.")
    //.GetType()
    //.GetMethods()
    //.Dump();
    .Show();
