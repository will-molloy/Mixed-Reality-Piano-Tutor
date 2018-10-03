using System;

namespace UnityEditor.PostProcessing
{
    public class PostProcessingModelEditorAttribute : Attribute
    {
        public readonly bool alwaysEnabled;
        public readonly Type type;

        public PostProcessingModelEditorAttribute(Type type, bool alwaysEnabled = false)
        {
            this.type = type;
            this.alwaysEnabled = alwaysEnabled;
        }
    }
}