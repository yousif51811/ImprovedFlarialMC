# FlarialReskin
A WPF Re-Skinned version of the flarial launcher
Inspired by the older flarial launcher.

<img src="/demo.png" alt="Demo" width="500"/>

> [!Note]
> This project is not affiliated with [FlarialMC](https://github.com/flarialmc) And is simply a reskinned version.

## How it works
Underneath, This project simply uses the real flarial client and launcher.
In [Clienthandler.StartGame()](/ClientHandler.cs) it runs:
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
#### 4. An exe will be available at `\bin\Release\net10.0-windows\publish`
--------------
Made with ❤️ by yousif51811
