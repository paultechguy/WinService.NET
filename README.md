# WinService.NET

This project template exists to provide a single C# application that can execute as a Windows Service, Scheduled Task, or an interactive Console Application. The intention is to provide the basic requirements for most scenarios and do it in an understandable and enjoyable way.

This project supersedes the now deprecated .NET 6 version at [paultechguy/WinService.NetCore](https://github.com/paultechguy/WinService.NetCore).

## Terminology
For this document, the use of the word *application* refers to this project template running as a Windows Service, a Scheduled Task, or as an interactive Console Application.

## Requirements
- Windows 10 or higher
- Visual Studio 2022 with .NET 8

## Features

The features supported in the code project are:

1. Execute as a Windows Service
2. Execute as a Scheduled Task
1. Execute as a Console Application (optionally interactive)
1. Use PowerShell *sc* commands to install, uninstall, start, and stop services
3. Perform file and console logging
4. Send email messages using an SMTP Server (e.g., Gmail, localhost Papercut SMTP)
5. Easily accept command-line parameters and access them using dependency injection (DI)
6. Update the base *code name* to satisfy specific developer preferences

The project template does not contain database support. It was intentional to leave this out since there are several databases and database frameworks that exist.  The goal was to not discourage developers from using this project template because it uses a specific database technology.

The project template will execute out-of-the-box, either as a Windows Service, a Scheduled task, or as a Console Application.

## Default Functionality
The default functionality, for demonstration purposes, is for the application to loop every five (5) seconds and output a message to the log file and console.

    [07:28:14 INF] Starting WinService.Main
    [07:28:14 INF] Press Ctrl-C to cancel
    [07:28:14 INF] DEVELOPMENT environment detected
    [07:28:14 INF] Starting WindowsBackgroundService.ExecuteAsync
    [07:28:14 INF] Starting: WorkerService.ExecuteAsync
    [07:28:14 WRN] Email support disabled.  See appSettings.WorkerServiceSettings.MessageIsEnabled
    [07:28:14 INF] #1: Doing something every 5000 milliseconds: WorkerService.ExecuteAsync...

>The output also provide a warning that email support is disabled until SMTP configuration is completed.

## Customization
Before you begin customizing the code, it is recommended that you give things a test drive.  When you are ready to customize it, modify the `ExecuteAsync` method in `WinService.Service\WorkerService.cs`.

Be sure the code in `ExecuteAsync` is sensitive to checking, somewhat frequently, the cancellation token to determine if there is an external request to stop the application. The application can be stopped by externally by Windows Service commands, Task Scheduler commands, or an interactive Ctrl-C while running as a Console Application.

>When the `ExecuteAsync` method exits, the application will stop.

## Build Solution
Using Visual Studio, open up the `src\WinService.sln` and build the solution.

## Execute as a Console Application
After building the solution in Visual Studio, press F5 to execute it or open up a console window and execute the following from the build directory:

        .\WinService.Application.exe

## Execute as a Scheduled Task
Executing the application as a scheduled task is similar to executing it as a console application. Using Windows, you create a Windows Scheduled Task adding an Action with the *Program/script* using the path of the application and specify the *Start in* option as the location of the `WinService.Application/WinService.Application.exe` application.  Optionally, you can add command-line arguments to the action.

## Execute as a Windows Service
The following commands can be executed in an administrator-mode console window.  For some of the steps, you can also use the standard Windows Services UI (e.g., start, stop).

1) Create the service

        sc.exe create "My Service Name" binpath="C:\...\{yourFullBinPath}\WinService.Application.exe"

2) Start the service

        sc.exe start "My Service Name"

    By default, the *Startup Type* for the service is Manual.  To automatically start the application when Windows starts, use the Windows Services UI to update the *Startup Type*.

3) Stop the service

        sc.exe stop "My Service Name"

4) Delete the service

        sc.exe delete "My Service Name"

If deleting a service fails, ensure you have closed the Windows Services UI before performing the delete.

>If you want to verify your service is running, you can check the log file for messages (see Logging).

## Logging
By default, log files are created in the subdirectory where the `WinService.Application.exe` application resides. The subdirectory for log files is called *logs*.  The most recent 31 days of log files are saved.  For more information on the [Serilog](https://serilog.net/) logging configuration, see the `WinService.Application\appsettings.json` file.  This file also lets you specify where the log files are created.  To avoid the loss of log files, it is recommended to change the logging directory to a location outside the application *bin* directory.

>Console logging is enabled from `Program_Configure.cs` in cases where the application is running in interactive mode.

## SMTP Email
By default, the application will not attempt to send email, but you can update the `WinService.Application\appsettings.json` file to enable it.  If enabled, a single demonstration email will be sent when the `ExecuteAsync` method starts.

To enable email, you will need to access to an external SMTP email server.  Once an SMTP email server is available, complete the following configuration steps:

1) Update the `WinService.Application/appsettings.json` file as indicated below:

        "applicationSettings": {
          "messageIsEnabled": true,
          "messageToEmailAddress": "email To name <email address>",
          "messageFromEmailAddress": "email From name <email address>",
          "messageReplyToEmailAddress": ""
        },

2) Configure an SMTP email server

    For localhost testing you can use [Papercut-SMTP](https://www.papercut-smtp.com/).  This is an excellent tool to verify email is working a production deployment.

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

    >See [Gmail Help](https://support.google.com/mail/answer/185833) for assistance in creating Gmail application passwords.

## Command-Line Parameters

The tempalte project contains support to intelligently parse command-line parameters.  Command-line parameters are enabled for dependency injection as demonstrated in the `WinService.Service.WorkerService.cs` file.

For demonstration purposes, the `WinService.Application.exe` accepts a *-d* parameter to specify a millisecond delay for message logging.  You can modify the accepted command-line parameters by updating the `WinService.Core\Models\CommandLineOptions.cs` file.  See [Command Line Parser](https://github.com/commandlineparser/commandline/) for understanding the command-line parsing package.

## Base Source Code Name
The base source code name, used in file names, C# namespaces, and a few miscellaneous areas, is `WinService`. For example, one template project is named `WinService.Application` and the main application executable is named `WinService.Application.exe`.  Both of these examples contain the name, *WinService*. If you want to differentiate several services or want your project to have a naming convention that better reflects the purpose of the work it accomplishes, you can adjust the base `WinService` code name.

To adjust the base name, you can invoke the PowerShell script `tools\RenameWinService.ps1`.  Open a PowerShell command window in the `tools\RenameWinService` directory and update the base name by executing the following PowerShell script (e.g., change the base name to *MonitorCpu*):

    .\RenameWinService -path ..\src -oldString WinService -newString MonitorCpu

>Before doing this backup your project template directory in case you want to revert back for some reason.  In addition, be aware that if you merge the latest project code from GitHub, there will be merge conflicts as a result of the base name change.

## .editorConfig
The code has its own set of formatting and language rules.  If you don't like these, feel free to modify the .editorConfig file, or remove it entirely from the project. If you remove the .editorConfig, then you can also remove the StyleCop.Anayzers nuget from all projects in the solution.

## License
[MIT](https://github.com/paultechguy/WinService.NetCore/blob/develop/LICENSE.txt)

## Credits
This project as inspired by the Microsoft documentation on [Windows Services](https://docs.microsoft.com/en-us/dotnet/core/extensions/windows-service).

## Author
paultechguy
