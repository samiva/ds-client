namespace BombPeliLib
{
    public class StateMachine
    {
        public State CurrentState { get; private set; }

        public void ChangeState(State state)
        {
            CurrentState.EndState();
            CurrentState = state;
            CurrentState.BeginState();
        }

        public void update() 
        {
            CurrentState.ProcessState();
        }

        public void shutdown() 
        {
            CurrentState.EndState();
        }

    }
}