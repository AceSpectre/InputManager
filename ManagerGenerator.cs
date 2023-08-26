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
    public InputActionAsset controlsAsset;
    private string indentation;
    private int indentCount = 0;

    /// <summary>
    /// Dictionary of method signatures 
    /// </summary>
    /// Note: Input type and action map sensitivity need to be specified in the name for the script to function
    private Dictionary<string, StringBuilder> methodBodies = new()
    {
        {"vector2MapSensitive", new StringBuilder("\tpublic static Vector2 GetVector2(string actionName, string mapName)") },
        {"vector2Insensitive", new StringBuilder("\tpublic static Vector2 GetVector2(string actionName)") },
        {"buttonMapSensitive", new StringBuilder("\tpublic static bool GetButton(string actionName, bool held, string mapName)") },
        {"buttonInsensitive", new StringBuilder("\tpublic static bool GetButton(string actionName, bool held)") }
    };

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

    private void SetIndentation(int indents)
    {
        indentation = new string('\t', indents);
    }

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

    private void AddStringToBodyGroup(bool mapSensitive, string content)
    {
        Dictionary<string, StringBuilder> updatedMethodBodies = new();
        foreach (var body in methodBodies)
        {
            if (mapSensitive && body.Key.Contains("MapSensitive"))
            {
                updatedMethodBodies[body.Key] = body.Value;
                updatedMethodBodies[body.Key].AppendLine(content);
            }
            else if (!mapSensitive && !body.Key.Contains("MapSensitive"))
            {
                updatedMethodBodies[body.Key] = body.Value;
                updatedMethodBodies[body.Key].AppendLine(content);
            }
        }
        UpdateBodies(updatedMethodBodies);
    }

    private void AddStringToBodyType(string inputType, string content)
    {
        Dictionary<string, StringBuilder> updatedMethodBodies = new();
        foreach (var body in methodBodies)
        {
            if (body.Key.Contains(inputType))
            {
                updatedMethodBodies[body.Key] = body.Value;
                updatedMethodBodies[body.Key].AppendLine(content);
            }
        }
        UpdateBodies(updatedMethodBodies);
    }

    /// <summary>
    /// Updates the values of the dictionary of method bodies using t
    /// </summary>
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
        StringBuilder classContent = new(@"using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
");
        classContent.AppendLine("public static class InputManager : object" + Environment.NewLine + "{");

        SetIndentation(++indentCount);


        classContent.AppendLine(indentation + $"public static {assetName} {assetName.ToLower()} = new();" + Environment.NewLine);

        AddStringToAllBodies(Environment.NewLine + indentation + "{" + Environment.NewLine + indentation + $"\t{assetName.ToLower()}.Enable();" + Environment.NewLine);

        SetIndentation(++indentCount);
        
        AddStringToBodyGroup(true, BeginSwitchStatement("mapName"));

        AddStringToBodyGroup(false, BeginSwitchStatement("actionName"));

        SetIndentation(++indentCount);
        foreach (InputActionMap map in controlsAsset.actionMaps)
        {
            AddStringToBodyGroup(true, indentation + $"case \"{map.name}\":");

            SetIndentation(++indentCount);

            AddStringToBodyGroup(true, BeginSwitchStatement("actionName"));

            SetIndentation(++indentCount);

            foreach (InputAction action in map.actions)
            {
                string currentAction;
                switch (action.expectedControlType)
                {
                    case "Button":
                        AddStringToBodyType("button", indentation + $"case \"{action.name}\":");

                        SetIndentation(++indentCount);
                        currentAction = $"{assetName.ToLower().Replace(" ", "")}.{map.name.Replace(" ", "")}.{action.name.Replace(" ", "")}";
                        
                        AddStringToBodyType("button", indentation + $"return held ? {currentAction}.triggered : {currentAction}.ReadValue<bool>();");

                        SetIndentation(--indentCount);
                        break;
                    case "Vector2":
                        AddStringToBodyType("vector2", indentation + $"case \"{action.name}\":");

                        SetIndentation(++indentCount);
                        currentAction = $"{assetName.ToLower().Replace(" ", "")}.{map.name.Replace(" ", "")}.{action.name.Replace(" ", "")}";

                        AddStringToBodyType("vector2", indentation + $"return {currentAction}.ReadValue<Vector2>();");

                        SetIndentation(--indentCount);
                        break;
                    default:
                        break;
                }
            }
            SetIndentation(--indentCount);

            AddStringToBodyGroup(true, EndSwitchStatement() + Environment.NewLine + indentation + "break;");

            SetIndentation(--indentCount);
        }
        string className = "InputManager";
        SetIndentation(--indentCount);

        AddStringToAllBodies(EndSwitchStatement());

        SetIndentation(--indentCount);

        AddStringToBodyType("vector2", $"\t\t{ assetName.ToLower()}.Disable();" + Environment.NewLine + "\t\treturn new Vector2(0f, 0f);" + Environment.NewLine + indentation + "}");
        AddStringToBodyType("button", $"\t\t{ assetName.ToLower()}.Disable();" + Environment.NewLine + "\t\treturn false;" + Environment.NewLine + indentation + "}");


        foreach (var body in methodBodies)
        {
            classContent.AppendLine(body.Value.ToString());
            Debug.Log("OKBUDDY");
        }

        classContent.AppendLine("}" + Environment.NewLine);

        // Get the directory path of the executing script
        string scriptDirectory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)));

        // Specify the file path to save the class
        string filePath = Path.Combine(scriptDirectory, className + ".cs");

        try
        {
            // Create the class file and write the content
            File.WriteAllText(filePath, classContent.ToString());

            Debug.Log("Class saved successfully!");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error occurred while saving the class: " + ex.Message);
        }
    }
}
