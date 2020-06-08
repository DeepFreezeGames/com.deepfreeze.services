# Services

Services are modules for your game that perform a specific task. These could be used for Auth, Events, Notifications, Scene Management etc. By breaking these into services, you can modularize you codebase and make it more manageable and easier to debug.

## Installation
To install the package:

 1. Open the Unity Package Manager (Window>Package Manager)
 2. Click the "+" button in the top-left of the Package Manager window
 3. Click the "Add package from Git URL" option from the drop down
 4. Paste "https://github.com/DeepFreezeGames/com.deepfreeze.services.git" into the text box

This will install the package in the Packages sub-directory of you Unity project

## IService structure

Each Service contains some default methods that are required for the manager to control them. The methods that are included are Initialize() and Cleanup(). Both of these methods are virtual methods so you can expand upon them if needed.

|Item Name      |Description|
|---------------|-------------------------------|
|onInitialize	|An action that is called when the Initialize() method completes|
|onCleanup      |An action that is called when the Cleanup() method completes|
|Initialize     |Runs any code that is needed before your service is registered|
|Cleanup		|Preforms any cleanup actions when your service is unregistered from the manager|

## Creating a service

Services can either inherit from the IService or Service class. The Service class contains basic implimentations of the IService framework.