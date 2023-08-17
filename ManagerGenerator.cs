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
    private Dictionary<string, StringBuilder> methodBodies = new Dictionary<string, StringBuilder>
    {
        {"vector2MapSensitive", new StringBuilder() },
        {"vector2Insensitive", new StringBuilder() },
        {"buttonMapSensitive", new StringBuilder() },
        {"buttonInsensitive", new StringBuilder() }
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

    private void UpdateBodies(Dictionary<string, StringBuilder> updatedMethodBodies)
    {
        foreach(var body in updatedMethodBodies)
        {
            methodBodies[body.Key] = body.Value;
        }
    }

    private string BeginSwitchStatement(string switchValue)
    {
        return indentation + $"switch({switchValue})" + Environment.NewLine + indentation + "{";
    }
    private string EndSwitchStatement()
    {
        return indentation + "\tdefault:" + Environment.NewLine + indentation + "\t\tbreak;" + Environment.NewLine + indentation + "}";
    }



    public void GenerateInputManager()
    {
        Debug.Log("cunt");
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

        methodBodies["vector2MapSensitive"].AppendLine(indentation + "public static Vector2 GetVector2(string actionName, string mapName)");
        methodBodies["buttonMapSensitive"].AppendLine(indentation + "public static bool GetButton(string actionName, bool held, string mapName)");
        methodBodies["vector2Insensitive"].AppendLine(indentation + "public static Vector2 GetVector2(string actionName)");
        methodBodies["buttonInsensitive"].AppendLine(indentation + "public static bool GetButton(string actionName, bool held)");

        AddStringToAllBodies(Environment.NewLine + indentation + "{" + Environment.NewLine + indentation + $"\t{assetName.ToLower()}.Enable();" + Environment.NewLine);

        //StringBuilder vector2MethodContent = new(indentation + "public static Vector2 GetVector2(string mapName, string actionName)" + Environment.NewLine + indentation + "{" + Environment.NewLine + indentation + $"\t{assetName.ToLower()}.Enable();" + Environment.NewLine);
        //StringBuilder buttonMethodContent = new(indentation + "public static bool GetButton(string mapName, string actionName, bool held)" + Environment.NewLine + indentation + "{" + Environment.NewLine + indentation + $"\t{assetName.ToLower()}.Enable();" + Environment.NewLine);

        SetIndentation(++indentCount);
        
        AddStringToBodyGroup(true, BeginSwitchStatement("mapName"));
        //vector2MethodContent.AppendLine(BeginSwitchStatement("mapName"));
        //buttonMethodContent.AppendLine(BeginSwitchStatement("mapName"));

        AddStringToBodyGroup(false, BeginSwitchStatement("actionName"));

        SetIndentation(++indentCount);
        foreach (InputActionMap map in controlsAsset.actionMaps)
        {
            AddStringToBodyGroup(true, indentation + $"case \"{map.name}\":");
            //vector2MethodContent.AppendLine(indentation + $"case \"{map.name}\":");
            //buttonMethodContent.AppendLine(indentation + $"case \"{map.name}\":");
            SetIndentation(++indentCount);

            AddStringToBodyGroup(true, BeginSwitchStatement("actionName"));
            //vector2MethodContent.AppendLine(BeginSwitchStatement("actionName"));
            //buttonMethodContent.AppendLine(BeginSwitchStatement("actionName"));
            SetIndentation(++indentCount);

            foreach (InputAction action in map.actions)
            {
                string currentAction;
                switch (action.expectedControlType)
                {
                    case "Button":
                        AddStringToBodyType("button", indentation + $"case \"{action.name}\":");
                        //buttonMethodContent.AppendLine(indentation + $"case \"{action.name}\":");

                        SetIndentation(++indentCount);
                        currentAction = $"{assetName.ToLower().Replace(" ", "")}.{map.name.Replace(" ", "")}.{action.name.Replace(" ", "")}";
                        //Debug.Log(currentAction);

                        AddStringToBodyType("button", indentation + $"return held ? {currentAction}.triggered : {currentAction}.ReadValue<bool>();");
                        //buttonMethodContent.AppendLine(indentation + $"return held ? {currentAction}.triggered : {currentAction}.ReadValue<bool>();");
                        SetIndentation(--indentCount);
                        break;
                    case "Vector2":
                        AddStringToBodyType("vector2", indentation + $"case \"{action.name}\":");
                        //vector2MethodContent.AppendLine(indentation + $"case \"{action.name}\":");

                        SetIndentation(++indentCount);
                        currentAction = $"{assetName.ToLower().Replace(" ", "")}.{map.name.Replace(" ", "")}.{action.name.Replace(" ", "")}";

                        AddStringToBodyType("vector2", indentation + $"return {currentAction}.ReadValue<Vector2>();");
                        //vector2MethodContent.AppendLine(indentation + $"return {currentAction}.ReadValue<Vector2>();");
                        SetIndentation(--indentCount);
                        break;
                    default:
                        break;
                }
            }
            SetIndentation(--indentCount);
            AddStringToAllBodies(EndSwitchStatement());
            AddStringToBodyGroup(true, Environment.NewLine + indentation + "break;");

            //vector2MethodContent.AppendLine(EndSwitchStatement() + Environment.NewLine + indentation + "break;");
            //.AppendLine(EndSwitchStatement() + Environment.NewLine + indentation + "break;");
            SetIndentation(--indentCount);
        }
        string className = "InputManager";
        SetIndentation(--indentCount);

        AddStringToBodyGroup(true, EndSwitchStatement());
        //vector2MethodContent.AppendLine(EndSwitchStatement());
        //buttonMethodContent.AppendLine(EndSwitchStatement());

        SetIndentation(--indentCount);

        AddStringToBodyType("vector2", $"\t\t{ assetName.ToLower()}.Disable();" + Environment.NewLine + "\t\treturn new Vector2(0f, 0f);" + Environment.NewLine + indentation + "}");
        AddStringToBodyType("button", $"\t\t{ assetName.ToLower()}.Disable();" + Environment.NewLine + "\t\treturn false;" + Environment.NewLine + indentation + "}");
        //vector2MethodContent.AppendLine($"\t\t{ assetName.ToLower()}.Disable();" + Environment.NewLine + "\t\treturn new Vector2(0f, 0f);" + Environment.NewLine + indentation + "}");
        //buttonMethodContent.AppendLine($"\t\t{ assetName.ToLower()}.Disable();" + Environment.NewLine + "\t\treturn false;" + Environment.NewLine + indentation + "}");

        //classContent.AppendLine(vector2MethodContent.ToString());
        //classContent.AppendLine(buttonMethodContent.ToString());

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
