
![Image](https://github.com/JohnathanADrews/Hularion/blob/main/Hularion%20image.png?raw=true)

# Hularion - *Software with a Strategy*

##### Hularion TM &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Software with a Strategy TM

&nbsp;

## Using Hularion Experience (HX)

To use Hularion Experience, there are two main options.


First, run the Hularion.sln in your development environment and set HularionDeveloper to your startup project. Running HularionDeveloper will enable you to run Hularion Experience applications.

The second way is to build Hularion Developer in Release mode. Then, the bin content can be relocated, and a shortcut to HularionDeveloper.exe can be created. Release mode is important since there are some embedded files that are referenced using relative file path while in debug mode, which enables debugging without needing to restart the application. These files are only available as embedded files in the bin. Doing this in Debug mode will cause an error when running the HularionDeveloper application.

When running HularionDeveloper, a .hx folder is created in the same path as the executable. This folder contains a database of user data, such as project and package references. This is also where packages are stored and split into various parts for fast retrieval. Having a standalone environment for HX projects you are developing will help prevent your .hx environment data from being overwritten.

#

Hularion Developer can run a HXProject or a HXPackage. A HXPackage is just a HXProject that has been compiled with a particular version number. Generally, a HXProject is created by creating a solution, and then adding two C# library projects. See the Cardarion sample card game repository as an example (https://github.com/JohnathanADrews/Cardarion).

One project contains the Hularion Experience code. This project could have many HXProjects in it. These could be spread out, but HularionDeveloper will find all HXProjects in all sub-folders, so it is convenient to keep them all in one IDE project, but not required.

The second IDE project contains the plugin code. This project contains plugin routes for data access, for example. This could also be broken into many projects. One way is not necessarily more convenient than another.

## Adding Project Sources

To add a HXProject to HularionBuilder, run the HularionBuilder application. Then, in the lower menu, select Package->View Sources. Then, click the Add Source button. The source has a name and a location. Set the name to the name of the project. If you downloaded the Cardarion project and want to add it, then set the name to Cardarion. Set the location to the full directory of the CardarionHX folder (e.g. c:\dev\sample\Cardarion\CardarionHX). Finally, click Create.

Next, go to Package->View Packages. There should be a few packages with a green tip, which indicates a HXProject. A blue tip indicates a HXPackage (i.e. compile project). Add the Cardarion package. Then, go to the Apps->My Apps menu item. There should be a Cardarion item in the list. Clicking Run will open the application in a new tab. Click the Play a Game button in the Cardarion application to see a game instance. It's still coming along, but it functions well as an example.

## Building a HXProject into a HXPackage

HularionDeveloper can run compiled HXProjects, which are HXPackages. First, a HXPackage must be built. To do this, open HularionDeveloper. If it is already open, be sure to click the first tab labeled Hularion Player. Then, go to Package->Build Package in the menu. This will bring up the Package Builder menu.

From here, set the source directory. You can use the Select Directory button to search for a directory, or you can click the "Enter the project source directory." text to enter it manually.

Then, set the destination directory the same way. Using the Cardarion example, this would be the full path for the CardarionHX folder. Select Overwrite if you want to overwrite a previous package the was made in error.

Finally, click build package. HularionDeveloper will build all the HXProjects it finds and create HXPackages. If there is no notification on clicking Build Package, just check the directory and they shoud be there. For the Cardarion example, there should be at least two, and probably four packages.

## Adding Package Sources

A package source is a directory containing *.hxpackage files. In Hularion Developer, go to Package->View Sources. Then, create a new source. Set the name to Packages. Set the location to the destination you used in the previous Build step. Then click Create. Finally, go back to Package->View Packages. If you used the Cardarion example, there should now be a few packages with a blue tip appearing in the list. If you Install the Cardarion package with the blue tip, an installation window will appear. Checking the "I have read and agree ..." checkbox and then clicking the install button will install the package (there may not be a notification yet).

Now, go to Apps->My Apps, and you should see the Cardarion app as a Package with a blue underline. Clicking Run, it should operate just as the HXProject version does. 

If you find the .hx directory, you will see a folder called packages. The Cardarion package and its details will be there. There will also be details for its dependency projects.


&nbsp;

# Creating HX Projects

HX Projects rely on a project folder structure. See the Cardarion repository for this example. The HX projects are in the CardarionHX folder. Except for back-end plugin/router logic, such as interacting with a DBMS for example, HX projects use primarily HTML snippet files and javascript files to run. Any text editor will work, but a familiar IDE that can manage files efficiently will be helpful.


### The HXProject File

From the CardarionHX folder, open the Cardarion folder. This folder contains the Cardarion.hxproject file, which contains the high level project details. In addition to metadata, this file contains references to other packages or projects. A project can be specified, but once built into a package, the package will use the package name and version number to run instead of the package.

The Cardarion HXPackage file also contains a DOM Element wrapper. This causes HX to wrap generated DOM elements using the provided wrapper logic. The h-package-import="CardarionDomeImport@1.0.0=>DomeImport" means "reference the package CardarionDomeImport with version 1.0.0 and assign it to alias 'DomeImport'" The next two lines create assign a set of javascript (called DOME) to an iframe. Finally, whenever a DOM Element (dome) is generated, use the function "e=>$(e);" to replace (wrap) it. This is useful in case you want to use a third-party javascript library (or several) to interact with DOM elements.

As you probably guessed, the HXProject files are HTML attribute driven. The hx element tag is just a placeholder. HTML is convenient since it is easy to embed javascript or style tags, which makes things easier since HX is essentially a javascript/HTML/CSS framework.


### The HXProject Sets

In HX, Different parts of the front-end javascript run in their own iframes. Doing this enables clear separation of code, ensuring that globals do not overwrite each other. An iframe loosely corresponds to a "set". Generally, an iframe will be produced for each set. When a set of code is loaded into the HX framework, an iframe is created with code that registers it with the parent window. Then, the set javascript code is loaded in. When a resouce is requested (e.g. widget code for a button or a menu), that item is produced in the iframe and then sent up to the parent window handler.

There are two main types of sets in HX, script sets and presenter sets. Script sets are just a collection of .js files. These all get bundeled together and run in their own iframe. Presenters are the UI unit of code, and each presenter has a bit of javascript. The javascript code for these presenters get bundled together and run into their own iframe as well.

To create a HXProject with an application, at least one presenter set must be created and at least one presenter must be created within that set. To add presenter sets to a project, simply add a folder called PresenterSets. Then, create another folder using the set name. For the Cardarion project, the main presenter set is CardarionPlayerPresenters. 

In that folder is the file CardarionPlayerEntry.html. This is the main entry point into the Cardarion application. If you look in there, you will see a script tag, and inside that tag is a function called CardarionPlayerEntry. That function also has a prototype method called start, which is called once for each presenter, after that presenter and its dependencies have been created. A dependency is, for example, the [hx h-presenter="cardarion.presenter/CardarionPlayer"] tag. This tag resolves to an instance of the CardarionPlayer presenter and is then placed at the same location in the CardarionPlayerEntry presenter. More on this further down.

### HX Applications

Now we need to let the project know that we have an application, and that the application should use CardarionPlayerEntry as the entry point. Each project can have multiple applications, and these are stored in the project's Applications folder. For Cardarion, you will see a Cardarion.html file. This file has some simple metatdata and points to the CardarionPlayerPresenters presenter set and the CardarionPlayerEntry presenter.

### HX Project Summary

You need to create the ProjectName.hxproject file in the main folder. Then, create a folder called PresenterSets, and then another folder in there having the name of the set. Then, create a presenter called (MyApplicationName)EntryPoint.html (or whatever you want to call it), and add the basic presenter code, which is the script tag and a function having the presenter name. You may wabt to create a label tag to make it apparent that the application has loaded the entry point presenter when the application is run. Finally, create an Applications folder and add a (MyApplicationName).html file containing the application name, and pointers to the presenter set and the presenter using the propert HTML attributes. 




 