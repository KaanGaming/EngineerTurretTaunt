# Build Instructions for EngineerTurretTaunt

## Step 0: Requirements

You'll need either Visual Studio or .NET CLI to be able to load NuGet packages, build the project, etc.

The .NET CLI is included with .NET SDKs.

This project also runs on .NET Framework 4.7.2, so you'll need to grab that here: https://dotnet.microsoft.com/en-us/download/dotnet-framework/net472

If you have Visual Studio installed, you won't need to worry about anything other than creating the symlinks or editing the [EngineerTurretTaunt.csproj](./EngineerTurretTaunt/EngineerTurretTaunt.csproj).

## Step 0.1: Set up references

### Method 1: Symlink setup

The project file references the required libraries via two symbolic links in the root folder of this repository. Those two symbolic links are .gitignore'd, so you'll have to create the symbolic links to those folders. These folders being the `BepInEx` folder (the symlink is named `bepinex` in root) and the `Managed` folder in Risk of Rain 2's directory (the symlink is named `api` in root). Create those symlinks in the root folder and you'll be ready to go.

On Unix based systems, this can be done by using the [`ln` command](https://man7.org/linux/man-pages/man1/ln.1.html) (click for man7.org page, or type `man ln` in the terminal).

On Windows 7 and future versions, it is recommended to use the [Link Shell Extension](https://schinagl.priv.at/nt/hardlinkshellext/linkshellextension.html) tool designed for Windows users to create symbolic links to folders, files, etc. (https://schinagl.priv.at/nt/hardlinkshellext/linkshellextension.html)

### Method 2: .csproj setup

If creating symbolic links is too much of a hassle, you can edit the paths to the symlinks in [EngineerTurretTaunt.csproj](./EngineerTurretTaunt/EngineerTurretTaunt.csproj).

Look for this section of the code in the given file:

```xml
...
    <!-- Edit these paths to make them point to the directions if necessary. -->
    <!-- But make sure to change these back to the original values when making a pull request! -->
    <RoRDir>../api</RoRDir>
    <BepInExDir>../bepinex</BepInExDir>
...
```

Edit the paths to make them point to the actual directions found in your system and reload the project!

## Step 1: Install NuGet packages

Type `dotnet restore` at either the root of the repository or the project folder, and it will download the required NuGet packages into the project.

## Step 2: Build

If you're using .NET CLI, type in `dotnet build` at the root of the repository to build the files.

If you're using Visual Studio, press CTRL+SHIFT+B (or CTRL+B) to build, or right click on the project to Build, or take a look at the topmost bar in your screen and go into the Build menu, and Build Solution.

## Step 3: Copy files

After building the project, to test the mod you'll have to copy the compiled mod into the `plugins` folder inside your `BepInEx` folder. Create a subfolder named `kanggamming-Engineer_Turret_Taunt` if it doesn't exist already, and overwrite the files with the new compiled ones, or copy the compiled files to the newly created subfolder.

After this, you'll be ready to go and launch the project!

# When rebuilding...

You'll only need to repeat Step 2 and Step 3 to playtest the mod.