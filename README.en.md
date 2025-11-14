# IntelliSenseLocalizer 
a tool for generate and install Localized IntelliSense files.

## intro
# IntelliSenseLocalizer

A tool for generating and installing localized IntelliSense files.

## Introduction

Prior to .NET 6, it was possible to download localized .NET IntelliSense files directly from the [official download page](https://dotnet.microsoft.com/en-us/download/intellisense). However, long after the release of .NET 6, the page still did not include localized IntelliSense files for the new version. According to [this issue](https://github.com/dotnet/docs/issues/27283) in the `dotnet/docs` repository, Microsoft confirmed that they would no longer be localizing IntelliSense — stating explicitly: *"Yes, unfortunately, we will no longer be localizing IntelliSense."*

Despite this, the [online documentation](https://docs.microsoft.com) continues to provide localized API descriptions. This gap between the discontinued IntelliSense localization and the availability of localized online content inspired the development of this tool.

`IntelliSenseLocalizer` generates localized IntelliSense files by extracting and processing content from the [online documentation](https://docs.microsoft.com). The tool automatically downloads relevant API pages, analyzes their structure and content to match the original IntelliSense data, and produces the corresponding localized XML files.

Thanks to the comprehensive localization and consistent page layout of the Microsoft Docs website, this tool can theoretically generate IntelliSense files for any supported locale.

## How to use

### 1. install the tool
```shell
dotnet tool install -g islocalizer
```

#### run `islocalizer -h` to see more command and helps.

Append the argument -h at the end of the command to view the help of the command. eg:在命令末尾添加参数-h，可以查看命令的帮助信息。例如:
```shell   “‘壳
islocalizer install auto -h
islocalizer cache -h
```

### 2. try install Localized IntelliSense files from nuget.org

#### View available packs [Nuget](https://www.nuget.org/packages/IntelliSenseLocalizer.LanguagePack)

This command try get the Localized IntelliSense files from nuget.org what moniker is `net6.0` and locale is `zh-cn`. And install it:这个命令尝试从nuget.org获取本地化的智能感知文件，名字是`net6.0`，语言环境是`zh-cn`。然后安装它：

```shell
islocalizer install auto -m net6.0 -l zh-cn
```
Also you can set the ContentCompareType by `-cc`
```shell
islocalizer install auto -m net6.0 -l zh-cn -cc LocaleFirst
```

### 3. build the local Localized IntelliSense files yourself

build files about `net6.0`:
```shell
islocalizer build -m net6.0
```
This command may take a whole day... But when cached all page it will be completed faster.
The archive package will be saved in the default output directory. You can found the path in console.

### 4. install builded file
```shell
islocalizer install {ArchivePackagePath}
```
`ArchivePackagePath` is the path of the archive package that you builded.

