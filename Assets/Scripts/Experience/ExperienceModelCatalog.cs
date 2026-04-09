using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ExperienceModelCatalog", menuName = "TraditionalXR/Experience Model Catalog")]
public class ExperienceModelCatalog : ScriptableObject
{
    public List<ExperienceModelEntry> entries = new List<ExperienceModelEntry>();

    public ExperienceModelEntry GetEntryOrDefault(string addressableKey)
    {
        if (string.IsNullOrEmpty(addressableKey) || entries == null)
            return null;
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i] != null && entries[i].addressableKey == addressableKey)
                return entries[i];
        }

        return null;
    }
}
