# FlarialReskin
A WPF Re-Skinned version of the flarial launcher
Inspired by the older flarial launcher.

<img src="/demo.png" alt="Demo" width="500"/>

> [!Note]
> This project is not affiliated with [FlarialMC](https://github.com/flarialmc) And is simply a reskinned version.

## How it works
Underneath, This project simply uses the real flarial client and launcher.
In [FlarialHandler.StartGame()](Services/FlarialHandler.cs) it runs:
```
> flarial.exe --inject [DLL]
```

## How to build
#### 1. Ensure you have the .NET SDK intalled
#### 2. Clone this repository
```
git clone https://github.com/yousif51811/FlarialReskin.git
```
#### 3. Publish the app.
```
dotnet publish -c Release
```
#### 4. An exe will be available at `\bin\Release\net48\publish`

> [!WARNING]
> **Windows Defender could flag the downloaded Flarial DLL/Launcher after download**
> You might want to add the folder to your exclusion list in windows defender.
--------------
Made with ❤️ by yousif51811
