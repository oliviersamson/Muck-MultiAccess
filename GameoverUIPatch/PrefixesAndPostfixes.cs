﻿using HarmonyLib;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BetterMultiplayer.GameoverUIPatch
{
    static class PrefixesAndPostfixes
    {
        [HarmonyPatch(typeof(GameoverUI), "Awake")]
        [HarmonyPostfix]
        static void PatchStart(GameoverUI __instance)
        {
            NetworkController.Instance.loading = false;

            GameObject header = GameObject.Find("Header");
            GameObject menuButton = GameObject.Find("MenuButton");

            Button returnToLobbyButton = UnityEngine.Object.Instantiate(menuButton, header.transform).GetComponent<Button>();
            returnToLobbyButton.name = "LobbyButton";
            returnToLobbyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Lobby";

            RectTransform rectTransform = returnToLobbyButton.GetComponent<RectTransform>();
            rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 25f, rectTransform.sizeDelta.x + 20f);

            for (int i = 0; i < returnToLobbyButton.onClick.GetPersistentEventCount(); i++)
            {
                returnToLobbyButton.onClick.SetPersistentListenerState(i, UnityEventCallState.Off);
            }

            GameObject scroll = GameObject.Find("Scroll");

            RectTransform scrollRectTransform = scroll.GetComponent<RectTransform>();
            scrollRectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 200f, scrollRectTransform.sizeDelta.x);

            RectTransform textRectTransform = __instance.nameText.GetComponent<RectTransform>();
            textRectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 165f, textRectTransform.sizeDelta.x);

            if (SteamManager.Instance.currentLobby.IsOwnedBy(SteamManager.Instance.PlayerSteamId))
            {
                returnToLobbyButton.onClick.AddListener(
                    () => {

                        if (GameManager.instance.GetPlayersInLobby() == 1)
                        {
                            SteamManager.Instance.SetGameStateToLobby();
                            return;
                        }

                        returnToLobbyButton.interactable = false;

                        Transform obj = Transform.Instantiate(Plugin.Overlay, Plugin.Overlay.position, Plugin.Overlay.rotation);

                        IEnumerator coroutine = SteamManager.Instance.LobbyCountdown();

                        SteamManager.Instance.StartCoroutine(coroutine);

                        obj.GetChild(0).GetChild(0).GetChild(1).GetComponent<Button>().onClick.AddListener(
                            () => {
                                SteamManager.Instance.StopCoroutine(coroutine);

                                returnToLobbyButton.interactable = true;

                                GameObject.Destroy(obj.gameObject);
                            });
                    });
            }
            else
            {
                returnToLobbyButton.interactable = false;
            }
        }
    }
}
