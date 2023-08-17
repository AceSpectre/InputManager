using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using System;
using System.IO;
using System.Text;

[CreateAssetMenu(fileName = "InputManagerGenerator", menuName = "Manager Generator")]
public class ManagerGenerator : ScriptableObject
{
    public InputActionAsset controlsAsset;
    private string indentation;
    private int indentCount = 0;

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
        indentCount = 0;
        string assetName = controlsAsset.name;
        StringBuilder classContent = new(@"using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
");
        classContent.AppendLine("public static class InputManager : object" + Environment.NewLine + "{");
        Debug.Log(indentCount);

        SetIndentation(++indentCount);

        Debug.Log(indentCount);

        classContent.AppendLine(indentation + $"public static {assetName} {assetName.ToLower()} = new();" + Environment.NewLine);


        StringBuilder vector2MethodContent = new(indentation + "public static Vector2 GetVector2(string mapName, string actionName)" + Environment.NewLine + indentation + "{" + Environment.NewLine + indentation + $"\t{assetName.ToLower()}.Enable();" + Environment.NewLine);
        StringBuilder buttonMethodContent = new(indentation + "public static bool GetButton(string mapName, string actionName, bool held)" + Environment.NewLine + indentation + "{" + Environment.NewLine + indentation + $"\t{assetName.ToLower()}.Enable();" + Environment.NewLine);

        SetIndentation(++indentCount);

        vector2MethodContent.AppendLine(BeginSwitchStatement("mapName"));
        buttonMethodContent.AppendLine(BeginSwitchStatement("mapName"));

        SetIndentation(++indentCount);
        foreach (InputActionMap map in controlsAsset.actionMaps)
        {
            vector2MethodContent.AppendLine(indentation + $"case \"{map.name}\":");
            buttonMethodContent.AppendLine(indentation + $"case \"{map.name}\":");
            SetIndentation(++indentCount);

            vector2MethodContent.AppendLine(BeginSwitchStatement("actionName"));
            buttonMethodContent.AppendLine(BeginSwitchStatement("actionName"));
            SetIndentation(++indentCount);

            foreach (InputAction action in map.actions)
            {
                string currentAction;
                switch (action.expectedControlType)
                {
                    case "Button":
                        buttonMethodContent.AppendLine(indentation + $"case \"{action.name}\":");
                        SetIndentation(++indentCount);
                        currentAction = $"{assetName.ToLower().Replace(" ", "")}.{map.name.Replace(" ", "")}.{action.name.Replace(" ", "")}";
                        Debug.Log(currentAction);
                        buttonMethodContent.AppendLine(indentation + $"return held ? {currentAction}.triggered : {currentAction}.ReadValue<bool>();");
                        SetIndentation(--indentCount);
                        break;
                    case "Vector2":
                        vector2MethodContent.AppendLine(indentation + $"case \"{action.name}\":");
                        SetIndentation(++indentCount);
                        currentAction = $"{assetName.ToLower().Replace(" ", "")}.{map.name.Replace(" ", "")}.{action.name.Replace(" ", "")}";
                        vector2MethodContent.AppendLine(indentation + $"return {currentAction}.ReadValue<Vector2>();");
                        SetIndentation(--indentCount);
                        break;
                    default:
                        break;
                }
            }
            SetIndentation(--indentCount);
            vector2MethodContent.AppendLine(EndSwitchStatement() + Environment.NewLine + indentation + "break;");
            buttonMethodContent.AppendLine(EndSwitchStatement() + Environment.NewLine + indentation + "break;");
            SetIndentation(--indentCount);
        }
        string className = "InputManager";
        SetIndentation(--indentCount);

        vector2MethodContent.AppendLine(EndSwitchStatement());
        buttonMethodContent.AppendLine(EndSwitchStatement());

        SetIndentation(--indentCount);

        vector2MethodContent.AppendLine($"\t\t{ assetName.ToLower()}.Disable();" + Environment.NewLine + "\t\treturn new Vector2(0f, 0f);" + Environment.NewLine + indentation + "}");
        buttonMethodContent.AppendLine($"\t\t{ assetName.ToLower()}.Disable();" + Environment.NewLine + "\t\treturn false;" + Environment.NewLine + indentation + "}");

        classContent.AppendLine(vector2MethodContent.ToString());
        classContent.AppendLine(buttonMethodContent.ToString());
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
