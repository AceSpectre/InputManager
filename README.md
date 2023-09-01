# InputManager
System for easily and directly checking actions in unity input system
* [Installation and Generation](#installation-and-generation)
* [Using InputManager](#using-inputmanager)

## Installation and Generation

## Using InputManager
Input manager is generated with several `public static` functions to get inputs from the action maps. Each function has an overload to specify which `InputActionMap` you want to recieve actions from. Functions where the action map isn't specified will grab the first input it finds with the matching action name, **so make sure to uniquely name actions if you use it this way**
### GetButton
### GetVector2

