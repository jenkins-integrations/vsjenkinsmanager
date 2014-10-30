Jenkins Manager for Visual Studio
================

![Screenshot](https://github.com/tomkuijsten/vsjenkinsmanager/blob/master/Devkoes.VSJenkinsManager/Devkoes.JenkinsManager.VSPackage/Resources/screenshot1.jpg)

================

A [visual studio package](http://visualstudiogallery.msdn.microsoft.com/3471d451-c1f1-4273-b305-acf81e4f8b32) to manage Jenkins:

Release 1.0:
 - [x] Cancel executing build / cancel scheduled build
 - [x] Start build / schedule build seperated
 - [x] Settings page is now scrollable
 - [x] Input fields are disabled when no server is selected
 - [x] Performance improvements (refresh data instead of replacing)
 - [x] Status bar removed (info available in output window)
 - [x] Small fix for build history (executing was always red)
 - [x] Preferences are removed when server is removed

Release 0.5:
 - [x] Show build history
 - [x] Progress bar visuals improved
 - [x] Progress of build is shown
 - [x] Log output directly visible in visual studio
 - [x] Preferred view restored when opening a solution
 - [x] Moved settings to the tools->settings menu

Release 0.4:
 - [x] Bug fix for view caching problem (wrong views for server)
 - [x] Disable dropdowns when switching server
 - [x] Progressbar when switching server
 - [x] Bug fix for servers where authentication is required for everything

Release 0.3.1:
 - [x] Bugfixes for context menu actions

Release 0.3:
 - [x] Jenkins View support
 - [x] JSON data reduction
 - [x] Tooltip on scheduled icon with the "why" reason

Release 0.2:
 - [x] Authentication (through API token)
 - [x] Auto refresh status when starting build
 - [x] Context menu in favor of toolbar

Release 0.1:
 - [x] Configure Jenkins servers
 - [x] List builds with status
 - [x] Start a job
 - [x] Connect a job to a visual studio solution
 - [x] Solution file context menu with a "Build on Jenkins" option
 - [x] Publish as visual studio extension
