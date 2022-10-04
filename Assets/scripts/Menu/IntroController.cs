using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class IntroController : MonoBehaviour
{
    public PlayerInput input;

    public GameHandler handler;

    private void Start()
    {
        input.actions["Submit"].performed += _ => { handler.SetState(State.Menu); };
    }
}
