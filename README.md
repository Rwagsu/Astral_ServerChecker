<div align="center">

# Astral_ServerChecker

![GitHub License](https://img.shields.io/github/license/Rwagsu/Astral_ServerChecker?style=for-the-badge&labelColor=F3F3F3&color=0077FF)
![GitHub Repo stars](https://img.shields.io/github/stars/Rwagsu/Astral_ServerChecker?style=for-the-badge&logo=Github&logoColor=000000&labelColor=F3F3F3&color=0077FF)
![Status](https://img.shields.io/badge/Status-R0-FF0000?style=for-the-badge&labelColor=F3F3F3)
![GitHub Downloads](https://img.shields.io/github/downloads/Rwagsu/Astral_ServerChecker/total?style=for-the-badge&logo=githubactions&logoColor=000000&labelColor=F3F3F3&color=0077FF)

Astral 的服务器延迟检测被砍掉了, 一气之下整了个这玩意()

Astral_ServerChecker 会检测 [Astral 官网的服务器列表](https://astral.fan/server-config/server-list/)的所有服务器的延迟和稳定性, 并挑出最好的几个.

</div>

## 系统支持

- Windows 10+
- macOS 12+
- 现代 Linux (具体版本号我也不到QAQ)

## 使用

>[!WARNING]
> 在安装之前, 请务必确定你的电脑安装了 .NET 10.0 运行时环境.
>
> 打开 [.NET 官网](https://dotnet.microsoft.com/zh-cn/download/dotnet/10.0), 找到 .NET 运行时, 选择与你的操作系统和架构匹配的安装包.
>
> Rwagsu 不会理会因为没装 .NET 所引起的报错问题.

1. 从 [Release](https://github.com/Rwagsu/Astral_ServerChecker/releases) 下载对应你系统和架构的压缩包.
2. 解压后运行压缩包中的可执行文件.

## 启动参数

Astral_ServerChecker 有一些启动参数可用, 你可以像这样使用它:

```bat
Astral_ServerChecker.exe <参数>

# 或者指定多个参数:
Astral_ServerChecker.exe <参数1> <参数2> ...
```

- `--local`: 使用内嵌的服务器列表, 而不是从 GitHub 的 Astral 存储库获取.<br>
  适用于无法访问 GitHub 或 json 格式变化的情况.
- `--count <number>`: 指定单个服务器的检测次数, 默认为 12 次.<br>
  更多的检测次数可以更准确的反映服务器的稳定性, 但会增加非常多的检测时间.
- `--top <number>`: 指定在最后输出时输出的最佳服务器数量, 默认 5 个.
- `--interval <number>`: 指定每次检测的间隔时间 (ms), 默认为 500ms.<br>
  更少的间隔时间可以加快检测速度, 但可能会因为瞬时请求过多被远程服务器封禁.
- `--timeout <number>`: 指定检测时服务器在请求多长时间 (ms) 未响应后被判断为超时.

## 构建

这只是简单的控制台程序, 所以你在 git clone 后可以直接使用命令构建:

```bat
dotnet build
```

## 引用

没有引用任何库()

但是贴出来 .NET: [dotnet - GitHub](https://github.com/microsoft/dotnet)