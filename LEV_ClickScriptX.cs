using FMODSyntax;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LEV_ClickSciptExtended
{
    public static class LEV_ClickScriptX
    {
        private static List<LEV_ClickScriptClaimRect> Claims = new List<LEV_ClickScriptClaimRect>();
        private static LEV_ClickScriptXObserver Observer;
        private static RectTransform BlockingUIPrefab;
        private static Transform Canvas;

        public static void SetObserver(LEV_ClickScriptXObserver observer)
        {
            Observer = observer;
            Observer.StartCoroutine(CreateBlockingUI());

            foreach (LEV_ClickScriptClaimRect claim in Claims)
            {
                claim.SetEnabled(true);
            }
        }

        public static IEnumerator CreateBlockingUI()
        {
            yield return new WaitForSeconds(0.1f);
            
            LEV_PauseMenu pauseMenu = GameObject.FindObjectOfType<LEV_PauseMenu>(true);
            Canvas = pauseMenu.gameObject.transform.parent;

            GameObject copy = GameObject.Instantiate(pauseMenu.gameObject, pauseMenu.transform.parent);
            copy.name = "BlockingUI";
            GameObject.Destroy(copy.GetComponent<LEV_PauseMenu>());
            copy.GetComponent<Image>().color = new Color(0, 0, 0, 0);

            foreach (Transform t in copy.transform)
            {
                GameObject.Destroy(t.gameObject);
            }

            BlockingUIPrefab = copy.GetComponent<RectTransform>();
            BlockingUIPrefab.gameObject.SetActive(false);
        }

        public static void ObserverDestroyed()
        {
            Observer = null;

            foreach (LEV_ClickScriptClaimRect claim in Claims)
            {
                claim.SetEnabled(false);
            }
        }

        public static void AddClaim(LEV_ClickScriptClaimRect claim)
        {
            if(Claims.Contains(claim))
            {
                return;
            }

            Claims.Add(claim);

            if (Observer != null)
            {
                claim.SetEnabled(true);
            }
        }

        public static void RemoveClaim(LEV_ClickScriptClaimRect claim)
        {
            Claims.Remove(claim);

            if (claim.RTransform != null)
            {
                GameObject.Destroy(claim.RTransform.gameObject);
            }
        }

        public static void UpdateClaimUI()
        {
            if (BlockingUIPrefab == null)
            {
                return;
            }

            foreach (LEV_ClickScriptClaimRect claim in Claims)
            {
                if (!claim.Enabled || !claim.Active)
                {
                    continue;
                }

                //Make sure it has a ui attached.
                if (claim.RTransform == null)
                {
                    claim.SetRectTransform(GameObject.Instantiate<RectTransform>(BlockingUIPrefab, Canvas));
                    claim.RTransform.gameObject.SetActive(true);
                }

                Vector2 corner = new Vector2(claim.Area.x / Screen.width, (Screen.height - (claim.Area.y + claim.Area.height)) / Screen.height);
                claim.RTransform.anchorMin = corner;
                claim.RTransform.anchorMax = corner + new Vector2(claim.Area.width / Screen.width, claim.Area.height / Screen.height);
            }
        }

        private static bool IsMouseOverClaim()
        {
            Vector2 mousePosition = Input.mousePosition;
            mousePosition.y = Screen.height - mousePosition.y;

            foreach (LEV_ClickScriptClaimRect claim in Claims)
            {
                if (!claim.Enabled || !claim.Active)
                {
                    continue;
                }

                if (claim.Area.Contains(mousePosition))
                {
                    return true;
                }
            }

            return false;
        }

        //Regular click script, with extra X part.
        public static bool Update(LEV_ClickScript instance)
        {
            //If observer is null, continue the regular code.
            if (Observer == null)
            {
                return true;
            }

            HandleMouseUpEvents(instance);
            UpdateClickState(instance);
            ResetHoverStates(instance);

            #region X
            UpdateClaimUI();

            // Check if the mouse is over any occupied area.
            if (IsMouseOverClaim())
            {
                // If over an occupied area, skip further processing.
                return false;
            }
            #endregion

            // Check if we can process raycasts.
            if (CanProcessRaycasts(instance))
            {
                ProcessRaycasts(instance);
                HandleObjectInteractions(instance);
            }
            else
            {
                // Finally, if no occupied area or raycast hit, handle UI click.
                CheckUIClick(instance);
            }

            return false;
        }
      
        // Helper methods
        private static void HandleMouseUpEvents(LEV_ClickScript instance)
        {
            if (Input.GetMouseButtonUp(0))
            {
                if (!instance.central.gizmos.isGrabbing)
                {
                    if (instance.isClickedGizmo)
                        instance.onReleaseGizmo.Invoke();

                    instance.ResetAllClick();
                }

                if (!instance.central.cam.IsCursorInGameView())
                    instance.isClickedUI = false;
            }
        }

        private static void UpdateClickState(LEV_ClickScript instance)
        {
            instance.isClickedUI_JustDown = Input.GetMouseButtonDown(0);
            instance.isClickedUI_JustUp = Input.GetMouseButtonUp(0);
        }

        private static void ResetHoverStates(LEV_ClickScript instance)
        {
            instance.isHoveringGizmo = false;
            instance.isHoveringBuilding = false;
        }

        private static bool CanProcessRaycasts(LEV_ClickScript instance)
        {
            return !instance.central.cam.GetRotating() &&
                   instance.central.cam.IsCursorInGameView() &&
                   !instance.central.cursor.IsButtonHovered() &&
                   !instance.locked && !instance.waitForClickUp;
        }

        private static void ProcessRaycasts(LEV_ClickScript instance)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, instance.gizmoLayer))
                HandleGizmoHover(instance, hitInfo);
            else if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, instance.buildingLayer))
                HandleBuildingHover(instance, hitInfo);
            else
                instance.DidNotHitAnything();
        }

        private static void HandleGizmoHover(LEV_ClickScript instance, RaycastHit hitInfo)
        {
            if (hitInfo.transform.CompareTag("MovementGizmo"))
            {
                instance.central.gizmos.HoverGizmo(hitInfo.transform);
                instance.isHoveringGizmo = true;
            }
            else
            {
                instance.DidNotHitGizmo();
                instance.DidNotHitBuilding();
            }
        }

        private static void HandleBuildingHover(LEV_ClickScript instance, RaycastHit hitInfo)
        {
            if (hitInfo.transform.parent != null && hitInfo.transform.parent.CompareTag("BuildingBlock"))
            {
                instance.isHoveringBuilding = true;
                instance.hoveredBuilding = hitInfo.transform.parent;
            }
            else
            {
                instance.DidNotHitBuilding();
            }
            instance.DidNotHitGizmo();
        }

        private static void HandleObjectInteractions(LEV_ClickScript instance)
        {
            if (!Input.GetMouseButtonDown(0))
                return;

            if (instance.isHoveringGizmo)
                HandleGizmoClick(instance);
            else if (instance.isHoveringBuilding)
                HandleBuildingClick(instance);
            else
                HandleNothingClick(instance);
        }

        private static void HandleGizmoClick(LEV_ClickScript instance)
        {
            instance.locked = true;
            instance.isClickedGizmo = true;
            instance.onClickGizmo.Invoke();
            AudioEvents.MenuClick.Play(null);
        }

        private static void HandleBuildingClick(LEV_ClickScript instance)
        {
            instance.isClickedBuilding = true;
            instance.onClickBuilding.Invoke();
            AudioEvents.MenuClick.Play(null);
        }

        private static void HandleNothingClick(LEV_ClickScript instance)
        {
            instance.isClickedNothing = true;
            instance.onClickNothing.Invoke();
        }

        private static void CheckUIClick(LEV_ClickScript instance)
        {
            if (instance.central.cam.GetRotating() || instance.central.cam.IsCursorInGameView() || !Input.GetMouseButtonDown(0))
                return;

            instance.isClickedUI = true;
            // Additional UI-specific logic can be added here if needed
        }
    }

    [HarmonyPatch(typeof(LEV_ClickScript), "Start")]
    public class LEVClickScriptStartPatch
    {
        public static void Postfix(LEV_PauseMenu __instance)
        {
            if (__instance.gameObject.GetComponent<LEV_ClickScriptXObserver>() == null)
            {
                LEV_ClickScriptX.SetObserver(__instance.gameObject.AddComponent<LEV_ClickScriptXObserver>());
            }
        }
    }

    [HarmonyPatch(typeof(LEV_ClickScript), "Update")]
    public class LEVClickScriptUpdatePatch
    {
        public static bool Prefix(LEV_ClickScript __instance)
        {
            return LEV_ClickScriptX.Update(__instance);
        }
    }
}
