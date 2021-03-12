using System;
using BombPeli;
using System.Collections.Generic;
using kevincastejon;


namespace client
{
    class Program
    {
        StateMachine stateMachine;
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            new Program().Start();
        }

        public void Start() {
            stateMachine.ChangeState(new InitState(stateMachine));
        }


    }
}
