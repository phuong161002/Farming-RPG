using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneControllerManager : SingletonMonobehavior<SceneControllerManager>
{
    private bool isFading;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private CanvasGroup faderCanvasGroup = null;
    [SerializeField] private Image faderImage = null;
    public SceneName startSceneName;

    private IEnumerator Start()
    {
        faderImage.color = new Color(0f, 0f, 0f, 1f);
        faderCanvasGroup.alpha = 1f;

        yield return StartCoroutine(LoadSceneAndSetActive(startSceneName.ToString()));

        EventHandler.CallAfterSceneLoadEvent();

        SaveLoadManager.Instance.RestoreCurrentSceneData();

        StartCoroutine(Fade(0f));
    }

    public void FadeAndLoadScene(string sceneName, Vector3 spawnPosition)
    {
        if(!isFading)
        {
            StartCoroutine(FadeAndSwitchScene(sceneName, spawnPosition));
        }
    }

    private IEnumerator FadeAndSwitchScene(string sceneName, Vector3 spawnPosition)
    {
        EventHandler.CallBeforeSceneUnloadFadeOutEvent();

        // Start fading to black and wait for it to finish before continuing
        yield return StartCoroutine(Fade(1f));

        // Store current scene data
        SaveLoadManager.Instance.StoreCurrentSceneData();

        // Set player position
        Player.Instance.gameObject.transform.position = spawnPosition;

        // Call before scene unload event
        EventHandler.CallBeforeSceneUnloadEvent();
        
        // Unload the current active scene
        yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);

        // Start loading the given scene and wait for it to finish
        yield return StartCoroutine(LoadSceneAndSetActive(sceneName));

        // Call after scene load event
        EventHandler.CallAfterSceneLoadEvent();

        // Restore new scene data
        SaveLoadManager.Instance.RestoreCurrentSceneData();

        // Start fading back in and wait for it to finish before exiting the function
        yield return StartCoroutine(Fade(0f));

        // Call after scene load fade in event
        EventHandler.CallAfterSceneLoadFadeInEvent();
    }

    private IEnumerator Fade(float finalAlpha)
    {
        // Set the fading flag to true so the FadeAndSwitchScenes coroutine won't be called again 
        isFading = true;

        // Make sure the CanvasGroup blocks raycasts into the scene so no more input can be accepted
        faderCanvasGroup.blocksRaycasts = true;
        Player.Instance.DisablePlayerInputAndResetMovement();

        float fadeSpeed = Mathf.Abs(faderCanvasGroup.alpha - finalAlpha) / fadeDuration;

        while(!Mathf.Approximately(faderCanvasGroup.alpha, finalAlpha))
        {
            // Move the alpha towards it's target alpha
            faderCanvasGroup.alpha = Mathf.MoveTowards(faderCanvasGroup.alpha, finalAlpha, fadeSpeed * Time.deltaTime);

            // Wait for a frame then continue
            yield return null;
        }

        // Set the flag to false since the fade has finished
        isFading = false;

        // Stop the CanvasGroup from blocking raycasts so input is no longer ignored
        faderCanvasGroup.blocksRaycasts = false;
        Player.Instance.EnablePlayerInput();
    }
    private IEnumerator LoadSceneAndSetActive(string sceneName)
    {
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        Scene newlyLoadScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

        SceneManager.SetActiveScene(newlyLoadScene);
    }
}
