using System.Collections.Generic;
using UnityEngine;
using Architecture;

public enum MessageCategory
{
    Undefined, State, Statistic, Save, Rig, Progress
}

[ExecuteAlways]
public class DebugController : MonoBehaviour, IService
{
    [SerializeField] private bool debugEnabled = true;
    public bool DebugEnabled { get => debugEnabled; }

    [SerializeField] private GameObject overlay;

    [SerializeField] private bool hideStateLogs;
    [SerializeField] private bool hideStatsLogs;
    [SerializeField] private bool hideSaveLogs;
    [SerializeField] private bool hideRigsLogs;
    [SerializeField] private bool hideProgressLogs;
   
    // Define a dictionary to map SystemEntry to color tags
    Dictionary<MessageCategory, string> colorTags = new Dictionary<MessageCategory, string>
    {
        { MessageCategory.Undefined, "" }, // Default color is empty
        { MessageCategory.State, "<color=#76448A>" },
        { MessageCategory.Statistic, "<color=#2E86C1>" },
        { MessageCategory.Save, "<color=#17A589>" },
        { MessageCategory.Rig, "<color=#1ABC9C>" },
        { MessageCategory.Progress, "<color=#F39C12>" },
    };
    
    public void Initialize()
    {
        // Initialization logic here
    }

    void OnValidate()
    {
        overlay.SetActive(debugEnabled);
    }

    public void Log(string message, MessageCategory entry)
    {
        switch (entry)
        {
            case MessageCategory.State: if (hideStateLogs) return; break;
            case MessageCategory.Statistic: if (hideStatsLogs) return; break;
            case MessageCategory.Save: if (hideSaveLogs) return; break;
            case MessageCategory.Rig: if (hideRigsLogs) return; break;
            case MessageCategory.Progress: if (hideProgressLogs) return; break;
        }
        // Get the color tag for the specified entry
        string colorTag = colorTags[entry];

        // Split the message into lines
        string[] lines = message.Split('\n');

        // Check if there are more than three lines
        if (lines.Length > 3)
        {
            // Concatenate the color tag with each line separately
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = colorTag + lines[i] + "</color>";
            }
            message = string.Join("\n", lines);

        }
        else
        {
            // If there are three or fewer lines, concatenate the color tag with each line separately
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i];
            }
            message = string.Join("\n", lines);
            message = colorTag + message + "</color>";
        }
        // Log the formatted message
        Debug.Log(message);
    }
}
