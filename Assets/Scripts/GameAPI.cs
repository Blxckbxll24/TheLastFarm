using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GameAPI : MonoBehaviour
{
    private string loginUrl = "https://backend-thelastfarm-latest.onrender.com/auth/login";
    private string progressUrl = "https://backend-thelastfarm-latest.onrender.com/game/updateProgress";

    // =============================
    // 1. LOGIN DESDE UNITY
    // =============================
    public IEnumerator LoginUnity(string email, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("email", email);
        form.AddField("password", password);

        UnityWebRequest req = UnityWebRequest.Post(loginUrl, form);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Login Response: " + req.downloadHandler.text);

            LoginResponse res = JsonUtility.FromJson<LoginResponse>(req.downloadHandler.text);

            PlayerPrefs.SetString("uid", res.user.id);
            PlayerPrefs.SetString("token", res.token);

            Debug.Log("Unity Login OK → UID: " + res.user.id);
        }
        else
        {
            Debug.LogError("Login error: " + req.error);
        }
    }

    // =============================
    // 2. ENVIAR PROGRESO (zanahorias)
    // =============================
    public IEnumerator SendCarrots(int amount)
    {
        string userId = PlayerPrefs.GetString("uid");

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("❌ No existe UID, primero haz login");
            yield break;
        }

        CarrotUpdate data = new CarrotUpdate
        {
            userId = userId,
            carrots = amount
        };

        string json = JsonUtility.ToJson(data);

        UnityWebRequest request = new UnityWebRequest(progressUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✔️ Zanahorias enviadas correctamente");
        }
        else
        {
            Debug.LogError("Error enviando zanahorias: " + request.error);
        }
    }
}

[System.Serializable]
public class LoginResponse
{
    public string message;
    public string token;
    public UserData user;
}

[System.Serializable]
public class UserData
{
    public string id;
    public string email;
    public string displayName;
    public string photoURL;
}

[System.Serializable]
public class CarrotUpdate
{
    public string userId;
    public int carrots;
}
