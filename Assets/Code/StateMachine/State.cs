public abstract class State
{
    protected Player _player;

    public State(Player player)
    {
        _player = player;
    }

    public virtual void Enter() {}
    public virtual void Update() {}
    public virtual void FixedUpdate() {}
    public virtual void Exit() {}
    public virtual void HandleInput() {}
}