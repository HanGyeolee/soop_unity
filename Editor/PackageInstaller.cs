using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace SoopExtension.Editor
{
    [InitializeOnLoad]
    public static class PackageInstaller
    {
        private static AddRequest addRequest;
        private static ListRequest listRequest;

        static PackageInstaller()
        {
            CheckAndInstallNativeWebSocket();
        }

        private static void CheckAndInstallNativeWebSocket()
        {
            listRequest = Client.List();
            EditorApplication.update += CheckListProgress;
        }

        private static void CheckListProgress()
        {
            if (listRequest.IsCompleted)
            {
                EditorApplication.update -= CheckListProgress;

                if (listRequest.Status == StatusCode.Success)
                {
                    bool hasNativeWebSocket = false;
                    foreach (var package in listRequest.Result)
                    {
                        if (package.name == "com.endel.nativewebsocket")
                        {
                            hasNativeWebSocket = true;
                            break;
                        }
                    }

                    if (!hasNativeWebSocket)
                    {
                        Debug.Log("SOOP Extension: Installing NativeWebSocket dependency...");
                        addRequest = Client.Add("https://github.com/endel/NativeWebSocket.git#upm");
                        EditorApplication.update += CheckAddProgress;
                    }
                    else
                    {
                        Debug.Log("SOOP Extension: NativeWebSocket dependency already installed.");
                    }
                }
                else
                {
                    Debug.LogError($"SOOP Extension: Failed to check packages: {listRequest.Error.message}");
                }
            }
        }

        private static void CheckAddProgress()
        {
            if (addRequest.IsCompleted)
            {
                EditorApplication.update -= CheckAddProgress;

                if (addRequest.Status == StatusCode.Success)
                {
                    Debug.Log("SOOP Extension: NativeWebSocket successfully installed!");
                }
                else
                {
                    Debug.LogError($"SOOP Extension: Failed to install NativeWebSocket: {addRequest.Error.message}");
                    EditorUtility.DisplayDialog(
                        "SOOP Extension Dependency Missing",
                        "NativeWebSocket package is required but could not be installed automatically.\n\n" +
                        "Please install it manually:\n" +
                        "1. Open Package Manager\n" +
                        "2. Click '+' > Add package from git URL\n" +
                        "3. Enter: https://github.com/endel/NativeWebSocket.git#upm",
                        "OK"
                    );
                }
            }
        }
    }
}
