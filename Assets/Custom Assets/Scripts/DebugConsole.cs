using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugConsole : MonoBehaviour {

    public GameObject debugGui = null;
    public Vector3 defaultGuiPosition = new Vector3(0.01f, 0.98f, 0f);
    public Vector3 defaultGuiScale = new Vector3(0.5f, 0.5f, 1f);
    public Color normal = Color.green;
    public Color warning = Color.yellow;
    public Color error = Color.red;
    public int maxMessages = 30;
    public float lineSpacing = 0.02f;
    public List<string> messages = new List<string>();
    public List<GameObject> guis = new List<GameObject>();
    public List<string> colors = new List<string>();
    public bool draggable = true;
    public bool visible = true;
    public bool pixelCorrect = false;

    public static bool isVisible
    {
        get
        {
            return instance.visible;
        }
        set
        {
            instance.visible = value;
            if (value)
            {
                instance.Display();
            }
            else
            {
                instance.ClearScreen();
            }
        }
    }

    static DebugConsole s_Instance = null;
    public static DebugConsole instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = FindObjectOfType<DebugConsole>();
                if (s_Instance == null)
                {
                    GameObject console = new GameObject();
                    DebugConsole dc = console.AddComponent<DebugConsole>();
                    console.name = "DebugConsoleController";
                    s_Instance = dc;
                    dc.InitGuis();
                }
            }
            return s_Instance;
        }
    }

    void Awake()
    {
        s_Instance = this;
        InitGuis();
    }

    protected bool guisCreated = false;
    protected float screenHeight = -1f;

    public void InitGuis()
    {
        float usedLineSpacing = lineSpacing;
        screenHeight = Screen.height;
        if (pixelCorrect)
        {
            usedLineSpacing /= screenHeight;
        }

        Vector3 position = debugGui.transform.position;
        if (!guisCreated)
        {
            if (debugGui == null)
            {
                debugGui = new GameObject();
                debugGui.AddComponent<GUIText>();
                debugGui.name = "DebugGUI(0)";
                debugGui.transform.position = defaultGuiPosition;
                debugGui.transform.localScale = defaultGuiScale;
            }

            guis.Add(debugGui);
            int x = 1;

            while (x < maxMessages)
            {
                position.y -= usedLineSpacing;
                GameObject clone = Instantiate(debugGui, position, transform.rotation);
                clone.name = string.Format("DebugGUI({0})", x);
                guis.Add(clone);
                position = clone.transform.position;
                x++;
            }

            foreach (GameObject gui in guis)
            {
                gui.transform.SetParent(debugGui.transform);
            }
            guisCreated = true;
        }
        else
        {
            foreach (GameObject gui in guis)
            {
                position.y -= usedLineSpacing;
                gui.transform.position = position;
            }
        }
    }

    bool connectedToMouse = false;

    void Update()
    {
        if (visible && screenHeight != Screen.height)
        {
            InitGuis();
        }

        if (draggable)
        {
            if (Input.GetMouseButton(0))
            {
                if (!connectedToMouse && debugGui.GetComponent<GUIText>().HitTest(Input.mousePosition))
                {
                    connectedToMouse = true;
                }
                else if (connectedToMouse)
                {
                    connectedToMouse = false;
                }
            }

            if (connectedToMouse)
            {
                debugGui.transform.position = Input.mousePosition / Screen.height;
            }
        }
    }

    public static void Log(string message, string color = "normal")
    {
        instance.AddMessage(message, color);
    }

    public static void Clear()
    {
        instance.ClearMessages();
    }

    public void AddMessage(string message, string color = "normal")
    {
        messages.Add(message);
        colors.Add(color);
        Display();
    }

    public void ClearMessages()
    {
        messages.Clear();
        colors.Clear();
        ClearScreen();
    }

    void ClearScreen()
    {
        if (guis.Count >= maxMessages)
        {
            foreach (GameObject gui in guis)
            {
                gui.GetComponent<GUIText>().text = "";
            }
        }
    }

    void Prune()
    {
        if (messages.Count > maxMessages)
        {
            int diff = messages.Count > 0 ? messages.Count - maxMessages : 0;
            messages.RemoveRange(0, diff);
            colors.RemoveRange(0, diff);
        }
    }

    void Display()
    {
        if (visible)
        {
            if (messages.Count > maxMessages)
            {
                Prune();
            }

            if (guis.Count >= maxMessages)
            {
                int x = 0;
                while (x < messages.Count)
                {
                    GUIText text = guis[x].GetComponent<GUIText>();

                    switch (colors[x])
                    {
                        case "normal":
                            text.material.color = normal;
                            break;

                        case "warning":
                            text.material.color = warning;
                            break;

                        case "error":
                            text.material.color = error;
                            break;
                    }

                    text.text = messages[x];
                    x++;
                }
            }
        }
        else
        {
            ClearScreen();
        }
    }
}
