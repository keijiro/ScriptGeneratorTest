using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;

namespace AICommand
{
    static class OpenAIUtil
    {
        static string CreateChatRequestBody(string prompt)
        {
            var msg = new OpenAI.RequestMessage
            {
                role = "user",
                content = prompt
            };

            var req = new OpenAI.Request
            {
                model = "gpt-3.5-turbo",
                messages = new[] { msg }
            };

            return JsonUtility.ToJson(req);
        }

        public static string InvokeChat(string prompt)
        {
            var settings = AICommandSettings.instance;

            // Create JSON payload
            var jsonBody = CreateChatRequestBody(prompt);
            var payload = System.Text.Encoding.UTF8.GetBytes(jsonBody);

            // POST request
            var post = new UnityWebRequest(OpenAI.Api.Url, "POST")
            {
                uploadHandler = new UploadHandlerRaw(payload),
                downloadHandler = new DownloadHandlerBuffer()
            };
            post.SetRequestHeader("Content-Type", "application/json");
            post.SetRequestHeader("Authorization", "Bearer " + settings.apiKey);
            post.timeout = settings.timeout;

            // Start request
            var operation = post.SendWebRequest();

            // Progress bar
            while (!operation.isDone)
            {
                EditorUtility.DisplayProgressBar("AI Command", "Generating...", operation.progress);
            }
            EditorUtility.ClearProgressBar();

            // Check for errors
            if (post.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {post.error}\nResponse: {post.downloadHandler.text}");
                return "Error: Unable to complete the request.";
            }

            // Parse the response
            try
            {
                var json = post.downloadHandler.text;
                var data = JsonUtility.FromJson<OpenAI.Response>(json);
                return data.choices[0].message.content;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error parsing response: {ex.Message}\nResponse: {post.downloadHandler.text}");
                return "Error: Failed to parse the response.";
            }
        }
    }
}
