using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.InputSystem;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public SaveData CurrentData { get; private set; }

    private string Path => System.IO.Path.Combine(Application.persistentDataPath, "saveData.json");

    private float autoSaveInterval = 30f; // 자동 저장 간격
    private float autoSaveTimer = 0f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        string dir = System.IO.Path.GetDirectoryName(Path);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        Load();
    }

    private void Update()
    {
        autoSaveTimer += Time.unscaledDeltaTime;
        if (autoSaveTimer >= autoSaveInterval)
        {
            Save();
            autoSaveTimer = 0f;
        }
    }

    public void Save()
    {
        if (CurrentData == null)
            return;

        if (InputSystem.actions != null)
            CurrentData.keyBindings = InputSystem.actions.SaveBindingOverridesAsJson();

        string json = JsonConvert.SerializeObject(CurrentData, Formatting.Indented);
        File.WriteAllText(Path, json);
    }

    public void Load()
    {
        if (!File.Exists(Path))
        {
            CurrentData = new SaveData(); // 새 게임 데이터 생성
            return;
        }

        string json = File.ReadAllText(Path);
        CurrentData = JsonConvert.DeserializeObject<SaveData>(json) ?? new SaveData();

        if (InputSystem.actions != null && !string.IsNullOrEmpty(CurrentData.keyBindings))
            InputSystem.actions.LoadBindingOverridesFromJson(CurrentData.keyBindings);
    }

    public void ResetData()
    {
        if (InputSystem.actions != null)
            InputSystem.actions.RemoveAllBindingOverrides();
        CurrentData = new SaveData();
        Save();
    }

    private void OnApplicationQuit()
    {
        Save();
    }
}
