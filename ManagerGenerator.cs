using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "InputManagerGenerator", menuName = "Manager Generator")]
public class ManagerGenerator : ScriptableObject
{
    /// <summary>
    /// The InputActionAsset to generate an InputManager from
    /// </summary>
    public InputActionAsset controlsAsset;

    /// <summary>
    /// Current indentation
    /// </summary>
    private string indentation;

    /// <summary>
    /// Current indentation amount
    /// </summary>
    private int indentCount = 0;

    /// <summary>
    /// Dictionary of method signatures 
    /// </summary>
    /// <remarks>
    /// Input type and action map sensitivity need to be specified in the key for the script to function
    /// </remarks>
    private Dictionary<string, StringBuilder> methodBodies = new()
    {
        {"vector2MapSensitive", new StringBuilder(@"
    /// <summary>
	/// Gets the vector2 input value of an action, specified by its input action map
	/// </summary>
	/// <param name=""actionName"">Name of the action</param>
	/// <param name=""mapName"">Name of the input action map</param>
    /// <returns>Vector2 input of the action</returns>
    public static Vector2 GetVector2(string actionName, string mapName)") },
        {"vector2Insensitive", new StringBuilder(@"
    /// <summary>
	/// Gets the vector2 input value of first action of the specified name
	/// </summary>
	/// <remarks>
	/// Only use this if input actions do not have any duplicate names across action maps
	/// </remarks>
	/// <param name=""actionName"">Name of the action</param>
    /// <returns>Vector2 input of the action</returns>
    public static Vector2 GetVector2(string actionName)") },
        {"buttonMapSensitive", new StringBuilder(@"
    /// <summary>
	/// Gets the bool input value of an action, specified by its input action map
	/// </summary>
	/// <param name=""actionName"">Name of the action</param>
    /// <param name=""held"">Check if the button is being held</param>
    /// <param name=""mapName"">Name of the input action map</param>
    /// <returns>Bool input of the action</returns>
    public static bool GetButton(string actionName, bool held, string mapName)") },
        {"buttonInsensitive", new StringBuilder(@"
    /// <summary>
	/// Gets the bool input value of first action of the specified name
	/// </summary>
	/// <param name=""actionName"">Name of the action</param>
	/// <param name=""held"">Check if the button is being held</param>
    /// <returns>Bool input of the action</returns>
    public static bool GetButton(string actionName, bool held)") }
    };
    // I would have kept the docstrings in a separate .json file but I couldnt get
    // system.text.json to work in a unity project

    /// <summary>
    /// Editor class for the ManagerGenerator class
    /// </summary>
    [CustomEditor(typeof(ManagerGenerator))]
    public class ManagerGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ManagerGenerator manager = (ManagerGenerator)target;

            if (GUILayout.Button("Generate Class"))
            {
                manager.GenerateInputManager();
            }
        }
    }

    /// <summary>
    /// Sets the current indentation amount
    /// </summary>
    /// <param name="indents">Numbers of indents to set "indentation" to</param>
    private void SetIndentation(int indents)
    {
        indentation = new string('\t', indents);
    }

    /// <summary>
    /// Appends a string to all method bodies
    /// </summary>
    /// <param name="content">Content to append to method bodies</param>
    private void AddStringToAllBodies(string content)
    {
        Dictionary<string, StringBuilder> updatedMethodBodies = new();
        foreach (var body in methodBodies)
        {
            updatedMethodBodies[body.Key] = body.Value;
            updatedMethodBodies[body.Key].AppendLine(content);
        }
        methodBodies = updatedMethodBodies;
    }

    /// <summary>
    /// Appends a string to method bodies whose key contains the string "type"
    /// </summary>
    /// <param name="type">String to filter type of method body to append to</param>
    /// <param name="content">Content to append to method bodies that match the filter</param>
    private void AddStringToBodyType(string type, string content)
    {
        Dictionary<string, StringBuilder> updatedMethodBodies = new();
        foreach (var body in methodBodies)
        {
            if (body.Key.Contains(type))
            {
                updatedMethodBodies[body.Key] = body.Value;
                updatedMethodBodies[body.Key].AppendLine(content);
            }
        }
        UpdateBodies(updatedMethodBodies);
    }

    /// <summary>
    /// Overwrites the values of the method body dictionary with values from a new dictionary
    /// </summary>
    /// <param name="updatedMethodBodies">Dictionary of updated method bodies</param>
    private void UpdateBodies(Dictionary<string, StringBuilder> updatedMethodBodies)
    {
        foreach(var body in updatedMethodBodies)
        {
            methodBodies[body.Key] = body.Value;
        }
    }

    /// <summary>
    /// Creates the beginning of a switch statement based on the current indentation
    /// </summary>
    /// <returns>Beginning of a switch statement with indentation</returns>
    private string BeginSwitchStatement(string switchValue)
    {
        return indentation + $"switch({switchValue})" + Environment.NewLine + indentation + "{";
    }

    /// <summary>
    /// Creates the end of a switch statement based on the current indentation
    /// </summary>
    /// <returns>End of a switch statement with indentation</returns>
    private string EndSwitchStatement()
    {
        return indentation + "\tdefault:" + Environment.NewLine + indentation + "\t\tbreak;" + Environment.NewLine + indentation + "}";
    }

    /// <summary>
    /// Generates the code for the input manager and writes it to InputManager.cs in the local directory
    /// </summary>
    public void GenerateInputManager()
    {
        indentCount = 0;
        string assetName = controlsAsset.name;

        //Set up the beginning of the InputManager class
        StringBuilder classContent = new(@"using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
");     
        
        //Append class signature
        classContent.AppendLine("public static class InputManager : object" + Environment.NewLine + "{");

        SetIndentation(++indentCount);

        //Append the input action asset for the class to use
        classContent.AppendLine(indentation + $"private static {assetName} {assetName.ToLower()} = new();" + Environment.NewLine);

        //Begin method content for all method bodies
        AddStringToAllBodies(Environment.NewLine + indentation + "{" + Environment.NewLine + indentation + $"\t{assetName.ToLower()}.Enable();" + Environment.NewLine);

        SetIndentation(++indentCount);

        //Begin switch statements for map sensitive and insensitive method bodies
        AddStringToBodyType("MapSensitive", BeginSwitchStatement("mapName"));
        AddStringToBodyType("Insensitive", BeginSwitchStatement("actionName"));

        SetIndentation(++indentCount);
        //MapSensitive: For each action map, create a switch statment for each type of input
        //Insensitive: Create a switch statement for each type of input
        foreach (InputActionMap map in controlsAsset.actionMaps)
        {
            AddStringToBodyType("MapSensitive", indentation + $"case \"{map.name}\":");

            SetIndentation(++indentCount);

            AddStringToBodyType("MapSensitive", BeginSwitchStatement("actionName"));

            SetIndentation(++indentCount);

            //For each action in the actionmap, create a switch statment for each type of action, adding to the respective bodies
            foreach (InputAction action in map.actions)
            {
                string currentAction;
                switch (action.expectedControlType)
                {
                    case "Button":
                        //Append the case line
                        AddStringToBodyType("button", indentation + $"case \"{action.name}\":");

                        SetIndentation(++indentCount);
                        currentAction = $"{assetName.ToLower().Replace(" ", "")}.{map.name.Replace(" ", "")}.{action.name.Replace(" ", "")}";
                        
                        //Append the return line
                        AddStringToBodyType("button", indentation + $"return held ? {currentAction}.triggered : {currentAction}.ReadValue<bool>();");

                        SetIndentation(--indentCount);
                        break;
                    case "Vector2":
                        //Append the case line
                        AddStringToBodyType("vector2", indentation + $"case \"{action.name}\":");

                        SetIndentation(++indentCount);
                        currentAction = $"{assetName.ToLower().Replace(" ", "")}.{map.name.Replace(" ", "")}.{action.name.Replace(" ", "")}";

                        //Append the return line
                        AddStringToBodyType("vector2", indentation + $"return {currentAction}.ReadValue<Vector2>();");

                        SetIndentation(--indentCount);
                        break;
                    default:
                        break;
                }
            }
            SetIndentation(--indentCount);

            //End the current switch statement in all map sensitive method bodies
            AddStringToBodyType("MapSensitive", EndSwitchStatement() + Environment.NewLine + indentation + "break;");

            SetIndentation(--indentCount);
        }
        string className = "InputManager";
        SetIndentation(--indentCount);

        AddStringToAllBodies(EndSwitchStatement());

        SetIndentation(--indentCount);

        //Add return paths for null values and print a debug statement if params were incorrect
        //AddStringToAllBodies($"\t\tDebug.Log(\"{"Parameters returned no value"}\")");
        AddStringToBodyType("vector2", $"\t\t{ assetName.ToLower()}.Disable();" + Environment.NewLine + "\t\treturn new Vector2(0f, 0f);" + Environment.NewLine + indentation + "}");
        AddStringToBodyType("button", $"\t\t{ assetName.ToLower()}.Disable();" + Environment.NewLine + "\t\treturn false;" + Environment.NewLine + indentation + "}");

        //For each method body, append the body to the class content
        foreach (var body in methodBodies)
        {
            classContent.AppendLine(body.Value.ToString());
        }

        //End the class
        classContent.AppendLine("}" + Environment.NewLine);

        //Get the directory path of the executing script
        string scriptDirectory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)));

        //Specify the file path to save the class
        string filePath = Path.Combine(scriptDirectory, className + ".cs");

        try
        {
            //Create the class file and write the content
            File.WriteAllText(filePath, classContent.ToString());

            //Log that writing was successful
            Debug.Log("Class saved successfully!");
        }
        catch (Exception ex)
        {
            //Log that writing failed
            Debug.LogError("Error occurred while saving the class: " + ex.Message);
        }
    }
}
