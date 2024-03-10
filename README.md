# VsSetupInfo
Command line application to show Visual Studio installation information. Prints details for installed Visual Studio locations and packages. This is somewhat similar to the `vswhere.exe` tool. 

# Example Output
```
Id: 1c7a650b
Name: VisualStudio/17.9.2+34622.214
DisplayName: Visual Studio Enterprise 2022
Location: C:\Program Files\Microsoft Visual Studio\2022\Enterprise
Version: 17.9.34622.214
State: Complete
Properties: [{"Key":"campaignId","Value":""},{"Key":"setupEngineFilePath","Value":"C:\\Program Files (x86)\\Microsoft Visual Studio\\Installer\\setup.exe"},{"Key":"nickname","Value":""},{"Key":"channelManifestId","Value":"VisualStudio.17.Release/17.9.2\u002B34622.214"}]
Product: {"Id":"Microsoft.VisualStudio.Product.Enterprise","Version":"17.9.34622.214","Chip":"x64","Language":null,"Branch":null,"Type":"Product","UniqueId":"Microsoft.VisualStudio.Product.Enterprise,version=17.9.34622.214,chip=x64","IsExtension":false,"ProductIsInstalled":true,"Product2SupportsExtensions":true}
ProductPath: Common7\IDE\devenv.exe
EnginePath: C:\Program Files (x86)\Microsoft Visual Studio\Installer\resources\app\ServiceHub\Services\Microsoft.VisualStudio.Setup.Service
Package: {"Id":"Microsoft.VisualStudio.Product.Enterprise","Version":"17.9.34622.214","Chip":"x64","Language":null,"Branch":null,"Type":"Product","UniqueId":"Microsoft.VisualStudio.Product.Enterprise,version=17.9.34622.214,chip=x64","IsExtension":false,"ProductIsInstalled":true,"Product2SupportsExtensions":true}
Package: {"Id":"Microsoft.VisualStudio.PackageGroup.LiveShare.VSCore","Version":"17.9.34511.75","Chip":null,"Language":null,"Branch":null,"Type":"Group","UniqueId":"Microsoft.VisualStudio.PackageGroup.LiveShare.VSCore,version=17.9.34511.75","IsExtension":false,"ProductIsInstalled":null,"Product2SupportsExtensions":null}
Package: {"Id":"Microsoft.VisualStudio.LiveShare.VSCore","Version":"2.0.2322.1","Chip":null,"Language":null,"Branch":null,"Type":"Vsix","UniqueId":"Microsoft.VisualStudio.LiveShare.VSCore,version=2.0.2322.1","IsExtension":false,"ProductIsInstalled":null,"Product2SupportsExtensions":null}
Package: {"Id":"Microsoft.VisualStudio.Component.VC.14.31.17.1.x86.x64.Spectre","Version":"17.9.34511.75","Chip":null,"Language":null,"Branch":null,"Type":"Component","UniqueId":"Microsoft.VisualStudio.Component.VC.14.31.17.1.x86.x64.Spectre,version=17.9.34511.75","IsExtension":false,"ProductIsInstalled":null,"Product2SupportsExtensions":null}

...

```
