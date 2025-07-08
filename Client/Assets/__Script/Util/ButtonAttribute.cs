using UnityEngine;

public class ButtonAttribute : PropertyAttribute
{
    public string MethodName;

    public ButtonAttribute(string methodName)
    {
        MethodName = methodName;
    }
}