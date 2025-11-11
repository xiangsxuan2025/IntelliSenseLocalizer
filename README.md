# 本地化 .NET IntelliSense 文件生成下载器

## 简介
`.NET6`之后，智能感知[.NET IntelliSense files](https://dotnet.microsoft.com/en-us/download/intellisense)文件微软[不再发布](https://github.com/dotnet/docs/issues/27283#issuecomment-985736340)。

但是，[微软在线文档](https://docs.microsoft.com)仍有本地化翻译。我们可以爬下来网页上的文字，自行生成
.NET IntelliSense files文件。

# 使用方法

## 1. 简体中文用户
简中翻译包[@stratosblue](https://github.com/stratosblue)同学已经[爬取好了](https://www.nuget.org/packages/IntelliSenseLocalizer.LanguagePack#versions-body-tab)，可以运行本程序直接更新。

<img src=".\src\IntelliSenseLocalizer.App\Assets\IntellisenseApp-preview.png" style="border-radius:4px" alt="界面">

（你可能没见过这么丑的程序，就用一次，将就下😂 ）

然后，就可以快捷编码啦！
<img src=".\src\IntelliSenseLocalizer.App\Assets\KeepEnglishAfter.png" style="border-radius:4px" alt="界面">



## 2. 其他用户（自行构建、非简体中文用户）
请参考[How to use脚本说明](https://github.com/stratosblue/IntelliSenseLocalizer/blob/main/README.md#how-to-use)，运行爬取程序、更新。
