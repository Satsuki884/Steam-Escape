using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UIElements;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }
    private SaveData saveData;
    [Header("Supabase Config")]
    [SerializeField] private string supabaseUrl = "https://qqozpjerhpealkcekeib.supabase.co";
    [SerializeField] private string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InFxb3pwamVyaHBlYWxrY2VrZWliIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTMwNDEyMTEsImV4cCI6MjA2ODYxNzIxMX0.9rlbWD2SjXLzMRnZFXgUguXMT64WJLCY1HkFBRTpxcA";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Удалить дубликат
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public IEnumerator SubmitScore(int score)
    {
        string username = PlayerPrefs.GetString("player_name", "Unknown");
        string json = JsonUtility.ToJson(new ScoreData(username, score));
        string endpoint = $"{supabaseUrl}/rest/v1/scores";

        var request = new UnityWebRequest(endpoint, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", supabaseKey);
        // request.SetRequestHeader("Authorization", "Bearer " + supabaseKey);
        request.SetRequestHeader("Prefer", "resolution=merge-duplicates");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            Debug.Log("Score submitted or updated!");
        else
            Debug.LogError("SubmitScore error: " + request.error);
    }

    public IEnumerator UpdateScore(int score)
    {
        var username = PlayerPrefs.GetString("player_name", "Unknown");
        username = username.ToUpper();
        string json = JsonUtility.ToJson(new ScoreData(username, score));
        string endpoint = $"{supabaseUrl}/rest/v1/scores?username=eq.{username}";

        var request = new UnityWebRequest(endpoint, "PATCH");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", supabaseKey);
        request.SetRequestHeader("Prefer", "return=representation");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            Debug.Log("Score updated!");
        else
            Debug.LogError("SubmitScore error: " + request.error);
    }



    public IEnumerator GetTopScores(int topCount, TMP_Text targetText)
    {
        string url = $"{supabaseUrl}/rest/v1/scores?select=*&order=score.desc&limit={topCount}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", supabaseKey);
        request.SetRequestHeader("Authorization", "Bearer " + supabaseKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            ScoreData[] scores = JsonHelper.FromJson<ScoreData>(json);

            string display = "";
            for (int i = 0; i < scores.Length; i++)
            {
                display += $"{i + 1}. {scores[i].username} — {scores[i].score}\n";
            }

            targetText.text = display;
        }
        else
        {
            Debug.LogError("GetTopScores error: " + request.error);
            targetText.text = "Error loading records.";
        }
    }

    public IEnumerator GetCurrentPlayerRank(TMP_Text targetText)
    {
        string username = PlayerPrefs.GetString("player_name", "");
        Debug.Log($"Getting rank for username: {username}");
        targetText.text = "Loading...";
        string url = $"{supabaseUrl}/rest/v1/scores?select=username,score&order=score.desc";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", supabaseKey);
        request.SetRequestHeader("Authorization", "Bearer " + supabaseKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            ScoreData[] scores = JsonHelper.FromJson<ScoreData>(json);

            int place = -1;
            int foundScore = 0;

            for (int i = 0; i < scores.Length; i++)
            {
                if (scores[i].username == username)
                {
                    place = i + 1;
                    foundScore = scores[i].score;
                    break;
                }
            }

            if (place > 0)
                targetText.text = $"{place}. ME — {foundScore}";
            else
                targetText.text = "-. - -";
        }
    }

    public IEnumerator CheckUsernameExists(string username, System.Action<bool> callback)
    {
        username = username.ToUpper();
        string url = $"{supabaseUrl}/rest/v1/scores?select=username&username=eq.{username}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", supabaseKey);
        request.SetRequestHeader("Authorization", "Bearer " + supabaseKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;

            if (!string.IsNullOrEmpty(json) && json != "[]")
            {
                callback(true);
            }
            else
            {
                callback(false);
            }
        }
        else
        {
            Debug.LogError("CheckUsernameExists error: " + request.error);
            callback(true); // Лучше заблокировать регистрацию при ошибке
        }
    }
    public void InitializePlayerAccount()
    {
        Debug.Log("Initializing player account...");
        if (PlayerPrefs.HasKey("player_name"))
        {
            Debug.Log("Player name already exists: " + PlayerPrefs.GetString("player_name"));
            return;
        }

        StartCoroutine(GenerateUniqueUsernameAndSubmit());
    }

    private IEnumerator GenerateUniqueUsernameAndSubmit()
    {
        Debug.Log("Generating unique username...");
        string username = "";
        bool exists = true;

        while (exists)
        {
            username = $"Player_{Random.Range(1000, 9999)}";
            yield return CheckUsernameExists(username, result => exists = result);
            Debug.Log($"Generated username: {username}, exists: {exists}");
        }

        Debug.Log($"Unique username generated: {username}");
        // Сохраняем имя
        PlayerPrefs.SetString("player_name", username);
        PlayerPrefs.Save();

        saveData = FindObjectOfType<SaveData>();
        Debug.Log($"SaveData instance found: {saveData != null}");
        if (saveData != null)
        {
            saveData.SetUsername(username);
        }

        // Отправляем на сервер с нулевым счётом
        yield return SubmitScore(0);

        Debug.Log($"Создан новый игрок: {username}");
    }

}

[System.Serializable]
public class ScoreData
{
    public string username;
    public int score;

    public ScoreData(string username, int score)
    {
        this.username = username;
        this.score = score;
    }
}


