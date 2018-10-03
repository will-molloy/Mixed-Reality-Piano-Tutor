using UnityEditor;

internal class BuildScript
{
    private static void CreateCSProj()
    {
        EditorApplication.ExecuteMenuItem("Assets/Open C# Project");
    }
}