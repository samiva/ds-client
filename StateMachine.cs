namespace BombPeli
{
    public class StateMachine
    {
        private State mCurrentState;

        public State CurrentState {
            get { return mCurrentState; }
           private set { mCurrentState=value; }
        }

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