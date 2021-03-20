namespace BombPeliLib 
{
    public abstract class State 
    {
        protected StateMachine stateMachine;
        public State(StateMachine sm)
        {
            stateMachine = sm;
        }

        abstract public void BeginState();
        abstract public void ProcessState();
        abstract public void EndState(); 
    }
}