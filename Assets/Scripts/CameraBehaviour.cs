﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts
{
    public class CameraBehaviour : MonoBehaviour
    {
        public GameObject Dwarves;
        
        private Camera cam;
        private float minDownScroll;
        private bool rotating;
        private Quaternion lockedCameraRotation;
        private Vector3 initialRotationPosition;
        private Ray ray;
        private RaycastHit hit;
        private Collider lockedObject;

        void Start()
        {
            cam = GetComponent<Camera>();
            minDownScroll = 0;
            rotating = false;
            lockedCameraRotation = cam.transform.rotation;
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                rotating = true;
                initialRotationPosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(1))
                rotating = false;

            if (rotating)
                RotateCamera();
            else
                MoveCamera();

            if (Input.GetMouseButtonDown(0))
            {
                Ray mouseRay = cam.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(mouseRay, out hit) && hit.collider.CompareTag("Agent"))
                    lockedObject = hit.collider; // on clicking on an agent, we set it as the camera lock
                else if (lockedObject && !EventSystem.current.IsPointerOverGameObject())
                    lockedObject = null; // if the click is not on an agent and not on an UI element, the camera unlocks
            }
            if (lockedObject)
                LockCamera(lockedObject);
            else
                DeactivateSpheres();
        }

        private void RotateCamera()
        {
            cam.transform.Rotate(0, -(initialRotationPosition.x - Input.mousePosition.x)/100, 0, Space.World);
        }

        private void MoveCamera()
        {
            if (Physics.Raycast(cam.transform.position, Vector3.down, out hit, 10)) // casts a ray from the camera towards the ground, at a max distance of 10 units and return the caracteristics of the ray in "hit". True if the ray hits
            {
                if (hit.distance < 10) // if the collided object is too close...
                {
                    cam.transform.position = new Vector3
                        (cam.transform.position.x,
                        cam.transform.position.y + (10 - hit.distance), // ... the camera position is set to 10 unit above it.
                        cam.transform.position.z);
                }
                minDownScroll = cam.transform.position.y;
            }
            else
                minDownScroll = 0;

            // moving the camera back and forth
            if (Input.mousePosition.y < 0)
                cam.transform.Translate(0, -(2f / 3f), -(1f / 3f), Space.Self);
            else if (Input.mousePosition.y > Screen.height)
                cam.transform.Translate(0, (2f / 3f), (1f / 3f), Space.Self);

            // moving the camera right and left
            if (Input.mousePosition.x < 0)
                cam.transform.Translate(Vector3.left, Space.Self);
            else if (Input.mousePosition.x > Screen.width)
                cam.transform.Translate(Vector3.right, Space.Self);

            // moving the camera up and down (if the mouse isn't above UI element)
            if (!EventSystem.current.IsPointerOverGameObject())
                cam.transform.position += new Vector3(0, -(Input.mouseScrollDelta.y * 2), 0);

            // we clamp the camera position above the "world", and limit the down scroll to 10 units from the ground
            cam.transform.position = new Vector3
            (
                Mathf.Clamp(cam.transform.position.x, 0, 500),
                Mathf.Clamp(cam.transform.position.y, minDownScroll, 250),
                Mathf.Clamp(cam.transform.position.z, 0, 500)
            );
        }

        public void LockCamera(Collider agent)
        {
            lockedObject = agent;
            cam.transform.rotation = lockedCameraRotation;
            cam.transform.position = new Vector3
                (agent.transform.position.x,
                cam.transform.position.y,
                agent.transform.position.z - 0.7f * (cam.transform.position.y - agent.transform.position.y)); // centers the camera on the target
            DeactivateSpheres();
            agent.gameObject.transform.FindChild("Sphere").gameObject.SetActive(true);
        }

        private void DeactivateSpheres()
        {
            for (int i = 0; i < Dwarves.transform.childCount; i++)
            {
                if (Dwarves.transform.GetChild(i).FindChild("Sphere").gameObject.activeSelf)
                    Dwarves.transform.GetChild(i).FindChild("Sphere").gameObject.SetActive(false);
            }
        }
    }
}
