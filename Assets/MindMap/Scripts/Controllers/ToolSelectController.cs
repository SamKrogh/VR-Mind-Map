﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolSelectController : MonoBehaviour {

    public TheWorld world;

    public EquipmentSlot model;
    public EquipmentSlotView nextView;
    public EquipmentSlotView previousView;
    public GameObject controlsPrompt;

    public InputController inputController;

    public Material selectionMaterial;

    // Use this for initialization
    void Start () {
        if (world == null)
            world = GetComponentInParent<TheWorld>();
        UpdateView();
    }

    private void OnEnable()
    {
        inputController.onSelect.AddListener(OnSelectButton);
        inputController.onActivate.AddListener(OnInteract);
        inputController.onMoved.AddListener(OnMoved);
    }

    private void OnDisable()
    {
        inputController.onSelect.RemoveListener(OnSelectButton);
        inputController.onActivate.RemoveListener(OnInteract);
        inputController.onMoved.RemoveListener(OnMoved);
    }

    // Update is called once per frame
    void Update () {
        bool isTouched = inputController.IsTouched;
        previousView.isHidden = !isTouched;
        nextView.isHidden = !isTouched;
        controlsPrompt.SetActive(isTouched);

        SceneNode selection = world.GetSelectionFor<SceneNode>(inputController.HandObject);
        if (selection != null)
        {
            MeshFilter filter = selection.GetComponentInChildren<MeshFilter>();
            Graphics.DrawMesh(filter.mesh, selection.CombinedParentXform, selectionMaterial, LayerMask.NameToLayer("TransparentFX"));
        }
    }

    void OnSelectButton(InputControlState state, bool isPress)
    {
        if (isPress)
        {
            Debug.Log("ToolSelect");
            Vector2 axis = state.AxisPosition;
            const float THRESHOLD = 0.4f;
            if (Mathf.Abs(axis.x) > THRESHOLD)
                if (model != null && model.ActiveTool != null && model.ActiveTool.isToggleAction) // if the active tool has a toggleable interaction/state
                    Interact(false); // stop interaction when changing tools
            if (axis.x < -THRESHOLD) // previous tool
                model.ActiveSlot -= 1;
            else if (axis.x > THRESHOLD) // next tool
                model.ActiveSlot += 1;
            UpdateView();
        }
    }

    private void OnMoved(InputControlState ics)
    {
            world.UpdateSelection(ics.HandObject);
    }

    void OnInteract(InputControlState state, bool isPress)
    {
        Debug.Log("OnInteract with " + transform.parent.name + " " + isPress + " from " + state.HandObject.parent.name);
        Interact(isPress);
    }

    void Interact(bool isOn)
    {
        Debug.Log("Interact with " + transform.parent.name + " " + isOn);
        if (model.ActiveTool == null)
            return;
        if (isOn)
            model.ActiveTool.onInteraction.Invoke(model.ActiveTool.Tip);
        else
            model.ActiveTool.offInteraction.Invoke(model.ActiveTool.Tip);
    }

    void UpdateView()
    {
        previousView.targetTool = model[model.ActiveSlot - 1];
        nextView.targetTool = model[model.ActiveSlot + 1];
        inputController.handObject = model.ActiveTool.Tip;
    }
}
