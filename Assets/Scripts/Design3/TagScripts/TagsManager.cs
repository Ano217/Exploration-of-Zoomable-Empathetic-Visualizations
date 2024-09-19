using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CustomTags
{
    BasculeButton,
    Exit360Button,
    Grabbable
}


public class TagsManager : MonoBehaviour
{
    public List<CustomTags> tags = new List<CustomTags>();

    public void addTag(CustomTags tag)
    {
        tags.Add(tag);
    }

    public void removeTag(CustomTags tag)
    {
        tags.Remove(tag);
    }

    public bool hasTag(CustomTags tag)
    {
        return tags.Contains(tag);
    }
}
