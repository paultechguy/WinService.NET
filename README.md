# WinService by Paul Carver

This project was created to make spinning up Windows Service or Scheduled Task easy. The intention is to keep things simple, but still provide most of the basics that are required.

This project supersedes the now deprecated .NET 6 version at [paultechguy/WinService.NetCore](https://github.com/paultechguy/WinService.NetCore).

## Terminology
For the purposes of of this document, the use of the word *application* refers to this code project running in a console window, as a Scheduled Task, or as a service.

## Requirements
- Windows 10 or higher
- Visual Studio 2022 with .NET 8

## Features

The features supported in this .NET 8 solution are:

1. Windows Service, long-running and support for the Windows *sc* commands
2. Scheduled Task support to allow execution as a standard console application
3. Logging is provided by Serilog and default to console and file
4. SSL Email support using an SMTP server (e.g. Gmail, localhost Papercut SMTP)
5. Intelligent command-line parameter parsing with dependency injection
6. Update source code naming conventions to suite your specific purpose

You may be wondering why there is no database support. It was very intentional to leave this out since there are several databases and database frameworks that exist.  I didn't want to complicate this starter project and get into a *religious* discussion about database technologies.  Would anyone be interested in using SQLite?

Things should work right out-of-the-box by running it in Visual Studio.  You can build the solution and run the application in a console window, use it in a Scheduled Task, or install as a service. See **Customize the Service** for making the application do your own magically feats.

## Project Customization
I suggest that before you begin customizing the code, you give things a test drive using the out-of-the-box functionality.  Then when you are ready to customize it, modify the `ExecuteAsync` method in `WinService.Service\WorkerService.cs`. Note that when this method exits, the application will stop.

Be sure the code in `ExecuteAsync` is sensitive to checking, somewhat frequently, the cancellation token to determine if there is an incoming request to stop the application.

## Build Solution
Using Visual Studio, open up the `src\WinService.sln` and build the solution.

## Run as a Console Application
After building, open up a console window and execute the following from your build directory:

        .\WinService.Application.exe

## Run as a Scheduled Task
Running the application as a scheduled task is similar to running it as a console application. You create a Windows Scheduled Task adding an Action with the *Program/script* using the path of the application and specify the *Start in* option as the location of the `WinService.Application/WinService.Application.exe` application.  Finally, you can optionally add command-line arguments to the action.

## Run as a Windows Service
The following commands can be executed in an elevated (administor mode) console window.  For some of the steps, you can also use the standard Windows Services user-interface (e.g. start, stop).

1) Create the service

        sc.exe create "My Service Name" binpath="C:\...\binpath\WinService.Application.exe"

2) Start the service

        sc.exe start "My Service Name"

    By default, the *Startup Type* for the service is Manual.  If you want it to automatically start when the computer is rebooted, use the Windows Services UI to update it.

3) Stop the service

        sc.exe stop "My Service Name"

4) Delete the service

        sc.exe delete "My Service Name"

If the delete fails, ensure you have closed down the Windows Services application. Thank you Microsoft.

**Note**: If you want to verify your service is running, you can check the log file for messages (see Logging).

## Logging
By default, log files are created in the subdirectory where the `WinService.Application.exe` application resides. The subdirectory for log files is called *logs*.  The most recent 31 days of log files are saved.  For more information on the [Serilog](https://serilog.net/) configuration, see the `WinService.Application\appsettings.json` file.  This file also lets you specify where the log files are created.

## SSL Email
By default the application will not attempt to send email, but you can update the `WinService.Application\appsettings.json` file to enable it.  If enabled, then a single demonstration email will be sent when the `ExecuteAsync` method starts.

To enable email, you will need to complete a few steps:

1) Update the `WinService.Application/appsettings.json` file as indicated below:

        "applicationSettings": {
          "messageIsEnabled": true,
          "messageToEmailAddress": "email name <email address>",
          "messageFromEmailAddress": "email name <email address>",
          "messageReplyToEmailAddress": ""
        },

2) Configure an SMTP email server

For localhost testing you can use [Papercut-SMTP](https://www.papercut-smtp.com/).  This is an excellent tool for verifying email is working before a deployment to production. You can also use Gmail by creating an *App Password* in your Gmail account.

The following two sections show the changes in `WinService.Application\appsettings.json` for configurating an SMTP email server.

    ### Papercut-SMTP Configuration

        "emailServerSettings": {
          "serverHost": "localhost",
          "serverPort": "25",
          "serverEnableSsl": "false",
          "serverUsername": "",
          "serverPassword": ""
        }

    ### Gmail SMTP Configuration

        "emailServerSettings": {
          "serverHost": "smtp.gmail.com",
          "serverPort": "587",
          "serverEnableSsl": "true",
          "serverUsername": "{your Gmail app username (i.e. your Gmail email address)}",
          "serverPassword": "{your Gmail app password}"
        }

## Command-Line Parameter Support

The out-of-the-box project contains everything you need to add intelligent parsing for command-line parameters.  Command-line parameters are enabled for dependency injection as demonstrated in the `WinService.Service.WorkerService.cs` file.

The default `WinService.Application.exe` accepts a *-d* parameter to specify a millisecond delay for the demostration application.  You can add other command-line parameters by updating the `WinService.Core\Models\CommandLineOptions.cs` file.  See [Command Line Parser](https://github.com/commandlineparser/commandline/) for understanding the command-line parsing package.

## Update Source Code Names
The base source code name, used in file names, C# namespaces, and a few miscellaneous areas, is `WinService`. For example, one project is called `WinService.Application` and the main application executable is called `WinService.Application.exe`.  If you want to differentiate several services or simply want your project to have a naming convention that better reflect the purpose of the work it accomplishes (e.g. monitor cpu), you can adjust the base `WinService` name.

To adjust the base names, you can invoke the PowerShell script `tools\RenameWinService.ps1`.  Open a PowerShell command window in the `tools\RenameWinService` directory and rename base names by using the following command (e.g. change code names to *MonitorCpu*):

    .\RenameWinService -path ..\src -oldString WinService -newString MonitorCpu

Before doing this, you should backup your project directory in case you want to revert back for some reason.  In addition, be aware that if you merge the latest project code from GitHub, there will be numerous merge changes given the base name change.

## .editorConfig
The code has its own set of formatting and language rules.  If you don't like these, feel free to modify the .editorConfig file, or remove it entirely from the project. If you remove the .editorConfig, then you can also remove the StyleCop.Anayzers nuget from all projects in the solution.

## License
[MIT](https://github.com/paultechguy/WinService.NetCore/blob/develop/LICENSE.txt)

## Credits
This project as inspired by the Microsoft documentation on [Windows Services](https://docs.microsoft.com/en-us/dotnet/core/extensions/windows-service).
