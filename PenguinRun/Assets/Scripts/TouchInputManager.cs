using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class TouchInputManager : MonoBehaviour
{
    private PlayerInput m_PlayerActionController;
    public Action tap;
    private void OnEnable()
    {
        m_PlayerActionController = new PlayerInput();
        m_PlayerActionController.Enable();
        InitialiseControls();
    }

    private void OnDisable()
    {
        m_PlayerActionController.Action.Jump.started -= Touch;
        //m_PlayerActionController.Action.Jump.performed -= Touch;
        //m_PlayerActionController.Action.Jump.canceled -= Touch;
    }

    private void InitialiseControls()
    {
        m_PlayerActionController.Action.Jump.started += Touch;
        //m_PlayerActionController.Action.Jump.performed += Touch;
       // m_PlayerActionController.Action.Jump.canceled += Touch;
    }

    private void Touch(InputAction.CallbackContext ctx)
    {
        tap?.Invoke();
    }

}