using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    public StationaryState stationaryState;    
    public RunningState runningState;
    public JumpingState jumpingState;
    public WallSlidingState wallSlidingState;
    public WallHuggingState wallHuggingState;

    private State _currentState;

    public StateMachine(Player player)
    {
        stationaryState = new StationaryState(player);
        runningState = new RunningState(player);
        jumpingState = new JumpingState(player);
        wallSlidingState = new WallSlidingState(player);
        wallHuggingState = new WallHuggingState(player);
        
        _currentState = stationaryState;
    }

    public void ChangeState(State newState)
    {
        _currentState.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    public void FixedUpdate()
    {
        _currentState.FixedUpdate();
    }

    public void HandleInput()
    {
        _currentState.HandleInput();
    }
}