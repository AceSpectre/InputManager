# InputManager
A system to more easily and directly check actions within the unity input system
* [Installation and Generation](#installation-and-generation)
* [Using InputManager](#using-inputmanager)

## Installation and Generation
Drag and drop the `ManagerGenerator.cs` file into your project. Use unity's new [input system](https://github.com/Unity-Technologies/InputSystem) to create an InputActionAsset and populate the maps and actions with whatever inputs you wish to use. **Make sure to click `Generate C# Class` on the InputActionAsset**

Create a ManagerGenerator asset (right-click>Create>ManagerGenerator) and drag and drop your InputActionAsset into the `Controls Asset` field and click the `Generate Class` button. An InputManager class will be created within the current directory (clicking off an on the window gets the project view to update faster).

## Using InputManager
Input manager is generated with several `public static` functions to get inputs from the action maps. Each function has an overload to specify which `InputActionMap` you want to recieve actions from. Functions where the action map isn't specified will grab the first input it finds with the matching action name, **so make sure to uniquely name actions if you use it this way**
### GetButton
```cs
public static Vector2 GetVector2(string actionName, string mapName)
```
Returns a Vector2 input specified by its action name and map name
```cs
public static Vector2 GetVector2(string actionName)
```
Returns a Vector2 input only specified by action name

### GetVector2
```cs
public static bool GetButton(string actionName, bool held, string mapName)
```
Returns a bool input specified by its action name and map name, returning true only on the first frame of pressing if `!held` and returning true on every frame of pressing if `held`
```cs
public static bool GetButton(string actionName, bool held)
```
Returns a bool input only specified by action name, returning true only on the first frame of pressing if `!held` and returning true on every frame of pressing if `held`

